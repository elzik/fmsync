$ErrorActionPreference = "Stop"

$repoRootPath = (Resolve-Path "$PSScriptRoot/../").Path

Import-Module $(Resolve-Path "$repoRootPath/Build/Test-ExitCode.psm1")

& $PSScriptRoot/build-and-test.ps1
Test-ExitCode
& $PSScriptRoot/build-windows-installer.ps1
Test-ExitCode