namespace Icfpc2023.Visualizer.ViewModels

open Icfpc2023

[<Struct>]
type MusicianModel =
    | MusicianModel of instrument: int * position: PointD
    member this.Position = let (MusicianModel(_, p)) = this in p

type FieldViewModel(problemId: int,
                    problem: Problem,
                    initialSolution: Solution option,
                    solutionMetadata: SolutionMetadata option) =

    let getSelected solution (a: PointD) (b: PointD) =
        let musicians = problem.Musicians
        let placements = solution.Placements
        Seq.zip musicians placements
        |> Seq.filter(fun(_, p) -> a.X <= p.X && p.X <= b.X && b.Y <= p.Y && p.Y <= a.Y)
        |> Seq.map MusicianModel
        |> Set.ofSeq

    let moveMusicians selected vector solution: Solution * MusicianModel seq =
        let allMusicians =
            let musicians = problem.Musicians
            let placements = solution.Placements
            Seq.zip musicians placements
            |> Seq.map MusicianModel

        let movedMusicians = ResizeArray()

        let sol =
            { solution with
                Placements = allMusicians |> Seq.map (fun m ->
                    let (MusicianModel(i, p)) = m
                    if Set.contains m selected then
                        movedMusicians.Add(MusicianModel(i, p + vector))
                        p + vector
                    else
                        p
                ) |> Seq.toArray
            }
        sol, movedMusicians

    member val ProblemId = problemId
    member val Problem = problem
    member val Solution = initialSolution with get, set
    member val SolutionMetadata = solutionMetadata with get, set

    member val Scale = 0.25 with get, set

    member val SelectionActive = false with get, set
    member val SelectionStart = PointD(0.0, 0.0) with get, set
    member val SelectionEnd = PointD(0.0, 0.0) with get, set
    member val SelectedMusicians = Set.empty with get, set

    member this.StartSelection(p: PointD) =
        this.SelectionStart <- p
        this.SelectionActive <- true

    member this.ExtendSelection(p: PointD) =
        this.SelectionEnd <- p

    member this.FinishSelection(p: PointD) =
        this.SelectionEnd <- p
        match this.Solution with
        | None -> ()
        | Some s -> this.SelectedMusicians <- getSelected s this.SelectionStart this.SelectionEnd
        this.SelectionActive <- false

    member this.MoveSelectedTo(p: PointD) =
        let delta =
            let minX = this.SelectedMusicians |> Seq.map(fun (MusicianModel(_, p)) -> p.X) |> Seq.min
            let minY = this.SelectedMusicians |> Seq.map(fun (MusicianModel(_, p)) -> p.Y) |> Seq.min
            let maxX = this.SelectedMusicians |> Seq.map(fun (MusicianModel(_, p)) -> p.X) |> Seq.max
            let maxY = this.SelectedMusicians |> Seq.map(fun (MusicianModel(_, p)) -> p.Y) |> Seq.max
            let center = PointD(minX + (maxX - minX) / 2.0, minY + (minY - maxY) / 2.0)
            p - center
        match this.Solution with
        | None -> ()
        | Some s ->
             let s', m = moveMusicians this.SelectedMusicians delta s
             this.Solution <- Some s'
             this.SelectedMusicians <- Set.ofSeq m
