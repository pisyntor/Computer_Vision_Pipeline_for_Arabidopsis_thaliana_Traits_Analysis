"""Tests for the mapping module."""

import numpy as np

from mapping import build_correspondence, build_mask_mapping_greedy_compatible


def test_build_correspondence():
    """Test the build_correspondence function."""
    # Test case 1: Empty masks
    masks_reference = []
    masks_predicted = []
    inds, ious = build_correspondence(masks_reference, masks_predicted)
    assert inds == []
    assert ious == []

    # Test case 2: One-to-one correspondence
    masks_reference = [np.array([[1, 0], [0, 1]])]
    masks_predicted = [np.array([[1, 0], [0, 1]])]
    inds, ious = build_correspondence(masks_reference, masks_predicted)
    assert inds == [0]  # Single mask matches perfectly
    assert ious == [1.0]  # IoU should be 1.0 for perfect match

    # Test case 3: Mismatch case
    masks_reference = [np.array([[1, 0], [0, 1]])]
    masks_predicted = [np.array([[0, 1], [1, 0]])]
    inds, ious = build_correspondence(masks_reference, masks_predicted)
    assert inds == [0]  # Even though it's different, it's the only mask
    assert ious == [0.0]  # IoU for this case would be 0

    # Test case 4: Multiple masks
    masks_reference = [np.array([[1, 0], [0, 1]]), np.array([[0, 1], [1, 0]])]
    masks_predicted = [np.array([[1, 0], [0, 1]]), np.array([[0, 1], [1, 0]])]
    inds, ious = build_correspondence(masks_reference, masks_predicted)
    assert inds == [0, 1]  # Both should match their corresponding mask
    assert ious == [1.0, 1.0]  # Both IoUs should be perfect

    # Test case 5: Masks with partial overlap
    masks_reference = [np.array([[1, 1], [0, 0]])]
    masks_predicted = [np.array([[1, 0], [1, 0]]), np.array([[1, 0], [0, 0]])]
    inds, ious = build_correspondence(masks_reference, masks_predicted)
    assert inds == [1]  # Best matching mask should be the second one
    assert ious == [0.5]  # IoU would be 0.5 in this case

    # Test case 6: Problematic as not really correspondence
    # Many partial overlap, not-optimal matching as first
    # reference mask is matched first
    masks_reference = [
        np.array([[0, 0, 0], [0, 1, 1]]),
        np.array([[1, 1, 0], [1, 1, 0]]),
    ]
    masks_predicted = [
        np.array([[0, 0, 1], [1, 0, 0]]),
        np.array([[1, 1, 0], [1, 1, 0]]),
    ]
    inds, ious = build_correspondence(masks_reference, masks_predicted)
    assert inds == [1, 1]  # Both masks are mapped to the second mask
    assert ious == [0.2, 1.0]  # IoU values for the masks

    # Test case 7: Bigger reference
    masks_reference = [np.array([[1, 0], [0, 0]]), np.array([[0, 1], [0, 0]])]
    masks_predicted = [
        np.array([[0, 1], [0, 1]]),
    ]
    inds, ious = build_correspondence(masks_reference, masks_predicted)
    assert inds == [0, 0]  # First mask is matched to the second mask
    assert ious == [0.0, 0.5]  # IoU values for the masks


def test_build_mask_mapping_greedy_compatible():
    """Test the build_mask_mapping_greedy_compatible function."""
    # Test case 1: Empty masks
    masks_reference = []
    masks_predicted = []
    inds, ious = build_mask_mapping_greedy_compatible(
        masks_reference, masks_predicted
    )
    assert inds == []
    assert ious == []

    # Test case 2: One-to-one correspondence
    masks_reference = [np.array([[1, 0], [0, 1]])]
    masks_predicted = [np.array([[1, 0], [0, 1]])]
    inds, ious = build_mask_mapping_greedy_compatible(
        masks_reference, masks_predicted
    )
    assert inds == [0]  # Single mask matches perfectly
    assert ious == [1.0]  # IoU should be 1.0 for perfect match

    # Test case 3: Mismatch case
    masks_reference = [np.array([[1, 0], [0, 1]])]
    masks_predicted = [np.array([[0, 1], [1, 0]])]
    inds, ious = build_mask_mapping_greedy_compatible(
        masks_reference, masks_predicted
    )
    assert inds == [0]  # Even though it's different, it's the only mask
    assert ious == [0.0]  # IoU for this case would be 0

    # Test case 4: Multiple masks
    masks_reference = [np.array([[1, 0], [0, 1]]), np.array([[0, 1], [1, 0]])]
    masks_predicted = [np.array([[1, 0], [0, 1]]), np.array([[0, 1], [1, 0]])]
    inds, ious = build_mask_mapping_greedy_compatible(
        masks_reference, masks_predicted
    )
    assert inds == [0, 1]  # Both should match their corresponding mask
    assert ious == [1.0, 1.0]  # Both IoUs should be perfect

    # Test case 5: Masks with partial overlap
    masks_reference = [np.array([[1, 1], [0, 0]])]
    masks_predicted = [np.array([[1, 0], [1, 0]]), np.array([[1, 0], [0, 0]])]
    inds, ious = build_mask_mapping_greedy_compatible(
        masks_reference, masks_predicted
    )
    assert inds == [1]  # Best matching mask should be the second one
    assert ious == [0.5]  # IoU would be 0.5 in this case

    # Test case 6: Many partial overlap, not-optimal matching as first
    # reference mask is matched first
    masks_reference = [
        np.array([[0, 0, 0], [0, 1, 1]]),
        np.array([[1, 1, 0], [1, 1, 0]]),
    ]
    masks_predicted = [
        np.array([[0, 0, 1], [1, 0, 0]]),
        np.array([[1, 1, 0], [1, 1, 0]]),
    ]
    inds, ious = build_mask_mapping_greedy_compatible(
        masks_reference, masks_predicted
    )
    assert inds == [0, 1]  # First mask is matched to the first mask!
    assert ious == [0, 1]  # IoU values for the masks

    # Test case 7: Bigger reference
    masks_reference = [np.array([[1, 0], [0, 0]]), np.array([[0, 1], [0, 0]])]
    masks_predicted = [
        np.array([[0, 1], [0, 1]]),
    ]
    inds, ious = build_mask_mapping_greedy_compatible(
        masks_reference, masks_predicted
    )
    assert inds == [None, 0]  # First mask is matched to the second mask
    assert ious == [None, 0.5]  # IoU values for the masks