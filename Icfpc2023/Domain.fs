namespace Icfpc2023

type Attendee = {
    X: double
    Y: double
    Tastes: double[]
}

type Pillar = {
    Center: PointD
    Radius: double
}

type Problem = {
    RoomWidth: double
    RoomHeight: double
    StageWidth: double
    StageHeight: double
    StageBottomLeft: PointD
    Musicians: int[]
    Attendees: Attendee[]
    Pillars: Pillar[]
}

type IPartialSolution =
    abstract member GetPlacedMusicians: allMusicians: int[] -> seq<PointD * int>

type Solution =
    { Placements: PointD[] }
    interface IPartialSolution with
        member this.GetPlacedMusicians allMusicians =
            allMusicians
            |> Seq.zip this.Placements

type Score = double
type SolverName = string

type SolutionMetadata = {
    Score: Score
    SolverName: SolverName
}
