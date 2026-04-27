using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Core.Tests.Architectures;

/// <summary>
///     记录请求通过管道次数的测试行为。
/// </summary>
/// <typeparam name="TRequest">请求类型。</typeparam>
/// <typeparam name="TResponse">响应类型。</typeparam>
public sealed class TrackingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    ///     获取当前测试进程中该请求类型对应的行为触发次数。
    /// </summary>
    public static int InvocationCount { get; set; }

    /// <summary>
    ///     记录一次行为执行，然后继续执行下一个处理器。
    /// </summary>
    /// <param name="message">当前请求消息。</param>
    /// <param name="next">下一个处理委托。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>下游处理器的响应结果。</returns>
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        InvocationCount++;
        return await next(message, cancellationToken).ConfigureAwait(false);
    }
}
