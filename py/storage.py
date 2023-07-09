import json

from domain import *
import numpy as np

SOLUTIONS_DIR = '../solutions/'
PROBLEMS_DIR = '../problems/'


def read_problem(id) -> ProblemData:
    with open(f"${PROBLEMS_DIR}/${id}.json", 'r') as f:
        js = json.load(f)

        mus_instruments = np.array(js["musicians"])

        att_places = np.array([[att["x"], att["y"]] for att in js["attendees"]], dtype='float64')
        att_tastes = np.array([att["tastes"] for att in js["attendees"]], dtype='float64')

        if js.get("pillars"):
            pillar_center_radius = np.array([p["center"] + [p["radius"]] for p in js["pillars"]], dtype='float64')
        else:
            pillar_center_radius = np.array([], dtype='float64')

        use_playing_together_ext = js.get("pillars") is not None
        return ProblemData(room_w=js["room_width"],
                           room_h=js["room_height"],
                           stage=Stage(x=js["stage_bottom_left"][0],
                                       y=js["stage_bottom_left"][1],
                                       w=js["stage_width"],
                                       h=js["stage_height"]),
                           mus_instruments=mus_instruments,
                           att_places=att_places,
                           att_tastes=att_tastes,
                           pillar_center_radius=pillar_center_radius,
                           use_playing_together_ext=use_playing_together_ext)


def write_solution(id, solution: np.ndarray):
    n_mus = solution.shape[0]
    mus_volumes = solution[:, 2] if solution.shape[1] == 3 else None
    placements = []
    volumes = []

    for i in range(n_mus):
        placements.append(dict(x=solution[i, 0], y=solution[i, 1]))
        volumes.append(mus_volumes[i] if mus_volumes else 10.0)

    js = dict(placements=placements, volumes=volumes)

    with open(f"${SOLUTIONS_DIR}/${id}.meta.json", 'w') as f:
        json.dump(js, f)


def read_metadata(id) -> SolutionMetadata:
    with open(f"${SOLUTIONS_DIR}/${id}.meta.json", 'r') as f:
        js = json.load(f)
        return SolutionMetadata(score=js["score"], solver=js["solver"])


def write_metadata(id, score: int):
    with open(f"${SOLUTIONS_DIR}/${id}.meta.json", 'w') as f:
        js = {"score": score, "solver": "ES"}
        json.dump(js, f)
