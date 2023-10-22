$ErrorActionPreference = "Stop"

& $PSScriptRoot/build-and-test.ps1
& $PSScriptRoot/build-windows-installer.ps1
