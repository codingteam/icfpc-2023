module Icfpc2023.Solvers.VulpesImporter

open System.IO
open Icfpc2023

let ImportIni(problemId: int): Solution =
    let newSolution = Path.Combine(DirectoryLookup.examplesDir, $"{problemId}.ini.new")
    let data = File.ReadAllText newSolution
    Converter.FromNewIni data
