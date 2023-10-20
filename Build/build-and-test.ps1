$repoRootPath = (Resolve-Path "$PSScriptRoot/../").Path
$repoRootPath = $repoRootPath.Substring(0,$repoRootPath.Length-1)
Write-Output "Repo root path:$repoRootPath"

dotnet test $repoRootPath/Elzik.FmSync.sln `
	-c Release `
	--verbosity normal `
	-p:CollectCoverage=true `
	-p:CoverletOutput=$repoRootPath/tests/TestResults/coverage.opencover.xml `
	-p:CoverletOutputFormat=opencover

dotnet tool update `
	--global dotnet-reportgenerator-globaltool `
	--version 5.1.8

reportgenerator `
	"-reports:$repoRootPath/tests/Elzik.FmSync.Application.Tests.Unit/TestResults/coverage.opencover.xml;$repoRootPath/tests/Elzik.FmSync.Infrastructure.Tests.Integration/TestResults/coverage.opencover.xml;" `
	"-targetdir:$repoRootPath/tests/TestResults" `
	"-reporttypes:Badges"
