"""Testing for the metrics module."""

import numpy as np

from metrics import (
    AccumulatorMOTA,
    MultiObjectTrackingAccuracy,
    MultiObjectTrackingPrecision,
)


def _to_list_of_dict(masks_list):
    return [dict(enumerate(masks)) for masks in masks_list]


def build_masks():
    """Build masks for testing."""

    def _build_rect_mask(x1, y1, x2, y2):
        mask = np.zeros((10, 10), dtype=bool)
        mask[y1:y2, x1:x2] = True
        return mask

    gt_masks = [
        {"segmentation": _build_rect_mask(0, 0, 5, 5), "label": "0"},
        {"segmentation": _build_rect_mask(5, 5, 10, 10), "label": "1"},
    ]
    gt_masks_sequence = [gt_masks, gt_masks[:1], gt_masks]

    pred_masks = [
        {"segmentation": _build_rect_mask(0, 0, 6, 6), "label": "almost 0"},
        {"segmentation": _build_rect_mask(0, 0, 5, 5), "label": "0"},
        {"segmentation": _build_rect_mask(0, 5, 5, 10), "label": "wrong"},
        {"segmentation": _build_rect_mask(5, 5, 10, 10), "label": "1"},
    ]
    pred_masks_sequence = [pred_masks, pred_masks[::-1], pred_masks[:1]]

    return gt_masks_sequence, pred_masks_sequence


class TestMultiOjbectTrackingAccuracy:
    """Test the MultiObjectTrackingAccuracy class."""

    def test(self):
        """test the MultiObjectTrackingAccuracy class."""
        mota = MultiObjectTrackingAccuracy(
            compatible_mode=True, overlap_threshold=0.5
        )
        gt_masks, pred_masks = build_masks()

        mota.add_sequence(
            _to_list_of_dict(gt_masks), _to_list_of_dict(pred_masks), name="1"
        )
        result = mota.get_aggregate_metrics()[
            "MultiObjectTrackingAccuracy-deprecated"
        ][0]
        mota.add_sequence(
            _to_list_of_dict(gt_masks), _to_list_of_dict(pred_masks), name="2"
        )
        result2 = mota.get_aggregate_metrics()[
            "MultiObjectTrackingAccuracy-deprecated"
        ][0]
        assert result == result2

        stats = [
            AccumulatorMOTA(fp=2, fn=0, id_s=0, gt_c=2),
            AccumulatorMOTA(fp=3, fn=0, id_s=1, gt_c=1),
            AccumulatorMOTA(fp=0, fn=1, id_s=1, gt_c=2),
        ]
        assert mota.seq_stats == {
            "1": stats,
            "2": stats,
        }

        mota.reset()
        mota.add_sequence(
            _to_list_of_dict(gt_masks), _to_list_of_dict(gt_masks), name="3"
        )
        assert (
            mota.get_aggregate_metrics()[
                "MultiObjectTrackingAccuracy-deprecated"
            ][0]
            == 1.0
        )

        mota = MultiObjectTrackingAccuracy(
            compatible_mode=True, overlap_threshold=0.99
        )
        mota.add_sequence(
            _to_list_of_dict(gt_masks), _to_list_of_dict(pred_masks), name="4"
        )
        result3 = mota.get_aggregate_metrics()[
            "MultiObjectTrackingAccuracy-deprecated"
        ][0]
        assert result3 < result2

    def test_empty(self):
        """Test the case when gt_masks and pred_masks are empty."""
        mota = MultiObjectTrackingAccuracy(
            compatible_mode=True, overlap_threshold=0.5
        )
        mota.add_sequence(_to_list_of_dict([]), _to_list_of_dict([]))
        assert np.isnan(
            mota.get_aggregate_metrics()[
                "MultiObjectTrackingAccuracy-deprecated"
            ][0]
        )


class TestMultiObjectTrackingPrecision:
    """Test the MultiObjectTrackingPrecision class."""

    def test(self):
        """test the MultiObjectTrackingPrecision class."""
        motp = MultiObjectTrackingPrecision(overlap_threshold=0.5)
        gt_masks, pred_masks = build_masks()

        motp.add_sequence(
            _to_list_of_dict(gt_masks), _to_list_of_dict(pred_masks), name="1"
        )
        result = motp.get_aggregate_metrics()["MultiObjectTrackingPrecision"][0]
        motp.add_sequence(
            _to_list_of_dict(gt_masks), _to_list_of_dict(pred_masks), name="2"
        )
        result2 = motp.get_aggregate_metrics()["MultiObjectTrackingPrecision"][
            0
        ]
        assert result == result2
        assert result < 1

        motp.reset()
        motp.add_sequence(
            _to_list_of_dict(gt_masks), _to_list_of_dict(gt_masks), name="3"
        )
        assert (
            motp.get_aggregate_metrics()["MultiObjectTrackingPrecision"][0]
            == 1.0
        )

        motp = MultiObjectTrackingPrecision(overlap_threshold=0.99)
        motp.add_sequence(
            _to_list_of_dict(gt_masks), _to_list_of_dict(pred_masks), name="4"
        )
        result3 = motp.get_aggregate_metrics()["MultiObjectTrackingPrecision"][
            0
        ]
        # assert result3 < result2
        # removing inaccurate detection with overlap_threshold=0.99
        # increases the precision (but with decreased accuracy)
        assert result3 == 1.0
        assert result3 > result2

    def test_empty(self):
        """Test the case when gt_masks and pred_masks are empty."""
        motp = MultiObjectTrackingPrecision(overlap_threshold=0.5)
        motp.add_sequence(_to_list_of_dict([]), _to_list_of_dict([]))
        assert np.isnan(
            motp.get_aggregate_metrics()["MultiObjectTrackingPrecision"][0]
        )