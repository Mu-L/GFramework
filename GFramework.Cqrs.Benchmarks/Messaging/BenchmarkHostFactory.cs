// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Ioc;
using GFramework.Cqrs.Abstractions.Cqrs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using LegacyICqrsRuntime = GFramework.Core.Abstractions.Cqrs.ICqrsRuntime;

namespace GFramework.Cqrs.Benchmarks.Messaging;

/// <summary>
///     为 benchmark 场景构建最小且可重复的 GFramework / MediatR 对照宿主。
/// </summary>
/// <remarks>
///     基准工程里的对照目标是“相同消息合同下的调度差异”，而不是程序集扫描量或容器生命周期差异。
///     因此这里统一封装两类宿主的最小注册形状，确保：
///     1. GFramework 容器在首次发送前已经冻结，可真实解析按类型注册的 handler；
///     2. MediatR 只扫描当前 benchmark 明确拥有的 handler / behavior 类型，避免整个程序集的额外注册污染结果。
/// </remarks>
internal static class BenchmarkHostFactory
{
    /// <summary>
    ///     创建一个已经冻结的 GFramework benchmark 容器。
    /// </summary>
    /// <param name="configure">向容器写入 benchmark 所需 handler / pipeline 的注册动作。</param>
    /// <returns>已冻结、可立即用于 runtime 分发的容器。</returns>
    internal static MicrosoftDiContainer CreateFrozenGFrameworkContainer(Action<MicrosoftDiContainer> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var container = new MicrosoftDiContainer();
        RegisterCqrsInfrastructure(container);
        configure(container);
        container.Freeze();
        return container;
    }

    /// <summary>
    ///     为 benchmark 宿主补齐默认 CQRS runtime seam，确保它既能手工注册 handler，也能走真实的程序集注册入口。
    /// </summary>
    /// <param name="container">当前 benchmark 拥有的 GFramework 容器。</param>
    /// <remarks>
    ///     `RegisterCqrsHandlersFromAssembly(...)` 依赖预先可见的 runtime / registrar / registration service 实例绑定。
    ///     benchmark 宿主直接使用裸 <see cref="MicrosoftDiContainer" />，因此需要在配置阶段先补齐这组基础设施，
    ///     避免各个 benchmark 用例各自复制同一段前置接线逻辑。
    /// </remarks>
    private static void RegisterCqrsInfrastructure(MicrosoftDiContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        if (container.Get<ICqrsRuntime>() is null)
        {
            var runtimeLogger = LoggerFactoryResolver.Provider.CreateLogger("CqrsDispatcher");
            var notificationPublisher = container.Get<GFramework.Cqrs.Notification.INotificationPublisher>();
            var runtime = GFramework.Cqrs.CqrsRuntimeFactory.CreateRuntime(container, runtimeLogger, notificationPublisher);
            container.Register(runtime);
            container.Register<LegacyICqrsRuntime>((LegacyICqrsRuntime)runtime);
        }
        else if (container.Get<LegacyICqrsRuntime>() is null)
        {
            container.Register<LegacyICqrsRuntime>((LegacyICqrsRuntime)container.GetRequired<ICqrsRuntime>());
        }

        if (container.Get<ICqrsHandlerRegistrar>() is null)
        {
            var registrarLogger = LoggerFactoryResolver.Provider.CreateLogger("DefaultCqrsHandlerRegistrar");
            var registrar = GFramework.Cqrs.CqrsRuntimeFactory.CreateHandlerRegistrar(container, registrarLogger);
            container.Register<ICqrsHandlerRegistrar>(registrar);
        }

        if (container.Get<ICqrsRegistrationService>() is null)
        {
            var registrationLogger = LoggerFactoryResolver.Provider.CreateLogger("DefaultCqrsRegistrationService");
            var registrar = container.GetRequired<ICqrsHandlerRegistrar>();
            var registrationService = GFramework.Cqrs.CqrsRuntimeFactory.CreateRegistrationService(registrar, registrationLogger);
            container.Register<ICqrsRegistrationService>(registrationService);
        }
    }

    /// <summary>
    ///     创建只承载当前 benchmark handler 集合的最小 MediatR 宿主。
    /// </summary>
    /// <param name="configure">补充当前场景的显式服务注册，例如手工单例 handler 或 pipeline 行为。</param>
    /// <param name="handlerAssemblyMarkerType">用于限定扫描程序集的标记类型。</param>
    /// <param name="handlerTypeFilter">
    ///     仅允许当前 benchmark 场景需要的 handler / behavior 类型通过扫描；
    ///     这样可保留 `AddMediatR` 的正常装配路径，同时避免整个基准程序集里的其他 handler 被一并注册。
    /// </param>
    /// <param name="lifetime">当前 benchmark 希望 MediatR 使用的默认注册生命周期。</param>
    /// <returns>只承载当前 benchmark 场景所需服务的 DI 宿主。</returns>
    internal static ServiceProvider CreateMediatRServiceProvider(
        Action<IServiceCollection>? configure,
        Type handlerAssemblyMarkerType,
        Func<Type, bool> handlerTypeFilter,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        ArgumentNullException.ThrowIfNull(handlerAssemblyMarkerType);
        ArgumentNullException.ThrowIfNull(handlerTypeFilter);

        var services = new ServiceCollection();
        services.AddLogging(static builder =>
            Microsoft.Extensions.Logging.FilterLoggingBuilderExtensions.AddFilter(
                builder,
                "LuckyPennySoftware.MediatR.License",
                Microsoft.Extensions.Logging.LogLevel.None));

        configure?.Invoke(services);

        services.AddMediatR(options =>
        {
            options.Lifetime = lifetime;
            options.TypeEvaluator = handlerTypeFilter;
            options.RegisterServicesFromAssembly(handlerAssemblyMarkerType.Assembly);
        });

        return services.BuildServiceProvider();
    }

    /// <summary>
    ///     创建承载 NuGet `Mediator` source-generated concrete mediator 的最小对照宿主。
    /// </summary>
    /// <param name="configure">补充当前场景的显式服务注册。</param>
    /// <returns>可直接解析 generated `Mediator.Mediator` 的 DI 宿主。</returns>
    /// <remarks>
    ///     当前 benchmark 只把 `Mediator` 作为单例 steady-state 对照组接入，
     ///     因为它的 lifetime 由 source generator 在编译期塑形；若后续需要 `Transient` / `Scoped` 矩阵，
    ///     应按 `Mediator` 官方 benchmark 的做法拆成独立 build config，而不是在同一编译产物里混用多个 lifetime。
    /// </remarks>
    internal static ServiceProvider CreateMediatorServiceProvider(Action<IServiceCollection>? configure)
    {
        var services = new ServiceCollection();
        configure?.Invoke(services);
        services.AddMediator();
        return services.BuildServiceProvider();
    }

    /// <summary>
    ///     判断某个类型是否正好实现了指定的闭合或开放 MediatR 合同。
    /// </summary>
    /// <param name="candidateType">待判断类型。</param>
    /// <param name="openGenericContract">目标开放泛型合同，例如 <see cref="MediatR.IRequestHandler{TRequest,TResponse}" />。</param>
    /// <returns>命中任一实现接口时返回 <see langword="true" />；否则返回 <see langword="false" />。</returns>
    internal static bool ImplementsOpenGenericContract(Type candidateType, Type openGenericContract)
    {
        ArgumentNullException.ThrowIfNull(candidateType);
        ArgumentNullException.ThrowIfNull(openGenericContract);

        return candidateType.GetInterfaces().Any(interfaceType =>
            interfaceType.IsGenericType &&
            interfaceType.GetGenericTypeDefinition() == openGenericContract);
    }
}
