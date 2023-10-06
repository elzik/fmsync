$runtimes = "win-x64", "osx-x64"

foreach ($runtime in $runtimes)
{
	dotnet publish $PSScriptRoot\..\src\Elzik.FmSync.Worker\Elzik.FmSync.Worker.csproj `
		-r $runtime `
		--self-contained true `
		-c Release `
		-o $PSScriptRoot\output\Elzik.FmSync.Worker\$runtime `
		-p:PublishSingleFile=true

	dotnet publish $PSScriptRoot\..\src\Elzik.FmSync.Console\Elzik.FmSync.Console.csproj `
		-r $runtime `
		--self-contained true `
		-c Release `
		-o $PSScriptRoot\output\Elzik.FmSync.Console\$runtime `
		-p:PublishSingleFile=true
}
