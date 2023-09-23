# Install console app
robocopy $PSScriptRoot\output\Elzik.FmSync.Console\win-x64 "c:\program files\fmsync\console" /e

# Install worker service
Stop-Service -Name 'fmsync'
Remove-Service -Name 'fmsync'

robocopy $PSScriptRoot\output\Elzik.FmSync.Worker\win-x64 "c:\program files\fmsync\worker" /e

New-Service -Name 'fmsync' `
	-BinaryPathName 'c:\program files\fmsync\worker\Elzik.FmSync.Worker.exe' `
	-StartupType Automatic
Start-Service -Name 'fmsync'