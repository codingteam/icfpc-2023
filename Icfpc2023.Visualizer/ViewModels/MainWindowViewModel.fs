namespace Icfpc2023.Visualizer.ViewModels

open System
open Icfpc2023
open ReactiveUI

type MainWindowViewModel() =
    inherit ViewModelBase()

    static member LoadProblem(problemId: int): FieldViewModel =
        let problem = Program.readProblem problemId
        let solution, solutionMetadata =
            Program.tryReadSolution problemId
            |> Option.map(fun (s, m) -> Some s, Some m)
            |> Option.defaultValue(None, None)
        FieldViewModel(problemId, problem, solution, solutionMetadata)

    member val Field =
        let cmdArgs = Environment.GetCommandLineArgs() |> Array.skip 1
        let problemId = if cmdArgs.Length > 0 then int cmdArgs[0] else 1
        MainWindowViewModel.LoadProblem(problemId)
        with get, set

    member this.ProblemId = string this.Field.ProblemId

    member this.MusiciansCount = string this.Field.Problem.Musicians.Length

    member this.PillarCount = string this.Field.Problem.Pillars.Length

    member this.Score = this.Field.SolutionMetadata |> Option.map(fun s -> string s.Score) |> Option.defaultValue "N/A"
    member this.Solver =
        this.Field.SolutionMetadata |> Option.map(fun m -> string m.SolverName) |> Option.defaultValue "N/A"

    member private this.LoadProblemById(problemId: int) =
        this.Field <- MainWindowViewModel.LoadProblem(problemId)
        this.RaisePropertyChanged(nameof this.ProblemId)
        this.RaisePropertyChanged(nameof this.MusiciansCount)
        this.RaisePropertyChanged(nameof this.Score)
        this.RaisePropertyChanged(nameof this.Solver)
        this.RaisePropertyChanged(nameof this.Field)

    member this.LoadPrev(): unit =
        try
            let oldProblem = this.ProblemId |> int
            let newProblem = oldProblem - 1
            this.LoadProblemById newProblem
        with
        | ex -> eprintfn $"Error: {ex.Message}\r\n{ex.StackTrace}"

    member this.LoadNext(): unit =
        try
            let oldProblem = this.ProblemId |> int
            let newProblem = oldProblem + 1
            this.LoadProblemById newProblem
        with
        | ex -> eprintfn $"Error: {ex.Message}\r\n{ex.StackTrace}"
