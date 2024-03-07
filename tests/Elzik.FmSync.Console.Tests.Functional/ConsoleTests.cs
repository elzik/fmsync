using FluentAssertions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Elzik.FmSync.Console.Tests.Functional
{
    public class ConsoleTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Process _consoleProcess;
        private const string FunctionalTestFilesPath = "../../../../TestFiles/Functional/Console";
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
            KillExistingWorkerProcesses(_buildOutputDirectory);

            var workerFilename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                       ? "fmsync.exe" : "fmsync";
            var workerExecutablePath = Path.Join(_buildOutputDirectory, workerFilename);
            _testOutputHelper.WriteLine("Worker under test: {0}", workerExecutablePath);
            _consoleProcess = new Process
            {
                StartInfo = new ProcessStartInfo(workerExecutablePath)
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            _consoleProcess.OutputDataReceived += OnConsoleDataReceivedLog;
            _consoleProcess.ErrorDataReceived += OnConsoleDataReceivedLog;

            Directory.CreateDirectory(FunctionalTestFilesPath);
        }
        [Fact]
        public async Task Synchronise_EmptyFolder_LogsToConsole()
        {
            // Arrange
            var consoleOutputLines = new List<string>();
            _consoleProcess.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    consoleOutputLines.Add(e.Data);
                }
            };

            // Act
            ValidateWorkerStart(_consoleProcess!.Start());
            _consoleProcess.BeginOutputReadLine();
            await _consoleProcess.WaitForExitAsync();

            // Assert
            var expectedWorkingDirectoryLogText = $"Synchronising *.md files in {_buildOutputDirectory}".TrimEnd('\\');
            _testOutputHelper.WriteLine($"expectedWorkingDirectoryLogText = {expectedWorkingDirectoryLogText}");
            consoleOutputLines.Should().Contain(line => line.EndsWith(expectedWorkingDirectoryLogText));
            consoleOutputLines.Should().Contain(line => line.Contains("Synchronised 0 files out of a total 0 in "));
        }

        private static void KillExistingWorkerProcesses(string? directoryPath)
        {
            var testWorkers = Process.GetProcessesByName("Elzik.FmSync.Worker")
                            .Where(p => p.MainModule!.FileName.StartsWith(directoryPath!));

            foreach (var testWorker in testWorkers)
            {
                testWorker.Kill();
            }
        }

        private void OnConsoleDataReceivedLog(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                _testOutputHelper.WriteLine(e.Data);
            }
        }

        private static void ValidateWorkerStart(bool workerStartResult)
        {
            if (!workerStartResult)
            {
                throw new InvalidOperationException("A new functional test Console process was not started.");
            }
        }
    }
}