dotnet build $PSScriptRoot\..\Elzik.FmSync.sln

dotnet test `
	--no-build `
	--verbosity normal `
	-p:CollectCoverage=true `
	-p:CoverletOutput=TestResults/coverage.opencover.xml `
	-p:CoverletOutputFormat=opencover

dotnet tool update `
	--global dotnet-reportgenerator-globaltool `
	--version 5.1.8

reportgenerator `
	"-reports:tests/Elzik.FmSync.Application.Tests.Unit/TestResults/coverage.opencover.xml;tests/Elzik.FmSync.Infrastructure.Tests.Integration/TestResults/coverage.opencover.xml;" `
	"-targetdir:tests/TestResults" `
	"-reporttypes:Badges"