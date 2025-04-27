"""Abstract model, setting the interface."""

import numpy as np
import torch


class AbstractModel:
    """Abstract model class, setting the interface for the model classes."""

    def __init__(self, config: dict, device: str):
        """Initialize the model with the configuration.

        Args:
            config (dict): Configuration dictionary.
            device (str): Device to use.
        """
        self.config = config
        self.device = device

    def predict_masks(self, images: list) -> list[dict[int, dict]]:
        """Predict masks for a sequence of images.

        Args:
            images (list): Sequence of images.

        Returns:
            list: Sequence of integer masks.
                0 - background
                1 - mask,
        """
        raise NotImplementedError()

    def get_config(self):
        """Get the configuration of the model.

        Returns:
            dict: Configuration dictionary.
        """
        return self.config

    @staticmethod
    def _renumerate_masks(
        masks: list[dict[int, dict]]
    ) -> list[dict[int, dict]]:
        """Make new ids for masks.

        Args:
            masks (list[dict[int, dict]]): masks with possibly strange ids

        Returns:
            list[dict[int, dict]]: masks with new ids
        """
        start_id = 0
        result_masks = []
        old_ids_to_new_ids = {}

        for cur_masks in masks:
            not_added_ids_to_size: dict[int, int] = {}
            for old_id, m in cur_masks.items():
                if old_id not in old_ids_to_new_ids:
                    not_added_ids_to_size[old_id] = m["segmentation"].sum()

            for old_id, _ in sorted(
                not_added_ids_to_size.items(), key=lambda x: x[1], reverse=True
            ):
                old_ids_to_new_ids[old_id] = start_id
                start_id += 1

            new_masks = {}
            for old_id, m in cur_masks.items():
                new_masks[old_ids_to_new_ids[old_id]] = m
            result_masks.append(new_masks)

        return result_masks

    @staticmethod
    def _filter_edge_areas(
        mask: np.ndarray, edge_share: float = 0.125, area_share_threshold=0.5
    ) -> bool:
        """
        Filter out the masks, with areas mostly on the edge.

        Args:
            mask (np.ndarray): mask
            edge_share (float): share of edge on the image
            area_share_threshold (float): area threshold

        Returns:
            bool: True if the mask is not mostly on the edge
        """
        h, w = mask.shape
        h_small = int(h * edge_share)
        h_large = int(h * (1 - edge_share))
        w_small = int(w * edge_share)
        w_large = int(w * (1 - edge_share))

        mask_area = mask.sum()
        for region in [
            mask[:h_small, :],
            mask[h_large:, :],
            mask[:, :w_small],
            mask[:, w_large:],
        ]:
            if region.sum() / mask_area > area_share_threshold:
                return False
        return True


def set_device(device: str) -> torch.device:
    """Set the device for the model."""
    torch_device = torch.device(device)
    if torch_device.type == "cuda:1":
        # use bfloat16 for the entire notebook
        torch.autocast("cuda:1", dtype=torch.bfloat16).__enter__()
        # turn on tfloat32 for Ampere GPUs
        # (https://pytorch.org/docs/stable/notes/
        #   cuda.html#tensorfloat-32-tf32-on-ampere-devices)
        if torch.cuda.get_device_properties(0).major >= 8:
            torch.backends.cuda.matmul.allow_tf32 = True
            torch.backends.cudnn.allow_tf32 = True

    return torch_device
