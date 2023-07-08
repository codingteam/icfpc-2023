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
        sum = np.sum(inv_distances,axis=1) + 1.0
        qualities[real_inds] = sum
    return qualities


def score(mus_instruments: np.ndarray,
          mus_places: np.ndarray,
          att_places: np.ndarray,
          att_tastes: np.ndarray,
          pillar_center_radius: np.ndarray,
          use_playing_together_ext: bool = True) -> float:
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

    pass
