using GFramework.Core.Abstractions.StateManagement;

namespace GFramework.Core.StateManagement;

/// <summary>
///     Store 构建器的默认实现。
///     该类型用于在 Store 创建之前集中配置比较器、reducer 和中间件，适合模块安装和测试工厂场景。
/// </summary>
/// <typeparam name="TState">状态树的根状态类型。</typeparam>
public class StoreBuilder<TState> : IStoreBuilder<TState>
{
    /// <summary>
    ///     延迟应用到 Store 的配置操作列表。
    ///     采用延迟配置而不是直接缓存 reducer 适配器，可复用 Store 自身的注册和验证逻辑。
    /// </summary>
    private readonly List<Action<Store<TState>>> _configurators = [];

    /// <summary>
    ///     状态比较器。
    /// </summary>
    private IEqualityComparer<TState>? _comparer;

    /// <summary>
    ///     配置状态比较器。
    /// </summary>
    /// <param name="comparer">状态比较器。</param>
    /// <returns>当前构建器实例。</returns>
    public IStoreBuilder<TState> WithComparer(IEqualityComparer<TState> comparer)
    {
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        return this;
    }

    /// <summary>
    ///     添加一个强类型 reducer。
    /// </summary>
    /// <typeparam name="TAction">当前 reducer 处理的 action 类型。</typeparam>
    /// <param name="reducer">要添加的 reducer。</param>
    /// <returns>当前构建器实例。</returns>
    public IStoreBuilder<TState> AddReducer<TAction>(IReducer<TState, TAction> reducer)
    {
        ArgumentNullException.ThrowIfNull(reducer);
        _configurators.Add(store => store.RegisterReducer(reducer));
        return this;
    }

    /// <summary>
    ///     使用委托快速添加一个 reducer。
    /// </summary>
    /// <typeparam name="TAction">当前 reducer 处理的 action 类型。</typeparam>
    /// <param name="reducer">执行归约的委托。</param>
    /// <returns>当前构建器实例。</returns>
    public IStoreBuilder<TState> AddReducer<TAction>(Func<TState, TAction, TState> reducer)
    {
        ArgumentNullException.ThrowIfNull(reducer);
        _configurators.Add(store => store.RegisterReducer(reducer));
        return this;
    }

    /// <summary>
    ///     添加一个 Store 中间件。
    /// </summary>
    /// <param name="middleware">要添加的中间件。</param>
    /// <returns>当前构建器实例。</returns>
    public IStoreBuilder<TState> UseMiddleware(IStoreMiddleware<TState> middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _configurators.Add(store => store.UseMiddleware(middleware));
        return this;
    }

    /// <summary>
    ///     基于给定初始状态创建一个新的 Store。
    /// </summary>
    /// <param name="initialState">Store 的初始状态。</param>
    /// <returns>已应用当前构建器配置的 Store 实例。</returns>
    public IStore<TState> Build(TState initialState)
    {
        var store = new Store<TState>(initialState, _comparer);
        foreach (var configurator in _configurators)
        {
            configurator(store);
        }

        return store;
    }
}