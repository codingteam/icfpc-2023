param ($Improver = 'lambda', $From = 1, $To = 90)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$From..$To | ForEach-Object -Parallel {
    Write-Host "Improving $_"
    dotnet run --configuration Release --project Icfpc2023 -- improve $_ lambda --preserve-best
    Write-Host "Improving $_"
}
