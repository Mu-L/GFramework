namespace GFramework.Core.Abstractions.architecture;

/// <summary>
///     架构模块注册表 - 用于外部模块的自动注册
/// </summary>
public static class ArchitectureModuleRegistry
{
    private static readonly List<Func<IServiceModule>> _factories = [];

    /// <summary>
    ///     注册模块工厂
    /// </summary>
    /// <param name="factory">模块工厂函数</param>
    public static void Register(Func<IServiceModule> factory)
    {
        _factories.Add(factory);
    }

    /// <summary>
    ///     创建所有已注册的模块实例
    /// </summary>
    /// <returns>模块实例集合</returns>
    public static IEnumerable<IServiceModule> CreateModules()
    {
        return _factories.Select(f => f());
    }

    /// <summary>
    ///     清空注册表（主要用于测试）
    /// </summary>
    public static void Clear()
    {
        _factories.Clear();
    }
}