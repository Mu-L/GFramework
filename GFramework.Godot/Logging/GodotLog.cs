using System;
using System.Threading;
using GFramework.Core.Abstractions.Logging;

namespace GFramework.Godot.Logging;

/// <summary>
///     Static Godot logging entry point with auto-discovered configuration and deferred logger creation.
/// </summary>
public static class GodotLog
{
#if NET9_0_OR_GREATER
    private static readonly System.Threading.Lock ConfigureLock = new();
#else
    private static readonly object ConfigureLock = new();
#endif
    private static Action<GodotLoggerOptions>? _configure;

    private static readonly Lazy<GodotLogConfigurationSource> LazyConfigurationSource = new(
        CreateConfigurationSource,
        LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly Lazy<GodotLoggerFactoryProvider> LazyProvider = new(
        static () => new GodotLoggerFactoryProvider(() => LazyConfigurationSource.Value.CurrentSettings),
        LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    ///     Applies imperative option overrides before the global Godot logger provider is materialized.
    /// </summary>
    /// <param name="configure">The options mutator.</param>
    public static void Configure(Action<GodotLoggerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        lock (ConfigureLock)
        {
            if (LazyProvider.IsValueCreated)
            {
                throw new InvalidOperationException(
                    "GodotLog.Configure must be called before any GodotLog logger is materialized.");
            }

            _configure = configure;
        }
    }

    /// <summary>
    ///     Gets the lazily-configured Godot logger provider.
    /// </summary>
    public static ILoggerFactoryProvider Provider
    {
        get
        {
            lock (ConfigureLock)
            {
                return LazyProvider.Value;
            }
        }
    }

    /// <summary>
    ///     Gets the discovered configuration file path, if any.
    /// </summary>
    public static string? ConfigurationPath => LazyConfigurationSource.Value.ConfigurationPath;

    /// <summary>
    ///     Creates a logger for the specified category without materializing the provider until first use.
    /// </summary>
    public static ILogger CreateLogger(string category)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        return new DeferredLogger(category, static () => Provider);
    }

    /// <summary>
    ///     Creates a logger for the specified type without materializing the provider until first use.
    /// </summary>
    public static ILogger CreateLogger<T>()
    {
        return CreateLogger(GetCategoryName(typeof(T)));
    }

    /// <summary>
    ///     Installs the Godot provider as the current global resolver provider.
    /// </summary>
    public static void UseAsDefaultProvider()
    {
        LoggerFactoryResolver.Provider = Provider;
    }

    private static GodotLogConfigurationSource CreateConfigurationSource()
    {
        lock (ConfigureLock)
        {
            return new GodotLogConfigurationSource(_configure);
        }
    }

    private static string GetCategoryName(Type type)
    {
        return type.FullName ?? type.Name;
    }
}
