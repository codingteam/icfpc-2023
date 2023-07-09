from dataclasses import dataclass

import numpy as np


@dataclass
class Stage:
    x: float
    y: float
    w: float
    h: float


@dataclass
class Problem:
    room_width: float
    room_height: float
    stage_width: float
    stage_height: float
    stage_bottom_left: [float, float]
    musicians: [Musician]
    attendees: [Attendee]
    pillars: [Pillar]


@dataclass
class ProblemData:
    room_w: float
    room_h: float
    stage: Stage
    mus_instruments: np.ndarray
    att_places: np.ndarray
    att_tastes: np.ndarray
    pillar_center_radius: np.ndarray
    use_playing_together_ext: bool


@dataclass
class SolutionMetadata:
    score: float
    solver: str
