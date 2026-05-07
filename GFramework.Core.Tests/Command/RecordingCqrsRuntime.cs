// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Core.Tests.Command;

/// <summary>
///     记录 bridge 执行线程与收到请求的最小 CQRS runtime 测试替身。
/// </summary>
internal sealed class RecordingCqrsRuntime(Func<object?, object?>? responseFactory = null) : ICqrsRuntime
{
    private static readonly Func<object?, object?> DefaultResponseFactory = _ => null;

    private readonly Func<object?, object?> _responseFactory = responseFactory ?? DefaultResponseFactory;

    /// <summary>
    ///     获取最近一次 <see cref="SendAsync{TResponse}" /> 观察到的同步上下文类型。
    /// </summary>
    public Type? ObservedSynchronizationContextType { get; private set; }

    /// <summary>
    ///     获取最近一次收到的请求实例。
    /// </summary>
    public object? LastRequest { get; private set; }

    /// <inheritdoc />
    public ValueTask<TResponse> SendAsync<TResponse>(
        ICqrsContext context,
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(request);

        ObservedSynchronizationContextType = SynchronizationContext.Current?.GetType();
        LastRequest = request;

        object? response = request switch
        {
            IRequest<Unit> => Unit.Value,
            _ => _responseFactory(request)
        };

        return ValueTask.FromResult((TResponse)response!);
    }

    /// <inheritdoc />
    public ValueTask PublishAsync<TNotification>(
        ICqrsContext context,
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        ICqrsContext context,
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}
