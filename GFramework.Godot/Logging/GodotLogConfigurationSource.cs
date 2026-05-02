using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GFramework.Core.Abstractions.Logging;

namespace GFramework.Godot.Logging;

internal sealed class GodotLogConfigurationSource : IDisposable
{
    private readonly Action<GodotLoggerOptions>? _configure;
    private readonly FileSystemWatcher? _watcher;
    private GodotLoggerSettings _currentSettings = GodotLoggerSettings.Default;

    public GodotLogConfigurationSource(Action<GodotLoggerOptions>? configure)
    {
        _configure = configure;
        ConfigurationPath = GodotLoggerSettingsLoader.DiscoverConfigurationPath();
        Reload(throwOnError: true);
        _watcher = CreateWatcher(ConfigurationPath);
    }

    public string? ConfigurationPath { get; }

    public GodotLoggerSettings CurrentSettings => Volatile.Read(ref _currentSettings);

    public void Dispose()
    {
        _watcher?.Dispose();
    }

    private FileSystemWatcher? CreateWatcher(string? configurationPath)
    {
        if (string.IsNullOrWhiteSpace(configurationPath))
        {
            return null;
        }

        var directory = Path.GetDirectoryName(configurationPath);
        var fileName = Path.GetFileName(configurationPath);
        if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var watcher = new FileSystemWatcher(directory, fileName)
        {
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.CreationTime
                           | NotifyFilters.FileName
                           | NotifyFilters.LastWrite
                           | NotifyFilters.Size
        };

        watcher.Changed += OnConfigurationChanged;
        watcher.Created += OnConfigurationChanged;
        watcher.Deleted += OnConfigurationChanged;
        watcher.Renamed += OnConfigurationRenamed;
        return watcher;
    }

    private void OnConfigurationChanged(object sender, FileSystemEventArgs e)
    {
        Reload(throwOnError: false);
    }

    private void OnConfigurationRenamed(object sender, RenamedEventArgs e)
    {
        Reload(throwOnError: false);
    }

    private void Reload(bool throwOnError)
    {
        try
        {
            var settings = LoadSettingsWithRetry();
            Volatile.Write(ref _currentSettings, settings);
        }
        catch when (!throwOnError)
        {
            // Ignore transient parse or file-lock failures during hot reload and keep the last good snapshot.
        }
    }

    private GodotLoggerSettings LoadSettingsWithRetry()
    {
        Exception? lastError = null;

        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                return LoadSettings();
            }
            catch (IOException ex)
            {
                lastError = ex;
            }
            catch (UnauthorizedAccessException ex)
            {
                lastError = ex;
            }

            Thread.Sleep(50);
        }

        throw lastError ?? new InvalidOperationException("Failed to load Godot logging configuration.");
    }

    private GodotLoggerSettings LoadSettings()
    {
        var settings = string.IsNullOrWhiteSpace(ConfigurationPath) || !File.Exists(ConfigurationPath)
            ? GodotLoggerSettings.Default
            : GodotLoggerSettingsLoader.LoadFromJsonFile(ConfigurationPath);

        if (_configure == null)
        {
            return settings;
        }

        var configuredOptions = CloneOptions(settings.Options);
        _configure(configuredOptions);
        return new GodotLoggerSettings(configuredOptions, settings.DefaultLogLevel, CopyLoggerLevels(settings));
    }

    private static GodotLoggerOptions CloneOptions(GodotLoggerOptions options)
    {
        return new GodotLoggerOptions
        {
            Mode = options.Mode,
            DebugMinLevel = options.DebugMinLevel,
            ReleaseMinLevel = options.ReleaseMinLevel,
            DebugOutputTemplate = options.DebugOutputTemplate,
            ReleaseOutputTemplate = options.ReleaseOutputTemplate,
            Colors = new Dictionary<GFramework.Core.Abstractions.Logging.LogLevel, string>(options.Colors)
        };
    }

    private static IReadOnlyDictionary<string, LogLevel> CopyLoggerLevels(
        GodotLoggerSettings settings)
    {
        var levels = new Dictionary<string, LogLevel>(StringComparer.Ordinal);
        foreach (var pair in settings.LoggerLevels)
        {
            levels[pair.Key] = pair.Value;
        }

        return levels;
    }
}
