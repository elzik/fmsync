using FluentAssertions;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Elzik.FmSync.Worker.Tests.Functional
{
    public class WorkerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Process? _workerProcess;
        private Func<DataReceivedEventArgs, bool> _validateConsoleOutput;

        public WorkerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper 
                ?? throw new ArgumentNullException(nameof(testOutputHelper));

            var outputDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var workerExecutablePath = Path.Join(outputDirectory, "Elzik.FmSync.Worker.exe");

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

            _validateConsoleOutput = (_) => throw new InvalidOperationException(
                "No console output validation implementation has been supplied.");
        }

        [Theory(Timeout = 5000)]
        [InlineData("has started.")]
        [InlineData("Configuring watcher on ../../../../TestFiles for new and changed *.md files.")]
        [InlineData("Watcher on ../../../../TestFiles has started.")]
        [InlineData("A total of 1 directory watchers are running.")]
        public async Task Test(string expectedLogOutput)
        {
            // Arrange
            _workerProcess!.OutputDataReceived += OnDataReceived;
            _workerProcess.ErrorDataReceived += OnDataReceived;
            _validateConsoleOutput = (DataReceivedEventArgs dataReceived) =>
            {
                return dataReceived.Data != null && 
                       dataReceived.Data.EndsWith(expectedLogOutput);
            };
            using var monitoredWorkerProcess = _workerProcess.Monitor();

            // Act
            _workerProcess!.Start();
            _workerProcess.BeginOutputReadLine();
            await _workerProcess.WaitForExitAsync();

            // Assert
            monitoredWorkerProcess.Should().Raise("OutputDataReceived")
                .WithArgs<DataReceivedEventArgs>(dataReceived =>_validateConsoleOutput(dataReceived));
        }

        
        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(e.Data != null)
            {
                _testOutputHelper.WriteLine(e.Data!);
            }

            if(_validateConsoleOutput(e))
            {
                _workerProcess!.Kill();
            }
        }
    }
}
