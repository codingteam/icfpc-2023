module Icfpc2023.JsonDefs

open System.IO
open Newtonsoft.Json

open Icfpc2023.Scoring

[<Struct>]
type AttendeeJson = {
    x: double
    y: double
    tastes: double[]
}

type ProblemJson = {
    room_width: double
    room_height: double
    stage_width: double
    stage_height: double
    stage_bottom_left: double[]
    musicians: int[]
    attendees: AttendeeJson[]
}

[<Struct>]
type PlacementJson = {
    x: double
    y: double
}

type SolutionJson = {
    placements: PlacementJson[]
}

type SolutionScoreJson = {
    score: SolutionScore
}

#nowarn "25"

let ReadProblemFromJson(json: string): Problem =
    let problem = JsonConvert.DeserializeObject<ProblemJson>(json)
    let [|stageBottomX; stageBottomY|] = problem.stage_bottom_left
    {
        RoomWidth = problem.room_width
        RoomHeight = problem.room_height
        StageWidth = problem.stage_width
        StageHeight = problem.stage_height
        StageBottomLeft = PointD(stageBottomX, stageBottomY)
        Musicians = problem.musicians
        Attendees = problem.attendees |> Array.map(fun a -> { X = a.x; Y = a.y; Tastes = a.tastes })
    }

let WriteSolutionToJson(solution: Solution): string =
    let solJson = {
        placements = solution.Placements |> Array.map(fun p -> { x = p.X; y = p.Y })
    }
    JsonConvert.SerializeObject(solJson)

let ReadSolutionFromJson(json: string): Solution =
    let solJson = JsonConvert.DeserializeObject<SolutionJson>(json)
    {
        Placements = solJson.placements |> Array.map(fun p -> PointD(p.x, p.y))
    }

let WriteSolutionScoreToJson(score: SolutionScore): string =
    let scoreJson = {
        score = score
    }
    JsonConvert.SerializeObject(scoreJson)

let ReadSolutionScoreFromJson(json: string): SolutionScore =
    JsonConvert.DeserializeObject<SolutionScoreJson>(json).score

let ReadProblemFromFile(filePath: string): Problem =
    let json = File.ReadAllText(filePath)
    ReadProblemFromJson json

let ReadSolutionFromFile(filePath: string): Solution =
    let json = File.ReadAllText(filePath)
    ReadSolutionFromJson json

let ReadSolutionScoreFromFile(filePath: string): SolutionScore =
    let json = File.ReadAllText(filePath)
    ReadSolutionScoreFromJson json
