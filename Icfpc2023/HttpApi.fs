module Icfpc2023.HttpApi

open System.Net
open System.Net.Http
open System.Net.Http.Headers
open System.Net.Mime
open System.Text
open System.Threading.Tasks
open Newtonsoft.Json

// api.icfpcontest.com/problems
type ProblemMetaInfo = {
    [<JsonProperty("number_of_problems")>]
    NumberOfProblems: int
}

type ProblemDef = {
    Success: string
}

let GetProblemCount(): Task<int> = task {
    use client = new HttpClient()
    let! body = client.GetStringAsync "https://api.icfpcontest.com/problems"
    return JsonConvert.DeserializeObject<ProblemMetaInfo>(body).NumberOfProblems
}

let DownloadProblem(number: int): Task<string> = task {
    use client = new HttpClient()
    let! body = client.GetStringAsync $"https://api.icfpcontest.com/problem?problem_id={string number}"
    return JsonConvert.DeserializeObject<ProblemDef>(body).Success
}

let SubmissionUrl = "https://api.icfpcontest.com/submission"

type Submission = {
    [<JsonProperty("problem_id")>]
    ProblemId: int
    [<JsonProperty("contents")>]
    Contents: string
}

let Upload(submission: Submission, token: string): Task = task {
    use client = new HttpClient()
    client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Bearer", token)

    use content = new StringContent(
        JsonConvert.SerializeObject(submission),
        Encoding.UTF8,
        MediaTypeNames.Application.Json
    )
    let! response = client.PostAsync(SubmissionUrl, content)
    if response.StatusCode <> HttpStatusCode.Created then
        let! responseContent = response.Content.ReadAsStringAsync()
        failwith $"Error {response.StatusCode}: {responseContent}"
}
