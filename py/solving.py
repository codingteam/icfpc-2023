from dataclasses import dataclass

import numpy as np

from scoring import *

from domain import *


def initial_solution(scene: Stage, n_musicians, sigmas) -> np.ndarray:
    center = [scene.x + 0.5 * scene.w, scene.y + 0.5 * scene.h, 10.0]
    r = np.random.normal(center, sigmas, (n_musicians, 3))
    r[r[:, 2] > 10, 2] = 10
    r[r[:, 2] < 0, 2] = 0
    return r


def ES_solve(scene: Stage,
             mus_instruments: np.ndarray,
             att_places: np.ndarray,
             att_tastes: np.ndarray,
             pillar_center_radius: np.ndarray,
             use_playing_together_ext: bool = True) -> (np.ndarray, float):
    initial_sigmas = np.array([scene.w / 3, scene.h / 3, 5 / 3], dtype='float64')
    step_count = 100
    population_size = 20
    score_k = 1
    mus_dist_penalty_k = -100
    mus_scene_penalty_k = -100

    n_musicians = mus_instruments.shape[0]

    def call_score(mus_places_volumes: np.ndarray) -> float:
        sc = mc_score(mus_instruments, mus_places_volumes, att_places, att_tastes, pillar_center_radius,
                      use_playing_together_ext,
                      mus_ratio=0.1,
                      att_ratio=0.1,
                      pillar_ratio=0.1,
                      n_eval=50)
        mus_dist_penalty = musicians_distance_penalty(mus_places_volumes)
        mus_scene_penalty = musicians_out_of_scene_penalty(scene, mus_places_volumes)
        return sc * score_k + mus_scene_penalty * mus_scene_penalty_k + mus_dist_penalty * mus_dist_penalty_k

    def gen_population(parent, sigmas) -> [np.ndarray]:
        def gen_one():
            r = np.random.normal(0, sigmas, (n_musicians, 3)) + parent
            r[r[:, 2] > 10, 2] = 10
            r[r[:, 2] < 0, 2] = 0
            return r

        return [gen_one() for _ in range(population_size)]

    current_solution = initial_solution(scene, n_musicians, [0.001, 0.001, 0.1])
    current_score = call_score(current_solution)
    for step in range(step_count):
        sigmas = initial_sigmas - initial_sigmas * step / step_count

        population = gen_population(current_solution, sigmas)
        # TODO: make solutions valid / filter invalid

        for solution in population:
            sol_score = call_score(solution)
            if sol_score > current_score:
                current_solution = solution
                current_score = sol_score

    real_score = score(mus_instruments, current_solution, att_places, att_tastes, pillar_center_radius,
                       use_playing_together_ext)
    return current_solution, real_score
