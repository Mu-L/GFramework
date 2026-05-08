// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Ioc;
using GFramework.Core.Logging;
using GFramework.Cqrs.Abstractions.Cqrs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GFramework.Cqrs.Benchmarks.Messaging;

/// <summary>
///     对比 stream 完整枚举在不同 handler 生命周期下的额外开销。
/// </summary>
/// <remarks>
///     当前矩阵只覆盖 `Singleton` 与 `Transient`。
///     `Scoped` 仍依赖真实的显式作用域边界；在当前“单根容器最小宿主”模型下直接加入 scoped 会把枚举宿主成本与生命周期成本混在一起，
///     因此保持与 request 生命周期矩阵相同的边界，留待后续 scoped host 基线具备后再扩展。
/// </remarks>
[Config(typeof(Config))]
public class StreamLifetimeBenchmarks
{
    private MicrosoftDiContainer _container = null!;
    private ICqrsRuntime _runtime = null!;
    private ServiceProvider _serviceProvider = null!;
    private IMediator _mediatr = null!;
    private BenchmarkStreamHandler _baselineHandler = null!;
    private BenchmarkStreamRequest _request = null!;

    /// <summary>
    ///     控制当前 benchmark 使用的 handler 生命周期。
    /// </summary>
    [Params(HandlerLifetime.Singleton, HandlerLifetime.Transient)]
    public HandlerLifetime Lifetime { get; set; }

    /// <summary>
    ///     可公平比较的 benchmark handler 生命周期集合。
    /// </summary>
    public enum HandlerLifetime
    {
        /// <summary>
        ///     复用单个 handler 实例。
        /// </summary>
        Singleton,

        /// <summary>
        ///     每次建流都重新解析新的 handler 实例。
        /// </summary>
        Transient
    }

    /// <summary>
    ///     配置 stream 生命周期 benchmark 的公共输出格式。
    /// </summary>
    private sealed class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.Default);
            AddColumnProvider(DefaultColumnProviders.Instance);
            AddColumn(new CustomColumn("Scenario", static (_, _) => "StreamLifetime"));
            AddDiagnoser(MemoryDiagnoser.Default);
            WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared));
        }
    }

    /// <summary>
    ///     构建当前生命周期下的 GFramework 与 MediatR stream 对照宿主。
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        LoggerFactoryResolver.Provider = new ConsoleLoggerFactoryProvider
        {
            MinLevel = LogLevel.Fatal
        };
        Fixture.Setup($"StreamLifetime/{Lifetime}", handlerCount: 1, pipelineCount: 0);
        BenchmarkDispatcherCacheHelper.ClearDispatcherCaches();

        _baselineHandler = new BenchmarkStreamHandler();
        _request = new BenchmarkStreamRequest(Guid.NewGuid(), 3);

        _container = BenchmarkHostFactory.CreateFrozenGFrameworkContainer(container =>
        {
            container.RegisterCqrsHandlersFromAssembly(typeof(StreamLifetimeBenchmarks).Assembly);
            RegisterGFrameworkHandler(container, Lifetime);
        });
        _runtime = GFramework.Cqrs.CqrsRuntimeFactory.CreateRuntime(
            _container,
            LoggerFactoryResolver.Provider.CreateLogger(nameof(StreamLifetimeBenchmarks) + "." + Lifetime));

        _serviceProvider = BenchmarkHostFactory.CreateMediatRServiceProvider(
            configure: null,
            typeof(StreamLifetimeBenchmarks),
            static candidateType => candidateType == typeof(BenchmarkStreamHandler),
            ResolveMediatRLifetime(Lifetime));
        _mediatr = _serviceProvider.GetRequiredService<IMediator>();
    }

    /// <summary>
    ///     释放当前生命周期矩阵持有的 benchmark 宿主资源，并清理 dispatcher 缓存。
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        try
        {
            BenchmarkCleanupHelper.DisposeAll(_container, _serviceProvider);
        }
        finally
        {
            BenchmarkDispatcherCacheHelper.ClearDispatcherCaches();
        }
    }

    /// <summary>
    ///     直接调用 handler 并完整枚举，作为不同生命周期矩阵下的 dispatch 额外开销 baseline。
    /// </summary>
    [Benchmark(Baseline = true)]
    public async ValueTask Stream_Baseline()
    {
        await foreach (var response in _baselineHandler.Handle(_request, CancellationToken.None).ConfigureAwait(false))
        {
            _ = response;
        }
    }

    /// <summary>
    ///     通过 GFramework.CQRS runtime 创建并完整枚举 stream。
    /// </summary>
    [Benchmark]
    public async ValueTask Stream_GFrameworkCqrs()
    {
        await foreach (var response in _runtime.CreateStream(BenchmarkContext.Instance, _request, CancellationToken.None)
                           .ConfigureAwait(false))
        {
            _ = response;
        }
    }

    /// <summary>
    ///     通过 MediatR 创建并完整枚举 stream，作为外部对照。
    /// </summary>
    [Benchmark]
    public async ValueTask Stream_MediatR()
    {
        await foreach (var response in _mediatr.CreateStream(_request, CancellationToken.None).ConfigureAwait(false))
        {
            _ = response;
        }
    }

    /// <summary>
    ///     按生命周期把 benchmark stream handler 注册到 GFramework 容器。
    /// </summary>
    /// <param name="container">当前 benchmark 拥有并负责释放的容器。</param>
    /// <param name="lifetime">待比较的 handler 生命周期。</param>
    /// <remarks>
    ///     先通过 generated registry 提供静态 descriptor，再显式覆盖 handler 生命周期，
    ///     可以把比较变量收敛到 handler 解析成本，而不是 descriptor 发现路径本身。
    /// </remarks>
    private static void RegisterGFrameworkHandler(MicrosoftDiContainer container, HandlerLifetime lifetime)
    {
        ArgumentNullException.ThrowIfNull(container);

        switch (lifetime)
        {
            case HandlerLifetime.Singleton:
                container.RegisterSingleton<
                    GFramework.Cqrs.Abstractions.Cqrs.IStreamRequestHandler<BenchmarkStreamRequest, BenchmarkResponse>,
                    BenchmarkStreamHandler>();
                return;

            case HandlerLifetime.Transient:
                container.RegisterTransient<
                    GFramework.Cqrs.Abstractions.Cqrs.IStreamRequestHandler<BenchmarkStreamRequest, BenchmarkResponse>,
                    BenchmarkStreamHandler>();
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Unsupported benchmark handler lifetime.");
        }
    }

    /// <summary>
    ///     将 benchmark 生命周期映射为 MediatR 组装所需的 <see cref="ServiceLifetime" />。
    /// </summary>
    /// <param name="lifetime">待比较的 handler 生命周期。</param>
    /// <returns>当前生命周期对应的 MediatR 注册方式。</returns>
    private static ServiceLifetime ResolveMediatRLifetime(HandlerLifetime lifetime)
    {
        return lifetime switch
        {
            HandlerLifetime.Singleton => ServiceLifetime.Singleton,
            HandlerLifetime.Transient => ServiceLifetime.Transient,
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, "Unsupported benchmark handler lifetime.")
        };
    }

    /// <summary>
    ///     Benchmark stream request。
    /// </summary>
    /// <param name="Id">请求标识。</param>
    /// <param name="ItemCount">返回元素数量。</param>
    public sealed record BenchmarkStreamRequest(Guid Id, int ItemCount) :
        GFramework.Cqrs.Abstractions.Cqrs.IStreamRequest<BenchmarkResponse>,
        MediatR.IStreamRequest<BenchmarkResponse>;

    /// <summary>
    ///     Benchmark stream response。
    /// </summary>
    /// <param name="Id">响应标识。</param>
    public sealed record BenchmarkResponse(Guid Id);

    /// <summary>
    ///     同时实现 GFramework.CQRS 与 MediatR 契约的最小 stream handler。
    /// </summary>
    public sealed class BenchmarkStreamHandler :
        GFramework.Cqrs.Abstractions.Cqrs.IStreamRequestHandler<BenchmarkStreamRequest, BenchmarkResponse>,
        MediatR.IStreamRequestHandler<BenchmarkStreamRequest, BenchmarkResponse>
    {
        /// <summary>
        ///     处理 GFramework.CQRS stream request。
        /// </summary>
        /// <param name="request">当前 benchmark stream 请求。</param>
        /// <param name="cancellationToken">用于中断异步枚举的取消令牌。</param>
        /// <returns>完整枚举所需的低噪声异步响应序列。</returns>
        public IAsyncEnumerable<BenchmarkResponse> Handle(
            BenchmarkStreamRequest request,
            CancellationToken cancellationToken)
        {
            return EnumerateAsync(request, cancellationToken);
        }

        /// <summary>
        ///     处理 MediatR stream request。
        /// </summary>
        /// <param name="request">当前 benchmark stream 请求。</param>
        /// <param name="cancellationToken">用于中断异步枚举的取消令牌。</param>
        /// <returns>完整枚举所需的低噪声异步响应序列。</returns>
        IAsyncEnumerable<BenchmarkResponse> MediatR.IStreamRequestHandler<BenchmarkStreamRequest, BenchmarkResponse>.Handle(
            BenchmarkStreamRequest request,
            CancellationToken cancellationToken)
        {
            return EnumerateAsync(request, cancellationToken);
        }

        /// <summary>
        ///     为生命周期矩阵构造稳定、低噪声的异步响应序列。
        /// </summary>
        /// <param name="request">当前 benchmark 请求。</param>
        /// <param name="cancellationToken">用于中断异步枚举的取消令牌。</param>
        /// <returns>按固定元素数量返回的异步响应序列。</returns>
        private static async IAsyncEnumerable<BenchmarkResponse> EnumerateAsync(
            BenchmarkStreamRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (var index = 0; index < request.ItemCount; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return new BenchmarkResponse(request.Id);
                await Task.CompletedTask.ConfigureAwait(false);
            }
        }
    }
}
