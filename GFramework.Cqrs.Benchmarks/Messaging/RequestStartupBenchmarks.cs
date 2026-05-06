// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System;
using System.Threading;
using System.Threading.Tasks;
using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Ioc;
using GFramework.Core.Logging;
using GFramework.Cqrs.Abstractions.Cqrs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ILogger = GFramework.Core.Abstractions.Logging.ILogger;

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
    private ICqrsRuntime _runtime = null!;

    /// <summary>
    ///     配置 request startup benchmark 的公共输出格式。
    /// </summary>
    private sealed class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.Default);
            AddColumnProvider(DefaultColumnProviders.Instance);
            AddColumn(new CustomColumn("Scenario", static (_, _) => "RequestStartup"), TargetMethodColumn.Method, CategoriesColumn.Default);
            AddDiagnoser(MemoryDiagnoser.Default);
            AddLogicalGroupRules(BenchmarkLogicalGroupRule.ByCategory);
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
        _runtime = CreateGFrameworkRuntime();
    }

    /// <summary>
    ///     释放 startup benchmark 复用的宿主对象。
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }

    /// <summary>
    ///     返回已构建宿主中的 MediatR mediator，作为 initialization 组的句柄解析 baseline。
    /// </summary>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Initialization")]
    public IMediator Initialization_MediatR()
    {
        return _mediatr;
    }

    /// <summary>
    ///     返回已构建宿主中的 GFramework.CQRS runtime，确保与 MediatR baseline 处于相同初始化阶段。
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Initialization")]
    public ICqrsRuntime Initialization_GFrameworkCqrs()
    {
        return _runtime;
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
    ///     在新 runtime 上首次发送 request，量化 GFramework.CQRS 的 first-hit 成本。
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("ColdStart")]
    public ValueTask<BenchmarkResponse> ColdStart_GFrameworkCqrs()
    {
        var runtime = CreateColdStartRuntime();
        return runtime.SendAsync(BenchmarkContext.Instance, Request, CancellationToken.None);
    }

    /// <summary>
    ///     为 cold-start benchmark 构建全新的 runtime，并在构建前显式清空 dispatcher 静态缓存。
    /// </summary>
    /// <remarks>
    ///     这里把缓存清理与 runtime 构建绑定在同一阶段，避免把额外的反射缓存清理成本混入 benchmark 方法主体，
    ///     只保留“新宿主 + 首次分发”的对照。
    /// </summary>
    private static ICqrsRuntime CreateColdStartRuntime()
    {
        BenchmarkDispatcherCacheHelper.ClearDispatcherCaches();
        return CreateGFrameworkRuntime();
    }

    /// <summary>
    ///     构建只承载当前 benchmark request 的最小 GFramework.CQRS runtime。
    /// </summary>
    /// <remarks>
    ///     该 benchmark 故意保持与 MediatR 对照组同样的“单 handler 最小宿主”模型，
    ///     因此这里继续使用单点手工注册，而不引入依赖完整 CQRS 注册协调器的程序集扫描路径。
    /// </remarks>
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
        services.AddLogging(static builder =>
            Microsoft.Extensions.Logging.FilterLoggingBuilderExtensions.AddFilter(
                builder,
                "LuckyPennySoftware.MediatR.License",
                Microsoft.Extensions.Logging.LogLevel.None));
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
