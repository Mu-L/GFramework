namespace GFramework.Core.Abstractions.StateManagement;

/// <summary>
///     可写状态容器接口，提供统一的状态分发入口。
///     所有状态变更都应通过分发 action 触发，以保持单向数据流和可测试性。
/// </summary>
/// <typeparam name="TState">状态树的根状态类型。</typeparam>
public interface IStore<out TState> : IReadonlyStore<TState>
{
    /// <summary>
    ///     分发一个 action 以触发状态演进。
    ///     Store 会按注册顺序执行与该 action 类型匹配的 reducer，并在状态变化后通知订阅者。
    /// </summary>
    /// <typeparam name="TAction">action 的具体类型。</typeparam>
    /// <param name="action">要分发的 action 实例。</param>
    void Dispatch<TAction>(TAction action);
}