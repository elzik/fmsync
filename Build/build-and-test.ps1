dotnet test $PSScriptRoot/..Elzik.FmSync.sln `
	-c Release `
	--verbosity normal `
	-p:CollectCoverage=true `
	-p:CoverletOutput=$PSScriptRoot/../tests/TestResults/coverage.opencover.xml `
	-p:CoverletOutputFormat=opencover

dotnet tool update `
	--global dotnet-reportgenerator-globaltool `
	--version 5.1.8

reportgenerator `
	"-reports:$PSScriptRoot/../tests/Elzik.FmSync.Application.Tests.Unit/TestResults/coverage.opencover.xml;$PSScriptRoot/../tests/Elzik.FmSync.Infrastructure.Tests.Integration/TestResults/coverage.opencover.xml;" `
	"-targetdir:$PSScriptRoot/../tests/TestResults" `
	"-reporttypes:Badges"
