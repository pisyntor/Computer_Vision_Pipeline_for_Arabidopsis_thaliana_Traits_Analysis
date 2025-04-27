"""Analyze the output of the leaf segmentation model."""

import pathlib

import numpy as np
import pandas as pd
from tqdm.auto import tqdm

from dataset import DEFAULT_DATA_PATH
from mapping import build_correspondence
from saveload import _imread_func, read_image, read_masks


def get_leafs(
    output_dir: str, ds_root: str = DEFAULT_DATA_PATH
) -> pd.DataFrame:
    """Get the leaf masks from the output directory.

    Args:
        output_dir (str): The path to the output directory,
            (including train/val...)

    Returns:
        pd.DataFrame: A DataFrame with the leaf masks.

    """
    output_path = pathlib.Path(output_dir)
    ds_path = pathlib.Path(ds_root)
    reps = list(output_path.glob("*/rep*"))

    items = []
    for rep in tqdm(reps):
        masks_paths = list(rep.glob("leaf*/*.png"))

        def img_leaf(x: pathlib.Path) -> tuple[str, int]:
            img = x.name
            leaf = int(x.parent.name.split("_")[-1])
            return img, -leaf

        masks_paths = sorted(masks_paths, key=img_leaf, reverse=True)
        for m in masks_paths:
            plant_id = rep.parent.name
            rep_id = rep.name
            items.append(
                {
                    "plant": plant_id,
                    "rep": rep_id,
                    "image_id": f"{plant_id}_{rep_id}_{m.name}",
                    "mask_path": str(m),
                    "image_path": str(
                        ds_path / "01_raw_dataset" / plant_id / rep_id / m.name
                    ),
                    "gt_mask_path": str(
                        ds_path / "02_leaf_labels" / plant_id / rep_id / m.name
                    ),
                }
            )

    return pd.DataFrame(items)


def add_gt_marks(df: pd.DataFrame, iou_threshold: float = 0.7) -> pd.DataFrame:
    """Mark masks as correct or incorrect.

    Args:
        df (pd.DataFrame): The DataFrame with the leaf masks.

    Returns:
        pd.DataFrame: The DataFrame with new rows:
            'is_true_positive' (not completely non-leaf data)
            'is_best_match' (the best match for the leaf)
    """
    df = df.copy()
    df["is_true_positive"] = None
    df["is_best_match"] = False

    for _, group in tqdm(df.groupby(["image_id"])):
        image = read_image(group.iloc[0])
        gt_masks = read_masks(group.iloc[0], key="gt_mask_path")
        gt_masks = list(gt_masks.values())

        gt_masks_union = np.zeros(image.shape[:2], dtype=bool)
        for m in gt_masks:
            gt_masks_union |= m["segmentation"]

        masks = []
        for m in group["mask_path"]:
            masks.append((_imread_func(m) > 0).any(axis=-1))

        inds, ious = build_correspondence(
            masks_reference=[m["segmentation"] for m in gt_masks],
            masks_predicted=masks,
        )

        for i, iou in zip(inds, ious):
            if iou > iou_threshold:
                df.loc[group.index[i], "is_best_match"] = True

        for i, m in enumerate(masks):
            intersection = m & gt_masks_union
            df.loc[group.index[i], "is_true_positive"] = (
                intersection.sum() / m.sum() > iou_threshold
            )

    return df.reset_index(drop=True)


def _chromaticity(image: np.ndarray, mask: np.ndarray) -> np.ndarray:
    """Return the chromaticity of the mask.

    Args:
        image (np.ndarray): The image WxHx3.
        mask (np.ndarray): The mask WxH.

    Returns:
        np.ndarray: The chromaticity of the mask (3).
    """
    img_mask = image[mask]
    return np.mean(img_mask, axis=0) / np.mean(img_mask)


def add_mask_features(
    df: pd.DataFrame, bigger_masks_count: int = 1
) -> pd.DataFrame:
    """Add features to the DataFrame.

    Args:
        df (pd.DataFrame): The DataFrame with the leaf masks.
        bigger_masks_count (int): The number of biggest masks to join.

    Returns:
        pd.DataFrame: The DataFrame with new columns:
            'area' (the area of the leaf mask)
            'dist_to_center' (the relative distance to the center of the image)
            'color_mask_bg' (relative dist to chromaticity of
                                    the bigger masks and background)
            'color_feature' (some formula for chromaticity)

    """
    df = df.copy()
    df["area"] = -1.0
    df["dist_to_center"] = -1.0
    df["color_mask_bg"] = -1.0
    df["color_feature"] = -1.0

    for _, group in tqdm(df.groupby("image_id")):
        image = read_image(group.iloc[0])
        masks = []
        for m in group["mask_path"]:
            masks.append((_imread_func(m) > 0).any(axis=-1))

        joined_bigger_masks = np.zeros_like(masks[0])
        for m in sorted(masks, key=lambda m: m.sum(), reverse=True)[
            :bigger_masks_count
        ]:
            joined_bigger_masks |= m

        plant_color = _chromaticity(image, joined_bigger_masks)
        bg_color = _chromaticity(image, ~joined_bigger_masks)

        for m, (ind, _row) in zip(masks, group.iterrows()):
            df.loc[ind, "area"] = m.sum()  # type: ignore
            mask_mass_center = np.array(np.where(m)).mean(axis=-1)
            mask_mass_center /= np.array(m.shape) - 1
            df.loc[ind, "dist_to_center"] = np.linalg.norm(  # type: ignore
                mask_mass_center - 0.5
            )

            chroma = _chromaticity(image, m)
            df.loc[ind, "color_mask_bg"] = (  # type: ignore
                (np.linalg.norm(chroma - bg_color))
                / (np.linalg.norm(chroma - plant_color) + 1)
            )
            df.loc[ind, "color_feature"] = (  # type: ignore
                3 * (0.35 - chroma[0]) + 0.38 - chroma[1]
            )

    return df.reset_index(drop=True)


def add_inside_features(
    df: pd.DataFrame,
    almost_all_threshold: float = 0.6,
    isin_threshold: float = 0.95,
) -> pd.DataFrame:
    """Add features to the DataFrame.

    Args:
        df (pd.DataFrame): The DataFrame with the leaf masks.

    Returns:
        pd.DataFrame: The DataFrame with new columns:
            'has_almost_all' (other masks are inside)
            'is_not_maximum' (the mask is not the biggest,
                excluding has_almost_all)
    """
    df = df.copy()
    df["has_almost_all"] = False
    df["is_not_maximum"] = False

    for _, group in tqdm(df.groupby("image_id")):
        masks = []
        for m in group["mask_path"]:
            masks.append((_imread_func(m) > 0).any(axis=-1))

        all_masks_union = np.zeros_like(masks[0])
        for m in masks:
            all_masks_union |= m

        for m, (ind, _row) in zip(masks, group.iterrows()):
            if m.sum() / all_masks_union.sum() > almost_all_threshold:
                df.loc[ind, "has_almost_all"] = True  # type: ignore

        for ind1, m1 in enumerate(masks):
            for ind2, m2 in enumerate(masks):
                if ind1 == ind2:
                    continue

                if (m1 & m2).sum() / m2.sum() > isin_threshold:
                    df.loc[group.index[ind1], "is_not_maximum"] = True

    return df.reset_index(drop=True)