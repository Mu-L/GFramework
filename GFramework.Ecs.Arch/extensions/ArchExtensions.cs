using GFramework.Core.Abstractions.architecture;
using Microsoft.Extensions.DependencyInjection;

namespace GFramework.Ecs.Arch.extensions;

/// <summary>
///     Arch ECS 扩展方法
/// </summary>
public static class ArchExtensions
{
    /// <summary>
    ///     配置 Arch ECS 选项
    /// </summary>
    public static IServiceCollection ConfigureArch(
        this IServiceCollection services,
        Action<ArchOptions> configure)
    {
        var options = new ArchOptions();
        configure(options);
        services.AddSingleton(options);
        return services;
    }

    /// <summary>
    ///     显式启用 Arch ECS 模块（备选方案）
    /// </summary>
    public static TArchitecture UseArch<TArchitecture>(this TArchitecture architecture)
        where TArchitecture : IArchitecture
    {
        // 此方法为显式注册提供支持
        // 实际注册由 ModuleInitializer 自动完成
        return architecture;
    }
}