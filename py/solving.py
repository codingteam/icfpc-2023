from dataclasses import dataclass

import numpy as np

from scoring import score


@dataclass
class Scene:
    x: float
    y: float
    w: float
    h: float


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
    n_musicians, = mus_instruments.shape

    def call_score(mus_places: np.ndarray) -> float:
        return score(mus_instruments, mus_places, att_places, att_tastes, pillar_center_radius,
                     use_playing_together_ext)

    def gen_population(parent, sigma) -> [np.ndarray]:
        return [np.random.normal(0, sigma, (n_musicians, 2)) + parent for _ in range(population_size)]

    current_solution = initial_solution(scene, n_musicians, 0.001)
    current_score = call_score(current_solution)
    for step in range(step_count):
        sigma = initial_sigma - initial_sigma * step / step_count
        population = gen_population(current_solution, sigma)

        for solution in population:
            sol_score = call_score(solution)
            if sol_score > current_score:
                current_solution = solution
                current_score = sol_score

    return current_score, current_solution
