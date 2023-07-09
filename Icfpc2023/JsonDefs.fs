module Icfpc2023.JsonDefs

open System.IO
open Newtonsoft.Json

[<Struct>]
type AttendeeJson = {
    x: double
    y: double
    tastes: double[]
}

[<Struct>]
type PillarJson = {
    center: double[]
    radius: double
}

type ProblemJson = {
    room_width: double
    room_height: double
    stage_width: double
    stage_height: double
    stage_bottom_left: double[]
    musicians: int[]
    attendees: AttendeeJson[]
    pillars: PillarJson[]
}

[<Struct>]
type PlacementJson = {
    x: double
    y: double
}

type SolutionLegacyJson = {
    placements: PlacementJson[]
}

type SolutionJson = {
    placements: PlacementJson[]
    volumes: double[]
}

type SolutionMetadataJson = {
    score: Score
    solver: SolverName
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
        Pillars = problem.pillars |> Array.map(fun a -> { Center = PointD(a.center.[0], a.center.[1]); Radius = a.radius })
    }

let WriteSolutionToJson(solution: Solution): string =
    let solJson = {
        placements = solution.Placements |> Array.map(fun p -> { x = p.X; y = p.Y })
        volumes = solution.Volumes
    }
    JsonConvert.SerializeObject(solJson)

let ReadSolutionLegacyFromJson(json: string): Solution =
    let solJson = JsonConvert.DeserializeObject<SolutionJson>(json)
    {
        Placements = solJson.placements |> Array.map(fun p -> PointD(p.x, p.y))
        Volumes = Solution.defaultVolumes solJson.placements.Length
    }

let ReadSolutionFromJson(json: string): Solution =
    let solJson = JsonConvert.DeserializeObject<SolutionJson>(json)
    {
        Placements = solJson.placements |> Array.map(fun p -> PointD(p.x, p.y))
        Volumes = solJson.volumes
    }

let WriteSolutionMetadataToJson(metadata: SolutionMetadata): string =
    let metadataJson = {
        score = metadata.Score
        solver = metadata.SolverName
    }
    JsonConvert.SerializeObject(metadataJson)

let ReadSolutionMetadataFromJson(json: string): SolutionMetadata =
    let metadataJson = JsonConvert.DeserializeObject<SolutionMetadataJson>(json)
    {
        Score = metadataJson.score
        SolverName = metadataJson.solver
    }

let ReadProblemFromFile(filePath: string): Problem =
    let json = File.ReadAllText(filePath)
    ReadProblemFromJson json

let ReadSolutionFromFile(filePath: string): Solution =
    let json = File.ReadAllText(filePath)
    ReadSolutionFromJson json

let ReadSolutionLegacyFromFile(filePath: string): Solution =
    let json = File.ReadAllText(filePath)
    ReadSolutionLegacyFromJson json

let ReadSolutionMetadataFromFile(filePath: string): SolutionMetadata =
    let json = File.ReadAllText(filePath)
    ReadSolutionMetadataFromJson json
