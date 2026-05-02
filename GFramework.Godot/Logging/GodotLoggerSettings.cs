using System;
using System.Collections.Generic;
using GFramework.Core.Abstractions.Logging;

namespace GFramework.Godot.Logging;

internal sealed class GodotLoggerSettings
{
    private readonly IReadOnlyDictionary<string, LogLevel> _loggerLevels;

    public static GodotLoggerSettings Default { get; } = new(new GodotLoggerOptions());

    public GodotLoggerSettings(
        GodotLoggerOptions options,
        LogLevel? defaultLogLevel = null,
        IReadOnlyDictionary<string, LogLevel>? loggerLevels = null)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        DefaultLogLevel = defaultLogLevel;
        _loggerLevels = loggerLevels ?? new Dictionary<string, LogLevel>(StringComparer.Ordinal);
    }

    public LogLevel? DefaultLogLevel { get; }

    public GodotLoggerOptions Options { get; }

    public IReadOnlyDictionary<string, LogLevel> LoggerLevels => _loggerLevels;

    public static GodotLoggerSettings FromOptions(GodotLoggerOptions options)
    {
        return new GodotLoggerSettings(options);
    }

    public LogLevel GetEffectiveMinLevel(string categoryName, LogLevel providerMinLevel)
    {
        ArgumentNullException.ThrowIfNull(categoryName);

        var effective = Max(Options.GetEffectiveMinLevel(), providerMinLevel);
        var configuredLevel = GetConfiguredMinLevel(categoryName);
        return configuredLevel.HasValue ? Max(effective, configuredLevel.Value) : effective;
    }

    private LogLevel? GetConfiguredMinLevel(string categoryName)
    {
        if (_loggerLevels.TryGetValue(categoryName, out var exactLevel))
        {
            return exactLevel;
        }

        var bestMatchLength = -1;
        LogLevel? bestMatchLevel = DefaultLogLevel;

        foreach (var pair in _loggerLevels)
        {
            if (!categoryName.StartsWith(pair.Key + ".", StringComparison.Ordinal))
            {
                continue;
            }

            if (pair.Key.Length <= bestMatchLength)
            {
                continue;
            }

            bestMatchLength = pair.Key.Length;
            bestMatchLevel = pair.Value;
        }

        return bestMatchLevel;
    }

    private static LogLevel Max(LogLevel left, LogLevel right)
    {
        return left > right ? left : right;
    }
}
