namespace GFramework.Cqrs.Abstractions.Cqrs;

/// <summary>
///     定义架构上下文使用的 CQRS runtime seam。
///     该抽象把请求分发、通知发布与流式处理从具体实现中解耦，
///     使 CQRS runtime 契约可独立归属到 <c>GFramework.Cqrs.Abstractions</c>。
/// </summary>
public interface ICqrsRuntime
{
    /// <summary>
    ///     发送请求并返回响应。
    /// </summary>
    /// <typeparam name="TResponse">响应类型。</typeparam>
    /// <param name="context">当前 CQRS 分发上下文。</param>
    /// <param name="request">要分发的请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>请求响应。</returns>
    ValueTask<TResponse> SendAsync<TResponse>(
        ICqrsContext context,
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     发布通知到所有已注册处理器。
    /// </summary>
    /// <typeparam name="TNotification">通知类型。</typeparam>
    /// <param name="context">当前 CQRS 分发上下文。</param>
    /// <param name="notification">要发布的通知。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>表示通知分发完成的值任务。</returns>
    ValueTask PublishAsync<TNotification>(
        ICqrsContext context,
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    ///     创建流式请求的异步响应序列。
    /// </summary>
    /// <typeparam name="TResponse">流元素类型。</typeparam>
    /// <param name="context">当前 CQRS 分发上下文。</param>
    /// <param name="request">流式请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>按需生成的异步响应序列。</returns>
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        ICqrsContext context,
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}
