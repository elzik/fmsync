$runtimes = "win-x64", "osx-x64"

foreach ($runtime in $runtimes)
{
	dotnet publish $PSScriptRoot\..\src\Elzik.FmSync.Worker\Elzik.FmSync.Worker.csproj `
		-r $runtime `
		--no-self-contained `
		-c Release `
		-o $PSScriptRoot\output\$runtime `
		-p:PublishSingleFile=true
}

robocopy $PSScriptRoot\output\win-x64 "c:\program files\fmsync" /e
robocopy $PSScriptRoot\output\win-x64 "\\TOWER\Applications\fmsync\win-x64" /e
robocopy $PSScriptRoot\output\osx-x64 "\\TOWER\Applications\fmsync\osx-x64" /e