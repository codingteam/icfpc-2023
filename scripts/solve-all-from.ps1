param ($From, $To = 90)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$From..$To | ForEach-Object -Parallel {
    Write-Host "Solving $_"
    dotnet run --configuration Release --project Icfpc2023 -- solve $_ best
    Write-Host "Solved $_"
}
