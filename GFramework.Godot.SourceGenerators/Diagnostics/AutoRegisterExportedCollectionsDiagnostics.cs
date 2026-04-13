using GFramework.SourceGenerators.Common.Constants;

namespace GFramework.Godot.SourceGenerators.Diagnostics;

/// <summary>
///     定义导出集合自动注册生成器使用的诊断描述符。
/// </summary>
/// <remarks>
///     这些规则用于在源生成阶段验证集合成员、注册目标以及元素类型推导，
///     避免把配置错误延后到生成代码编译或运行时才暴露。
/// </remarks>
internal static class AutoRegisterExportedCollectionsDiagnostics
{
    private const string Category = $"{PathContests.GodotNamespace}.SourceGenerators.Registration";

    /// <summary>
    ///     报告自动注册生成器不支持嵌套类型。
    /// </summary>
    public static readonly DiagnosticDescriptor NestedClassNotSupported = new(
        "GF_AutoExport_001",
        "AutoRegisterExportedCollections does not support nested classes",
        "AutoRegisterExportedCollections does not support nested class '{0}'",
        Category,
        DiagnosticSeverity.Error,
        true);

    /// <summary>
    ///     报告特性引用的注册表成员在宿主类型上不存在。
    /// </summary>
    public static readonly DiagnosticDescriptor RegistryMemberNotFound = new(
        "GF_AutoExport_002",
        "Registry member was not found",
        "Member '{0}' referenced by exported collection '{1}' was not found on '{2}'",
        Category,
        DiagnosticSeverity.Error,
        true);

    /// <summary>
    ///     报告注册表上未找到与集合元素类型兼容的注册方法。
    /// </summary>
    public static readonly DiagnosticDescriptor RegisterMethodNotFound = new(
        "GF_AutoExport_003",
        "Register method was not found",
        "Method '{0}' was not found on registry member '{1}' for exported collection '{2}'",
        Category,
        DiagnosticSeverity.Error,
        true);

    /// <summary>
    ///     报告被标记成员不是可枚举集合，因此无法执行批量注册。
    /// </summary>
    public static readonly DiagnosticDescriptor CollectionTypeMustBeEnumerable = new(
        "GF_AutoExport_004",
        "Exported collection must be enumerable",
        "Member '{0}' must be enumerable to use RegisterExportedCollection",
        Category,
        DiagnosticSeverity.Error,
        true);

    /// <summary>
    ///     报告集合元素类型无法在编译期推导，因此无法安全匹配注册方法。
    /// </summary>
    public static readonly DiagnosticDescriptor CollectionElementTypeCouldNotBeInferred = new(
        "GF_AutoExport_005",
        "Exported collection element type could not be inferred",
        "Member '{0}' must expose a generic enumerable element type to use RegisterExportedCollection safely",
        Category,
        DiagnosticSeverity.Error,
        true);
}
