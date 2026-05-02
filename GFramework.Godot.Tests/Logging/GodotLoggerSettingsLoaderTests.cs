using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GFramework.Core.Abstractions.Logging;
using GFramework.Godot.Logging;

namespace GFramework.Godot.Tests.Logging;

[TestFixture]
public sealed class GodotLoggerSettingsLoaderTests
{
    [Test]
    public void DiscoverConfigurationPath_Should_Prefer_EnvironmentVariable_Then_ProcessPath_Then_ProjectPath()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var envPath = Path.Combine(root, "env.json");
            File.WriteAllText(envPath, "{}");

            var executableDirectory = Path.Combine(root, "bin");
            Directory.CreateDirectory(executableDirectory);
            var processPath = Path.Combine(executableDirectory, "game.exe");
            var executableConfigPath = Path.Combine(executableDirectory, "appsettings.json");
            File.WriteAllText(executableConfigPath, "{}");

            var projectPath = Path.Combine(root, "project-appsettings.json");
            File.WriteAllText(projectPath, "{}");

            var discoveredFromEnvironment = GodotLoggerSettingsLoader.DiscoverConfigurationPath(
                environmentPath: envPath,
                processPath: processPath,
                projectPathResolver: _ => projectPath);
            var discoveredFromProcess = GodotLoggerSettingsLoader.DiscoverConfigurationPath(
                environmentPath: Path.Combine(root, "missing-env.json"),
                processPath: processPath,
                projectPathResolver: _ => projectPath);
            var discoveredFromProject = GodotLoggerSettingsLoader.DiscoverConfigurationPath(
                environmentPath: Path.Combine(root, "missing-env.json"),
                processPath: Path.Combine(root, "missing", "game.exe"),
                projectPathResolver: _ => projectPath);

            Assert.That(discoveredFromEnvironment, Is.EqualTo(envPath));
            Assert.That(discoveredFromProcess, Is.EqualTo(executableConfigPath));
            Assert.That(discoveredFromProject, Is.EqualTo(projectPath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Test]
    public void LoadFromJsonString_Should_Read_GodotLogger_Options_And_Category_Levels()
    {
        const string json = """
                            {
                              "Logging": {
                                "LogLevel": {
                                  "Default": "Warning",
                                  "Game.Services": "Error"
                                },
                                "GodotLogger": {
                                  "Mode": "Release",
                                  "DebugMinLevel": "Debug",
                                  "ReleaseMinLevel": "Information",
                                  "DebugOutputTemplate": "[dbg] {message}",
                                  "ReleaseOutputTemplate": "[rel] {message}{properties}",
                                  "Colors": {
                                    "Info": "aqua"
                                  }
                                }
                              }
                            }
                            """;

        var settings = GodotLoggerSettingsLoader.LoadFromJsonString(json);

        Assert.Multiple(() =>
        {
            Assert.That(settings.Options.Mode, Is.EqualTo(GodotLoggerMode.Release));
            Assert.That(settings.Options.ReleaseOutputTemplate, Is.EqualTo("[rel] {message}{properties}"));
            Assert.That(settings.Options.GetColor(LogLevel.Info), Is.EqualTo("aqua"));
            Assert.That(settings.GetEffectiveMinLevel("Game.Services.Inventory", LogLevel.Trace), Is.EqualTo(LogLevel.Error));
            Assert.That(settings.GetEffectiveMinLevel("Game.Other", LogLevel.Trace), Is.EqualTo(LogLevel.Warning));
        });
    }

    [Test]
    public void LoadFromJsonString_Should_Normalize_Null_GodotLogger_Options()
    {
        const string json = """
                            {
                              "Logging": {
                                "GodotLogger": {
                                  "DebugOutputTemplate": null,
                                  "ReleaseOutputTemplate": null,
                                  "Colors": null
                                }
                              }
                            }
                            """;

        var settings = GodotLoggerSettingsLoader.LoadFromJsonString(json);

        Assert.Multiple(() =>
        {
            Assert.That(settings.Options.DebugOutputTemplate, Is.Not.Null.And.Not.Empty);
            Assert.That(settings.Options.ReleaseOutputTemplate, Is.Not.Null.And.Not.Empty);
            Assert.That(settings.Options.GetColor(LogLevel.Info), Is.EqualTo("white"));
            Assert.That(settings.Options.GetColor(LogLevel.Error), Is.EqualTo("red"));
        });
    }

    [Test]
    public void LoadFromJsonString_Should_Reject_Invalid_Numeric_LogLevel()
    {
        const string json = """
                            {
                              "Logging": {
                                "LogLevel": {
                                  "Default": 999
                                }
                              }
                            }
                            """;

        var error = Assert.Throws<JsonException>(() => GodotLoggerSettingsLoader.LoadFromJsonString(json));

        Assert.That(error?.Message, Does.Contain("Unsupported numeric LogLevel value '999'"));
    }

    [Test]
    public void Provider_Should_Apply_Updated_Settings_To_Existing_Loggers()
    {
        var settings = new GodotLoggerSettings(new GodotLoggerOptions
        {
            Mode = GodotLoggerMode.Debug,
            DebugMinLevel = LogLevel.Error,
            ReleaseMinLevel = LogLevel.Error
        });
        var provider = new GodotLoggerFactoryProvider(() => settings);
        var logger = provider.CreateLogger("Game.Services.Inventory");

        Assert.That(logger.IsInfoEnabled(), Is.False);

        settings = new GodotLoggerSettings(
            new GodotLoggerOptions
            {
                Mode = GodotLoggerMode.Debug,
                DebugMinLevel = LogLevel.Info,
                ReleaseMinLevel = LogLevel.Info
            },
            defaultLogLevel: LogLevel.Trace,
            loggerLevels: new Dictionary<string, LogLevel>(StringComparer.Ordinal)
            {
                ["Game.Services"] = LogLevel.Debug
            });

        Assert.Multiple(() =>
        {
            Assert.That(logger.IsInfoEnabled(), Is.True);
            Assert.That(logger.IsDebugEnabled(), Is.False);
        });
    }
}
