// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     记录双 stream pipeline 的内层行为顺序。
/// </summary>
internal sealed class DispatcherStreamPipelineOrderInnerBehavior : IStreamPipelineBehavior<DispatcherStreamPipelineOrderRequest, int>
{
    /// <summary>
    ///     在进入和离开下游阶段时记录顺序。
    /// </summary>
    /// <param name="request">当前流式请求。</param>
    /// <param name="next">下一个处理阶段。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>下游处理阶段返回的异步流。</returns>
    public async IAsyncEnumerable<int> Handle(
        DispatcherStreamPipelineOrderRequest request,
        StreamMessageHandlerDelegate<DispatcherStreamPipelineOrderRequest, int> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        DispatcherStreamPipelineOrderState.Record("Inner:Before");

        await foreach (var item in next(request, cancellationToken)
                           .WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            yield return item;
        }

        DispatcherStreamPipelineOrderState.Record("Inner:After");
    }
}
