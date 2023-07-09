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

type Problem =
    {
        RoomWidth: double
        RoomHeight: double
        StageWidth: double
        StageHeight: double
        StageBottomLeft: PointD
        Musicians: int[]
        Attendees: Attendee[]
        Pillars: Pillar[]
    }
    /// Extension 2: Playing Together
    member this.IsTogetherExtensionActive =
        // Confirmed by the organizers: https://discord.com/channels/1118159165060292668/1126853058186444942/1127283248943353928
        this.Pillars.Length > 0

type IPartialSolution =
    abstract member GetPlacedMusicians: allMusicians: int[] -> seq<PointD * double * int>

type Solution =
    {
        Placements: PointD[]
        Volumes: double[]
    }
    interface IPartialSolution with
        member this.GetPlacedMusicians allMusicians =
            Seq.zip3 this.Placements this.Volumes allMusicians

module Solution =
    let defaultVolumes numOfMusicians = Array.replicate numOfMusicians 10.0

type Score = double
type SolverName = string

type SolutionMetadata = {
    Score: Score
    SolverName: SolverName
}
