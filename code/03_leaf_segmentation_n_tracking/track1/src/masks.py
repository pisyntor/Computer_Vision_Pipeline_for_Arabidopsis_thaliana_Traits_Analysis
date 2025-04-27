"""Color encoding and decoding functions for masks."""

import numpy as np

DEFAULT_COLORS = [
    (244, 64, 14),
    (48, 57, 249),
    (234, 250, 37),
    (24, 193, 65),
    (245, 130, 49),
    (231, 80, 219),
    (0, 182, 173),
    (115, 0, 218),
    (191, 239, 69),
    (255, 250, 200),
    (250, 190, 212),
    (66, 212, 244),
    (155, 99, 36),
    (220, 190, 255),
    (69, 158, 220),
    (255, 216, 177),
    (98, 2, 37),
    (227, 213, 12),
    (79, 159, 83),
    (170, 23, 101),
    (170, 255, 195),
    (169, 169, 169),
    (181, 111, 119),
    (144, 121, 171),
    (9, 125, 244),
    (184, 70, 30),
    (154, 35, 246),
    (229, 225, 238),
    (141, 254, 82),
    (31, 200, 209),
    (194, 217, 105),
    (91, 20, 124),
    (181, 220, 171),
    (37, 3, 193),
]
OUT_OF_LIST_COLOR = (255, 255, 255)


def _draw_masks_on_canvas(canvas: np.ndarray, masks: dict[int, dict]) -> None:
    """Draw masks on the canvas.

    Args:
        canvas (np.ndarray): canvas
        masks (dict[int, dict]): masks

    Returns:
        None
    """
    for i, m in masks.items():
        color = (
            DEFAULT_COLORS[i] if i < len(DEFAULT_COLORS) else OUT_OF_LIST_COLOR
        )

        canvas[m["segmentation"]] = color


def draw_joined_masks_on_image(
    image: np.ndarray, masks: dict[int, dict], not_on_image: bool
) -> np.ndarray:
    """Draw masks on the image.

    Args:
        image (np.ndarray): image
        masks (dict): masks in SAM2 format
        not_on_image (bool): draw masks on the black background

    Returns:
        np.ndarray: image with masks
    """
    canvas = np.zeros_like(image) if not_on_image else image.copy()
    _draw_masks_on_canvas(canvas, masks)
    return canvas


def mask_joined_to_masks_dict(mask: np.ndarray) -> dict[int, dict]:
    """Split joined masks to separate masks.

    Args:
        mask (np.ndarray): joined masks

    Returns:
        list: masks in SAM2 format
    """
    masks = {}

    all_masks_colors = set(
        tuple(x.tolist()) for x in np.unique(mask.reshape(-1, 3), axis=0)
    )
    for c in all_masks_colors:
        if not (
            c in DEFAULT_COLORS or c == OUT_OF_LIST_COLOR or c == (0, 0, 0)
        ):
            # # More strict would be to raise error instead
            # raise RuntimeError(f"Unexpected color {c}")
            print(
                f"Unexpected color {c}, adding temporally to known ones.\n"
                f"New expected colors set size: {len(DEFAULT_COLORS) + 1}"
            )
            DEFAULT_COLORS.append(c)

    for i, color in enumerate(DEFAULT_COLORS + [OUT_OF_LIST_COLOR]):
        if color not in all_masks_colors:
            continue

        mask_i = np.all(mask == color, axis=-1)
        if mask_i.sum() > 0:
            masks[i] = {"segmentation": mask_i, "_detection_index": i}
    return masks


def mask_joined_to_masks_dict_deprecated(mask: np.ndarray) -> list[dict]:
    """Deprecated version with list output."""
    masks = mask_joined_to_masks_dict(mask)
    keys = sorted(masks.keys())
    return [masks[k] for k in keys]


def leaf_mask_to_image(
    image: np.ndarray, leaf_mask: np.ndarray, binary: bool
) -> np.ndarray:
    """Convert mask to color.

    Args:
        image (np.ndarray): image
        leaf_mask (np.ndarray): mask
        binary (bool): make image binary or cut leaf from the image

    Returns:
        np.ndarray: color mask
    """
    if binary:
        color_mask = np.zeros_like(image)
        assert color_mask.dtype == np.uint8
        color_mask[leaf_mask] = 255

    else:
        color_mask = np.zeros_like(image)
        color_mask[leaf_mask] = image[leaf_mask]
    return color_mask
