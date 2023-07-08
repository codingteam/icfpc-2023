namespace Icfpc2023.Visualizer.ViewModels

open ReactiveUI

type MainWindowViewModel() =
    inherit ViewModelBase()

    static member LoadProblem(problemId: int): FieldViewModel =
        let problem = Program.readProblem problemId
        let solution = Program.readSolution problemId
        let score = Program.readScoreOrCompute problemId problem
        FieldViewModel(problemId, problem, solution, score)

    member val Field =
        MainWindowViewModel.LoadProblem(1)
        with get, set

    member this.ProblemId = string this.Field.ProblemId

    member this.MusiciansCount = string this.Field.Problem.Musicians.Length

    member this.Score = string this.Field.Score

    member private this.LoadProblemById(problemId: int) =
        this.Field <- MainWindowViewModel.LoadProblem(problemId)
        this.RaisePropertyChanged(nameof this.ProblemId)
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
