Param(
    [Parameter(Mandatory=$True)]
    [String] $outputDirectory,
    [Parameter(Mandatory=$True)]
    [String] $operatingSystem
)

if ($operatingSystem -eq 'Windows_NT')
{
    $platformSpecificLogPath = 'C:/ProgramData/Elzik/fmsync'
}
else
{
    $platformSpecificLogPath = '~/Library/Logs/Elzik/fmsync'
}

$appSettingsPath = "$outputDirectory/appSettings.json"

(Get-Content $appSettingsPath).Replace('[PLATFORM_SPECIFIC_LOG_PATH]', $platformSpecificLogPath) `
    | Set-Content $appSettingsPath