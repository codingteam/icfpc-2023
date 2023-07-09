from dataclasses import dataclass

import numpy as np

from scoring import *

from domain import *


def initial_solution(scene: Scene, n_musicians, sigma) -> np.ndarray:
    center = [scene.x + 0.5 * scene.w, scene.y + 0.5 * scene.h]
    return np.random.normal(center, sigma, n_musicians)


def ES_solve(scene: Scene,
             mus_instruments: np.ndarray,
             att_places: np.ndarray,
             att_tastes: np.ndarray,
             pillar_center_radius: np.ndarray,
             use_playing_together_ext: bool = True) -> (np.ndarray, float):
    initial_sigma = 10
    step_count = 100
    population_size = 20
    score_k = 1
    mus_dist_penalty_k = -100
    mus_scene_penalty_k = -100

    n_musicians = mus_instruments.shape[0]

    def call_score(mus_places: np.ndarray) -> float:
        sc = mc_score(mus_instruments, mus_places, att_places, att_tastes, pillar_center_radius,
                      use_playing_together_ext,
                      mus_ratio=0.1,
                      att_ratio=0.1,
                      pillar_ratio=0.1,
                      n_eval=50)
        mus_dist_penalty = musicians_distance_penalty(mus_places)
        mus_scene_penalty = musicians_out_of_scene_penalty(scene, mus_places)
        return sc * score_k + mus_scene_penalty * mus_scene_penalty_k + mus_dist_penalty * mus_dist_penalty_k

    def gen_population(parent, sigma) -> [np.ndarray]:
        return [np.random.normal(0, sigma, (n_musicians, 2)) + parent for _ in range(population_size)]

    current_solution = initial_solution(scene, n_musicians, 0.001)
    current_score = call_score(current_solution)
    for step in range(step_count):
        sigma = initial_sigma - initial_sigma * step / step_count
        population = gen_population(current_solution, sigma)
        # TODO: make solutions valid / filter invalid

        for solution in population:
            sol_score = call_score(solution)
            if sol_score > current_score:
                current_solution = solution
                current_score = sol_score
    real_score = score(mus_instruments, current_solution, att_places, att_tastes, pillar_center_radius,
                       use_playing_together_ext)
    return real_score, current_solution
