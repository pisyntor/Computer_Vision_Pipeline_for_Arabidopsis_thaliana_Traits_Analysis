"""Final model."""

# pylint: disable=wrong-import-position, wrong-import-order

import pathlib
import sys
import tempfile

import cv2
import numpy as np
import torch

from model.abstract_model import AbstractModel, set_device
from model.tracking import (
    build_mask_mapping_greedy_sam_format,
    ensure_same_image_sizes,
)
from model.yolo_models import YoloTrackerModel
from saveload import _imsave_func

THIRDPARTY_PATH = str(
    (pathlib.Path(__file__).parent.parent.parent / "thirdparty").resolve()
)

sys.path.append(str(pathlib.Path(THIRDPARTY_PATH) / "segment-anything-2"))

# fmt: off
from sam2.build_sam import build_sam2, build_sam2_video_predictor  # noqa: E402
from sam2.sam2_image_predictor import SAM2ImagePredictor  # noqa: E402

# fmt: on


class VideoSAMFinal(AbstractModel):
    """Model based on SAM, mostly from the pre-contract one for debugging."""

    # class-level parameters to avoid too complicated config
    ensure_same_image_sizes = True
    remove_edge_detections = True
    simple_sam2_mode = True
    iou_refine_thresh = 0.55

    edge_threshold = 0.0625
    area_share_threshold = 0.25

    morphology_joining_iou_thresh = 0.7
    morphology_joining_kernel_size = 9

    remove_stems_parts = False
    remove_stems_parts_kernel_size = 3

    EXPECTED_INPUT_KEYS = {
        "yolo_model",
        "yolo_threshold",
        "sam2_cfg",
        "sam2_model",
        "morphology_join_stems",
    }

    def __init__(self, config: dict, device: str):
        """Initialize the model with the configuration.

        Args:
            config (dict): Configuration dictionary.
            device (str): Device.
        """
        config = config.copy()

        input_keys = set(config.keys())
        assert (
            input_keys == self.EXPECTED_INPUT_KEYS
        ), f"Incorrect config, diff is {input_keys ^ self.EXPECTED_INPUT_KEYS}"

        config["ensure_same_image_sizes"] = self.ensure_same_image_sizes
        config["remove_edge_detections"] = self.remove_edge_detections
        config["simple_sam2_mode"] = self.simple_sam2_mode
        config["iou_refine_thresh"] = self.iou_refine_thresh
        config["edge_threshold"] = self.edge_threshold
        config["area_share_threshold"] = self.area_share_threshold
        config["morphology_joining_iou_thresh"] = (
            self.morphology_joining_iou_thresh
        )
        config["morphology_joining_kernel_size"] = (
            self.morphology_joining_kernel_size
        )
        config["remove_stems_parts"] = self.remove_stems_parts
        config["remove_stems_parts_kernel_size"] = (
            self.remove_stems_parts_kernel_size
        )

        assert (not config["remove_stems_parts"]) or (
            not config["morphology_join_stems"]
        ), (
            "Mostly one of remove_stems_parts and morphology_join_stems "
            "should be set to True"
        )
        super().__init__(config, device)

        self.yolo_tracker_model = YoloTrackerModel(
            {
                "yolo_model": config["yolo_model"],
                "yolo_threshold": config["yolo_threshold"],
            },
            device,
        )

        if self.config["simple_sam2_mode"]:
            self.sam2_model = SAM2ImagePredictor(
                build_sam2(
                    self.config["sam2_cfg"],
                    self.config["sam2_model"],
                    device=set_device(self.device),
                    apply_postprocessing=False,
                )
            )
        else:

            self.sam2_model = build_sam2_video_predictor(
                self.config["sam2_cfg"],
                self.config["sam2_model"],
                device=set_device(self.device),
                apply_postprocessing=False,
            )

    def _remove_stems_parts_mophology(self, mask):
        ks = self.config["remove_stems_parts_kernel_size"]
        kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (ks, ks))

        mask_erode = cv2.erode(mask.astype(np.uint8), kernel, iterations=1)
        mask_erode = cv2.dilate(mask_erode, kernel, iterations=2)
        mask = mask & (mask_erode == 1)
        return mask

    def _join_masks_morphology_stems(
        self,
        yolo_mask: np.ndarray,
        sam_mask: np.ndarray,
    ) -> np.ndarray:
        """Join masks in a way possibly saving stems from yolo masks."""
        assert yolo_mask.shape == sam_mask.shape
        assert yolo_mask.dtype == bool
        assert sam_mask.dtype == bool

        ks = self.config["morphology_joining_kernel_size"]
        kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (ks, ks))

        no_stems_erode = cv2.erode(
            yolo_mask.astype(np.uint8), kernel, iterations=1
        )
        no_stems_erode = cv2.dilate(no_stems_erode, kernel, iterations=2)

        stems_only = yolo_mask & (no_stems_erode == 0) & (~sam_mask)
        joined = sam_mask | stems_only

        stems_joined = joined.astype(np.uint8)
        stems_joined = cv2.dilate(stems_joined, kernel, iterations=3)
        stems_joined = cv2.erode(stems_joined, kernel, iterations=3)

        stems_only_erode = cv2.dilate(
            stems_only.astype(np.uint8), kernel, iterations=3
        )

        stems_final = (stems_only_erode == 1) & (stems_joined == 1) & yolo_mask
        return sam_mask | stems_final

    def _sam_predict_simple(self, image, masks):
        if len(masks) == 0:
            return masks

        self.sam2_model.set_image(image)
        keys = []
        boxes = []
        for ann_obj_id, m in masks.items():
            mask = m["segmentation"]
            mask_where = np.array(np.where(mask))
            if mask_where.size == 0:  # len() wouldn't work here
                continue

            ls = mask_where.min(axis=-1)[::-1]
            rs = mask_where.max(axis=-1)[::-1]
            keys.append(ann_obj_id)
            boxes.append(np.array([ls, rs]))

            # # alternative approach could be to use xywh instead
            # x, y, w, h = m["xywh"]
            # boxes.append(np.array([x-w/2, y-h/2, x+w/2, y+h/2]))

        boxes = np.array(boxes)
        masks, _scores, _ = self.sam2_model.predict(
            point_coords=None,
            point_labels=None,
            box=boxes,
            multimask_output=False,
        )
        return {k: {"segmentation": v[0] > 0.5} for k, v in zip(keys, masks)}

    def _sam_predict_complicated(self, image, initial_masks):
        video_segments = []
        with tempfile.TemporaryDirectory() as tmpdirname:
            # save images to tmp dir
            _imsave_func(str(pathlib.Path(tmpdirname) / f"{0}.jpg"), image)
            inference_state = self.predictor.init_state(video_path=tmpdirname)
            self.predictor.reset_state(inference_state)

            # prompts = {}
            for ann_obj_id, m in initial_masks.items():
                mask = m["segmentation"]
                mask_where = np.array(np.where(mask))
                if mask_where.size == 0:  # len() wouldn't work here
                    continue

                ls = mask_where.min(axis=-1)[::-1]
                rs = mask_where.max(axis=-1)[::-1]

                ann_frame_idx = 0  # the frame index we interact with

                # `add_new_points_or_box` returns masks
                # for all objects added so far on this interacted frame
                _, out_obj_ids, out_mask_logits = (
                    self.predictor.add_new_points_or_box(
                        inference_state=inference_state,
                        frame_idx=ann_frame_idx,
                        obj_id=ann_obj_id,
                        box=np.array([ls, rs]),
                    )
                )

            for frame_ind, (
                out_frame_idx,
                out_obj_ids,
                out_mask_logits,
            ) in enumerate(self.predictor.propagate_in_video(inference_state)):
                assert frame_ind == out_frame_idx
                video_segments.append(
                    {
                        out_obj_id: {
                            "segmentation": (out_mask_logits[i][0] > 0.0)
                            .cpu()
                            .numpy()
                        }
                        for i, out_obj_id in enumerate(out_obj_ids)
                    }
                )

        return video_segments[0]

    def _sam_refine_one_image_masks(
        self, image: np.ndarray, masks: dict[int, dict]
    ) -> dict[int, dict]:
        """Refine masks with SAM2
        Args:
            image (np.ndarray): image
            masks (dict): masks

        Returns:
            dict: refined masks
        """

        sam_masks = (
            self._sam_predict_simple(image, masks)
            if self.config["simple_sam2_mode"]
            else self._sam_predict_complicated(image, masks)
        )

        mapping = build_mask_mapping_greedy_sam_format(masks, sam_masks)
        for old_id, (new_id, iou) in mapping.items():
            if iou > self.config["iou_refine_thresh"]:
                refined_mask = sam_masks[new_id]["segmentation"]

                if self.config["remove_stems_parts"]:
                    refined_mask = self._remove_stems_parts_mophology(
                        refined_mask
                    )

                if (
                    self.config["morphology_join_stems"]
                    and iou > self.config["morphology_joining_iou_thresh"]
                ):
                    refined_mask = self._join_masks_morphology_stems(
                        masks[old_id]["segmentation"], refined_mask
                    )

                masks[old_id]["segmentation"] = refined_mask

        return masks

    def _predict_masks(self, images: list) -> list[dict]:
        """Predict masks for a sequence of images."""
        masks = self.yolo_tracker_model.predict_masks(images)

        if self.config["remove_edge_detections"]:
            masks = [
                {
                    k: v
                    for k, v in masks_frame.items()
                    if self._filter_edge_areas(
                        v["segmentation"],
                        self.config["edge_threshold"],
                        self.config["area_share_threshold"],
                    )
                }
                for masks_frame in masks
            ]

        refined_masks = []
        for image, masks_frame in zip(images, masks):
            if self.device.startswith("cuda"):
                with torch.autocast("cuda", dtype=torch.bfloat16):
                    if torch.cuda.get_device_properties(0).major >= 8:
                        torch.backends.cuda.matmul.allow_tf32 = True
                        torch.backends.cudnn.allow_tf32 = True

                    refined = self._sam_refine_one_image_masks(
                        image, masks_frame
                    )
            else:
                refined = self._sam_refine_one_image_masks(image, masks_frame)

            refined_masks.append(refined)

        return refined_masks

    def predict_masks(self, images: list) -> list[dict[int, dict]]:
        """Predict masks for a sequence of images.

        Args:
            images (list): Sequence of images.

        Returns:
            masks (list): Sequences of masks.
                i-th element is a dict of masks for the i-th image.
        """
        if self.config["ensure_same_image_sizes"]:
            images = ensure_same_image_sizes(images, "")
        masks = self._predict_masks(images)
        masks = self._renumerate_masks(masks)
        return masks
