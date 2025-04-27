"""Metrics code for evaluating the performance of the model."""

from collections import namedtuple
from typing import Any

import numpy as np
import pandas as pd

from mapping import build_correspondence


def dict_to_sorted_list(objects: dict) -> list:
    """Convert dict to list sorted by keys. Useful for masks formats.

    Args:
        objects (dict): dictionary to convert

    Returns:
        list: list of objects sorted by keys
    """
    return [objects[key] for key in sorted(objects.keys())]


def _masks_to_simple_list(masks: dict[int, dict]) -> list[np.ndarray]:
    """Convert masks to a list of np.ndarrays.

    Args:
        masks (dict[int, dict]): masks in SAM2 format

    Returns:
        list[np.ndarray]: list of masks
    """
    return [m["segmentation"] for m in dict_to_sorted_list(masks)]


class AbstractMetric:
    """Abstract class for metrics"""

    def __init__(self, *args: Any, **kwargs: Any):
        """Initialize the metric with the configuration.

        Args:
            config (dict): Configuration dictionary.
        """
        self.args = args
        self.kwargs = kwargs

    def add_sequence(
        self,
        masks_reference: list[dict[int, dict]],
        masks_predicted: list[dict[int, dict]],
        name: str = "",
    ):
        """Update on new masks set.

        Args:
            masks_reference (list[dict[int, dict]]): reference masks
            masks_predicted (list[dict[int, dict]]): predicted masks
            name (str): name of the sequence
        """
        raise NotImplementedError

    def get_aggregate_metrics(self, per_seq: bool = False) -> pd.DataFrame:
        """Get the aggregate metrics value.

        Args:
            per_seq (bool): whether to return per-sequence metrics or overall

        Returns:
            pd.DataFrame: aggregate metrics
        """
        raise NotImplementedError

    def get_name(self):
        """Get the name of the metric.

        Returns:
            str: metric name
        """
        return self.__class__.__name__

    def reset(self) -> None:
        """Reset the metric for a new dataset."""
        raise NotImplementedError()


class SimpleMetric(AbstractMetric):
    """Simple metric that calculates the mean of the sequences values.

    reimplement _aggregate and _calc_sequence to implement the metric.

    """

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.seq_stats = {}

    def _calc_sequence_statistics(
        self,
        masks_reference: list[dict[int, dict]],
        masks_predicted: list[dict[int, dict]],
        name: str = "",
    ) -> Any:
        """Calculate the metric data for a single sequence.

        Args:
            masks_reference (list[dict[int, dict]]): reference masks
            masks_predicted (list[dict[int, dict]]): predicted masks
            name (str): name of the sequence

        Returns:
            Any: some data for the sequence for aggregation later

        """
        raise NotImplementedError()

    def _calc_aggregate_metrics(self, results_list: list) -> dict:
        """Function to aggregate the values of the sequences.

        Args:
            results_list (list): list of the sequence data

        Returns:
            dict: aggregated metric values
        """
        raise NotImplementedError()

    def add_sequence(
        self,
        masks_reference: list[dict[int, dict]],
        masks_predicted: list[dict[int, dict]],
        name: str = "",
    ):
        """Update on new masks set.

        Args:
            masks_reference (list[dict[int, dict]]): reference masks
            masks_predicted (list[dict[int, dict]]): predicted masks
            name (str): name of the sequence

        """
        assert (
            self.seq_stats.get(name) is None
        ), "Sequence {name} already added, please specify another one"
        self.seq_stats[name] = self._calc_sequence_statistics(
            masks_reference, masks_predicted, name
        )

    def get_aggregate_metrics(self, per_seq: bool = False) -> pd.DataFrame:
        """Get the aggregate metrics value.

        Args:
            per_seq (bool): whether to return per-sequence metrics or overall

        Returns:
            pd.DataFrame: aggregate metrics
        """
        if not per_seq:
            all_values = list(self.seq_stats.values())
            res = pd.DataFrame([self._calc_aggregate_metrics(all_values)])

        else:
            metrics_by_seq = []
            for name, value in self.seq_stats.items():
                metrics = self._calc_aggregate_metrics([value])
                metrics["rep"] = name
                metrics_by_seq.append(metrics)

            res = pd.DataFrame(metrics_by_seq).set_index("rep")

        return res

    def reset(self):
        self.seq_stats = {}


class FrameBasedIOU(SimpleMetric):
    """Frame-based Intersection over Union metric.
    1. Find IOU-best correspondence between reference and predicted masks.
        (independently one for each mask_ref)
    2. Calculate share of correctly correspondent pixels.
    """

    @staticmethod
    def _calc_one_frame(
        masks_reference_dicts: dict[int, dict],
        masks_predicted_dicts: dict[int, dict],
    ) -> float:
        """Add a mask to the metric.

        Args:
            masks_reference_dicts (dict[int, dict]): Reference masks.
            masks_predicted_dicts (dict[int, dict]): Predicted masks.
        """
        masks_reference = _masks_to_simple_list(masks_reference_dicts)
        masks_predicted = _masks_to_simple_list(masks_predicted_dicts)

        if len(masks_reference) == 0 or len(masks_predicted) == 0:
            return 0

        inds, _ = build_correspondence(masks_reference, masks_predicted)

        corresponded_pixels = 0
        for mask_ref, ind in zip(masks_reference, inds):
            assert ind is not None, "Error, no masks found"
            corresponded_pixels += np.logical_and(
                mask_ref, masks_predicted[ind]
            ).sum()

        all_pixels_mask = np.zeros_like(masks_reference[0])
        for m in list(masks_reference) + list(masks_predicted):
            all_pixels_mask = np.logical_or(all_pixels_mask, m)

        return corresponded_pixels / all_pixels_mask.sum()

    def _calc_sequence_statistics(
        self, masks_reference: list, masks_predicted: list, name: str = ""
    ) -> list[float]:
        """Calculate IOUs for all frames in the sequence.

        Args:
            masks_reference (list): Reference masks for one image.
            masks_predicted (list): Predicted masks for one image.
            name (str): name of the sequence.
        """
        values = []
        for masks_ref, masks_pred in zip(masks_reference, masks_predicted):
            values.append(self._calc_one_frame(masks_ref, masks_pred))

        return values

    def _calc_aggregate_metrics(self, results_list: list[list[float]]) -> dict:
        """Get the aggregate metric value.

        Returns:
            float: metric value
        """
        flatten_list = [x for sublist in results_list for x in sublist]
        if len(flatten_list) == 0:
            return {"FrameBasedIOU": np.nan}
        return {"FrameBasedIOU": np.mean(flatten_list)}


class MultiObjectTrackingPrecision(SimpleMetric):
    """Multi-object tracking precision metric."""

    def __init__(self, overlap_threshold: float = 0.5):
        super().__init__(overlap_threshold=overlap_threshold)
        self.overlap_threshold = overlap_threshold

    def _calc_one_frame(
        self,
        masks_reference_dicts: dict[int, dict],
        masks_predicted_dicts: dict[int, dict],
    ) -> list[float]:
        """Add a mask to the metric.

        Args:
            masks_reference_dicts (dict): Reference masks for one image.
            masks_predicted_dicts (dict): Predicted masks for one image.

        return ious_list
        """
        masks_reference = _masks_to_simple_list(masks_reference_dicts)
        masks_predicted = _masks_to_simple_list(masks_predicted_dicts)

        if len(masks_reference) == 0 or len(masks_predicted) == 0:
            return []

        _inds, ious = build_correspondence(masks_reference, masks_predicted)
        big_ious = [x for x in ious if x > self.overlap_threshold]
        return big_ious

    def _calc_sequence_statistics(
        self, masks_reference: list, masks_predicted: list, name: str = ""
    ) -> list[list[float]]:
        """Calculate IOUs for all frames in the sequence.

        Args:
            masks_reference (list): Reference masks for one image.
            masks_predicted (list): Predicted masks for one image.
            name (str): name of the sequence.
        """
        frames_ious = []
        for masks_ref, masks_pred in zip(masks_reference, masks_predicted):
            frames_ious.append(self._calc_one_frame(masks_ref, masks_pred))

        return frames_ious

    def _calc_aggregate_metrics(self, results_list) -> dict:
        """Get the aggregate metric value.
        Args:
            results_list (list[list[list[float]]]): list of the sequence data

        Returns:
            float: metric value
        """
        expanded = []
        for r in results_list:
            for f in r:
                for i in f:
                    expanded.append(i)

        return {"MultiObjectTrackingPrecision": np.mean(expanded)}


AccumulatorMOTA = namedtuple("AccumulatorMOTA", ["fp", "fn", "id_s", "gt_c"])


class MultiObjectTrackingAccuracy(SimpleMetric):
    """Multi-object tracking accuracy metric."""

    def __init__(
        self, compatible_mode: bool = False, overlap_threshold: float = 0.5
    ):
        super().__init__(overlap_threshold=overlap_threshold)
        self.overlap_threshold = overlap_threshold
        self.compatible_mode = compatible_mode

    def _calc_sequence_statistics(
        self, masks_reference: list, masks_predicted: list, name: str = ""
    ) -> list[AccumulatorMOTA]:
        """Calculate IOUs for all frames in the sequence.

        Args:
            masks_reference (list): Reference masks for one image.
            masks_predicted (list): Predicted masks for one image.
            name (str): name of the sequence.

        Returns:
        """
        accs = []
        previous_id_map: dict[int, int] = {}
        for masks_ref, masks_pred in zip(masks_reference, masks_predicted):
            acc, previous_id_map = self._calc_one_frame(
                masks_ref, masks_pred, previous_id_map
            )
            accs.append(acc)

        return accs

    def _calc_aggregate_metrics(
        self, results_list: list[list[AccumulatorMOTA]]
    ) -> dict:
        """Get the aggregate metric value.

        Args:
            results_list (list[list[AccumulatorMOTA]]): lists of accs

        Returns:
            dict: aggregated metric values
        """

        flat_list = []
        for r in results_list:
            for a in r:
                flat_list.append(a)

        if len(flat_list) == 0:
            sum_stat = AccumulatorMOTA(np.nan, np.nan, np.nan, np.nan)
        else:
            sum_stat = AccumulatorMOTA(*np.array(flat_list).sum(axis=0))

        metrics = {
            "FalsePositives": sum_stat.fp,
            "FalseNegatives": sum_stat.fn,
            "IDSwitches": sum_stat.id_s,
            "GroundTruthMasksCount": sum_stat.gt_c,
            "MultiObjectTrackingAccuracy": 1
            - (sum_stat.fp + sum_stat.fn + sum_stat.id_s) / sum_stat.gt_c,
        }

        if self.compatible_mode:
            metrics = {k + "-deprecated": v for k, v in metrics.items()}
            # metrics = {
            #   "MultiObjectTrackingAccuracy-deprecated":
            #   metrics["MultiObjectTrackingAccuracy"]
            # }

        return metrics

    @staticmethod
    def _calc_id_switches(previous_id_map: dict, id_map: dict):
        """Given two id maps, calculate the number of id switches.

        Args:
            previous_id_map (dict): previous id map (ref_id -> predicted_id)
            id_map (dict): current id map (ref_id -> predicted_id)

        Returns:
            int: number of id switches
        """
        id_switches = 0

        for k in set(previous_id_map.keys()) & set(id_map.keys()):
            i1 = previous_id_map[k]
            i2 = id_map[k]
            if i1 is not None and i2 is not None and i1 != i2:
                id_switches += 1

        return id_switches

    def _calc_one_frame(
        self,
        masks_reference_dicts: dict[int, dict],
        masks_predicted_dicts: dict[int, dict],
        previous_id_map: dict[int, int],
    ) -> tuple[AccumulatorMOTA, dict[int, int]]:
        """Add a mask to the metric.

        Args:
            masks_reference_dicts (list[dict]): Reference masks for one image.
            masks_predicted_dicts (list[dict]): Predicted masks for one image.
            previous_id_map (dict): previous id map (ref_id -> predicted_id)

        Returns:
            tuple: MOTA accumulator and id map
            id_map (dict): current id map (ref_id -> predicted_id)
        """

        def _to_list_dicts(masks_dict, compat_mode):
            keys = sorted(masks_dict.keys())
            res = []
            for i, k in enumerate(keys):
                res.append(
                    {
                        "segmentation": masks_dict[k]["segmentation"],
                        "key": i if compat_mode else k,
                    }
                )
            return res

        masks_reference = _to_list_dicts(
            masks_reference_dicts, compat_mode=self.compatible_mode
        )
        masks_predicted = _to_list_dicts(
            masks_predicted_dicts, compat_mode=self.compatible_mode
        )

        if len(masks_reference) == 0 or len(masks_predicted) == 0:
            return AccumulatorMOTA(0, 0, 0, 0), {}

        inds_unfiltered, ious = build_correspondence(
            masks_reference=[m["segmentation"] for m in masks_reference],
            masks_predicted=[m["segmentation"] for m in masks_predicted],
        )

        # inds_unfiltered[i] = ind in list of predicted_masks,
        #   corresponding to i'th reference mask
        # id_map[reference_mask_key] = predicted_mask_key
        id_map = {}
        correct_correspondences = 0
        for i, (ind, iou) in enumerate(zip(inds_unfiltered, ious)):
            if iou > self.overlap_threshold:
                new_ind = ind
                correct_correspondences += 1
            else:
                new_ind = None

            id_map[masks_reference[i]["key"]] = (
                masks_predicted[new_ind]["key"] if new_ind is not None else None
            )

        acc = AccumulatorMOTA(
            fp=len(masks_predicted) - correct_correspondences,
            fn=len(masks_reference) - correct_correspondences,
            id_s=self._calc_id_switches(previous_id_map, id_map),
            gt_c=len(masks_reference),
        )
        return acc, id_map

    def get_name(self):
        """Get the name of the metric.

        Returns:
            str: metric name
        """
        if self.compatible_mode:
            return "MOTA-deprecated"
        return self.__class__.__name__
