"""YOLO-based models for leaf segmentation."""

# pylint: disable=wrong-import-position, wrong-import-order

import pathlib
import sys
from typing import Any

import cv2
import numpy as np
from ultralytics import YOLO

from model.abstract_model import AbstractModel
from model.tracking import DemoTrackerDictVersion, SimpleTracker

THIRDPARTY_PATH = str(
    (pathlib.Path(__file__).parent.parent.parent / "thirdparty").resolve()
)

sys.path.append(str(pathlib.Path(THIRDPARTY_PATH) / "segment-anything-2"))

# fmt: off
from sam2.build_sam import build_sam2  # noqa: E402
from sam2.sam2_image_predictor import SAM2ImagePredictor  # noqa: E402

# fmt: on


class TrackedAbstractModel(AbstractModel):
    """Abstract model for leaf segmentation with tracking."""

    def __init__(self, config: dict, device: str):
        super().__init__(config, device)
        assert "tracker" in self.config, "Tracker is not defined in config"
        if self.config["tracker"] == "DemoTracker":
            self.tracker = DemoTrackerDictVersion()
        elif self.config["tracker"] == "SimpleTracker":
            self.tracker = SimpleTracker()

        expected_keys = set(self.CONFIG_KEYS) | {"tracker"}
        assert (
            set(self.config.keys()) == expected_keys
        ), f"Expected keys: {expected_keys}, got {self.config.keys()}"

    def _predict_image(self, image: np.ndarray) -> tuple[list[np.ndarray], Any]:
        raise NotImplementedError

    def predict_masks(self, images: list) -> list[dict[int, dict]]:
        """Predict masks for a sequence of images.

        Args:
            images (list): Sequence of images.

        Returns:
            list: Sequence of integer masks.
                0 - background
                1 - mask,
        """
        self.tracker.reset()
        masks_log: list[dict[int, dict]] = []
        for img in images:
            predicted_masks, _scores = self._predict_image(img)
            masks = [{"segmentation": m} for m in predicted_masks]
            tracked_masks = self.tracker.update_on_new_masks(masks)

            masks_log.append(tracked_masks)

        return masks_log


class SimpleYoloSamModel(TrackedAbstractModel):
    """Model YOLO for detection and SAM2 for segmentation."""

    CONFIG_KEYS = {
        "yolo_model",
        "sam2_checkpoint",
        "sam2_model_cfg",
        "yolo_threshold",
    }

    def __init__(self, config: dict, device: str):
        super().__init__(config, device)

        assert set(self.config.keys()) == set(
            self.CONFIG_KEYS
        ), f"Expected keys: {self.CONFIG_KEYS}, got {self.config.keys()}"

        self.yolo_model = YOLO(config["yolo_model"]).to(self.device)
        sam2_model = build_sam2(
            config["model_cfg"], config["sam2_checkpoint"], device=self.device
        )
        self.predictor = SAM2ImagePredictor(sam2_model)

    def _predict_image(self, image: np.ndarray) -> tuple[list[np.ndarray], Any]:
        yolo_res = self.yolo_model(image, verbose=False)

        over_threshold = yolo_res[0].boxes.xywh[
            yolo_res[0].boxes.conf > self.config["yolo_threshold"]
        ]
        points = over_threshold.cpu().detach().numpy()[:, None, :2]
        labels = np.ones([len(points), 1])

        self.predictor.set_image(image)
        masks, scores, _ = self.predictor.predict(
            point_coords=points,
            point_labels=labels,
            box=None,
            multimask_output=False,
        )
        return masks[:, 0, :, :].astype(bool), scores


class YoloSegmentationModel(TrackedAbstractModel):
    """Model YOLO for detection and segmentation with masks."""

    CONFIG_KEYS = {
        "yolo_model",
        "yolo_threshold",
    }

    def __init__(self, config: dict, device: str):
        super().__init__(config, device)
        self.yolo_model = YOLO(config["yolo_model"]).to(self.device)

    def _predict_image(self, image: np.ndarray) -> tuple[list[np.ndarray], Any]:
        yolo_res = self.yolo_model(image, verbose=False)
        if yolo_res[0].masks is None:
            return [], None

        over_threshold = (
            (yolo_res[0].boxes.conf > self.config["yolo_threshold"])
            .cpu()
            .detach()
            .numpy()
        )
        masks = []
        for contour, over in zip(yolo_res[0].masks.xy, over_threshold):
            if not over:
                continue

            b_mask = np.zeros(image.shape[:2], np.uint8)
            contour = contour.astype(np.int32)
            contour = contour.reshape(-1, 1, 2)
            _ = cv2.drawContours(
                b_mask, [contour], -1, (255, 255, 255), cv2.FILLED
            )
            masks.append(b_mask > 0)

        return masks, None


def refine_masks_by_superpixels(
    masks: np.ndarray, superpixels: np.ndarray, threshold: float = 0.5
) -> np.ndarray:
    """Refine masks by superpixels.
        New masks would include only superpixels,
        which share of area in mask is more than threshold.

    Args:
        masks (np.ndarray): masks to average, shape (n_masks, h, w)
        superpixels (np.ndarray): superpixels, shape (h, w)
        threshold (float): threshold for mask area in superpixel

    Returns:
        np.ndarray: averaged masks, shape (n_masks, h, w)
    """

    max_value = superpixels.max()

    # how manay each superpixel have values
    sizes = np.zeros(max_value + 1)
    np.add.at(sizes, superpixels, 1)

    # how many each superpixel have values for each mask
    mask_sizes = np.zeros((max_value + 1, masks.shape[0]))
    np.add.at(mask_sizes, superpixels, masks.transpose(1, 2, 0))

    # which superpixels have more than threshold area of mask
    mask_superpixels = (mask_sizes / sizes[:, np.newaxis]) > threshold

    refined_masks = mask_superpixels[superpixels]  # shape (h, w, n_masks)
    return refined_masks.transpose(2, 0, 1)


class YoloTrackerModel(AbstractModel):
    """Model YOLO with out of the box tracker."""

    CONFIG_KEYS = {
        "yolo_model",
        "yolo_threshold",
    }

    def __init__(self, config: dict, device: str):
        super().__init__(config, device)
        self.yolo_model = YOLO(config["yolo_model"]).to(self.device)
        self.yolo_model.track(
            (np.random.random((416, 416, 3)) * 255).astype("uint8"),
            persist=True,
        )  # init tracker

        assert (
            set(self.config.keys()) == self.CONFIG_KEYS
        ), f"Expected keys: {self.CONFIG_KEYS}, got {self.config.keys()}"

    def predict_masks(self, images: list) -> list[dict[int, dict]]:
        """Predict masks for a sequence of images.

        Args:
            images (list): Sequence of images.

        Returns:
            list: Sequence of integer masks.
                0 - background
                1 - mask,
        """
        masks_log: list[dict[int, dict]] = []

        if self.yolo_model.predictor is not None:
            self.yolo_model.predictor.trackers[0].reset()

        for _i, image in enumerate(images):
            # Duplicate tracks slightly improve results on the dataset.
            # More accurate params tuning could achieve better results.
            yolo_res = self.yolo_model.track(
                image[:, :, ::-1],
                persist=True,
                verbose=False,
                retina_masks=True,
            )
            yolo_res = self.yolo_model.track(
                image[:, :, ::-1],
                persist=True,
                verbose=False,
                retina_masks=True,
            )

            if yolo_res[0].masks is None:
                masks_log.append({})
                continue

            over_threshold = (
                (yolo_res[0].boxes.conf > self.config["yolo_threshold"])
                .cpu()
                .detach()
                .numpy()
            )
            masks = {}

            if yolo_res[0].masks.xy is None or yolo_res[0].boxes.id is None:
                masks_log.append({})
                continue

            for contour, over, mask_id, xywh in zip(
                yolo_res[0].masks.xy,
                over_threshold,
                yolo_res[0].boxes.id,
                yolo_res[0].boxes.xywh,
            ):
                if not over:
                    continue

                b_mask = np.zeros(image.shape[:2], np.uint8)
                contour = contour.astype(np.int32)
                contour = contour.reshape(-1, 1, 2)
                _ = cv2.drawContours(
                    b_mask, [contour], -1, (255, 255, 255), cv2.FILLED
                )
                masks[int(mask_id)] = {
                    "segmentation": b_mask > 0,
                    "xywh": xywh.cpu().detach().numpy(),
                }

            masks_log.append(masks)

        self.yolo_model.predictor.trackers[0].reset()
        masks_log = self._renumerate_masks(masks_log)
        return masks_log
