// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using GFramework.Core.Abstractions.Bases;
using GFramework.Core.Abstractions.Ioc;
using GFramework.Core.Abstractions.Rule;
using GFramework.Core.Abstractions.Systems;
using GFramework.Core.Ioc;
using Microsoft.Extensions.DependencyInjection;

namespace GFramework.Cqrs.Benchmarks.Messaging;

/// <summary>
///     把冻结后的 benchmark 根容器与单个 <see cref="IServiceScope" /> 组合成 request 级解析视图。
/// </summary>
/// <remarks>
///     `CqrsDispatcher` 会直接依赖 <see cref="IIocContainer" /> 做 handler / pipeline 解析，
///     因此 request lifetime benchmark 需要一个既保留根容器注册元数据，又把实例解析切换到显式作用域 provider
///     的最小适配层。该类型只覆盖 benchmark 当前 request 路径会使用到的解析相关入口；
///     任何注册、清空或冻结修改操作都应继续发生在根容器构建阶段，因此这里统一拒绝可变更 API。
/// </remarks>
internal sealed class ScopedBenchmarkContainer : IIocContainer
{
    private readonly MicrosoftDiContainer _rootContainer;
    private readonly IServiceProvider _scopedProvider;

    /// <summary>
    ///     初始化一个绑定到单个 request 作用域的 benchmark 容器适配器。
    /// </summary>
    /// <param name="rootContainer">已冻结的 benchmark 根容器。</param>
    /// <param name="scope">当前 request 独占的作用域实例。</param>
    internal ScopedBenchmarkContainer(MicrosoftDiContainer rootContainer, IServiceScope scope)
    {
        _rootContainer = rootContainer ?? throw new ArgumentNullException(nameof(rootContainer));
        ArgumentNullException.ThrowIfNull(scope);
        _scopedProvider = scope.ServiceProvider;
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterSingleton<T>(T instance)
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterSingleton<TService, TImpl>()
        where TImpl : class, TService
        where TService : class
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterTransient<TService, TImpl>()
        where TImpl : class, TService
        where TService : class
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterScoped<TService, TImpl>()
        where TImpl : class, TService
        where TService : class
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterPlurality(object instance)
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterPlurality<T>() where T : class
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterSystem(ISystem system)
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void Register<T>(T instance)
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void Register(Type type, object instance)
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterFactory<TService>(Func<IServiceProvider, TService> factory) where TService : class
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterCqrsPipelineBehavior<TBehavior>() where TBehavior : class
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterCqrsStreamPipelineBehavior<TBehavior>() where TBehavior : class
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterCqrsHandlersFromAssembly(System.Reflection.Assembly assembly)
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持在 request 作用域内追加注册。
    /// </summary>
    public void RegisterCqrsHandlersFromAssemblies(IEnumerable<System.Reflection.Assembly> assemblies)
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持执行额外的服务配置钩子。
    /// </summary>
    public void ExecuteServicesHook(Action<IServiceCollection>? configurator = null)
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     从当前 request 作用域解析单个服务实例。
    /// </summary>
    public T? Get<T>() where T : class
    {
        return _scopedProvider.GetService<T>();
    }

    /// <summary>
    ///     从当前 request 作用域解析单个服务实例。
    /// </summary>
    public object? Get(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _scopedProvider.GetService(type);
    }

    /// <summary>
    ///     从当前 request 作用域解析必需的单个服务实例。
    /// </summary>
    public T GetRequired<T>() where T : class
    {
        return _scopedProvider.GetRequiredService<T>();
    }

    /// <summary>
    ///     从当前 request 作用域解析必需的单个服务实例。
    /// </summary>
    public object GetRequired(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _scopedProvider.GetRequiredService(type);
    }

    /// <summary>
    ///     从当前 request 作用域解析全部服务实例。
    /// </summary>
    public IReadOnlyList<T> GetAll<T>() where T : class
    {
        return _scopedProvider.GetServices<T>().ToList();
    }

    /// <summary>
    ///     从当前 request 作用域解析全部服务实例。
    /// </summary>
    public IReadOnlyList<object> GetAll(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _scopedProvider.GetServices(type).Where(static service => service is not null).Cast<object>().ToList();
    }

    /// <summary>
    ///     从当前 request 作用域解析全部服务实例，并按调用方比较器排序。
    /// </summary>
    public IReadOnlyList<T> GetAllSorted<T>(Comparison<T> comparison) where T : class
    {
        ArgumentNullException.ThrowIfNull(comparison);

        var services = GetAll<T>().ToList();
        services.Sort(comparison);
        return services;
    }

    /// <summary>
    ///     从当前 request 作用域解析全部服务实例，并按优先级排序。
    /// </summary>
    public IReadOnlyList<T> GetAllByPriority<T>() where T : class
    {
        return SortByPriority(GetAll<T>());
    }

    /// <summary>
    ///     从当前 request 作用域解析全部服务实例，并按优先级排序。
    /// </summary>
    public IReadOnlyList<object> GetAllByPriority(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return SortByPriority(GetAll(type));
    }

    /// <summary>
    ///     判断根容器是否声明了目标服务键。
    /// </summary>
    /// <remarks>
    ///     `CqrsDispatcher` 在热路径上先做注册存在性判断，再决定是否枚举 pipeline；这里沿用根容器冻结后的注册视图，
    ///     避免把“当前 scope 还未物化实例”误判成“没有注册该行为”。
    /// </remarks>
    public bool HasRegistration(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return _rootContainer.HasRegistration(type);
    }

    /// <summary>
    ///     判断根容器是否声明了目标服务键。
    /// </summary>
    public bool Contains<T>() where T : class
    {
        return _rootContainer.Contains<T>();
    }

    /// <summary>
    ///     当前 request 作用域适配器不追踪实例归属。
    /// </summary>
    public bool ContainsInstance(object instance)
    {
        return _rootContainer.ContainsInstance(instance);
    }

    /// <summary>
    ///     当前适配器不支持清空注册。
    /// </summary>
    public void Clear()
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     当前适配器不支持重新冻结。
    /// </summary>
    public void Freeze()
    {
        throw CreateMutationNotSupportedException();
    }

    /// <summary>
    ///     继续暴露根容器底层服务集合，仅用于接口兼容。
    /// </summary>
    public IServiceCollection GetServicesUnsafe => _rootContainer.GetServicesUnsafe;

    /// <summary>
    ///     基于当前 request 作用域继续创建嵌套作用域。
    /// </summary>
    public IServiceScope CreateScope()
    {
        return _scopedProvider.CreateScope();
    }

    /// <summary>
    ///     将上下文转发给根容器，保持与 request 生命周期无关的上下文缓存行为一致。
    /// </summary>
    public void SetContext(GFramework.Core.Abstractions.Architectures.IArchitectureContext context)
    {
        ((IContextAware)_rootContainer).SetContext(context);
    }

    /// <summary>
    ///     读取根容器当前持有的架构上下文。
    /// </summary>
    public GFramework.Core.Abstractions.Architectures.IArchitectureContext GetContext()
    {
        return ((IContextAware)_rootContainer).GetContext();
    }

    /// <summary>
    ///     释放当前 request 适配器时不拥有作用域；外层 benchmark 调度入口负责统一释放。
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    ///     生成统一的只读适配器异常，避免 benchmark 误把 request 级容器当成可变组合根。
    /// </summary>
    private static InvalidOperationException CreateMutationNotSupportedException()
    {
        return new InvalidOperationException(
            "Scoped benchmark containers are read-only request views. Mutate registrations on the root benchmark host before freezing it.");
    }

    /// <summary>
    ///     复用与根容器一致的优先级排序语义。
    /// </summary>
    /// <typeparam name="T">服务实例类型。</typeparam>
    /// <param name="services">待排序服务集合。</param>
    /// <returns>按优先级稳定排序后的服务列表。</returns>
    private static IReadOnlyList<T> SortByPriority<T>(IReadOnlyList<T> services) where T : class
    {
        if (services.Count <= 1)
        {
            return services;
        }

        return services
            .Select((service, index) => new { Service = service, Index = index })
            .OrderBy(static x =>
            {
                var priority = x.Service is IPrioritized prioritized ? prioritized.Priority : 0;
                return (priority, x.Index);
            })
            .Select(static x => x.Service)
            .ToList();
    }
}
