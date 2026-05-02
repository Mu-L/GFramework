using System;
using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Logging;
using Godot;

namespace GFramework.Godot.Logging;

/// <summary>
///     Godot platform logger implementation.
/// </summary>
public sealed class GodotLogger : AbstractLogger
{
    private readonly GodotLoggerOptions _options;

    /// <summary>
    ///     Initializes a logger that preserves the historical fixed-format template.
    /// </summary>
    /// <param name="name">The logger name.</param>
    /// <param name="minLevel">The minimum enabled log level.</param>
    public GodotLogger(string? name = null, LogLevel minLevel = LogLevel.Info)
        : this(name, GodotLoggerOptions.ForMinimumLevel(minLevel))
    {
    }

    /// <summary>
    ///     Initializes a logger with Godot-specific formatting options.
    /// </summary>
    /// <param name="name">The logger name.</param>
    /// <param name="options">The logger options.</param>
    public GodotLogger(string? name, GodotLoggerOptions options)
        : base(name ?? RootLoggerName, (options ?? throw new ArgumentNullException(nameof(options))).GetEffectiveMinLevel())
    {
        _options = options;
    }

    /// <summary>
    ///     Writes a log entry to Godot.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <param name="message">The rendered message body.</param>
    /// <param name="exception">The optional exception.</param>
    protected override void Write(LogLevel level, string message, Exception? exception)
    {
        var templateText = _options.Mode == GodotLoggerMode.Debug
            ? _options.DebugOutputTemplate
            : _options.ReleaseOutputTemplate;
        var context = new GodotLogRenderContext(
            DateTime.UtcNow,
            level,
            Name(),
            message,
            _options.GetColor(level));
        var rendered = GodotLogTemplate.Parse(templateText).Render(context);

        if (_options.Mode == GodotLoggerMode.Debug)
        {
            WriteDebug(level, rendered);
        }
        else
        {
            GD.Print(rendered);
        }

        if (exception != null)
        {
            GD.PrintErr(exception.ToString());
        }
    }

    private static void WriteDebug(LogLevel level, string rendered)
    {
        GD.PrintRich(rendered);

        switch (level)
        {
            case LogLevel.Fatal:
            case LogLevel.Error:
                GD.PushError(rendered);
                break;
            case LogLevel.Warning:
                GD.PushWarning(rendered);
                break;
        }
    }
}
