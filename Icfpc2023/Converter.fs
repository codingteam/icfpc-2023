module Icfpc2023.Converter

open System
open System.Text

let ToIni (problem: Problem) (solution: Solution option): string =
    let result = StringBuilder()
    let version = if problem.IsTogetherExtensionActive then "2" else "1"
    result.AppendLine $"""[version]
{version}
[room_height]
{string problem.RoomHeight}
[room_width]
{string problem.RoomWidth}
[stage_height]
{string problem.StageHeight}
[stage_width]
{string problem.StageWidth}
[stage_bottom_left]
{string problem.StageBottomLeft.X} {problem.StageBottomLeft.Y}
[musicians]
{string problem.Musicians.Length}""" |> ignore
    let musicianPositions =
        solution
        |> Option.map(fun s -> Seq.ofArray s.Placements)
        |> Option.defaultValue(Seq.initInfinite (fun _ -> PointD(-1.0, -1.0)))
    for instrument, position in Seq.zip problem.Musicians musicianPositions do
        result.AppendLine $"{string position.X} {string position.Y} {string instrument}" |> ignore
    result.AppendLine $"""[attendees]
{string problem.Attendees.Length}""" |> ignore
    for attendee in problem.Attendees do
        result.Append $"{string attendee.X} {string attendee.Y}" |> ignore
        for taste in attendee.Tastes do
            result.Append $" {string taste}" |> ignore
        result.AppendLine() |> ignore

    result.AppendLine $"""[pillars]
{string problem.Pillars.Length}""" |> ignore
    for pillar in problem.Pillars do
        result.AppendLine $"{string pillar.Center.X} {string pillar.Center.Y} {string pillar.Radius}" |> ignore

    result.ToString().ReplaceLineEndings("\n")

#nowarn "25"

let FromNewIni(text: string): Solution =
    let lines = text.Split('\n') |> Seq.map(fun t -> t.Trim()) |> Seq.filter(fun t -> t <> "") |> Seq.toArray
    let header = lines[0].Trim()
    if header <> "[musicians]" then failwith $"Expected [musicians] but got {header}."
    let musiciansCount = int (lines[1].Trim())

    let placements = Array.zeroCreate musiciansCount
    for i in 1..musiciansCount do
        let line = lines[i + 1].Trim()
        let components = line.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries)
        let [| xs; ys; _is |] = components
        let point = PointD(double xs, double ys)
        placements[i - 1] <- point
    { Placements = placements }
