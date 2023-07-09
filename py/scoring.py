import math

import numpy as np

from domain import Stage


# from numba import *

# @jit()
def musician_qualities(mus_instruments: np.ndarray,
                       mus_places: np.ndarray) -> np.ndarray:
    n_mus = mus_instruments.shape[0]
    instruments_set = {*mus_instruments}
    mus_instruments_with_index = np.append(np.reshape(mus_instruments, newshape=(n_mus, 1)),
                                           np.reshape(range(n_mus), newshape=(n_mus, 1)),
                                           axis=1)
    qualities = np.ones(shape=n_mus, dtype='float64')
    for instr in instruments_set:
        instr_inds = mus_instruments_with_index[mus_instruments_with_index[:, 0] == instr, :]
        real_inds = instr_inds[:, 1]
        places = mus_places[real_inds]
        n_places, _ = places.shape

        d = lambda i, j: np.linalg.norm(places[i] - places[j])
        inv_distances = np.zeros(shape=(n_places, n_places), dtype='float64')
        for i in range(n_places):
            for j in range(i + 1, n_places):
                dd = 1.0 / d(i, j)
                inv_distances[i, j] = dd
                inv_distances[j, i] = dd
        sum = np.sum(inv_distances, axis=1) + 1.0
        qualities[real_inds] = sum
    return qualities


def create_inhibition_matrix(mus_places: np.ndarray,
                             att_places: np.ndarray,
                             pillar_center_radius: np.ndarray) -> np.ndarray:
    n_mus = mus_places.shape[0]
    n_att = att_places.shape[0]
    n_pil = pillar_center_radius.shape[0]
    mus_radius = 5
    att_mus_inhibited = np.zeros((n_att, n_mus), dtype='bool')

    if pillar_center_radius.shape[0] > 0:
        block_coords = np.append(mus_places, pillar_center_radius[:, 0:2], axis=0)
        block_radiuses = np.append(np.ones((n_mus, 1)) * mus_radius, pillar_center_radius[:, 2], axis=0)
    else:
        block_coords = mus_places
        block_radiuses = np.ones((n_mus, 1)) * mus_radius

    n_block = n_mus + n_pil
    for i_att in range(n_att):
        att = att_places[i_att]
        for i_mus in range(n_mus):
            mus = att_places[i_mus]
            AM = mus - att
            lenAM = np.linalg.norm(AM)
            AM = AM / lenAM
            for i_block in range(n_block):
                block = block_coords[i_block]
                AB = block - att
                s = np.dot(AB, AM)
                if s >= lenAM or s <= 0:
                    continue
                d = np.linalg.norm(np.cross(AB, AM))
                block_radius = block_radiuses[i_block]
                if d < block_radius:
                    att_mus_inhibited[i_att, i_mus] = True
                    break
    return att_mus_inhibited


def score(mus_instruments: np.ndarray,
          mus_places_volumes: np.ndarray,
          att_places: np.ndarray,
          att_tastes: np.ndarray,
          pillar_center_radius: np.ndarray,
          use_playing_together_ext: bool) -> float:
    n_mus = mus_instruments.shape[0]
    n_att = att_places.shape[0]
    if mus_places_volumes.shape[1] == 3:
        mus_places = mus_places_volumes[:, 0:2]
        mus_volumes = mus_places_volumes[:, 2]
    else:
        mus_places = mus_places_volumes
        mus_volumes = np.ones(n_mus)

    if use_playing_together_ext:
        together_qualities = musician_qualities(mus_instruments, mus_places)
    else:
        together_qualities = np.ones(shape=n_mus, dtype='float64')

    att_mus_inhibited = create_inhibition_matrix(mus_places, att_places, pillar_center_radius)

    score_sum = 0.0
    for i_att in range(n_att):
        for i_mus in range(n_mus):
            if att_mus_inhibited[i_att, i_mus]:
                continue
            diff = mus_places[i_mus] - att_places[i_att]
            d2 = diff[0] * diff[0] + diff[1] * diff[1]
            taste = att_tastes[i_att, mus_instruments[i_mus]] * together_qualities[i_mus] * mus_volumes[i_mus]
            s = math.ceil(1_000_000 * taste / d2)
            score_sum += s
    return score_sum


def mc_score(mus_instruments: np.ndarray,
             mus_places_volumes: np.ndarray,
             att_places: np.ndarray,
             att_tastes: np.ndarray,
             pillar_center_radius: np.ndarray,
             use_playing_together_ext: bool,
             mus_ratio: float,
             att_ratio: float,
             pillar_ratio: float,
             n_eval: int) -> float:
    def random_inds(arr, ratio):
        size = arr.shape[0]
        return np.random.choice(range(size), math.ceil(size * ratio), False)

    score_sum = 0.0
    for _ in range(n_eval):
        mus_inds = random_inds(mus_instruments, mus_ratio)
        att_inds = random_inds(att_places, att_ratio)
        pillar_inds = random_inds(pillar_center_radius, pillar_ratio)
        sc = score(mus_instruments[mus_inds],
                   mus_places_volumes[mus_inds],
                   att_places[att_inds],
                   att_tastes[att_inds],
                   pillar_center_radius[pillar_inds] if pillar_inds else np.array([]),
                   use_playing_together_ext)
        score_sum += sc

    return score_sum / n_eval / mus_ratio / att_ratio


def musicians_out_of_scene_penalty(scene: Stage, mus_places_volumes: np.ndarray) -> float:
    radius = 10
    mus_places = mus_places_volumes[:, 0:2] if mus_places_volumes.shape[1] == 3 else mus_places_volumes
    left_bound = scene.x + radius
    bottom_bound = scene.y + radius
    right_bound = scene.x + scene.w - radius
    top_bound = scene.y + scene.h - radius
    left_bottom_diff = [left_bound, bottom_bound] - mus_places
    right_top_diff = mus_places - [right_bound, top_bound]
    return np.sum(left_bottom_diff, where=left_bottom_diff > 0) + np.sum(right_top_diff, where=right_top_diff > 0)


def musicians_distance_penalty(mus_places_volumes: np.ndarray) -> float:
    radius = 10
    mus_places = mus_places_volumes[:, 0:2] if mus_places_volumes.shape[1] == 3 else mus_places_volumes
    n_mus = mus_places.shape[0]
    penalty = 0.0
    for i in range(n_mus):
        for j in range(i + 1, n_mus):
            d = np.linalg.norm(mus_places[i] - mus_places[j])
            diff = radius - d
            if diff > 0:
                penalty += diff
    return penalty


def placing_valid(stage: Stage, mus_places_volumes: np.ndarray) -> bool:
    return musicians_out_of_scene_penalty(stage, mus_places_volumes) <= 0 and musicians_distance_penalty(
        mus_places_volumes) <= 0
