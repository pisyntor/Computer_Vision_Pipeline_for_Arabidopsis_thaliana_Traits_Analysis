"""Tests for the saveload module."""

import tempfile

import numpy as np
import pandas as pd

from saveload import Loader, Saver


def test_saver_loader():
    """Test that Saver and Loader work correctly together."""
    with tempfile.TemporaryDirectory() as tmpdir:
        # Create test data
        image = np.zeros((100, 100, 3), dtype=np.uint8)
        ds_row = pd.Series(
            {
                "plant": "test_plant",
                "rep": "test_rep",
                "image_num": 0,
                "nn_role": "test",
                "image_path": "test.png",
                "mask_path": "mask.png",
            }
        )

        # Create test masks with gaps in indices
        masks = {
            0: {"segmentation": np.zeros((100, 100), dtype=bool)},
            2: {
                "segmentation": np.zeros((100, 100), dtype=bool)
            },  # Skip index 1
        }
        masks[0]["segmentation"][20:40, 20:40] = True
        masks[2]["segmentation"][60:80, 60:80] = True

        # Save masks
        saver = Saver(tmpdir, save_leafs=True)
        saver.save_masks(image, ds_row, masks)

        # Load masks
        loader = Loader(tmpdir)
        loaded_masks = loader.load_masks(ds_row)

        # Compare original and loaded masks
        assert set(loaded_masks.keys()) == set(masks.keys())
        for idx, masks_idx in masks.items():
            np.testing.assert_array_equal(
                masks_idx["segmentation"], loaded_masks[idx]["segmentation"]
            )
