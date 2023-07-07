module Icfpc2023.HttpApi

open System.Net.Http
open System.Threading.Tasks

let DownloadProblem(number: int): Task<string> = task {
    use client = new HttpClient()
    return! client.GetStringAsync $"https://cdn.icfpcontest.com/problems/{string number}.json"
}
