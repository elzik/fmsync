# fmsync
![fmsync](Images/fmsync-high-resolution-color-logo-reduced-height.png)

[![Build](https://img.shields.io/github/actions/workflow/status/elzik/fmsync/continuous-integration.yml?color=95BE1A)](https://github.com/elzik/mecon/actions/workflows/continuous-integration.yml)
[![Coverage](https://gist.githubusercontent.com/elzik/527882e89a938dc78f61a08c300edec4/raw/a38fa7f10fa009f3848ca9ec20f17b82c2057bb3/fmsync-code-coverage-main.svg)](https://gist.githubusercontent.com/elzik/527882e89a938dc78f61a08c300edec4/raw/a38fa7f10fa009f3848ca9ec20f17b82c2057bb3/fmsync-code-coverage-main.svg)
[![Code quality](https://img.shields.io/codacy/grade/3313621663794a6c81e6bde6136fcc36?color=95BE1A)](https://app.codacy.com/gh/elzik/fmsync/dashboard)
[![License](https://img.shields.io/github/license/elzik/fmsync)](https://github.com/elzik/fmsync/blob/regex-filters/LICENSE)
[![Release](https://img.shields.io/github/v/release/elzik/fmsync?display_name=tag&sort=semver)](https://github.com/elzik/fmsync/releases)

Ensure that a Markdown file's created date is synchronised with the created-at date found in its Front Matter

## Usage

Execute FmSync passing a path to a directory which contains files you wish to recursively scan. For any Markdown (*.md) files found, the file's created date will be updated to match that of the `created` date found in the file's Front Matter where one exists.

```powershell
fmsync c:\my-markdownfiles
```

## Configuration

FmSync is configured through its appSettings.json file which contains the following sections:

### Logging
By default, logging is implemented using a single-line simple console logger with a log level of `Information`. This can be reconfigured in many ways. However, this configuration is not in the scope of this documentation; instead, refer to [Microsoft's documentation for Console logging and its various options](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line). 

### FrontMatterOptions
This contains a single setting, `TimeZoneId`, which by default is empty. When this setting is empty are not present all `created` dates found in Front Matter sections at the beginning of a file will be considered as if they were in the current time zone settings for the machine the application is running on.

Alternatively, `TimeZoneId` can be set to any time zone as specified in the `Timezone` column of [this documentation](https://learn.microsoft.com/en-us/windows-hardware/manufacture/desktop/default-time-zones). FmSync will then use this timezone when setting the created date on a file.

If the date given in a file's Front Matter contains a time offset, the TimeZoneId given here will be ignored and the offset given will be taken into account when setting the created date on a file.

