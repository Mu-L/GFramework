// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using GFramework.Cqrs.Abstractions.Cqrs;
using GFramework.Cqrs.Cqrs;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     记录缓存 stream pipeline executor 复用场景下每次建流注入到 behavior 的上下文与实例身份。
/// </summary>
internal sealed class DispatcherStreamPipelineContextRefreshBehavior
    : CqrsContextAwareHandlerBase,
        IStreamPipelineBehavior<DispatcherStreamContextRefreshRequest, int>
{
    private readonly int _instanceId = DispatcherStreamContextRefreshState.AllocateBehaviorInstanceId();

    /// <summary>
    ///     记录当前 behavior 实例实际收到的上下文，然后继续执行下游处理阶段。
    /// </summary>
    /// <param name="request">当前流式请求。</param>
    /// <param name="next">下一个处理阶段。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>下游处理阶段返回的异步流。</returns>
    public async IAsyncEnumerable<int> Handle(
        DispatcherStreamContextRefreshRequest request,
        StreamMessageHandlerDelegate<DispatcherStreamContextRefreshRequest, int> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        DispatcherStreamContextRefreshState.RecordBehavior(request.DispatchId, _instanceId, Context);

        await foreach (var item in next(request, cancellationToken)
                           .WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            yield return item;
        }
    }
}
