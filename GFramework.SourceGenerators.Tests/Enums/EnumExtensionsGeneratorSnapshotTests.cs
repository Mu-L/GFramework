using System.IO;
using GFramework.Core.SourceGenerators.Enums;
using GFramework.SourceGenerators.Tests.Core;

namespace GFramework.SourceGenerators.Tests.Enums;

[TestFixture]
public class EnumExtensionsGeneratorSnapshotTests
{
    private const string EnumAttributeNamespace = "GFramework.Core.SourceGenerators.Abstractions.Enums";

    [Test]
    public async Task Snapshot_BasicEnum_IsMethods()
    {
        var source = BuildSource(
            """
            public enum Status
            {
                Active,
                Inactive,
                Pending
            }
            """);

        await GeneratorSnapshotTest<EnumExtensionsGenerator>.RunAsync(
            source,
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "enums",
                "snapshots",
                "EnumExtensionsGenerator",
                "BasicEnum_IsMethods"));
    }

    [Test]
    public async Task Snapshot_BasicEnum_IsInMethod()
    {
        var source = BuildSource(
            """
            public enum Status
            {
                Active,
                Inactive
            }
            """);

        await GeneratorSnapshotTest<EnumExtensionsGenerator>.RunAsync(
            source,
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "enums",
                "snapshots",
                "EnumExtensionsGenerator",
                "BasicEnum_IsInMethod"));
    }

    [Test]
    public async Task Snapshot_EnumWithFlagValues()
    {
        var source = BuildSource(
            """
            [Flags]
            public enum Permissions
            {
                None = 0,
                Read = 1,
                Write = 2,
                Execute = 4
            }
            """);

        await GeneratorSnapshotTest<EnumExtensionsGenerator>.RunAsync(
            source,
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "enums",
                "snapshots",
                "EnumExtensionsGenerator",
                "EnumWithFlagValues"));
    }

    [Test]
    public async Task Snapshot_DisableIsMethods()
    {
        var source = BuildSource(
            """
            public enum Status
            {
                Active,
                Inactive
            }
            """,
            "[GenerateEnumExtensions(GenerateIsMethods = false)]");

        await GeneratorSnapshotTest<EnumExtensionsGenerator>.RunAsync(
            source,
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "enums",
                "snapshots",
                "EnumExtensionsGenerator",
                "DisableIsMethods"));
    }

    [Test]
    public async Task Snapshot_DisableIsInMethod()
    {
        var source = BuildSource(
            """
            public enum Status
            {
                Active,
                Inactive
            }
            """,
            "[GenerateEnumExtensions(GenerateIsInMethod = false)]");

        await GeneratorSnapshotTest<EnumExtensionsGenerator>.RunAsync(
            source,
            Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "enums",
                "snapshots",
                "EnumExtensionsGenerator",
                "DisableIsInMethod"));
    }

    private static string BuildSource(string enumBody, string attributeUsage = "[GenerateEnumExtensions]")
    {
        return $$"""
                 using System;

                 namespace {{EnumAttributeNamespace}}
                 {
                     [AttributeUsage(AttributeTargets.Enum)]
                     public sealed class GenerateEnumExtensionsAttribute : Attribute
                     {
                         public bool GenerateIsMethods { get; set; } = true;
                         public bool GenerateIsInMethod { get; set; } = true;
                     }
                 }

                 namespace TestApp
                 {
                     using {{EnumAttributeNamespace}};

                     {{attributeUsage}}
                     {{enumBody}}
                 }
                 """;
    }
}
