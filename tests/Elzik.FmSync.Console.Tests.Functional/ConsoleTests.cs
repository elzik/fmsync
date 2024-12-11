using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace Elzik.FmSync.Console.Tests.Functional
{
    public class ConsoleTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Process _consoleProcess;
        private const string FunctionalTestFilesPath = "../../../../TestFiles/Functional/Console";
        private const string SerlogPathKey = "Serilog:WriteTo:1:Args:path";
        private readonly string _logPath;
        private readonly string _buildOutputDirectory;

        public ConsoleTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper
                ?? throw new ArgumentNullException(nameof(testOutputHelper));

            if(AppDomain.CurrentDomain.SetupInformation.ApplicationBase == null)
            {
                throw new InvalidOperationException("Unable to find build output directory.");
            }
            _buildOutputDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            KillExistingConsoleProcesses(_buildOutputDirectory);

            var consoleExecutableFilename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                       ? "fmsync.exe" : "fmsync";
            var consoleExecutablePath = Path.Join(_buildOutputDirectory, consoleExecutableFilename);
            _testOutputHelper.WriteLine("Console process under test: {0}", consoleExecutablePath);
            _consoleProcess = new Process
            {
                StartInfo = new ProcessStartInfo(consoleExecutablePath)
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            _consoleProcess.OutputDataReceived += OnConsoleDataReceivedLog;
            _consoleProcess.ErrorDataReceived += OnConsoleDataReceivedLog;

            Directory.CreateDirectory(FunctionalTestFilesPath);

            var config = GetIConfigurationRoot();
            var configurationSection = config.GetSection(SerlogPathKey);
            if (configurationSection == null || configurationSection.Value == null)
            {
                throw new InvalidOperationException($"No log file path set in appSettings at {SerlogPathKey}");
            }
            _logPath = configurationSection.Value;
            if (File.Exists(_logPath))
            {
                File.Delete(_logPath);
            }
        }

        [Fact]
        public async Task Synchronise_EmptyFolder_LogsToConsole()
        {
            // Arrange
            var consoleOutputLines = new List<string>();
            _consoleProcess.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    consoleOutputLines.Add(e.Data);
                }
            };

            // Act
            ValidateConsoleProcessStart(_consoleProcess!.Start());
            _consoleProcess.BeginOutputReadLine();
            await _consoleProcess.WaitForExitAsync();

            // Assert
            var expectedWorkingDirectoryLogText = $"Synchronising *.md files in {_buildOutputDirectory}".TrimEnd('\\','/');
            _testOutputHelper.WriteLine($"expectedWorkingDirectoryLogText = {expectedWorkingDirectoryLogText}");
            consoleOutputLines.Should().Contain(line => line.EndsWith(expectedWorkingDirectoryLogText));
            consoleOutputLines.Should().Contain(line => line.Contains("Synchronised 0 files out of a total 0 in "));
        }

        [Fact]
        public async Task Synchronise_EmptyFolder_LogsToFile()
        {
            // Act
            ValidateConsoleProcessStart(_consoleProcess!.Start());
            _consoleProcess.BeginOutputReadLine();
            await _consoleProcess.WaitForExitAsync();

            // Assert
            var fileLog = await File.ReadAllTextAsync(_logPath);
            var fileLogLines = fileLog.Split([Environment.NewLine], StringSplitOptions.None);
            var expectedWorkingDirectoryLogText = $"Synchronising *.md files in {_buildOutputDirectory}".TrimEnd('\\', '/');
            _testOutputHelper.WriteLine($"expectedWorkingDirectoryLogText = {expectedWorkingDirectoryLogText}");
            fileLogLines.Should().Contain(line => line.EndsWith(expectedWorkingDirectoryLogText));
            fileLogLines.Should().Contain(line => line.Contains("Synchronised 0 files out of a total 0 in "));
        }

        [Fact(Timeout = 5000)]
        public async Task ConsoleAppIsExecuted_WithMismatchingFrontMatterAndFileCreatedDates_FileCreatedDateIsUpdated()
        {
            // Arrange
            var testFiles = GetHappyPathTestFiles();

            _consoleProcess.StartInfo.Arguments = FunctionalTestFilesPath;

            // Act
            ValidateConsoleProcessStart(_consoleProcess!.Start());
            _consoleProcess.BeginOutputReadLine();
            await _consoleProcess.WaitForExitAsync();

            // Assert
            foreach(var testFile in testFiles)
            {
                var testFileInfo = new FileInfo(testFile.Path);
                testFileInfo.CreationTimeUtc.Should().Be(testFile.ExpectedCreatedDate, "the Console app should have updated the " +
                    "created date in response to a file edit");
                testFileInfo.LastWriteTimeUtc.Should().NotBe(testFile.ExpectedCreatedDate, "the Console app should not have " +
                    "updated the modified date to be the same as the created date in response to a file edit");
            }
        }

        private static (string Path, DateTime ExpectedCreatedDate) GetHappyPathTestFile()
        {
            const string fileToCopyPath = "../../../../TestFiles/YamlContainsOnlyCreatedDate.md";
            var testFilePath = Path.GetFullPath(Path.Join(FunctionalTestFilesPath, $"{Guid.NewGuid()}.md"));
            File.Copy(fileToCopyPath, testFilePath, true);
            var expectedCreatedDate = new DateTime(2023, 01, 07, 14, 28, 22, DateTimeKind.Utc);
            return (testFilePath, expectedCreatedDate);
        }

        private static (string Path, DateTime ExpectedCreatedDate)[] GetHappyPathTestFiles()
        {
            var testFiles = new (string Path, DateTime ExpectedCreatedDate)[3];

            for(int i = 0; i < 3; i++)
            {
                testFiles[i] = GetHappyPathTestFile();
            }

            return testFiles;
        }


        private static void KillExistingConsoleProcesses(string? directoryPath)
        {
            var testConsoleProcesses = Process.GetProcessesByName("Elzik.FmSync.Console")
                            .Where(p => p.MainModule!.FileName.StartsWith(directoryPath!));

            foreach (var testConsoleProcess in testConsoleProcesses)
            {
                if (!testConsoleProcess.HasExited)
                {
                    testConsoleProcess.Kill();
                }
            }
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        private void OnConsoleDataReceivedLog(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                _testOutputHelper.WriteLine(e.Data);
            }
        }

        private static void ValidateConsoleProcessStart(bool consoleProcessStartResult)
        {
            if (!consoleProcessStartResult)
            {
                throw new InvalidOperationException("A new functional test Console process was not started.");
            }
        }
    }
}