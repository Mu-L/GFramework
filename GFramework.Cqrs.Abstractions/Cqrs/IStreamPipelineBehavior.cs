// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

namespace GFramework.Cqrs.Abstractions.Cqrs;

/// <summary>
///     定义流式 CQRS 请求在建流阶段使用的管道行为。
/// </summary>
/// <typeparam name="TRequest">流式请求类型。</typeparam>
/// <typeparam name="TResponse">流式响应元素类型。</typeparam>
public interface IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    ///     处理当前流式请求，并决定是否继续调用后续行为或最终处理器。
    /// </summary>
    /// <param name="message">当前流式请求消息。</param>
    /// <param name="next">下一个处理委托。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>异步响应序列。</returns>
    IAsyncEnumerable<TResponse> Handle(
        TRequest message,
        StreamMessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken);
}
