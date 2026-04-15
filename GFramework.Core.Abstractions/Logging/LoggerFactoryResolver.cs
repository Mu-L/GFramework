namespace GFramework.Core.Abstractions.Logging;

/// <summary>
///     提供全局日志工厂访问入口。
/// </summary>
/// <remarks>
///     该类型位于抽象层，是为了让上层模块可以在不依赖 <c>GFramework.Core</c> 实现程序集的前提下
///     获取日志记录器。默认 provider 会优先通过反射解析 <c>GFramework.Core</c> 中的控制台实现，
///     若宿主未加载该程序集，则退回到静默 provider，避免抽象层形成实现层循环依赖。
/// </remarks>
public static class LoggerFactoryResolver
{
    private const string DefaultProviderTypeName =
        "GFramework.Core.Logging.ConsoleLoggerFactoryProvider, GFramework.Core";

    /// <summary>
    ///     获取或设置当前日志工厂提供程序。
    /// </summary>
    /// <exception cref="ArgumentNullException">
    ///     当赋值为 <see langword="null" /> 时抛出。
    /// </exception>
    public static ILoggerFactoryProvider Provider
    {
        get => field ??= CreateDefaultProvider();
        set => field = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     获取或设置新创建日志记录器的最小日志级别。
    /// </summary>
    /// <remarks>
    ///     该属性直接代理到当前 <see cref="Provider" />，确保调用方调整级别后立即影响后续创建的日志器。
    /// </remarks>
    public static LogLevel MinLevel
    {
        get => Provider.MinLevel;
        set => Provider.MinLevel = value;
    }

    private static ILoggerFactoryProvider CreateDefaultProvider()
    {
        if (Type.GetType(DefaultProviderTypeName, throwOnError: false) is { } providerType &&
            Activator.CreateInstance(providerType) is ILoggerFactoryProvider provider)
        {
            provider.MinLevel = LogLevel.Info;
            return provider;
        }

        return new SilentLoggerFactoryProvider();
    }

    /// <summary>
    ///     当宿主未提供默认日志实现时使用的静默 provider。
    /// </summary>
    private sealed class SilentLoggerFactoryProvider : ILoggerFactoryProvider
    {
        public LogLevel MinLevel { get; set; } = LogLevel.Info;

        public ILogger CreateLogger(string name)
        {
            return new SilentLogger(name);
        }
    }

    /// <summary>
    ///     默认日志实现不可用时的 no-op 日志器。
    /// </summary>
    private sealed class SilentLogger(string name) : ILogger
    {
        public string Name()
        {
            return name;
        }

        public bool IsTraceEnabled()
        {
            return false;
        }

        public bool IsDebugEnabled()
        {
            return false;
        }

        public bool IsInfoEnabled()
        {
            return false;
        }

        public bool IsWarnEnabled()
        {
            return false;
        }

        public bool IsErrorEnabled()
        {
            return false;
        }

        public bool IsFatalEnabled()
        {
            return false;
        }

        public bool IsEnabledForLevel(LogLevel level)
        {
            return false;
        }

        public void Trace(string msg)
        {
        }

        public void Trace(string format, object arg)
        {
        }

        public void Trace(string format, object arg1, object arg2)
        {
        }

        public void Trace(string format, params object[] arguments)
        {
        }

        public void Trace(string msg, Exception t)
        {
        }

        public void Debug(string msg)
        {
        }

        public void Debug(string format, object arg)
        {
        }

        public void Debug(string format, object arg1, object arg2)
        {
        }

        public void Debug(string format, params object[] arguments)
        {
        }

        public void Debug(string msg, Exception t)
        {
        }

        public void Info(string msg)
        {
        }

        public void Info(string format, object arg)
        {
        }

        public void Info(string format, object arg1, object arg2)
        {
        }

        public void Info(string format, params object[] arguments)
        {
        }

        public void Info(string msg, Exception t)
        {
        }

        public void Warn(string msg)
        {
        }

        public void Warn(string format, object arg)
        {
        }

        public void Warn(string format, object arg1, object arg2)
        {
        }

        public void Warn(string format, params object[] arguments)
        {
        }

        public void Warn(string msg, Exception t)
        {
        }

        public void Error(string msg)
        {
        }

        public void Error(string format, object arg)
        {
        }

        public void Error(string format, object arg1, object arg2)
        {
        }

        public void Error(string format, params object[] arguments)
        {
        }

        public void Error(string msg, Exception t)
        {
        }

        public void Fatal(string msg)
        {
        }

        public void Fatal(string format, object arg)
        {
        }

        public void Fatal(string format, object arg1, object arg2)
        {
        }

        public void Fatal(string format, params object[] arguments)
        {
        }

        public void Fatal(string msg, Exception t)
        {
        }

        public void Log(LogLevel level, string message)
        {
        }

        public void Log(LogLevel level, string format, object arg)
        {
        }

        public void Log(LogLevel level, string format, object arg1, object arg2)
        {
        }

        public void Log(LogLevel level, string format, params object[] arguments)
        {
        }

        public void Log(LogLevel level, string message, Exception exception)
        {
        }
    }
}
