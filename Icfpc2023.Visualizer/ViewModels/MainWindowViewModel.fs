namespace Icfpc2023.Visualizer.ViewModels

open ReactiveUI

type MainWindowViewModel() =
    inherit ViewModelBase()

    static member LoadProblem(problemId: int): FieldViewModel =
        let problem = Program.readProblem problemId
        let solution = Program.readSolution problemId
        FieldViewModel(problemId, problem, solution)

    member val Field =
        MainWindowViewModel.LoadProblem(1)
        with get, set

    member this.CurrentProblemId = string this.Field.ProblemId

    member private this.LoadProblemById(problemId: int) =
        this.Field <- MainWindowViewModel.LoadProblem(problemId)
        this.RaisePropertyChanged(nameof this.CurrentProblemId)
        this.RaisePropertyChanged(nameof this.Field)

    member this.LoadPrev(): unit =
        try
            let oldProblem = this.CurrentProblemId |> int
            let newProblem = oldProblem - 1
            this.LoadProblemById newProblem
        with
        | ex -> eprintfn $"Error: {ex.Message}"

    member this.LoadNext(): unit =
        try
            let oldProblem = this.CurrentProblemId |> int
            let newProblem = oldProblem + 1
            this.LoadProblemById newProblem
        with
        | ex -> eprintfn $"Error: {ex.Message}"
