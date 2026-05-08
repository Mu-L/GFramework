// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs.Notification;

/// <summary>
///     以内置顺序策略逐个分发通知处理器。
/// </summary>
/// <remarks>
///     <para>该实现完整保留默认 CQRS runtime 的既有通知语义：按已解析顺序逐个执行处理器。</para>
///     <para>当任意处理器抛出异常时，后续处理器不会继续执行，因此更适合存在顺序依赖或希望尽早暴露首个失败的场景。</para>
/// </remarks>
public sealed class SequentialNotificationPublisher : INotificationPublisher
{
    /// <summary>
    ///     按既定顺序逐个执行当前通知的处理器。
    /// </summary>
    /// <typeparam name="TNotification">通知类型。</typeparam>
    /// <param name="context">当前发布调用的执行上下文。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>表示通知发布完成的值任务。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context" /> 为 <see langword="null" />。</exception>
    public async ValueTask PublishAsync<TNotification>(
        NotificationPublishContext<TNotification> context,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var handler in context.Handlers)
        {
            await context.InvokeHandlerAsync(handler, cancellationToken).ConfigureAwait(false);
        }
    }
}
