namespace Icfpc2023

type Attendee = {
    X: double
    Y: double
    Tastes: double[]
}

type Problem = {
    RoomWidth: double
    RoomHeight: double
    StageWidth: double
    StageHeight: double
    StageBottomLeft: PointD
    Musicians: int[]
    Attendees: Attendee[]
}

type Solution = {
    Placements: PointD[]
}

type Score = double
type SolverName = string

type SolutionMetadata = {
    Score: Score
    SolverName: SolverName
}
