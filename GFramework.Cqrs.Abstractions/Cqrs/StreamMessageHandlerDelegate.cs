// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

namespace GFramework.Cqrs.Abstractions.Cqrs;

/// <summary>
///     表示流式 CQRS 请求在管道中继续向下执行的处理委托。
/// </summary>
/// <remarks>
///     <para>stream 行为可以通过不调用该委托来短路整个流式处理链。</para>
///     <para>除显式实现重试、回放或分支等高级语义外，行为通常应最多调用一次该委托，以维持单次建流的确定性。</para>
///     <para>调用方应传递当前收到的 <paramref name="cancellationToken" />，确保取消信号沿建流入口与后续枚举链路一致传播。</para>
/// </remarks>
/// <typeparam name="TRequest">流式请求类型。</typeparam>
/// <typeparam name="TResponse">流式响应元素类型。</typeparam>
/// <param name="message">当前流式请求消息。</param>
/// <param name="cancellationToken">取消令牌。</param>
/// <returns>异步响应序列。</returns>
public delegate IAsyncEnumerable<TResponse> StreamMessageHandlerDelegate<in TRequest, out TResponse>(
    TRequest message,
    CancellationToken cancellationToken)
    where TRequest : IStreamRequest<TResponse>;
