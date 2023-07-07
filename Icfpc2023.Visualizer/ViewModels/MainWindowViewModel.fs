namespace Icfpc2023.Visualizer.ViewModels

open System.IO
open Icfpc2023
open ReactiveUI

type MainWindowViewModel() =
    inherit ViewModelBase()

    static member LoadProblem(path: string): FieldViewModel =
        let problem = JsonDefs.ReadProblemFromFile path
        FieldViewModel(path, problem)

    member val Field =
        MainWindowViewModel.LoadProblem(Path.Combine(Program.solutionDirectory, "problems", "1.json"))
        with get, set

    member this.CurrentProblemId: string =
        Path.GetFileNameWithoutExtension(this.Field.Path)

    member private this.LoadProblemByNumber(number: int) =
        this.Field <- MainWindowViewModel.LoadProblem(
            Path.Combine(Program.solutionDirectory, "problems", $"{string number}.json")
        )
        this.RaisePropertyChanged(nameof this.CurrentProblemId)
        this.RaisePropertyChanged(nameof this.Field)

    member this.LoadPrev(): unit =
        try
            let oldProblem = this.CurrentProblemId |> int
            let newProblem = oldProblem - 1
            this.LoadProblemByNumber newProblem
        with
        | ex -> eprintfn $"Error: {ex.Message}"

    member this.LoadNext(): unit =
        try
            let oldProblem = this.CurrentProblemId |> int
            let newProblem = oldProblem + 1
            this.LoadProblemByNumber newProblem
        with
        | ex -> eprintfn $"Error: {ex.Message}"
