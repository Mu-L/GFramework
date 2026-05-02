using System;
using System.Linq;
using System.Threading;
using GFramework.Core.Abstractions.Logging;

namespace GFramework.Godot.Logging;

/// <summary>
///     Defers resolving the real Godot logger until the first logging operation needs it.
/// </summary>
/// <remarks>
///     This wrapper allows static logger fields to be created before <see cref="GodotLog.Configure"/> or
///     <see cref="GodotLog.UseAsDefaultProvider"/> runs. The resolved inner logger is published with an atomic compare
///     exchange so concurrent first-use calls converge on one cached instance without relying on the non-atomic
///     null-coalescing assignment pattern.
/// </remarks>
internal sealed class DeferredLogger(string category, Func<ILoggerFactoryProvider> providerAccessor) : IStructuredLogger
{
    private ILogger? _inner;

    private ILogger Inner
    {
        get
        {
            var current = Volatile.Read(ref _inner);
            if (current != null)
            {
                return current;
            }

            var createdLogger = ResolveLogger();

            // Multiple callers can resolve concurrently; only one publishes the cached reference.
            return Interlocked.CompareExchange(ref _inner, createdLogger, null) ?? createdLogger;
        }
    }

    public string Name()
    {
        return Inner.Name();
    }

    public bool IsTraceEnabled()
    {
        return Inner.IsTraceEnabled();
    }

    public bool IsDebugEnabled()
    {
        return Inner.IsDebugEnabled();
    }

    public bool IsInfoEnabled()
    {
        return Inner.IsInfoEnabled();
    }

    public bool IsWarnEnabled()
    {
        return Inner.IsWarnEnabled();
    }

    public bool IsErrorEnabled()
    {
        return Inner.IsErrorEnabled();
    }

    public bool IsFatalEnabled()
    {
        return Inner.IsFatalEnabled();
    }

    public bool IsEnabledForLevel(LogLevel level)
    {
        return Inner.IsEnabledForLevel(level);
    }

    public void Trace(string msg)
    {
        Inner.Trace(msg);
    }

    public void Trace(string format, object arg)
    {
        Inner.Trace(format, arg);
    }

    public void Trace(string format, object arg1, object arg2)
    {
        Inner.Trace(format, arg1, arg2);
    }

    public void Trace(string format, params object[] arguments)
    {
        Inner.Trace(format, arguments);
    }

    public void Trace(string msg, Exception t)
    {
        Inner.Trace(msg, t);
    }

    public void Debug(string msg)
    {
        Inner.Debug(msg);
    }

    public void Debug(string format, object arg)
    {
        Inner.Debug(format, arg);
    }

    public void Debug(string format, object arg1, object arg2)
    {
        Inner.Debug(format, arg1, arg2);
    }

    public void Debug(string format, params object[] arguments)
    {
        Inner.Debug(format, arguments);
    }

    public void Debug(string msg, Exception t)
    {
        Inner.Debug(msg, t);
    }

    public void Info(string msg)
    {
        Inner.Info(msg);
    }

    public void Info(string format, object arg)
    {
        Inner.Info(format, arg);
    }

    public void Info(string format, object arg1, object arg2)
    {
        Inner.Info(format, arg1, arg2);
    }

    public void Info(string format, params object[] arguments)
    {
        Inner.Info(format, arguments);
    }

    public void Info(string msg, Exception t)
    {
        Inner.Info(msg, t);
    }

    public void Warn(string msg)
    {
        Inner.Warn(msg);
    }

    public void Warn(string format, object arg)
    {
        Inner.Warn(format, arg);
    }

    public void Warn(string format, object arg1, object arg2)
    {
        Inner.Warn(format, arg1, arg2);
    }

    public void Warn(string format, params object[] arguments)
    {
        Inner.Warn(format, arguments);
    }

    public void Warn(string msg, Exception t)
    {
        Inner.Warn(msg, t);
    }

    public void Error(string msg)
    {
        Inner.Error(msg);
    }

    public void Error(string format, object arg)
    {
        Inner.Error(format, arg);
    }

    public void Error(string format, object arg1, object arg2)
    {
        Inner.Error(format, arg1, arg2);
    }

    public void Error(string format, params object[] arguments)
    {
        Inner.Error(format, arguments);
    }

    public void Error(string msg, Exception t)
    {
        Inner.Error(msg, t);
    }

    public void Fatal(string msg)
    {
        Inner.Fatal(msg);
    }

    public void Fatal(string format, object arg)
    {
        Inner.Fatal(format, arg);
    }

    public void Fatal(string format, object arg1, object arg2)
    {
        Inner.Fatal(format, arg1, arg2);
    }

    public void Fatal(string format, params object[] arguments)
    {
        Inner.Fatal(format, arguments);
    }

    public void Fatal(string msg, Exception t)
    {
        Inner.Fatal(msg, t);
    }

    public void Log(LogLevel level, string message)
    {
        LogFallback(level, message, exception: null);
    }

    public void Log(LogLevel level, string format, object arg)
    {
        LogFallback(level, string.Format(format, arg), exception: null);
    }

    public void Log(LogLevel level, string format, object arg1, object arg2)
    {
        LogFallback(level, string.Format(format, arg1, arg2), exception: null);
    }

    public void Log(LogLevel level, string format, params object[] arguments)
    {
        LogFallback(level, string.Format(format, arguments), exception: null);
    }

    public void Log(LogLevel level, string message, Exception exception)
    {
        LogFallback(level, message, exception);
    }

    public void Log(LogLevel level, string message, params (string Key, object? Value)[] properties)
    {
        if (Inner is IStructuredLogger structuredLogger)
        {
            structuredLogger.Log(level, message, properties);
            return;
        }

        LogFallback(level, message, exception: null, properties);
    }

    public void Log(LogLevel level, string message, Exception? exception, params (string Key, object? Value)[] properties)
    {
        if (Inner is IStructuredLogger structuredLogger)
        {
            structuredLogger.Log(level, message, exception, properties);
            return;
        }

        LogFallback(level, message, exception, properties);
    }

    private ILogger ResolveLogger()
    {
        return providerAccessor().CreateLogger(category);
    }

    private void LogFallback(
        LogLevel level,
        string message,
        Exception? exception,
        params (string Key, object? Value)[] properties)
    {
        var suffix = properties.Length == 0
            ? string.Empty
            : " | " + string.Join(", ", properties.Select(static property => $"{property.Key}={property.Value}"));
        var rendered = message + suffix;

        switch (level)
        {
            case LogLevel.Trace:
                WriteFallback(rendered, exception, Inner.Trace, Inner.Trace);
                break;
            case LogLevel.Debug:
                WriteFallback(rendered, exception, Inner.Debug, Inner.Debug);
                break;
            case LogLevel.Info:
                WriteFallback(rendered, exception, Inner.Info, Inner.Info);
                break;
            case LogLevel.Warning:
                WriteFallback(rendered, exception, Inner.Warn, Inner.Warn);
                break;
            case LogLevel.Error:
                WriteFallback(rendered, exception, Inner.Error, Inner.Error);
                break;
            case LogLevel.Fatal:
                WriteFallback(rendered, exception, Inner.Fatal, Inner.Fatal);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, "Unsupported log level.");
        }
    }

    private void LogFallback(LogLevel level, string message, Exception? exception)
    {
        LogFallback(level, message, exception, []);
    }

    private static void WriteFallback(
        string message,
        Exception? exception,
        Action<string> writeMessage,
        Action<string, Exception> writeException)
    {
        if (exception == null)
        {
            writeMessage(message);
        }
        else
        {
            writeException(message, exception);
        }
    }
}
