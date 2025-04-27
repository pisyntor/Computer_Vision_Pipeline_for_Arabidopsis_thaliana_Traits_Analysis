"""Building correspondence between masks on two images."""

import logging

import numpy as np


def build_correspondence(
    masks_reference: list[np.ndarray], masks_predicted: list[np.ndarray]
) -> tuple[list[int], list[float]]:
    """Calculate correspondence between masks.

    Args:
        masks_reference (list[np.ndarray]): reference masks
        masks_predicted (list[np.ndarray]): predicted masks

    Returns:
        list[int]: indices of the predicted masks
            that correspond to the reference masks
            i.e. len(inds) == len(masks_reference)
    """
    inds = []
    ious = []
    for mask_ref in masks_reference:
        max_iou = -1.0
        max_iou_ind = None

        for i, mask_pred in enumerate(masks_predicted):
            intersection = np.logical_and(mask_ref, mask_pred)
            union = np.logical_or(mask_ref, mask_pred)
            iou = np.sum(intersection) / np.sum(union)
            if iou > max_iou:
                max_iou = iou
                max_iou_ind = i

        assert max_iou_ind is not None, "Error, no predicted_masks found"
        inds.append(max_iou_ind)
        ious.append(max_iou)

    if len(set(inds)) != len(inds):
        logging.debug(f"Some masks are not unique, see {inds}")

    assert len(inds) == len(masks_reference)
    return inds, ious


def _build_mapping_greedy(
    keys_ref: list[int], keys_exp: list[int], similarity: np.ndarray
):
    """Build mapping between keys_ref and keys_exp using greedy algorithm.

    Args:
        keys_ref (list): list of keys from the first set
        keys_exp (list): list of keys from the second set
        similarity (np.ndarray): matrix of IoU values [ref, exp]

    Returns:
        dict: ref -> exp mapping
    """
    assert isinstance(keys_ref, list), "keys_ref should be a list"
    assert isinstance(keys_exp, list), "keys_exp should be a list"
    assert (
        similarity.min(initial=0) > -1
    ), "similarity should be a matrix of nonegative values"

    mapping = {}
    similarity = similarity.copy()
    for _i in range(min(len(keys_ref), len(keys_exp))):
        # pylint: disable=unbalanced-tuple-unpacking
        ref_i, exp_i = np.unravel_index(np.argmax(similarity), similarity.shape)
        mapping[keys_ref[ref_i]] = keys_exp[exp_i]
        similarity[ref_i, :] = -1
        similarity[:, exp_i] = -1

    return mapping


def _calc_iou(mask1: np.ndarray, mask2: np.ndarray) -> float:
    """Calculate IoU between two masks.

    Args:
        mask1 (np.ndarray): first mask
        mask2 (np.ndarray): second mask

    Returns:
        float: IoU value
    """
    intersection = np.logical_and(mask1, mask2)
    union = np.logical_or(mask1, mask2)
    iou = np.sum(intersection) / np.sum(union)
    return iou


def build_mask_mapping_greedy_lists(
    masks_reference: list[np.ndarray], masks_predicted: list[np.ndarray]
) -> dict[int, tuple[int, float]]:
    """Calculate correspondence between masks.

    Args:
        masks_reference (list[np.ndarray]): reference masks
        masks_predicted (list[np.ndarray]): predicted masks

    Returns:
        dict[int, (int, float)]: mapping reference_ind -> (predicted_ind, iou)
    """
    ious_mat = np.zeros((len(masks_reference), len(masks_predicted)))
    for i, mask_ref in enumerate(masks_reference):
        for j, mask_pred in enumerate(masks_predicted):
            ious_mat[i, j] = _calc_iou(mask_ref, mask_pred)

    mapping = _build_mapping_greedy(
        list(range(len(masks_reference))),
        list(range(len(masks_predicted))),
        ious_mat,
    )

    return {k: (mk, ious_mat[k, mk]) for k, mk in mapping.items()}


def build_mask_mapping_greedy_dicts(
    masks_reference: dict[int, np.ndarray],
    masks_predicted: dict[int, np.ndarray],
) -> dict[int, tuple[int, float]]:
    """Calculate correspondence between masks.

    Args:
        masks_reference (list[np.ndarray]): reference masks
        masks_predicted (list[np.ndarray]): predicted masks

    Returns:
        dict[int, (int, float)]: mapping reference_id -> (predicted_id, iou)
    """

    def _make_list(masks):
        keys = list(masks.keys())
        return keys, [masks[k] for k in keys]

    keys_ref, masks_ref_list = _make_list(masks_reference)
    keys_exp, masks_pred_list = _make_list(masks_predicted)
    mapping = build_mask_mapping_greedy_lists(masks_ref_list, masks_pred_list)

    return {
        keys_ref[kr]: (keys_exp[ke], iou) for kr, (ke, iou) in mapping.items()
    }


def build_mask_mapping_greedy_compatible(
    masks_reference: list[np.ndarray], masks_predicted: list[np.ndarray]
) -> tuple[list[int | None], list[float | None]]:
    """Calculate mapping between masks with format,
        compatible with the build_correspondence.

    Args:
        masks_reference (list[np.ndarray]): reference masks
        masks_predicted (list[np.ndarray]): predicted masks

    Returns:
        tuple[list[int], list[float]]: indices of the predicted masks
            that correspond to the reference masks
            i.e. len(inds) == len(masks_reference)
            and the corresponding IoU values
    """
    mapping = build_mask_mapping_greedy_lists(masks_reference, masks_predicted)
    inds: list[int | None] = []
    ious: list[float | None] = []
    for i in range(len(masks_reference)):
        if i in mapping:
            inds.append(mapping[i][0])
            ious.append(mapping[i][1])
        else:
            inds.append(None)
            ious.append(None)

    return inds, ious
