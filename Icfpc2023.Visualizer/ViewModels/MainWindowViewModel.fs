namespace Icfpc2023.Visualizer.ViewModels

open System.IO
open Icfpc2023

type MainWindowViewModel() =
    inherit ViewModelBase()

    static member LoadProblem(path: string): FieldViewModel =
        let problem = JsonDefs.ReadProblemFromFile path
        FieldViewModel(problem)

    member val Field =
        MainWindowViewModel.LoadProblem(Path.Combine(Program.solutionDirectory, "problems", "1.json"))
        with get, set
