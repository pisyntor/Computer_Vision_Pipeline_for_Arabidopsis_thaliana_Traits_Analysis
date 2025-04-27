"""Testing the tracking module."""

import numpy as np
from model.tracking import (
    DemoObjectTracker,
    SimpleTracker,
    change_mask_resolution,
    ensure_same_image_sizes,
)


def test_change_resolution():
    """Test the change_resolution function."""
    mask = np.array([[1, 0], [0, 1]], dtype=bool)
    new_size = (4, 4)
    new_mask = change_mask_resolution(mask, new_size)
    assert new_mask.shape == new_size
    assert new_mask[0, 0] == 1
    assert new_mask[0, 1] == 1
    assert new_mask[1, 0] == 1
    assert new_mask[1, 1] == 1
    assert new_mask.dtype == bool

    mask = np.array([[1, 0], [0, 1]], dtype=bool)
    new_size = (2, 4)
    new_mask = change_mask_resolution(mask, new_size)
    assert new_mask.shape == new_size
    assert np.all(new_mask[0, :2] == 1)
    assert np.all(new_mask[1, :2] == 0)
    assert np.all(new_mask[0, 2:] == 0)
    assert np.all(new_mask[1, 2:] == 1)
    assert new_mask.dtype == bool


def test_ensure_same_image_sizes():
    """Test the ensure_same_image_sizes function."""
    images = [
        np.ones((2, 2, 3), dtype=np.uint8),
        np.ones((2, 2, 3), dtype=np.uint8),
    ]
    new_images = ensure_same_image_sizes(images, "for test")
    assert images == new_images  # nothing should change
    # assert len(new_images) == 2
    # assert all(img.shape == (2, 2, 3) for img in new_images)
    # assert all(img.dtype == np.uint8 for img in new_images)

    images = [
        np.ones((2, 2, 3), dtype=np.uint8),
        np.ones((2, 3, 3), dtype=np.uint8),
        np.ones((2, 3, 3), dtype=np.uint8),
    ]
    new_images = ensure_same_image_sizes(images, "for test")
    assert len(new_images) == 3
    assert all(img.shape == (2, 3, 3) for img in new_images)
    assert all(img.dtype == np.uint8 for img in new_images)


class TestDemoObjectTracker:
    """Test the DemoObjectTracker class."""

    def test_init(self):
        """Test the __init__ method."""
        tracker = DemoObjectTracker()
        assert tracker.prev_masks == []

    def test_new_tracking_list(self):
        """Test the new_tracking_list method."""
        tracker = DemoObjectTracker()
        prev_masks = [
            {"segmentation": np.array([[1, 0], [0, 1]], dtype=bool)},
            {"segmentation": np.array([[1, 1], [0, 1]], dtype=bool)},
        ]
        next_masks = [
            {"segmentation": np.array([[1, 0], [0, 1]], dtype=bool)},
            {"segmentation": np.array([[1, 1], [0, 1]], dtype=bool)},
            {"segmentation": np.array([[1, 0], [0, 1]], dtype=bool)},
        ]
        new_masks = tracker.new_tracking_list(prev_masks, next_masks)
        assert len(new_masks) == 3
        assert all("segmentation" in m for m in new_masks)
        assert all(m["segmentation"].shape == (2, 2) for m in new_masks)
        assert all(m["segmentation"].dtype == bool for m in new_masks)

    def test_update_on_new_masks(self):
        """Test the update_on_new_masks method."""
        tracker = DemoObjectTracker()
        new_masks = [
            {"segmentation": np.array([[1, 0], [0, 1]], dtype=bool)},
            {"segmentation": np.array([[1, 1], [0, 1]], dtype=bool)},
        ]
        updated_masks = tracker.update_on_new_masks(new_masks)
        assert len(updated_masks) == 2
        assert all("segmentation" in m for m in updated_masks)
        assert all(m["segmentation"].shape == (2, 2) for m in updated_masks)
        assert all(m["segmentation"].dtype == bool for m in updated_masks)


def _to_sam(masks: dict[int, np.ndarray]) -> list[dict]:
    return [{"segmentation": m} for m in masks.values()]


class TestSimpleTracker:
    """Test the SimpleTracker class."""

    def test_init(self):
        """Test the __init__ method."""
        tracker = SimpleTracker()
        assert tracker.prev_shape is None
        assert tracker.prev_masks == []
        assert tracker.id_stat == {}

    def test_reset(self):
        """Test the reset method."""
        tracker = SimpleTracker()
        tracker.prev_shape = (2, 2)
        tracker.prev_masks = [{0: np.array([[1, 0], [0, 1]], dtype=bool)}]
        tracker.id_stat = {0: 1}
        tracker.reset()
        assert tracker.prev_shape is None
        assert tracker.prev_masks == []
        assert tracker.id_stat == {}

    def test_update_on_new_masks(self):
        """Test the update_on_new_masks method."""

        # Test added sorted
        def _assert_same(updated_masks, new_masks):
            assert len(updated_masks) == len(new_masks)
            for k in updated_masks:
                assert np.all(
                    updated_masks[k]["segmentation"] == new_masks[str(k)]
                )

        tracker = SimpleTracker()
        new_masks = {
            "1": np.array([[1, 0, 0, 0, 0], [1, 0, 0, 0, 0]], dtype=bool),
            "0": np.array([[1, 1, 0, 0, 0], [1, 0, 0, 0, 0]], dtype=bool),
        }
        updated_masks = tracker.update_on_new_masks(_to_sam(new_masks))
        _assert_same(updated_masks, new_masks)

        # Test reasonably tracked: 100% overlap for 2, some for 99
        new_masks = {
            "1": np.array([[1, 0, 0, 0, 0], [1, 0, 0, 0, 0]], dtype=bool),
            "2": np.array([[0, 1, 1, 0, 0], [1, 0, 0, 0, 0]], dtype=bool),
            "0": np.array([[1, 1, 1, 0, 0], [1, 0, 0, 0, 0]], dtype=bool),
        }
        updated_masks = tracker.update_on_new_masks(_to_sam(new_masks))
        _assert_same(updated_masks, new_masks)

        # Test zero iou masks are not added
        new_masks = {
            "1": np.array([[1, 0, 0, 0, 0], [1, 0, 0, 0, 0]], dtype=bool),
            "3": np.array([[0, 0, 0, 1, 0], [0, 0, 0, 1, 0]], dtype=bool),
            "2": np.array([[0, 1, 1, 0, 0], [1, 0, 0, 0, 0]], dtype=bool),
        }
        updated_masks = tracker.update_on_new_masks(_to_sam(new_masks))
        _assert_same(updated_masks, new_masks)

    def test_resize(self):
        """Test the _ensure_same_size method."""

        def _assert_mask_sizes(result, size):
            for m in result.values():
                assert m["segmentation"].shape == size

        # add some data
        tracker = SimpleTracker()
        new_masks = {
            "1": np.array([[1, 0, 0, 0, 0], [1, 0, 0, 0, 0]], dtype=bool),
            "0": np.array([[1, 1, 0, 0, 0], [1, 0, 0, 0, 0]], dtype=bool),
        }
        res1 = tracker.update_on_new_masks(_to_sam(new_masks))
        _assert_mask_sizes(res1, (2, 5))
        assert tracker.prev_shape == (2, 5)

        # add new data with different size 2, 3
        new_masks = {
            "1": np.array([[0, 0, 1], [0, 0, 1]], dtype=bool),
            "0": np.array([[0, 0, 1], [0, 1, 1]], dtype=bool),
        }
        res2 = tracker.update_on_new_masks(_to_sam(new_masks))
        _assert_mask_sizes(res2, (2, 3))
        assert tracker.prev_shape == (2, 3)

        # add new data with different size 3, 2
        new_masks = {
            "1": np.array([[1, 0], [1, 0], [1, 0]], dtype=bool),
            "0": np.array([[1, 1], [1, 0], [1, 0]], dtype=bool),
        }
        res3 = tracker.update_on_new_masks(_to_sam(new_masks))
        _assert_mask_sizes(res3, (3, 2))
        assert tracker.prev_shape == (3, 2)

        # Test not changed afterwards
        _assert_mask_sizes(res1, (2, 5))
        _assert_mask_sizes(res2, (2, 3))
        _assert_mask_sizes(res3, (3, 2))
