Param(
    [Parameter(Mandatory=$True)]
    [String] $outputDirectory,
    [Parameter(Mandatory=$True)]
    [String] $operatingSystem
)

$ErrorActionPreference = "Stop"

if ($operatingSystem -eq 'Windows_NT')
{
    $platformSpecificLogPath = 'C:/ProgramData/Elzik/fmsync'
}
else
{
    $platformSpecificLogPath = '~/Library/Logs/Elzik/fmsync'
}

Write-Output "Platform specific log path: $platformSpecificLogPath"

$appSettingsPath = "$outputDirectory/appSettings.json"

Write-Output "appSettings Path: $appSettingsPath"

(Get-Content $appSettingsPath).Replace('[PLATFORM_SPECIFIC_LOG_PATH]', $platformSpecificLogPath) `
    | Set-Content $appSettingsPath