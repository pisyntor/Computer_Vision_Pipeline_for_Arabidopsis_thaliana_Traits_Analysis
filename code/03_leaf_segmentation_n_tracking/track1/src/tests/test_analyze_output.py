"""Tests for the analyze_output module."""

from unittest.mock import patch

import numpy as np
import pandas as pd

from analyze_output import add_mask_features


def test_add_mask_features():
    """Test the add_mask_features function."""
    df = pd.DataFrame(
        {
            "image_path": ["image_1", "image_1", "image_1", "image_2"],
            "image_id": [
                "image_id_1",
                "image_id_1",
                "image_id_1",
                "image_id_2",
            ],
            "mask_path": ["mask_1_1", "mask_1_2", "mask_1_3", "mask_2"],
        }
    )

    # 2d versions
    image_1 = np.arange(0, 121).reshape(11, 11)
    image_2 = np.arange(0, 121).reshape(11, 11) + 1

    mask_1_1, mask_1_2, mask_1_3, mask_2 = [
        np.zeros((11, 11), dtype=np.uint8) for _ in range(4)
    ]
    mask_1_1[4:7, 4:7] = 1
    mask_1_2[6:, 6:] = 1
    mask_1_3[:7, :7] = 1
    mask_2[5:6, 5:6] = 1

    def read_image_mask(path):
        name = path.split("/")[-1]
        image = {
            "image_1": image_1,
            "image_2": image_2,
            "mask_1_1": mask_1_1,
            "mask_1_2": mask_1_2,
            "mask_1_3": mask_1_3,
            "mask_2": mask_2,
        }[name]
        img_3d = np.stack([image, image, image], axis=-1)
        if "image" in name:
            # chroma
            img_3d[..., 1:] += 1

        return img_3d

    with patch("saveload._imread_func", side_effect=read_image_mask):
        with patch("analyze_output._imread_func", side_effect=read_image_mask):
            df = add_mask_features(df)

            assert df["area"].tolist() == [9, 25, 49, 1]
            assert (
                df["dist_to_center"] - np.array([0.0, 0.424, 0.282, 0.0]) < 1e-3
            ).all()
            assert (
                df["color_mask_bg"] - np.array([0.002, 0.002, 0.011, 0.0])
                < 1e-3
            ).all()
            assert (
                df["color_feature"] - np.array([-2.542, -2.552, -2.524, -2.542])
                < 1e-3
            ).all()
