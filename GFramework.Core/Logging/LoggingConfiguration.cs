using GFramework.Core.Abstractions.Logging;

namespace GFramework.Core.Logging;

/// <summary>
///     日志配置类
/// </summary>
public sealed class LoggingConfiguration
{
    /// <summary>
    ///     全局最小日志级别
    /// </summary>
    public LogLevel MinLevel { get; set; } = LogLevel.Info;

    /// <summary>
    ///     Appender 配置列表
    /// </summary>
    public IList<AppenderConfiguration> Appenders { get; set; } = new List<AppenderConfiguration>();

    /// <summary>
    ///     特定 Logger 的日志级别配置
    /// </summary>
    public IDictionary<string, LogLevel> LoggerLevels { get; set; } =
        new Dictionary<string, LogLevel>(StringComparer.Ordinal);
}
