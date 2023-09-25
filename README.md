# fmsync
![fmsync](Images/fmsync-high-resolution-color-logo-reduced-height.png)

[![Build](https://img.shields.io/github/actions/workflow/status/elzik/fmsync/continuous-integration.yml?color=95BE1A)](https://github.com/elzik/mecon/actions/workflows/continuous-integration.yml)
[![Coverage](https://gist.githubusercontent.com/elzik/527882e89a938dc78f61a08c300edec4/raw/a38fa7f10fa009f3848ca9ec20f17b82c2057bb3/fmsync-code-coverage-main.svg)](https://gist.githubusercontent.com/elzik/527882e89a938dc78f61a08c300edec4/raw/a38fa7f10fa009f3848ca9ec20f17b82c2057bb3/fmsync-code-coverage-main.svg)
[![Code quality](https://img.shields.io/codacy/grade/3313621663794a6c81e6bde6136fcc36?color=95BE1A)](https://app.codacy.com/gh/elzik/fmsync/dashboard)
[![License](https://img.shields.io/github/license/elzik/fmsync)](https://github.com/elzik/fmsync/blob/regex-filters/LICENSE)
[![Release](https://img.shields.io/github/v/release/elzik/fmsync?display_name=tag&sort=semver)](https://github.com/elzik/fmsync/releases)

# Introduction

fmsync ensures that a Markdown file's created date is synchronised with the `created` date found in its Front Matter. It can do this either on the command line or by using a service configured to constantly watch for changes in Front Matter of files in one or more directories.

# Installation

There are not yet any official releases. To install on Windows:
1. Clone the repository
2. Run `/build/publish.ps1`
3. Run `/build/local-install.ps1`

You should add the command line app's path to your `PATH` environment variable: `C:\Program Files\fmsync\console`. The watcher service will be automatically started, watching the folder specified in its `appSettings.config` file.

# Command Line

## Usage

Execute FmSync passing a path to a directory which contains files you wish to recursively scan. For any Markdown files found, the file's created date will be updated to match that of the `created` date found in the file's Front Matter where one exists.

```powershell
fmsync c:\my-markdownfiles
```

## Logging

By default, only Information, Warnings and Errors are logged to both the console and to a file located in `C:\ProgramData\fmsync\Elzik.FmSync.ConsoleYYYYMMDD.log`. A new file will be created for each day that the tool is used and files older than 7 days are removed.

# Worker Service

## Usage

By running `/build/local-install.ps1`, FmSync will be running as a Windows service, watching all of the directories listed in `WatcherOptions:WatchedDirectoryPaths`. Each time a file is created or edited, FmSync will check if the Front Matter `created` date is the same as the underlying file's `created` date and then update the latter to match the former if necessary.

Instead of running the service as a Windows service, it can also be run as an executable or on the command line.

## Logging

By default, only Information, Warnings and Errors are logged to both the console (when started on the command line) and to a file located in `C:\ProgramData\fmsync\Elzik.FmSync.WorkerYYYYMMDD.log` (in all circumstances). A new file will be created for each day that the tool is used and files older than 7 days are removed.

# Configuration

FmSync is configured through a separate appSettings.json file for both the command line tool and the service which contains the following sections:

### WatcherOptions (Only applicable when running as a service)

This contains a single setting `WatchedDirectoryPaths` which contains an array of paths, one for each directory to watch for new and changed files which need their created date synchronising.

### Serilog
Logging is provided for by Serilog with the default behaviours as described in the [Logging](##Logging) section above. It is beyond the scope of this readme to document this configuration. See the [Serilog documentation](https://github.com/serilog/serilog-settings-configuration#readme) for general information about configuration. Currently, [Console](https://github.com/serilog/serilog-sinks-console#readme) and [File](https://github.com/serilog/serilog-sinks-file#readme) Sinks are implemented and more information can be found on their respective repos.

When opening any Github Issues, change the `MinimumLevel` configuration to `Debug` to increase the amount of information being logged.

### FrontMatterOptions

#### TimeZoneId

- This contains a single setting, `TimeZoneId`, which by default is empty. When this setting is empty are not present all `created` dates found in Front Matter sections at the beginning of a file will be considered as if they were in the current time zone settings for the machine the application is running on.
- Alternatively, `TimeZoneId` can be set to any time zone as specified in the `Timezone` column of [this documentation](https://learn.microsoft.com/en-us/windows-hardware/manufacture/desktop/default-time-zones). FmSync will then use this timezone when setting the created date on a file.
- If the date given in a file's Front Matter contains a time offset, the TimeZoneId given here will be ignored and the offset given will be taken into account when setting the created date on a file.

### FileSystemOptions

This contains a single setting, `FilenamePattern`, which by default is `*.md`. Only files matching this filter will be acted upon by FmSync.

