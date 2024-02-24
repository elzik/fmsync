Import-Module $(Resolve-Path ./Build/Test-ExitCode.psm1)
$ErrorActionPreference = "Stop"

& $PSScriptRoot/build-and-test.ps1
Test-ExitCode
& $PSScriptRoot/build-windows-installer.ps1
Test-ExitCode