using FluentAssertions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace Elzik.FmSync.Worker.Tests.Functional
{
    public sealed class WorkerTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Process? _workerProcess;
        private Func<DataReceivedEventArgs, bool>? _expectedConsoleOutputReceived;
        private Func<FileSystemEventArgs, bool>? _expectedFileChangeMade;
        private readonly FileSystemWatcher? _testFileWatcher;
        private const string FunctionalTestFilesPath = "../../../../TestFiles/Functional";

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
                    RedirectStandardError = true,
                    UseShellExecute = false,
                }
            };
            _workerProcess.OutputDataReceived += OnConsoleDataReceivedLog;
            _workerProcess.ErrorDataReceived += OnConsoleDataReceivedLog;

            Directory.CreateDirectory(FunctionalTestFilesPath);

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
        public async Task WorkerIsStarted_ExpectedLogMessagesAreReceived(string expectedLogOutput)
        {
            // Arrange
            _workerProcess!.OutputDataReceived += OnConsoleDataReceivedillProcess;
            _expectedConsoleOutputReceived = (DataReceivedEventArgs dataReceived) =>
            {
                return dataReceived.Data != null && 
                       dataReceived.Data.EndsWith(expectedLogOutput);
            };
            using var monitoredWorkerProcess = _workerProcess.Monitor();

            // Act
            ValidateWorkerStart(_workerProcess!.Start());
            _workerProcess.BeginOutputReadLine();
            await _workerProcess.WaitForExitAsync();

            // Assert
            monitoredWorkerProcess.Should().Raise("OutputDataReceived")
                .WithArgs<DataReceivedEventArgs>(dataReceived =>_expectedConsoleOutputReceived(dataReceived));
        }

        [Fact(Timeout = 15000)]
        public async Task FrontMatterIsUpdated_WithNewCreatedDate_FileCreatedDateIsUpdated()
        {
            // Arrange
            const string fileToCopyPath = "../../../../TestFiles/YamlContainsOnlyCreatedDate.md";
            var testFilePath = Path.GetFullPath(Path.Join(FunctionalTestFilesPath, $"{Guid.NewGuid()}.md"));
            File.Copy(fileToCopyPath, testFilePath, true);

            var expectedCreatedDate = new DateTime(2023, 01, 07, 14, 28, 22, DateTimeKind.Utc);
            _expectedFileChangeMade = (FileSystemEventArgs fileSystemEventArgs) =>
            {
                var eventFilePath = Path.GetFullPath(fileSystemEventArgs.FullPath);
                var eventFileInfo = new FileInfo(eventFilePath);
                
                var expectedChangeMade = eventFilePath == testFilePath &&
                                       eventFileInfo.CreationTimeUtc == expectedCreatedDate;

                return expectedChangeMade;
            };

            using var monitoredFileWatcher = _testFileWatcher.Monitor();

            // Act
            ValidateWorkerStart(_workerProcess!.Start());
            _workerProcess.BeginOutputReadLine();
            _testFileWatcher!.EnableRaisingEvents = true;

            await WaitForWorketToStart();

            _testOutputHelper.WriteLine("Performing test edit...");
            await File.AppendAllLinesAsync(testFilePath, ["Test edit..."]);

            await _workerProcess.WaitForExitAsync();

            // Assert
            monitoredFileWatcher.Should().Raise("Changed").
                WithArgs<FileSystemEventArgs>(fileSystemEvent => _expectedFileChangeMade(fileSystemEvent));
            var testFileInfo = new FileInfo(testFilePath);
            testFileInfo.CreationTimeUtc.Should().Be(expectedCreatedDate, "the Worker should have updated the created date " +
                "in response to a file edit");
            testFileInfo.LastWriteTimeUtc.Should().NotBe(expectedCreatedDate, "the Worker should not have updated the " +
                "modified date to be the same as the created date in response to a file edit");

        }

        [Fact(Timeout = 20000)]
        public async Task FrontMatterIsUpdated_WithNewCreatedDateAndLockedFile_FileCreatedDateIsEventuallyUpdated()
        {
            // Arrange
            const string fileToCopyPath = "../../../../TestFiles/YamlContainsOnlyCreatedDate.md";
            var testFilePath = Path.GetFullPath(Path.Join(FunctionalTestFilesPath, $"{Guid.NewGuid()}.md"));
            File.Copy(fileToCopyPath, testFilePath, true);

            var expectedCreatedDate = new DateTime(2023, 01, 07, 14, 28, 22, DateTimeKind.Utc);
            _expectedFileChangeMade = (FileSystemEventArgs fileSystemEventArgs) =>
            {
                var eventFilePath = Path.GetFullPath(fileSystemEventArgs.FullPath);
                var eventFileInfo = new FileInfo(eventFilePath);

                var expectedChangeMade = eventFilePath == testFilePath &&
                                       eventFileInfo.CreationTimeUtc == expectedCreatedDate;

                return expectedChangeMade;
            };

            using var monitoredFileWatcher = _testFileWatcher.Monitor();

            // Act
            ValidateWorkerStart(_workerProcess!.Start());
            _workerProcess.BeginOutputReadLine();
            _testFileWatcher!.EnableRaisingEvents = true;

            await WaitForWorketToStart();

            _testOutputHelper.WriteLine("Performing test edit...");
            await File.AppendAllLinesAsync(testFilePath, ["Test edit..."]);

            await LockFileTemporarily(testFilePath, 2000);

            await _workerProcess.WaitForExitAsync();

            // Assert
            monitoredFileWatcher.Should().Raise("Changed").
                WithArgs<FileSystemEventArgs>(fileSystemEvent => _expectedFileChangeMade(fileSystemEvent));
            var testFileInfo = new FileInfo(testFilePath);
            testFileInfo.CreationTimeUtc.Should().Be(expectedCreatedDate, "the Worker should have updated the created date " +
                "in response to a file edit");
            testFileInfo.LastWriteTimeUtc.Should().NotBe(expectedCreatedDate, "the Worker should not have updated the " +
                "modified date to be the same as the created date in response to a file edit");

        }

        private static async Task WaitForWorketToStart()
        {
            await Task.Delay(2000);
        }

        private async Task LockFileTemporarily(string path, int lockForMilliseconds)
        {
            _testOutputHelper.WriteLine("Locking file...");

            using var testFileStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            await Task.Delay(lockForMilliseconds);

            _testOutputHelper.WriteLine("Filestream about to go out of scope...");
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
                _workerProcess!.Kill();
            }
        }

        private void OnConsoleDataReceivedillProcess(object sender, DataReceivedEventArgs e)
        {
            if(_expectedConsoleOutputReceived != null && _expectedConsoleOutputReceived(e))
            {
                _testOutputHelper.WriteLine("Test log: Expected console data received, killing Worker process...");
                _workerProcess!.Kill();
            }
        }

        public void Dispose()
        {
            _workerProcess!.Dispose();
            _testFileWatcher!.Dispose();
            Directory.Delete(FunctionalTestFilesPath, true);
        }
    }
}
