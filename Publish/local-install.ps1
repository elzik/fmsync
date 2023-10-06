# Install console app
robocopy $PSScriptRoot\output\Elzik.FmSync.Console\win-x64 "c:\program files\fmsync\console" /e

# Install worker service
Stop-Service -Name 'fmsync'
Remove-Service -Name 'fmsync'

robocopy $PSScriptRoot\output\Elzik.FmSync.Worker\win-x64 "c:\program files\fmsync\worker" /e

(get-content "c:\program files\fmsync\worker\appSettings.json") `
	| foreach-object {$_ -replace '"WatchedDirectoryPaths": \[\]', '"WatchedDirectoryPaths": ["C:\\Users\\justin.elzik\\Obsidian"]'} 
	| foreach-object {$_ -replace '"MinimumLevel": "Information"', '"MinimumLevel": "Debug"'} `
	| set-content "c:\program files\fmsync\worker\appSettings.json"

New-Service -Name 'fmsync' `
	-BinaryPathName 'c:\program files\fmsync\worker\Elzik.FmSync.Worker.exe' `
	-StartupType Automatic
Start-Service -Name 'fmsync'