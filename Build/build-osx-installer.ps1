$ErrorActionPreference = "Stop"

$repoRootPath = (Resolve-Path "$PSScriptRoot/../").Path
$releasePath = "$repoRootPath/Installer/Elzik.FmSync.OsxInstaller/x64/Release/"
$consolePublishSourcePath = "$repoRootPath/src/Elzik.FmSync.Console/bin/x64/Release/net8.0/osx-x64/publish"
$workerPublishSourcePath = "$repoRootPath/src/Elzik.FmSync.Worker/bin/x64/Release/net8.0/osx-x64/publish"

If((Test-Path -PathType container "$releasePath"))
{
	Remove-Item -Recurse -Path "$releasePath"
}

Import-Module $(Resolve-Path "$repoRootPath/Build/Test-ExitCode.psm1")

If((Test-Path -PathType container $workerPublishSourcePath))
{
	Remove-Item -Recurse -Path $workerPublishSourcePath
}
dotnet publish $repoRootPath/src/Elzik.FmSync.Worker/Elzik.FmSync.Worker.csproj `
	--verbosity normal `
	--runtime osx-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true
Test-ExitCode

If((Test-Path -PathType container $consolePublishSourcePath))
{
	Remove-Item -Recurse -Path $consolePublishSourcePath
} 
dotnet publish $repoRootPath/src/Elzik.FmSync.Console/Elzik.FmSync.Console.csproj `
	--verbosity normal `
	--runtime osx-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true
Test-ExitCode

$publishDestinationPath = "$repoRootPath/Build/Publish/osx-x64/"

$consolePublishDestinationPath = "$publishDestinationPath/Console"
If(!(Test-Path -PathType container $consolePublishDestinationPath))
{
	New-Item -ItemType Directory -Path $consolePublishDestinationPath
}
Copy-Item "$consolePublishSourcePath/fmsync" `
	-Destination $consolePublishDestinationPath
Copy-Item "$consolePublishSourcePath/appSettings.json" `
	-Destination $consolePublishDestinationPath
Test-ExitCode

$workerPublishDestinationPath = "$publishDestinationPath/Worker"
If(!(Test-Path -PathType container $workerPublishDestinationPath))
{
	New-Item -ItemType Directory -Path $workerPublishDestinationPath
}
Copy-Item "$workerPublishSourcePath/Elzik.FmSync.Worker" `
	-Destination $workerPublishDestinationPath
Copy-Item "$workerPublishSourcePath/appSettings.json" `
	-Destination $workerPublishDestinationPath
Test-ExitCode

If(!(Test-Path -PathType container $releasePath))
{
	New-Item -ItemType Directory -Path $releasePath
}
Compress-Archive `
	-Path "$publishDestinationPath/*" `
	-DestinationPath "$releasePath/fmsync.zip" `
	-Force
Test-ExitCode

dotnet tool update --global GitVersion.Tool
Test-ExitCode

$SemVer = (dotnet-gitversion | ConvertFrom-Json).SemVer
Copy-Item "$releasePath/fmsync.zip" "$releasePath/fmsync-osx-x64-v$SemVer.zip"
Test-ExitCode