param ($From = 1, $To = 90)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

dotnet build --configuration Release

$From..$To | ForEach-Object -Parallel {
    Write-Host "Improving $_"
    dotnet run --no-build --configuration Release --project Icfpc2023 -- improve $_ 'suosi' --preserve-best
    Write-Host "Improving $_"
}
