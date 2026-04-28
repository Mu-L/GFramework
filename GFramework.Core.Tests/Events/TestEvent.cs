namespace GFramework.Core.Tests.Events;

/// <summary>
///     表示包含整型载荷的测试事件。
/// </summary>
public sealed class TestEvent
{
    /// <summary>
    ///     获取初始化阶段写入的接收值。
    /// </summary>
    public int ReceivedValue { get; init; }
}
