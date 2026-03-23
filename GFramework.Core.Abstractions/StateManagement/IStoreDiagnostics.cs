namespace GFramework.Core.Abstractions.StateManagement;

/// <summary>
///     暴露 Store 的诊断信息。
///     该接口用于调试、监控和后续时间旅行能力的扩展，不参与状态写入流程。
/// </summary>
/// <typeparam name="TState">状态树的根状态类型。</typeparam>
public interface IStoreDiagnostics<TState>
{
    /// <summary>
    ///     获取当前已注册的订阅者数量。
    /// </summary>
    int SubscriberCount { get; }

    /// <summary>
    ///     获取最近一次分发的 action 类型。
    ///     即使该次分发未引起状态变化，该值也会更新。
    /// </summary>
    Type? LastActionType { get; }

    /// <summary>
    ///     获取最近一次真正改变状态的时间戳。
    ///     若尚未发生状态变化，则返回 <see langword="null"/>。
    /// </summary>
    DateTimeOffset? LastStateChangedAt { get; }

    /// <summary>
    ///     获取最近一次分发记录。
    /// </summary>
    StoreDispatchRecord<TState>? LastDispatchRecord { get; }
}