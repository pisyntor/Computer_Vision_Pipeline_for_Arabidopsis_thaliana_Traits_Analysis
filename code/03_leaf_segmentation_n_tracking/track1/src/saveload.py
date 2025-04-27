"""Module for reading and writing data."""

import json
import pathlib
import sys
import traceback

import imageio
import numpy as np
import pandas as pd
from moviepy.video.io.ImageSequenceClip import ImageSequenceClip

from dataset import OutputProjectPahts, get_parent_dir, string_nums_sorting_key
from masks import (
    draw_joined_masks_on_image,
    leaf_mask_to_image,
    mask_joined_to_masks_dict,
)


def _read_images(image_paths: pd.Series):
    """Read images from the dataset.

    Args:
        image_paths (pd.Series): dataset

    Returns:
        list[np.ndarray]: images
    """
    return [imageio.imread(path) for path in image_paths]


def _imsave_func(path: str, image: np.ndarray) -> None:
    imageio.imwrite(path, image)


def _imread_func(path: str) -> np.ndarray:
    return imageio.imread(path)


def read_image(ds_row: pd.Series, key="image_path") -> np.ndarray:
    """Read image from the dataset.

    Args:
        ds_row (pd.Series): dataset row
        key (str): key for the image path

    Returns:
        np.ndarray: image
    """
    return _imread_func(ds_row[key])


def read_masks(ds_row: pd.Series, key="mask_path") -> list[dict]:
    """Read masks from the dataset.

    Args:
        ds_row (pd.Series): dataset row
        key (str): key for the mask path

    Returns:
        list[dict]: masks in SAM2 format
    """
    joined_masks = _imread_func(ds_row[key])
    return mask_joined_to_masks_dict(joined_masks)


def _create_parent_dirs(path):
    pathlib.Path(path).parent.mkdir(parents=True, exist_ok=True)


class Saver:
    """Save output masks in specific formats.

    * save masks separately
    * save masks on image
    * save separate leafs masks
    * TODO: join video of masks
    """

    def __init__(self, out_dir: str, save_leafs: bool = True):
        """Init saver.

        Args:
            out_dir (str): dir for saving
            save_leafs (bool): save leafs masks separately (can be slow)

        """
        self.out_dir = out_dir
        self.save_leafs = save_leafs
        self.saved_leafs_inds: list[int] = []
        self.out_paths = OutputProjectPahts(out_dir)

    def save_masks(
        self,
        image: np.ndarray,
        ds_row: pd.Series,
        out_masks: dict[int, dict],
    ) -> None:
        """Save output masks.

        Args:
            image (np.ndarray): image
            ds_row (pd.Series): dataset row
            out_masks (dict[int, dict]): masks in SAM2 format:
                i-th element is a dict of masks for the i-th image.
                leaf_index -> {"segmentation": np.ndarray}

        """
        for not_on_image in [True, False]:
            image_with_masks = draw_joined_masks_on_image(
                image, out_masks, not_on_image=not_on_image
            )
            joined_masks_path = self.out_paths.make_joined_mask_path(
                ds_row, not_on_image=not_on_image
            )
            _create_parent_dirs(joined_masks_path)
            imageio.imwrite(joined_masks_path, image_with_masks)

        if not self.save_leafs:
            return

        for ind, mask in out_masks.items():
            self.saved_leafs_inds.append(ind)
            for binary in [True, False]:
                leaf_image = leaf_mask_to_image(
                    image, mask["segmentation"], binary
                )
                leaf_path = self.out_paths.make_leaf_i_mask_path(
                    ds_row, ind, binary=binary
                )
                _create_parent_dirs(leaf_path)
                imageio.imwrite(leaf_path, leaf_image)

    @staticmethod
    def save_dir_to_video(dir_path: str, video_path: str):
        """Save video from images.

        Args:
            images (list[np.ndarray]): images
            path (str): path to save video
        """
        video_path = video_path.replace(".mp4", ".avi")
        try:
            files = [
                str(p)
                for p in sorted(
                    pathlib.Path(dir_path).glob("*.png"),
                    key=lambda x: string_nums_sorting_key(x.name),
                )
            ]
            clip = ImageSequenceClip(files, fps=10)
            clip.write_videofile(
                video_path, verbose=False, logger=None, codec="png"
            )
        except KeyboardInterrupt:
            print("Keyboard interrupt while saving video")
            raise
        except Exception:
            print(f"Error while saving video: {video_path}")
            traceback.print_exc(file=sys.stdout)

    def finalize_sequence(self, row: pd.Series):
        """Finalize sequence, e.g. save videos.
        Args:
            sequence (pd.Series): sequence
        """
        for not_on_image in [True, False]:
            masks_path = get_parent_dir(
                self.out_paths.make_joined_mask_path(row, not_on_image)
            )
            output_video_path = self.out_paths.make_colored_masks_video_path(
                row, not_on_image
            )
            self.save_dir_to_video(masks_path, output_video_path)

        self.saved_leafs_inds = []

    def save_metrics(self, metrics: pd.DataFrame, per_seq: bool = False):
        """Save metrics to the output directory.

        Args:
            metrics (pd.DataFrame): metrics
            per_seq (bool): metrics per sequence or total
        """
        metrics_path = self.out_paths.make_metrics_path(per_seq)
        _create_parent_dirs(metrics_path)
        metrics.to_csv(metrics_path, index=True)

    def save_configs(self, configs: dict):
        """Save metrics to the output directory.

        Args:
            metrics (pd.DataFrame): metrics
        """
        configs_path = self.out_paths.make_config_path()
        _create_parent_dirs(configs_path)
        with open(configs_path, "w", encoding="utf-8") as f:
            json.dump(configs, f)


class EmptySaver(Saver):
    """Not saving for skipping saving."""

    def save_masks(self, image, ds_row, out_masks):
        pass

    def finalize_sequence(self, row):
        pass

    def save_metrics(self, metrics, per_seq=False):
        pass

    def save_configs(self, configs):
        pass


class Loader:
    """Load masks saved by Saver class."""

    def __init__(self, out_dir: str):
        """Init loader.

        Args:
            out_dir (str): dir for loading
        """
        self.out_paths = OutputProjectPahts(out_dir)

    def _get_leaf_indices(self, ds_row: pd.Series) -> list[int]:
        """Get available leaf indices from folder structure.

        Args:
            ds_row (pd.Series): dataset row

        Returns:
            list[int]: available leaf indices
        """
        indices: list[int] = []
        binary_mask_dir = pathlib.Path(
            self.out_paths.make_leafs_dir_path(ds_row, binary=True)
        )
        assert (
            binary_mask_dir.exists()
        ), f"Leafs dir not found: {binary_mask_dir}"

        for leaf_dir in binary_mask_dir.glob("leaf_*"):
            try:
                # Extract index from "leaf_N" format
                idx = int(leaf_dir.name.split("_")[1]) - 1
                indices.append(idx)
            except (IndexError, ValueError):
                continue

        return sorted(indices)

    def check_sequence_masks_exist(self, ds_row: pd.Series) -> bool:
        """Check if output for the sequence exists.

        Args:
            ds_row (pd.Series): some row for a sequence

        Returns:
            bool: output exists
        """
        return pathlib.Path(
            self.out_paths.make_leafs_dir_path(ds_row, binary=True)
        ).exists()

    def load_masks(self, ds_row: pd.Series) -> dict[int, dict]:
        """Load output masks.

        Args:
            ds_row (pd.Series): dataset row

        Returns:
            dict[int, dict]: masks in SAM2 format:
                i-th element is a dict of masks for the i-th image.
                leaf_index -> {"segmentation": np.ndarray}
        """
        masks = {}
        leaf_indices = self._get_leaf_indices(ds_row)

        for leaf_idx in leaf_indices:
            try:
                mask_path = self.out_paths.make_leaf_i_mask_path(
                    ds_row, leaf_idx, binary=True
                )
                if not pathlib.Path(mask_path).exists():
                    continue

                binary_mask = imageio.imread(mask_path)
                mask = (binary_mask > 0).any(axis=2)
                assert len(mask.shape) == 2, "Mask is not binary"
                masks[leaf_idx] = {"segmentation": mask}

            except KeyboardInterrupt:
                print("Keyboard interrupt while loading mask")
                raise

            except Exception:
                print(f"Error while loading mask: {mask_path}")
                traceback.print_exc()

        return masks
