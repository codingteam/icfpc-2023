param ($From, $To = 90)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$From..$To | ForEach-Object {
    Write-Host "Uploading $_"
    dotnet run --configuration Release --project Icfpc2023 -- upload $_
    Write-Host "Uploaded $_"
}
