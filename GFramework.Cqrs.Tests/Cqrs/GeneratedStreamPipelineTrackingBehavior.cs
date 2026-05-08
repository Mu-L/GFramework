// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using System.Threading;
using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     记录 generated stream invoker 与 stream pipeline 行为组合时的命中次数。
/// </summary>
internal sealed class GeneratedStreamPipelineTrackingBehavior
    : IStreamPipelineBehavior<GeneratedStreamInvokerRequest, int>
{
    private static int _invocationCount;

    /// <summary>
    ///     获取或重置当前测试进程中的行为触发次数。
    /// </summary>
    public static int InvocationCount
    {
        get => Volatile.Read(ref _invocationCount);
        set => Volatile.Write(ref _invocationCount, value);
    }

    /// <summary>
    ///     记录一次行为执行，然后继续执行 generated stream invoker。
    /// </summary>
    /// <param name="message">当前流式请求消息。</param>
    /// <param name="next">下一个处理阶段。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>下游处理阶段返回的异步流。</returns>
    public IAsyncEnumerable<int> Handle(
        GeneratedStreamInvokerRequest message,
        StreamMessageHandlerDelegate<GeneratedStreamInvokerRequest, int> next,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _invocationCount);
        return next(message, cancellationToken);
    }
}
