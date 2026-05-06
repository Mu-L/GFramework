// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System;
using System.Reflection;
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
///     对比 request 宿主的初始化与首次分发成本，作为后续吸收 `Mediator` comparison benchmark 设计的 startup 基线。
/// </summary>
[Config(typeof(Config))]
public class RequestStartupBenchmarks
{
    private static readonly ILogger RuntimeLogger = CreateLogger(nameof(RequestStartupBenchmarks));
    private static readonly BenchmarkRequest Request = new(Guid.NewGuid());

    private ServiceProvider _serviceProvider = null!;
    private IMediator _mediatr = null!;

    /// <summary>
    ///     配置 request startup benchmark 的公共输出格式。
    /// </summary>
    private sealed class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.Default);
            AddColumnProvider(DefaultColumnProviders.Instance);
            AddColumn(new CustomColumn("Scenario", static (_, _) => "RequestStartup"));
            AddDiagnoser(MemoryDiagnoser.Default);
            WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared));
        }
    }

    /// <summary>
    ///     构建 steady-state 初始化 benchmark 复用的宿主对象。
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        Fixture.Setup("RequestStartup", handlerCount: 1, pipelineCount: 0);

        _serviceProvider = CreateMediatRServiceProvider();
        _mediatr = _serviceProvider.GetRequiredService<IMediator>();
    }

    /// <summary>
    ///     释放 MediatR 对照组使用的 DI 宿主。
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }

    /// <summary>
    ///     解析 MediatR mediator，作为 startup 句柄解析成本的 baseline。
    /// </summary>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Initialization")]
    public IMediator Initialization_MediatR()
    {
        return _serviceProvider.GetRequiredService<IMediator>();
    }

    /// <summary>
    ///     创建 GFramework.CQRS runtime，作为同层级 startup 句柄创建成本的对照。
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Initialization")]
    public ICqrsRuntime Initialization_GFrameworkCqrs()
    {
        return CreateGFrameworkRuntime();
    }

    /// <summary>
    ///     在新宿主上首次发送 request，作为 MediatR 的 cold-start baseline。
    /// </summary>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ColdStart")]
    public async Task<BenchmarkResponse> ColdStart_MediatR()
    {
        using var serviceProvider = CreateMediatRServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        return await mediator.Send(Request, CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    ///     在清空 dispatcher 静态缓存后，于新宿主上首次发送 request，量化 GFramework.CQRS 的 first-hit 成本。
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("ColdStart")]
    public ValueTask<BenchmarkResponse> ColdStart_GFrameworkCqrs()
    {
        ClearDispatcherCaches();
        var runtime = CreateGFrameworkRuntime();
        return runtime.SendAsync(BenchmarkContext.Instance, Request, CancellationToken.None);
    }

    /// <summary>
    ///     构建只承载当前 benchmark request 的最小 GFramework.CQRS runtime。
    /// </summary>
    private static ICqrsRuntime CreateGFrameworkRuntime()
    {
        var container = new MicrosoftDiContainer();
        container.RegisterTransient<GFramework.Cqrs.Abstractions.Cqrs.IRequestHandler<BenchmarkRequest, BenchmarkResponse>, BenchmarkRequestHandler>();
        return GFramework.Cqrs.CqrsRuntimeFactory.CreateRuntime(container, RuntimeLogger);
    }

    /// <summary>
    ///     构建只承载当前 benchmark request 的最小 MediatR 对照宿主。
    /// </summary>
    private static ServiceProvider CreateMediatRServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<MediatR.IRequestHandler<BenchmarkRequest, BenchmarkResponse>, BenchmarkRequestHandler>();
        services.AddMediatR(static options => options.RegisterServicesFromAssembly(typeof(RequestStartupBenchmarks).Assembly));
        return services.BuildServiceProvider();
    }

    /// <summary>
    ///     为 benchmark 创建稳定的 fatal 级 logger，避免把日志成本混入 startup 测量。
    /// </summary>
    private static ILogger CreateLogger(string categoryName)
    {
        LoggerFactoryResolver.Provider = new ConsoleLoggerFactoryProvider
        {
            MinLevel = LogLevel.Fatal
        };
        return LoggerFactoryResolver.Provider.CreateLogger(categoryName);
    }

    /// <summary>
    ///     清空 dispatcher 静态缓存，避免同一进程中的前一轮分发污染 cold-start 结果。
    /// </summary>
    private static void ClearDispatcherCaches()
    {
        ClearDispatcherCache("NotificationDispatchBindings");
        ClearDispatcherCache("RequestDispatchBindings");
        ClearDispatcherCache("StreamDispatchBindings");
        ClearDispatcherCache("GeneratedRequestInvokers");
        ClearDispatcherCache("GeneratedStreamInvokers");
    }

    /// <summary>
    ///     通过反射定位并清空 dispatcher 的指定缓存字段。
    /// </summary>
    /// <param name="fieldName">要清理的静态缓存字段名。</param>
    private static void ClearDispatcherCache(string fieldName)
    {
        var field = typeof(GFramework.Cqrs.CqrsRuntimeFactory).Assembly
            .GetType("GFramework.Cqrs.Internal.CqrsDispatcher", throwOnError: true)!
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Missing dispatcher cache field {fieldName}.");
        var cache = field.GetValue(null)
                    ?? throw new InvalidOperationException($"Dispatcher cache field {fieldName} returned null.");
        var clearMethod = cache.GetType().GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance)
                          ?? throw new InvalidOperationException(
                              $"Dispatcher cache field {fieldName} does not expose a Clear method.");
        _ = clearMethod.Invoke(cache, null);
    }

    /// <summary>
    ///     Benchmark request。
    /// </summary>
    /// <param name="Id">请求标识。</param>
    public sealed record BenchmarkRequest(Guid Id) :
        GFramework.Cqrs.Abstractions.Cqrs.IRequest<BenchmarkResponse>,
        MediatR.IRequest<BenchmarkResponse>;

    /// <summary>
    ///     Benchmark response。
    /// </summary>
    /// <param name="Id">响应标识。</param>
    public sealed record BenchmarkResponse(Guid Id);

    /// <summary>
    ///     同时实现 GFramework.CQRS 与 MediatR 契约的最小 request handler。
    /// </summary>
    public sealed class BenchmarkRequestHandler :
        GFramework.Cqrs.Abstractions.Cqrs.IRequestHandler<BenchmarkRequest, BenchmarkResponse>,
        MediatR.IRequestHandler<BenchmarkRequest, BenchmarkResponse>
    {
        /// <summary>
        ///     处理 GFramework.CQRS request。
        /// </summary>
        public ValueTask<BenchmarkResponse> Handle(BenchmarkRequest request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(new BenchmarkResponse(request.Id));
        }

        /// <summary>
        ///     处理 MediatR request。
        /// </summary>
        Task<BenchmarkResponse> MediatR.IRequestHandler<BenchmarkRequest, BenchmarkResponse>.Handle(
            BenchmarkRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new BenchmarkResponse(request.Id));
        }
    }
}
