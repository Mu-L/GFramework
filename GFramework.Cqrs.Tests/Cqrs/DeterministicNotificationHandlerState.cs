using System.Collections.Generic;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     记录确定性通知处理器的实际执行顺序。
/// </summary>
internal static class DeterministicNotificationHandlerState
{
    /// <summary>
    ///     获取当前测试中的通知处理器执行顺序。
    /// </summary>
    public static List<string> InvocationOrder { get; } = [];

    /// <summary>
    ///     重置共享的执行顺序状态。
    /// </summary>
    public static void Reset()
    {
        InvocationOrder.Clear();
    }
}
