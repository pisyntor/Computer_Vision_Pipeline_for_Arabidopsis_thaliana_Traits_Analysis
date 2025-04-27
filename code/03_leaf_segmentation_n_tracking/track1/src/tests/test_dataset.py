"""Testing for the dataset module."""

import pathlib
import random
import tempfile

import pandas as pd

from dataset import (
    _assign_roles,
    _parse_ds_files,
    _split_dataset_rep_based,
    load_dataset,
    load_or_build_dataset,
    save_dataset,
    sort_strings_with_numbers,
)


def test__parse_ds_files():
    """Smoke test the _parse_ds_files function."""
    _parse_ds_files()


def test__assign_roles():
    """Test the _assign_roles function."""
    reps = ["rep1", "rep2", "rep3"]

    # test basic functionality
    roles_assigned = []
    for i in range(100):
        roles = _assign_roles(reps, random.Random(i), 0.1, 0.2)
        assert set(roles.values()) == {"test", "val", "train"}
        assert len(roles) == len(reps)
        roles_assigned.append(roles)

    for r in reps:
        assert set(roles[r] for roles in roles_assigned) == {
            "test",
            "val",
            "train",
        }

    # test reproducibility
    roles = _assign_roles(reps, random.Random(42), 0.1, 0.2)
    for i in range(10):
        roles2 = _assign_roles(reps, random.Random(42), 0.1, 0.2)
        assert roles == roles2


def test__split_dataset_rep_based():
    """Smoke test the _split_dataset_rep_based function."""
    df = pd.DataFrame(
        [
            {
                "plant": plant,
                "rep": rep,
                "nn_role": None,
            }
            for plant in ["plant1", "plant2"]
            for rep in ["rep1", "rep2", "rep3"]
        ]
    )

    df = _split_dataset_rep_based(df, 0.1, 0.2)
    assert set(df["nn_role"]) == {"test", "val", "train"}
    assert len(df) == 6


def test_load_dataset_save_dataset():
    """Test loading and saving are inverse operations."""
    with tempfile.TemporaryDirectory() as tmpdir:
        tmpdir = str(pathlib.Path(tmpdir).resolve())
        df = pd.DataFrame(
            [
                {
                    "plant": plant,
                    "rep": rep,
                    "nn_role": "train",
                    "image_path": f"{tmpdir}/subdir/image",
                    "mask_path": f"{tmpdir}/subdir/mask",
                }
                for plant in ["plant1", "plant2"]
                for rep in ["rep1", "rep2", "rep3"]
            ]
        )
        save_path = str(pathlib.Path(tmpdir) / "test.csv")

        save_dataset(df, save_path, images_root=tmpdir, masks_root=tmpdir)
        df_loaded = load_dataset(save_path, tmpdir, tmpdir)
        assert df.equals(df_loaded)
        df_loaded2 = load_or_build_dataset(save_path, tmpdir, tmpdir)
        assert df.equals(df_loaded2)

        df_loaded_manually = pd.read_csv(save_path)
        assert set(df_loaded_manually["image_path"]) == {"subdir/image"}
        assert set(df_loaded_manually["mask_path"]) == {"subdir/mask"}

        df_loaded_no_masks = load_or_build_dataset(save_path, tmpdir, None)
        assert set(df_loaded_no_masks["mask_path"]) == {""}
        assert df_loaded_no_masks["image_path"].equals(df_loaded2["image_path"])


def test_sort_strings_with_numbers_complex():
    """Test sorting with more complex strings"""
    input_list = [
        "aa5923",
        "abc23ave",
        "abc224cd",
        "abc234ave",
        "abc235a123",
        "abc235b12e",
        "abc235b111e",
    ]
    expected = [
        "aa5923",
        "abc23ave",
        "abc224cd",
        "abc234ave",
        "abc235a123",
        "abc235b12e",
        "abc235b111e",
    ]
    assert sort_strings_with_numbers(input_list) == expected


def test_sort_strings_edge_cases():
    """Test various edge cases"""
    # Test empty list sorting
    assert sort_strings_with_numbers([]) == []

    # Test strings without numbers
    input_no_numbers = ["abc", "def", "ghi"]
    expected_no_numbers = ["abc", "def", "ghi"]
    assert sort_strings_with_numbers(input_no_numbers) == expected_no_numbers

    # Test strings with same prefix but different numbers
    input_same_prefix = ["file9.txt", "file10.txt", "file1.txt"]
    expected_same_prefix = ["file1.txt", "file9.txt", "file10.txt"]
    assert sort_strings_with_numbers(input_same_prefix) == expected_same_prefix

    # Test strings with leading numbers
    input_leading_numbers = [
        "1file.txt",
        "10file.txt",
        "2file.txt",
        "0file.txt",
    ]
    expected_leading_numbers = [
        "0file.txt",
        "1file.txt",
        "2file.txt",
        "10file.txt",
    ]
    assert (
        sort_strings_with_numbers(input_leading_numbers)
        == expected_leading_numbers
    )

    # Test strings with trailing numbers
    input_trailing_numbers = [
        "file1.txt",
        "file10.txt",
        "file2.txt",
        "file0.txt",
    ]
    expected_trailing_numbers = [
        "file0.txt",
        "file1.txt",
        "file2.txt",
        "file10.txt",
    ]
    assert (
        sort_strings_with_numbers(input_trailing_numbers)
        == expected_trailing_numbers
    )

    # Test strings with zeros
    input_zeros = [
        "file01.txt",
        "file0001.txt",
        "file001.txt",
    ]
    expected_zeros = ["file0001.txt", "file001.txt", "file01.txt"]
    assert sort_strings_with_numbers(input_zeros) == expected_zeros
