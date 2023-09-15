namespace Elzik.FmSync.Application.Tests.Unit;

using Microsoft.Extensions.Logging;

// This class is necessary because mocking an ILogger will not work; further explanation here:
// https://github.com/nsubstitute/NSubstitute/issues/597#issuecomment-1081422618

public abstract class MockLogger<T> : ILogger<T>
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var unboxed = (IReadOnlyList<KeyValuePair<string, object>>)state!;
        var message = formatter(state, exception);

        Log();
        Log(logLevel, message);
        Log(logLevel, message, exception);
        Log(logLevel, unboxed.ToDictionary(k =>
            k.Key, v => v.Value));
        Log(logLevel, unboxed.ToDictionary(k => 
            k.Key, v => v.Value), exception);
    }

    public abstract void Log();

    public abstract void Log(LogLevel logLevel, string message);

    public abstract void Log(LogLevel logLevel, string message, Exception? exception);

    public abstract void Log(LogLevel logLevel, IDictionary<string, object> state);

    public abstract void Log(LogLevel logLevel, IDictionary<string, object> state, Exception? exception);

    public virtual bool IsEnabled(LogLevel logLevel) => true;

    public abstract IDisposable? BeginScope<TState>(TState state) where TState : notnull;
}