"""Procssing paths for the project."""

import logging
import math
import os
import pathlib
import random
import re
from typing import Optional

import pandas as pd

GIT_DIR_ROOT = pathlib.Path(__file__).resolve().parent.parent
DEFAULT_DATA_PATH = str(GIT_DIR_ROOT.parent / "leaf_dataset")
DEFAULT_IMAGE_ROOT = str(
    GIT_DIR_ROOT.parent / "leaf_dataset" / "01_raw_dataset"
)
DEFAULT_MASK_ROOT = str(GIT_DIR_ROOT.parent / "leaf_dataset" / "02_leaf_labels")
DEFAULT_DS_PATH = str(GIT_DIR_ROOT / "data_meta" / "ds.csv")


def string_nums_sorting_key(s: str) -> list[str | tuple[int, str]]:
    """
    Split a string into parts,
        converting number segments to tuples of (int_value, original_str)
        for consistent sorting of zero-padded numbers.

    Args:
        s (str): Input string containing numbers

    Returns:
        list[str | tuple[int, str]]: List of string parts
    """
    parts = re.split(r"(\d+)", s)
    return [(int(part), part) if part.isdigit() else part for part in parts]


def sort_strings_with_numbers(strings: list[str]) -> list[str]:
    """
    Sort a list of strings, taking into account the numerical values.

    Args:
        strings (list[str]): List of strings to sort

    Returns:
        list[str]: Sorted list of strings
    """
    return sorted(strings, key=string_nums_sorting_key)


def get_parent_dir(path_str: str) -> str:
    """Return the parent directory of the path.
    Args:
        path_str (str): Path to the file.

    Returns:
        str: Parent directory of the file.
    """
    return str(pathlib.Path(path_str).parent)


def save_dataset(
    ds: pd.DataFrame, path: str, images_root: str, masks_root: Optional[str]
) -> None:
    """
    Save the dataset to a CSV file with paths relative to the file.

    Args:
        ds (pd.DataFrame): DataFrame with the dataset.
        path (str): Path to the CSV file.
        images_root (str): Root directory with images (using relpaths in ds)
        masks_root (Optional[str]): Root directory with masks (relpaths in ds)
    """
    ds = ds.copy()
    assert not pathlib.Path(path).exists(), f"File already exists: {path}"

    def _to_relative_image(p: str) -> str:
        return os.path.relpath(p, images_root)

    def _to_relative_mask(p: str) -> str:
        if masks_root is None:
            assert len(p) == 0, "Mask path is not empty, but masks_root is None"
        return "" if len(p) == 0 else os.path.relpath(p, masks_root)

    ds["image_path"] = ds["image_path"].apply(_to_relative_image)
    ds["mask_path"] = ds["mask_path"].apply(_to_relative_mask)
    ds.to_csv(path, index=False)


def load_dataset(
    path: Optional[str] = None,
    images_root: str = DEFAULT_IMAGE_ROOT,
    masks_root: Optional[str] = DEFAULT_MASK_ROOT,
) -> pd.DataFrame:
    """
    Load the dataset from a CSV file
        and replace relative to file paths to global paths.

    Args:
        path (str): Path to the CSV file, use default project DS by default.
        images_root (str): Root directory with images (using relpaths in ds)
        masks_root (Optional[str]): Root directory with masks (relpaths in ds)

    Returns:
        pd.DataFrame: DataFrame with the dataset.
    """
    if path is None:
        path = DEFAULT_DS_PATH

    ds = pd.read_csv(path)

    def _to_global_image(p: str) -> str:
        return str((pathlib.Path(images_root) / p).resolve())

    def _to_global_mask(p: str) -> str:
        if len(p) == 0:
            assert (
                masks_root is None
            ), "Mask path is empty, but masks_root is not None"
            return ""
        return (
            ""
            if masks_root is None
            else str((pathlib.Path(masks_root) / p).resolve())
        )

    ds["image_path"] = ds["image_path"].apply(_to_global_image)
    ds["mask_path"] = ds["mask_path"].apply(_to_global_mask)
    return ds


def load_or_build_dataset(
    path: Optional[str] = "default",
    images_root: str = DEFAULT_IMAGE_ROOT,
    masks_root: Optional[str] = DEFAULT_MASK_ROOT,
) -> pd.DataFrame:
    """Load the dataset, if none - build it.

    Args:
        path (Optional[str]): .csv with dataset or dir with files to build one
        images_root (str): root dir with images
        masks_root (Optional[str]): root dir with masks (None if not available)

    Returns:
        pd.DataFrame: DataFrame with the dataset.
    """
    if path == "default":
        path = DEFAULT_DS_PATH

    if path is not None:
        ds = load_dataset(path, images_root=images_root, masks_root=masks_root)
    else:
        logging.warning(
            "No path is provided, building the dataset with "
            f"images in {images_root} and masks in {masks_root}."
        )
        ds = _parse_ds_files(images_root=images_root, masks_root=masks_root)
        logging.warning(f"Loaded ds with {len(ds)} test images.")
        ds["nn_role"] = "test"

    return ds


class ProjectPaths:
    """Class to handle paths for the project."""

    def __init__(
        self,
        images_root: str = DEFAULT_IMAGE_ROOT,
        masks_root: Optional[str] = DEFAULT_MASK_ROOT,
    ):
        """Initialize the class with paths.

        Args:
            image_path (str): Path to the data directory.
            mask_path (Optional[str]): Path to the mask directory
                None if not available.
        """
        self.images_root = pathlib.Path(images_root)
        self.masks_root = (
            None if masks_root is None else pathlib.Path(masks_root)
        )

    def get_plants(self) -> list[str]:
        """
        Get the list of plant names.

        Returns:
            list[str]: List of plant names.
        """
        return sorted(
            (
                p.name
                for p in self.images_root.glob("*")
                if not p.name.startswith(".")
            ),
            key=string_nums_sorting_key,
        )

    def get_reps(self, plant_name: str) -> list[str]:
        """ "
        Get the list of repetitions names for a plant.

        Args:
            plant_name (str): Name of the plant.

        Returns:
            list[str]: List of repetition names.
        """
        return sorted(
            (p.name for p in self.images_root.glob(f"{plant_name}/*")),
            key=string_nums_sorting_key,
        )

    def get_images(self, plant_name: str, rep_name: str) -> list[str]:
        """
        Get the list of image names for a plant and repetition.

        Args:
            plant_name (str): Name of the plant.
            rep_name (str): Name of the repetition.

        Returns:
            list[str]: List of image names.
        """
        return sorted(
            (
                p.name
                for p in self.images_root.glob(f"{plant_name}/{rep_name}/*.png")
            ),
            key=string_nums_sorting_key,
        )

    def get_image_masks(
        self, plant_name: str, rep_name: str
    ) -> list[tuple[str, str]]:
        """
        Get the list of image and mask paths for a plant and repetition.

        Args:
            plant_name (str): Name of the plant.
            rep_name (str): Name of the repetition.

        Returns:
            list[tuple[str, str]]:
                Sorted list of tuples with image and mask paths.
                masks are "" if labels are not available (masks_root is None).

        """

        def _get_image_path(image):
            return str(self.images_root / plant_name / rep_name / image)

        def _get_mask_path(image):
            return (
                str(self.masks_root / plant_name / rep_name / image)
                if self.masks_root
                else ""
            )

        return sorted(
            (
                (
                    _get_image_path(image),
                    _get_mask_path(image),
                )
                for image in self.get_images(plant_name, rep_name)
            ),
            key=lambda x: string_nums_sorting_key(x[0]),
        )


class OutputProjectPahts:
    """Build paths for output project files.

    out_dir
    - metrics.csv
    - train/val/test
    -- plant
    --- rep_*
    ---- all_masks_on_black.mp4
    ---- all_masks_on_black
    ----- *.png
    ---- all_masks_on_image.mp4
    ---- all_masks_on_image
    ----- *.png
    ---- leaf_binary_masks
    ----- leaf_{i}
    ------ *.png
    ---- leaf_extracted_images
    ----- leaf_{i}
    ------ *.png

    """

    def __init__(self, out_dir: str, separate_nn_role: bool = True):
        """Initialize the class with paths.

        Args:
            out_dir (str): Path to place output files.
        """
        self.data_path = pathlib.Path(out_dir)
        self.separate_nn_role = separate_nn_role

    def _rep_path(self, ds_row: pd.Series) -> pathlib.Path:
        if self.separate_nn_role:
            role_path = self.data_path / ds_row["nn_role"]
        else:
            role_path = self.data_path
        return role_path / ds_row["plant"] / ds_row["rep"]

    def _basename(self, ds_row: pd.Series) -> str:
        return pathlib.Path(ds_row["image_path"]).name

    def make_joined_mask_path(
        self, ds_row: pd.Series, not_on_image: bool
    ) -> str:
        """Make paths for image-wise output files.

        Args:
            ds_row (pd.Series): dataset row
            not_on_image (bool): mask black+colored or background+colored

        Returns:
        """
        subdir = "all_masks_on_black" if not_on_image else "all_masks_on_image"
        return str(
            self._rep_path(ds_row) / subdir / f"{self._basename(ds_row)}"
        )

    def make_leafs_dir_path(self, ds_row: pd.Series, binary: bool) -> str:
        """Make path for the directory with leafs.

        Args:
            ds_row (pd.Series): dataset row
            binary (bool): binary mask or extracted image

        Returns:
            str: path to the directory with leaf masks
        """
        subdir = "leaf_binary_masks" if binary else "leaf_extracted_images"
        return str(self._rep_path(ds_row) / subdir)

    def make_leaf_i_mask_path(
        self, ds_row: pd.Series, i: int, binary: bool
    ) -> str:
        """Make paths for image-wise output files.

        Args:
            ds_row (pd.Series): dataset row
            i (int): leaf number
            binary (bool): binary mask or extracted image

        Returns:
        """
        leaf_dif_path = self.make_leafs_dir_path(ds_row, binary)
        return str(
            pathlib.Path(leaf_dif_path)
            / f"leaf_{i+1}"
            / f"{self._basename(ds_row)}"
        )

    def make_leaf_i_mask_video_path(self, leaf_i_dir_path: str) -> str:
        """Make paths for video-wise output files.

        Args:
            leaf_i_dir_path (str): path to the directory with leaf masks

        Returns:
        """
        return get_parent_dir(leaf_i_dir_path) + ".mp4"

    def make_colored_masks_video_path(
        self, ds_row: pd.Series, not_on_image: bool
    ) -> str:
        """Make paths for video-wise output files.

        Args:
            ds_row (pd.Series): dataset row

        Returns:
        """
        colored_mask_dir = get_parent_dir(
            self.make_joined_mask_path(ds_row, not_on_image)
        )
        return str(colored_mask_dir) + ".mp4"

    def make_metrics_path(self, per_seq: bool) -> str:
        """Make path for the metrics.csv file.

        Args:
            per_seq (bool): metrics per sequence or total
        """
        name = "metrics_per_sequence" if per_seq else "metrics"
        return str(self.data_path / f"{name}.csv")

    def make_config_path(self) -> str:
        """Make path for the config.json file."""
        return str(self.data_path / "config.json")


def _parse_ds_files(
    images_root: str = DEFAULT_IMAGE_ROOT,
    masks_root: Optional[str] = DEFAULT_MASK_ROOT,
) -> pd.DataFrame:
    """Parse files from data directory to build a dataset."""

    ds_items = []
    paths = ProjectPaths(images_root=images_root, masks_root=masks_root)

    plants = paths.get_plants()
    for plant in plants:
        reps = paths.get_reps(plant)
        for rep in reps:
            for img_num, (image_path, mask_path) in enumerate(
                # is already sorted w.r.t. nums
                paths.get_image_masks(plant, rep)
            ):
                assert pathlib.Path(
                    image_path
                ).exists(), f"Image not found: {image_path}"
                assert (
                    len(mask_path) == 0 or pathlib.Path(mask_path).exists()
                ), f"Mask not found: {mask_path}"
                ds_items.append(
                    {
                        "plant": plant,
                        "rep": rep,
                        "image_num": img_num,
                        "image_path": image_path,
                        "mask_path": mask_path,
                        "nn_role": None,
                    }
                )

    return pd.DataFrame(ds_items)


def _assign_roles(
    objects: list[object],
    rnd_generator: random.Random,
    minimal_test_share: float,
    minimal_val_share: float,
) -> dict[object, str]:
    """Assign roles to objects based on the shares."""
    objects = objects.copy()
    rnd_generator.shuffle(objects)

    n_test = math.ceil(minimal_test_share * len(objects))
    n_val = math.ceil(minimal_val_share * len(objects))
    assert n_test + n_val < len(objects), "Not enough objects for train"

    test_objects = objects[:n_test]
    val_objects = objects[n_test : n_test + n_val]
    train_objects = objects[n_test + n_val :]

    objects_roles = {}
    for objects_group, group_role in [
        (test_objects, "test"),
        (val_objects, "val"),
        (train_objects, "train"),
    ]:
        for r in objects_group:
            objects_roles[r] = group_role

    return objects_roles


def _split_dataset_rep_based(
    ds: pd.DataFrame,
    minimal_test_share: float,
    minimal_val_share: float,
    seed: int = 1,
) -> pd.DataFrame:
    """Split dataset based on the rep column."""
    assert minimal_test_share > 0, "Test share should be positive"

    rnd_generator = random.Random(seed)
    ds = ds.copy()
    assert (
        ds["nn_role"].isna().all()
    ), "Dataset should not have nn_role assigned yet"

    # Groupby plant and rep
    plants = ds["plant"].unique()
    for plant in plants:
        reps = list(ds[ds["plant"] == plant]["rep"].unique())
        rep_roles = _assign_roles(
            reps, rnd_generator, minimal_test_share, minimal_val_share
        )
        for rep, role in rep_roles.items():
            ds.loc[(ds["plant"] == plant) & (ds["rep"] == rep), "nn_role"] = (
                role
            )

    return ds


def build_dataset(
    images_root: str = DEFAULT_IMAGE_ROOT,
    masks_root: str = DEFAULT_MASK_ROOT,
    minimal_test_share: float = 0.1,
    minimal_val_share: float = 0.2,
    seed: int = 1,
) -> pd.DataFrame:
    """Build dataset (table) from the project data.

    Args:
        images_root (str): Path to the data directory.
        masks_root (str): Path to the mask directory
            None if not available.
        minimal_test_share (float): Share of test images.
        minimal_val_share (float): Share of validation images.

    Returns:
        pd.DataFrame: DataFrame with the dataset.
    """
    ds = _parse_ds_files(images_root=images_root, masks_root=masks_root)
    assert len(ds) > 0, (
        f"No images found in the dataset with IMG_ROOT={images_root}. "
        "Possible reasons: incorrect directory; "
        "not following the structure 'IMG_ROOT/<group>/<rep>/<image.png>'."
    )
    ds = _split_dataset_rep_based(
        ds, minimal_test_share, minimal_val_share, seed
    )
    return ds
