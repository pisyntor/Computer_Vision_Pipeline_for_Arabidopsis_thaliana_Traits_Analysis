"""Module for object tracking."""

import warnings
from collections import Counter
import numpy as np
from PIL import Image
from mapping import build_mask_mapping_greedy_dicts


def build_mask_mapping_greedy_sam_format(
    masks_reference: dict[int, dict],
    masks_predicted: dict[int, dict],
) -> dict[int, tuple[int, float]]:
    """Calculate correspondence between masks given with SAM format."""
    ref_masks_np  = {k: v["segmentation"] for k, v in masks_reference.items()}
    pred_masks_np = {k: v["segmentation"] for k, v in masks_predicted.items()}

    return build_mask_mapping_greedy_dicts(ref_masks_np, pred_masks_np)


def change_mask_resolution(
    mask: np.ndarray, new_size: tuple[int, int]
) -> np.ndarray:
    """Change resolution of the mask.

    Args:
        mask (np.ndarray): mask
        new_size (tuple[int, int]): new size as np.shape

    Returns:
        np.ndarray: new mask
    """
    assert len(new_size) == 2, "New size should be 2D"
    assert len(mask.shape) == 2, "Mask should be 2D"
    new_size = new_size[::-1]  # PIL uses (width, height)
    return np.array(
        Image.fromarray(mask).resize(
            new_size, resample=Image.Resampling.NEAREST
        )
    )


def _change_resolution_image(
    image: np.ndarray, new_size: tuple[int, int]
) -> np.ndarray:
    """Change resolution of the image.

    Args:
        image (np.ndarray): image
        new_size (tuple[int, int]): new size as np.shape

    Returns:
        np.ndarray: new image
    """
    assert len(new_size) == 2, "New size should be 2D"
    assert len(image.shape) == 3, "Image should be 3D"

    new_size = new_size[::-1]  # PIL uses (width, height)
    return np.array(
        Image.fromarray(image).resize(
            new_size, resample=Image.Resampling.BICUBIC
        )
    )


def ensure_same_image_sizes(
    images: list[np.ndarray], description: str
) -> list[np.ndarray]:
    """Ensure all the images have the same size,
        change resolution otherwise.

    Args:
        images (list[np.ndarray]): list of images
        description (str): description for warning

    Returns:
        list[np.ndarray]: list of images with the same size
    """
    sizes = Counter([img.shape for img in images])
    if len(sizes) <= 1:
        return images

    print(f"Warning! Sizes of images {description} are different.")
    print(
        f"    There are {len(sizes)} different sizes: {sizes.most_common(3)}..."
    )
    print(
        "    Resizing images to the most common size: "
        f"{sizes.most_common(1)[0][0]}"
    )

    new_size = sizes.most_common(1)[0][0][:2]
    return [_change_resolution_image(img, new_size) for img in images]


class DemoObjectTracker:
    """Simplest object tracking."""

    def __init__(self):
        """Init class."""
        self.prev_masks = []

    @staticmethod
    def new_tracking_list(previous_masks, next_masks, threshold=0.1):
        """Function to update list of masks with new masks."""
        result_masks = []
        next_masks = list(next_masks)
        for cur_prev_mask in previous_masks:
            max_similarity = 0
            max_similarity_ind = None
            for i, m in enumerate(next_masks):
                similarity = (
                    cur_prev_mask["segmentation"] * m["segmentation"]
                ).sum() / cur_prev_mask["segmentation"].sum()
                if similarity > max_similarity:
                    max_similarity = similarity
                    max_similarity_ind = i

            if max_similarity > threshold:
                result_masks.append(next_masks[max_similarity_ind])
                next_masks.pop(max_similarity_ind)

        other_sorted_decreased_size = sorted(
            next_masks, key=lambda x: x["segmentation"].sum(), reverse=True
        )
        return result_masks + other_sorted_decreased_size

    # @classmethod
    # def _sam2_to_onemask(self, masks: list) -> np.ndarray:
    #     """Join masks from SAM2 format to a single array with
    #         0 - bacground
    #         1 - mask #1
    #         2 - mask #2
    #         ...

    #     Args:
    #         masks (list): masks in SAM2 format

    #     Returns:
    #         np.ndarray: joined to one image masks
    #     """

    def update_on_new_masks(self, masks: list) -> list:
        """Function to update on new masks from new image.

        Args:
            masks (list): - list of new occurance (sam2 format)

        Returns:
            list: updated image with masks
        """
        if (
            len(self.prev_masks) != 0
            and len(masks) != 0
            and self.prev_masks[0]["segmentation"].shape
            != masks[0]["segmentation"].shape
        ):
            self.prev_masks = [
                change_mask_resolution(
                    m["segmentation"], masks[0]["segmentation"].shape
                )
                for m in self.prev_masks
            ]
            warnings.warn(
                "Resolution change detected, resizing previous masks."
            )

        self.prev_masks = self.new_tracking_list(self.prev_masks, masks)
        return self.prev_masks

    def reset(self):
        """Reset the tracker."""
        self.prev_masks = []


class DemoTrackerDictVersion:
    """Demo tracker with dict interface."""

    def __init__(self):
        self.tracker = DemoObjectTracker()

    def update_on_new_masks(self, masks: list) -> dict:
        """Update on new masks."""
        res = self.tracker.update_on_new_masks(masks)
        return dict(enumerate(res))

    def reset(self):
        """Reset the tracker."""
        self.tracker.reset()


class SimpleTracker:
    """Object tracker by ID."""

    def __init__(self, iou_threshold: float = 0.3):
        """Init class."""
        self.prev_shape = None
        self.prev_masks: list[dict] = []
        self.id_stat: dict[int, int] = {}
        self.iou_threshold = iou_threshold

    def _ensure_same_size(self, new_masks: list[dict]):
        """Function to fix the size of the previous masks."""
        if len(new_masks) == 0:
            return

        new_size = new_masks[0]["segmentation"].shape
        for m in new_masks:
            assert m["segmentation"].shape == new_size, (
                "All masks should have the same size, "
                "found {m.shape} and {new_size}"
            )

        if self.prev_shape is not None and self.prev_shape != new_size:
            warnings.warn(
                "Resolution change detected, "
                "resizing previous masks {self.prev_shape} -> {new_size}."
            )

            new_prev_masks = []
            for prev_mask in self.prev_masks:
                pm_copy = prev_mask.copy()
                if pm_copy["segmentation"].shape != new_size:
                    pm_copy["segmentation"] = change_mask_resolution(
                        pm_copy["segmentation"], new_size
                    )
                new_prev_masks.append(pm_copy)
            self.prev_masks = new_prev_masks

        self.prev_shape = new_size

    @staticmethod
    def _assign_prev_ids(
        prev_masks: dict[int, dict],
        new_masks: dict[int, dict],
        iou_threshold: float,
    ):
        """Function to assign ids from previous to masks."""
        prev_masks_np = {k: v["segmentation"] for k, v in prev_masks.items()}
        new_masks_np = {k: v["segmentation"] for k, v in new_masks.items()}

        mapping = build_mask_mapping_greedy_dicts(prev_masks_np, new_masks_np)
        _ious = [iou for _, (_, iou) in mapping.items()]
        assert sorted(_ious, reverse=True) == _ious, "Mapping is not sorted"
        return {
            mask_ind: prev_id
            for (prev_id, (mask_ind, iou)) in mapping.items()
            if iou > iou_threshold
        }

    def get_new_id(self):
        """Function to get new index."""
        return max(self.id_stat.keys(), default=-1) + 1

    def update_on_new_masks(self, masks: list[dict]) -> dict[int, dict]:
        """Function to update on new masks from new image.

        Args:
            masks (list): - list of new occurance (sam2 format)

        Returns:
            dict: updated image with masks
        """
        self._ensure_same_size(masks)

        masks = sorted(
            masks, key=lambda x: x["segmentation"].sum(), reverse=True
        )
        masks = [m.copy() for m in masks]
        for m in masks:
            m["id"] = None

        for mask_ind, prev_id in self._assign_prev_ids(
            prev_masks={m["id"]: m for m in self.prev_masks},
            new_masks=dict(enumerate(masks)),
            iou_threshold=self.iou_threshold,
        ).items():
            masks[mask_ind]["id"] = prev_id
            self.id_stat[prev_id] += 1

        for m in masks:
            if m["id"] is None:
                new_id = self.get_new_id()
                self.id_stat[new_id] = 1
                m["id"] = new_id

        self.prev_masks = masks
        return {m["id"]: m for m in self.prev_masks}

    def reset(self):
        """Reset the tracker."""
        self.prev_shape = None
        self.prev_masks = []
        self.id_stat = {}
