using System;
using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Logging;

namespace GFramework.Godot.Logging;

/// <summary>
///     Provides cached Godot logger instances.
/// </summary>
public sealed class GodotLoggerFactoryProvider : ILoggerFactoryProvider
{
    private readonly ILoggerFactory _cachedFactory;

    /// <summary>
    ///     Initializes a Godot logger provider with the default logger factory.
    /// </summary>
    public GodotLoggerFactoryProvider()
    {
        _cachedFactory = CreateCachedFactory(new GodotLoggerFactory());
    }

    /// <summary>
    ///     Initializes a Godot logger provider with Godot-specific formatting options.
    /// </summary>
    /// <param name="options">The logger options.</param>
    public GodotLoggerFactoryProvider(GodotLoggerOptions options)
    {
        _cachedFactory = CreateCachedFactory(new GodotLoggerFactory(options));
    }

    /// <summary>
    ///     Gets or sets the provider minimum level.
    /// </summary>
    public LogLevel MinLevel { get; set; }

    /// <summary>
    ///     Creates a cached logger with the specified name.
    /// </summary>
    /// <param name="name">The logger name.</param>
    /// <returns>A logger configured with <see cref="MinLevel"/>.</returns>
    public ILogger CreateLogger(string name)
    {
        return _cachedFactory.GetLogger(name, MinLevel);
    }

    private static ILoggerFactory CreateCachedFactory(ILoggerFactory innerFactory)
    {
        ArgumentNullException.ThrowIfNull(innerFactory);
        return new CachedLoggerFactory(innerFactory);
    }
}
