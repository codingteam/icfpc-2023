import math
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


def ES_solve(stage: Stage,
             mus_instruments: np.ndarray,
             att_places: np.ndarray,
             att_tastes: np.ndarray,
             pillar_center_radius: np.ndarray,
             use_playing_together_ext: bool = True) -> (np.ndarray, float, bool):
    initial_sigmas = np.array([stage.w / 3 , stage.h / 3 , 5 / 3 ], dtype='float64')
    step_count = 100
    population_size = 20
    score_k = 1
    mus_dist_penalty_k = -1e9
    mus_scene_penalty_k = -1e9
    mutation_probability = 0.2

    n_musicians = mus_instruments.shape[0]

    def call_score(mus_places_volumes: np.ndarray) -> (float, bool, float, float):
        sc = mc_score(mus_instruments, mus_places_volumes, att_places, att_tastes, pillar_center_radius,
                      use_playing_together_ext,
                      mus_ratio=0.01,
                      att_ratio=0.01,
                      pillar_ratio=0.01,
                      n_eval=30)
        mus_dist_penalty = musicians_distance_penalty(mus_places_volumes)
        mus_scene_penalty = musicians_out_of_scene_penalty(stage, mus_places_volumes)
        valid = mus_dist_penalty <= 0 and mus_scene_penalty <= 0
        return sc * score_k + mus_scene_penalty * mus_scene_penalty_k + mus_dist_penalty * mus_dist_penalty_k, \
            valid, mus_dist_penalty, mus_scene_penalty

    def gen_population(parent, sigmas) -> [np.ndarray]:
        def gen_one():
            shift = np.random.normal(0, sigmas, (n_musicians, 3))
            mask = np.random.random((n_musicians, 3)) < mutation_probability
            r = shift * mask + parent
            r[r[:, 2] > 10, 2] = 10
            r[r[:, 2] < 0, 2] = 0
            return r

        return [gen_one() for _ in range(population_size)]

    current_solution = initial_solution(stage, n_musicians, [0.001, 0.001, 0.1])
    current_score = call_score(current_solution)
    for step in range(step_count):
        # sigmas = initial_sigmas * (1.0 - step / step_count)
        sigmas = initial_sigmas * (math.exp(-step / step_count * 6))
        print(f"Step {step}/{step_count} Approx.score={current_score} sigmas={sigmas}")
        population = gen_population(current_solution, sigmas)
        # TODO: make solutions valid / filter invalid

        for solution in population:
            sol_score = call_score(solution)
            current_score = call_score(current_solution)
            if sol_score[0] > current_score[0] and (step < step_count * 0.7 or sol_score[1] >= current_score[1]):
                current_solution = solution
                current_score = sol_score
    print(f"Finished {step_count} steps. Approx.score={current_score}")

    real_score = score(mus_instruments, current_solution, att_places, att_tastes, pillar_center_radius,
                       use_playing_together_ext)
    valid = placing_valid(stage, current_solution)
    print(f"Real score={real_score} valid={valid}")
    return current_solution, real_score, valid
