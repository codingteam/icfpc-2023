namespace Icfpc2023

open Newtonsoft.Json

[<Struct>]
type PointD =
    | PointD of double * double
    member this.X = match this with PointD(x, _) -> x
    member this.Y = match this with PointD(_, y) -> y

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
