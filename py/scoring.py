import math

import numpy as np


# from numba import *

# @jit()
def musician_qualities(mus_instruments: np.ndarray,
                       mus_places: np.ndarray) -> np.ndarray:
    n_mus, = mus_instruments.shape
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


def score(mus_instruments: np.ndarray,
          mus_places: np.ndarray,
          att_places: np.ndarray,
          att_tastes: np.ndarray,
          pillar_center_radius: np.ndarray,
          use_playing_together_ext: bool) -> float:
    n_mus, = mus_instruments.shape
    n_att, = att_places.shape
    n_pil, = pillar_center_radius.shape

    att_mus_distances_sq = np.ndarray(shape=(n_mus, n_att), dtype='float64')
    for i_att in range(n_att):
        for i_mus in range(n_mus):
            diff = mus_places[i_mus] - att_places[i_att]
            d2 = diff[0] ** 2 + diff[1] ** 2
            att_mus_distances_sq[i_att, i_mus] = d2
    if use_playing_together_ext:
        together_qualities = musician_qualities(mus_instruments, mus_places)
    else:
        together_qualities = np.ones(shape=n_mus, dtype='float64')

    att_mus_inhibited = np.zeros((n_att, n_mus), dtype='bool')
    # TODO: fill inhibition matrix
    # TODO: fill pillar inhibition matrix

    score_sum = 0.0
    for i_att in range(n_att):
        for i_mus in range(n_mus):
            if att_mus_inhibited[i_att, i_mus]:
                continue
            diff = mus_places[i_mus] - att_places[i_att]
            d2 = diff[0] * diff[0] + diff[1] * diff[1]
            taste = att_tastes[i_att, mus_instruments[i_mus]] * together_qualities[i_mus]
            s = math.ceil(1_000_000 * taste / d2)
            score_sum += s
    return score_sum


def mc_score(mus_instruments: np.ndarray,
             mus_places: np.ndarray,
             att_places: np.ndarray,
             att_tastes: np.ndarray,
             pillar_center_radius: np.ndarray,
             use_playing_together_ext: bool,
             mus_ratio: float,
             att_ratio: float,
             pillar_ratio: float,
             n_eval: int) -> float:
    def random_inds(arr, ratio):
        size, = arr.shape
        return np.random.choice(range(size), math.ceil(size * ratio), False)

    score_sum = 0.0
    for _ in range(n_eval):
        mus_inds = random_inds(mus_instruments, mus_ratio)
        att_inds = random_inds(att_places, att_ratio)
        pillar_inds = random_inds(pillar_center_radius, pillar_ratio)
        sc = score(mus_instruments[mus_inds],
                   mus_places[mus_inds],
                   att_places[att_inds],
                   att_tastes[att_inds],
                   pillar_center_radius[pillar_inds],
                   use_playing_together_ext)
        score_sum += sc

    return score_sum / n_eval / mus_ratio / att_ratio
