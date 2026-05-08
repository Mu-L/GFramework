// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     为 <see cref="DispatcherCacheStreamRequest" /> 提供最小 stream pipeline 行为，
///     用于命中 dispatcher 的 stream pipeline invoker 缓存分支。
/// </summary>
internal sealed class DispatcherStreamPipelineCacheBehavior : IStreamPipelineBehavior<DispatcherCacheStreamRequest, int>
{
    /// <summary>
    ///     直接转发到下一个处理阶段。
    /// </summary>
    /// <param name="request">当前流式请求。</param>
    /// <param name="next">下一个处理阶段。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>下游处理阶段返回的异步流。</returns>
    public IAsyncEnumerable<int> Handle(
        DispatcherCacheStreamRequest request,
        StreamMessageHandlerDelegate<DispatcherCacheStreamRequest, int> next,
        CancellationToken cancellationToken)
    {
        return next(request, cancellationToken);
    }
}
