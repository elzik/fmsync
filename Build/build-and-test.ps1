Import-Module $(Resolve-Path ./Build/Test-ExitCode.psm1)
$ErrorActionPreference = "Stop"

$repoRootPath = (Resolve-Path "$PSScriptRoot/../").Path

dotnet test $repoRootPath/Elzik.FmSync.sln `
	-c Release `
	--verbosity normal `
	-p:CollectCoverage=true `
	-p:CoverletOutput=TestResults/coverage.opencover.xml `
	-p:CoverletOutputFormat=opencover
Test-ExitCode

dotnet tool update `
	--global dotnet-reportgenerator-globaltool `
	--version 5.1.8
Test-ExitCode

reportgenerator `
	"-reports:$repoRootPath/tests/Elzik.FmSync.Application.Tests.Unit/TestResults/coverage.opencover.xml;$repoRootPath/tests/Elzik.FmSync.Infrastructure.Tests.Integration/TestResults/coverage.opencover.xml;" `
	"-targetdir:$repoRootPath/tests/TestResults" `
	"-reporttypes:Badges;Cobertura"
Test-ExitCode