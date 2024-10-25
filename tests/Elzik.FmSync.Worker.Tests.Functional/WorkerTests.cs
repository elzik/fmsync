using FluentAssertions;
using FluentAssertions.Events;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace Elzik.FmSync.Worker.Tests.Functional
{
    public sealed class WorkerTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Process _workerProcess;
        private Func<DataReceivedEventArgs, bool>? _expectedConsoleOutputReceived;
        private Func<FileSystemEventArgs, bool>? _expectedFileChangeMade;
        private readonly FileSystemWatcher _testFileWatcher;
        private const string FunctionalTestFilesPath = "../../../../TestFiles/Functional/Worker";
        private const string SerlogPathKey = "Serilog:WriteTo:1:Args:path";
        private readonly string LogPath;

        public WorkerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper
                ?? throw new ArgumentNullException(nameof(testOutputHelper));

            var buildOutputDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            KillExistingWorkerProcesses(buildOutputDirectory);

            var workerFilename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                       ? "Elzik.FmSync.Worker.exe" : "Elzik.FmSync.Worker";
            var workerExecutablePath = Path.Join(buildOutputDirectory, workerFilename);
            _testOutputHelper.WriteLine("Worker under test: {0}", workerExecutablePath);
            _workerProcess = new Process
            {
                StartInfo = new ProcessStartInfo(workerExecutablePath)
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            _workerProcess.OutputDataReceived += OnConsoleDataReceivedLog;
            _workerProcess.ErrorDataReceived += OnConsoleDataReceivedLog;

            Directory.CreateDirectory(FunctionalTestFilesPath);

            var config = GetIConfigurationRoot();
            var configurationSection = config.GetSection(SerlogPathKey);
            if (configurationSection == null || configurationSection.Value == null)
            {
                throw new InvalidOperationException($"No log file path set in appSettings at {SerlogPathKey}");
            }
            LogPath = configurationSection.Value;
            if (File.Exists(LogPath))
            {
                File.Delete(LogPath);
            }

            _testFileWatcher = new FileSystemWatcher(FunctionalTestFilesPath, "*.md")
            {
                EnableRaisingEvents = false,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime
            };
            _testFileWatcher.Changed += OnTestFileChanged;
            _testFileWatcher.Created += OnTestFileChanged;
        }

        [Theory(Timeout = 5000)]
        [InlineData("has started.")]
        [InlineData("Configuring watcher on ../../../../TestFiles for new and changed *.md files.")]
        [InlineData("Watcher on ../../../../TestFiles has started.")]
        [InlineData("A total of 1 directory watchers are running.")]
        public async Task WorkerIsStarted_ExpectedConsoleLogMessagesAreReceived(string expectedLogOutput)
        {
            // Arrange
            _workerProcess.OutputDataReceived += OnConsoleDataReceivedKillProcess;
            MonitorConsoleForOutput(expectedLogOutput);
            using var monitoredWorkerProcess = _workerProcess.Monitor();

            // Act
            ValidateWorkerStart(_workerProcess.Start());
            _workerProcess.BeginOutputReadLine();
            await _workerProcess.WaitForExitAsync();

            // Assert
            monitoredWorkerProcess.Should().Raise("OutputDataReceived")
                .WithArgs<DataReceivedEventArgs>(dataReceived => _expectedConsoleOutputReceived != null 
                                                              && _expectedConsoleOutputReceived(dataReceived));
        }

        [Fact(Timeout = 10000)]
        public async Task WorkerIsStarted_ExpectedFileLogMessagesAreReceived()
        {
            // Arrange
            _workerProcess.OutputDataReceived += OnConsoleDataReceivedKillProcess;
            MonitorConsoleForOutput("Hosting started");
            using var monitoredWorkerProcess = _workerProcess.Monitor();

            // Act
            ValidateWorkerStart(_workerProcess.Start());
            _workerProcess.BeginOutputReadLine();
            await _workerProcess.WaitForExitAsync();

            // Assert
            var logFileEntries = await File.ReadAllLinesAsync(LogPath);
            logFileEntries.Should().Contain(entry => entry.EndsWith("has started."));
            logFileEntries.Should().Contain(entry => entry.EndsWith("Configuring watcher on ../../../../TestFiles for new and changed *.md files."));
            logFileEntries.Should().Contain(entry => entry.EndsWith("Watcher on ../../../../TestFiles has started."));
            logFileEntries.Should().Contain(entry => entry.EndsWith("A total of 1 directory watchers are running."));
        }

        [Fact(Timeout = 15000)]
        public async Task FrontMatterIsUpdated_WithNewCreatedDate_FileCreatedDateIsUpdated()
        {
            // Arrange
            var testFile = GetHappyPathTestFile();
            MonitorFilesForChange(testFile);
            using var monitoredFileWatcher = _testFileWatcher.Monitor();

            // Act
            ValidateWorkerStart(_workerProcess.Start());
            _workerProcess.BeginOutputReadLine();
            _testFileWatcher.EnableRaisingEvents = true;

            await WaitForWorketToStart();

            _testOutputHelper.WriteLine("Performing test edit...");
            await File.AppendAllLinesAsync(testFile.Path, ["Test edit..."]);

            await _workerProcess.WaitForExitAsync();

            // Assert
            AssertFileWasChanged(testFile, monitoredFileWatcher);

        }

        [Fact(Timeout = 20000)]
        public async Task FrontMatterIsUpdated_WithNewCreatedDateAndLockedFile_FileCreatedDateIsEventuallyUpdated()
        {
            // Arrange
            var testFile = GetHappyPathTestFile();
            MonitorFilesForChange(testFile);
            using var monitoredFileWatcher = _testFileWatcher.Monitor();

            // Act
            ValidateWorkerStart(_workerProcess.Start());
            _workerProcess.BeginOutputReadLine();
            _testFileWatcher.EnableRaisingEvents = true;

            await WaitForWorketToStart();

            _testOutputHelper.WriteLine("Performing test edit...");
            await File.AppendAllLinesAsync(testFile.Path, ["Test edit..."]);

            await LockFileTemporarily(testFile.Path, 2000);

            await _workerProcess.WaitForExitAsync();

            // Assert
            AssertFileWasChanged(testFile, monitoredFileWatcher);

        }

        [Fact(Timeout = 20000)]
        public async Task FrontMatterIsUpdated_WithNewCreatedDateAfterInvalidCreatedDate_FileCreatedDateIsEventuallyUpdated()
        {
            // Arrange
            var testFile = GetHappyPathTestFile();
            MonitorFilesForChange(testFile);
            using var monitoredFileWatcher = _testFileWatcher.Monitor();

            var testOriginalFileContents = await File.ReadAllTextAsync(testFile.Path);
            var testInvalidatedDateContents = testOriginalFileContents.Replace(
                "created: 2023-01-07 14:28:22", "created: InvalidDate!-01-07 14:28:22");

            // Act
            ValidateWorkerStart(_workerProcess!.Start());
            _workerProcess.BeginOutputReadLine();
            _testFileWatcher!.EnableRaisingEvents = true;

            await WaitForWorketToStart();

            _testOutputHelper.WriteLine("Performing test edit of invalid date...");
            await File.WriteAllTextAsync(testFile.Path, testInvalidatedDateContents);

            await Task.Delay(2000);

            _testOutputHelper.WriteLine("Performing test edit of valid date...");
            await File.WriteAllTextAsync(testFile.Path, testOriginalFileContents);

            await _workerProcess.WaitForExitAsync();

            // Assert
            AssertFileWasChanged(testFile, monitoredFileWatcher);

        }

        private void AssertFileWasChanged((string Path, DateTime ExpectedCreatedDate) testFile, IMonitor<FileSystemWatcher> monitoredFileWatcher)
        {
            monitoredFileWatcher.Should().Raise("Changed").
                WithArgs<FileSystemEventArgs>(fileSystemEvent => _expectedFileChangeMade != null 
                                                              && _expectedFileChangeMade(fileSystemEvent));
            var testFileInfo = new FileInfo(testFile.Path);
            testFileInfo.CreationTimeUtc.Should().Be(testFile.ExpectedCreatedDate, "the Worker should have updated the created date " +
                "in response to a file edit");
            testFileInfo.LastWriteTimeUtc.Should().NotBe(testFile.ExpectedCreatedDate, "the Worker should not have updated the " +
                "modified date to be the same as the created date in response to a file edit");
        }

        private void MonitorConsoleForOutput(string expectedLogOutput)
        {
            _expectedConsoleOutputReceived = (DataReceivedEventArgs dataReceived) =>
            {
                return dataReceived.Data != null &&
                       dataReceived.Data.EndsWith(expectedLogOutput);
            };
        }

        private void MonitorFilesForChange((string Path, DateTime ExpectedCreatedDate) testFile)
        {
            _expectedFileChangeMade = (FileSystemEventArgs fileSystemEventArgs) =>
            {
                var eventFilePath = Path.GetFullPath(fileSystemEventArgs.FullPath);
                var eventFileInfo = new FileInfo(eventFilePath);

                var expectedChangeMade = eventFilePath == testFile.Path &&
                                       eventFileInfo.CreationTimeUtc == testFile.ExpectedCreatedDate;

                return expectedChangeMade;
            };
        }

        private static (string Path, DateTime ExpectedCreatedDate) GetHappyPathTestFile()
        {
            const string fileToCopyPath = "../../../../TestFiles/YamlContainsOnlyCreatedDate.md";
            var testFilePath = Path.GetFullPath(Path.Join(FunctionalTestFilesPath, $"{Guid.NewGuid()}.md"));
            File.Copy(fileToCopyPath, testFilePath, true);
            var expectedCreatedDate = new DateTime(2023, 01, 07, 14, 28, 22, DateTimeKind.Utc);
            return (testFilePath, expectedCreatedDate);
        }

        private static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                .Build();
        }

        private static async Task WaitForWorketToStart()
        {
            await Task.Delay(3000);
        }

        private async Task LockFileTemporarily(string path, int lockForMilliseconds)
        {
            _testOutputHelper.WriteLine("Locking file...");

            await using var testFileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            await Task.Delay(lockForMilliseconds);

            _testOutputHelper.WriteLine("Filestream about to go out of scope...");
        }

        private static void KillExistingWorkerProcesses(string? directoryPath)
        {
            var testWorkers = Process.GetProcessesByName("Elzik.FmSync.Worker")
                            .Where(p => p.MainModule != null && p.MainModule.FileName.StartsWith(directoryPath!));

            foreach (var testWorker in testWorkers)
            {
                if (!testWorker.HasExited)
                {
                    testWorker.Kill();
                }
            }
        }

        private static void ValidateWorkerStart(bool workerStartResult)
        {
            if(!workerStartResult)
            {
                throw new InvalidOperationException("A new functional test Worker process was not started.");
            }
        }

        private void OnConsoleDataReceivedLog(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                _testOutputHelper.WriteLine(e.Data);
            }
        }

        private void OnTestFileChanged(object sender, FileSystemEventArgs e)
        {
            _testOutputHelper.WriteLine("Test log: File {0} for {1}",e.ChangeType, e.FullPath);

            if (_expectedFileChangeMade != null && _expectedFileChangeMade(e))
            {
                _testOutputHelper.WriteLine("Test log: Expected files change made, killing Worker process...");
                if (!_workerProcess.HasExited)
                {
                    _workerProcess.Kill();
                }
            }
        }

        private void OnConsoleDataReceivedKillProcess(object sender, DataReceivedEventArgs e)
        {
            if(_expectedConsoleOutputReceived != null && _expectedConsoleOutputReceived(e))
            {
                _testOutputHelper.WriteLine("Test log: Expected console data received, killing Worker process...");
                if (!_workerProcess.HasExited)
                {
                    _workerProcess.Kill();
                }
            }
        }

        public void Dispose()
        {
            _workerProcess.Dispose();
            _testFileWatcher.Dispose();
            Directory.Delete(FunctionalTestFilesPath, true);
        }
    }
}
