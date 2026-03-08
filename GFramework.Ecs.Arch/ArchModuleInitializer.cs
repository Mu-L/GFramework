using System.Runtime.CompilerServices;
using GFramework.Core.Abstractions.architecture;

namespace GFramework.Ecs.Arch;

/// <summary>
///     Arch ECS 模块自动初始化器
///     使用 ModuleInitializer 特性在程序启动时自动注册模块
/// </summary>
public static class ArchModuleInitializer
{
    /// <summary>
    ///     模块初始化方法，在程序启动时自动调用
    /// </summary>
    [ModuleInitializer]
    public static void Initialize()
    {
        // 注册 Arch ECS 模块工厂
        ArchitectureModuleRegistry.Register(() => new ArchEcsModule(enabled: true));
    }
}