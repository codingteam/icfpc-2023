namespace Icfpc2023.Visualizer.ViewModels

open Icfpc2023
open ReactiveUI

type MainWindowViewModel() =
    inherit ViewModelBase()

    static member LoadProblem(problemId: int): FieldViewModel =
        let problem = Program.readProblem problemId
        let solution, solutionMetadata = Program.readSolution problemId
        FieldViewModel(problemId, problem, solution, solutionMetadata)

    member val Field =
        MainWindowViewModel.LoadProblem(1)
        with get, set

    member this.ProblemId = string this.Field.ProblemId

    member this.MusiciansCount = string this.Field.Problem.Musicians.Length

    member this.Score = string this.Field.SolutionMetadata.Score
    member this.Solver = string this.Field.SolutionMetadata.Solver

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
        | ex -> eprintfn $"Error: {ex.Message}"

    member this.LoadNext(): unit =
        try
            let oldProblem = this.ProblemId |> int
            let newProblem = oldProblem + 1
            this.LoadProblemById newProblem
        with
        | ex -> eprintfn $"Error: {ex.Message}"
