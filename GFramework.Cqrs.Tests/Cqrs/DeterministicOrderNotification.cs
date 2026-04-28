using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     用于验证同一通知的多个处理器是否按稳定顺序执行。
/// </summary>
internal sealed record DeterministicOrderNotification : INotification;
