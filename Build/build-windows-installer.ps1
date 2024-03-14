$ErrorActionPreference = "Stop"

$repoRootPath = (Resolve-Path "$PSScriptRoot/../").Path

Import-Module $(Resolve-Path "$repoRootPath/Build/Test-ExitCode.psm1")

dotnet publish $repoRootPath\src\Elzik.FmSync.Worker\Elzik.FmSync.Worker.csproj `
	--runtime win-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true
Test-ExitCode

dotnet publish $repoRootPath\src\Elzik.FmSync.Console\Elzik.FmSync.Console.csproj `
	--runtime win-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true
Test-ExitCode

dotnet build $repoRootPath\Installer\Elzik.FmSync.WindowsInstaller\Elzik.FmSync.WindowsInstaller.wixproj `
	--runtime win-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true
Test-ExitCode

dotnet tool update --global GitVersion.Tool
Test-ExitCode

$SemVer = (dotnet-gitversion | ConvertFrom-Json).SemVer
Copy-Item $repoRootPath\Installer\Elzik.FmSync.WindowsInstaller\bin\x64\Release\en-US\Elzik.FmSync.WindowsInstaller.msi "$repoRootPath\Installer\Elzik.FmSync.WindowsInstaller\bin\x64\Release\en-US\fmsync v$SemVer.msi"
Test-ExitCode