using GFramework.Core.Abstractions.Architectures;
using GFramework.Core.Abstractions.Command;
using GFramework.Core.Abstractions.Environment;
using GFramework.Core.Abstractions.Events;
using GFramework.Core.Abstractions.Ioc;
using GFramework.Core.Abstractions.Model;
using GFramework.Core.Abstractions.Query;
using GFramework.Core.Abstractions.Systems;
using GFramework.Core.Abstractions.Utility;
using GFramework.Core.Command;
using GFramework.Core.Environment;
using GFramework.Core.Events;
using GFramework.Core.Ioc;
using GFramework.Core.Query;
using GFramework.Cqrs.Abstractions.Cqrs;
using ICommand = GFramework.Core.Abstractions.Command.ICommand;

namespace GFramework.Core.Tests.Architectures;

/// <summary>
///     为 <see cref="GameContextTests" /> 提供最小可用的架构上下文测试桩。
/// </summary>
/// <remarks>
///     该类型只实现当前测试切片会触达的基础行为，其余 CQRS 入口显式抛出 <see cref="NotSupportedException" />，
///     避免测试误把未覆盖能力当成可用实现。
/// </remarks>
public class TestArchitectureContext : IArchitectureContext
{
    private readonly MicrosoftDiContainer _container = new();

    /// <summary>
    ///     获取用于解析测试服务的依赖注入容器。
    /// </summary>
    public IIocContainer Container => _container;

    /// <summary>
    ///     获取测试事件总线实例。
    /// </summary>
    public IEventBus EventBus => new EventBus();

    /// <summary>
    ///     获取测试命令执行器实例。
    /// </summary>
    public ICommandExecutor CommandExecutor => new CommandExecutor();

    /// <summary>
    ///     获取测试查询执行器实例。
    /// </summary>
    public IQueryExecutor QueryExecutor => new QueryExecutor();

    /// <summary>
    ///     获取默认测试环境对象。
    /// </summary>
    public IEnvironment Environment => new DefaultEnvironment();

    /// <summary>
    ///     获取指定类型的服务实例。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <returns>已注册的服务实例。</returns>
    /// <exception cref="InvalidOperationException">未注册服务时抛出。</exception>
    public TService GetService<TService>() where TService : class
    {
        return _container.GetRequired<TService>();
    }

    /// <summary>
    ///     获取指定类型的所有服务实例。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <returns>服务实例列表。</returns>
    public IReadOnlyList<TService> GetServices<TService>() where TService : class
    {
        return _container.GetAll<TService>();
    }

    /// <summary>
    ///     获取指定类型的模型实例。
    /// </summary>
    /// <typeparam name="TModel">模型类型。</typeparam>
    /// <returns>已注册的模型实例。</returns>
    /// <exception cref="InvalidOperationException">未注册模型时抛出。</exception>
    public TModel GetModel<TModel>() where TModel : class, IModel
    {
        return _container.GetRequired<TModel>();
    }

    /// <summary>
    ///     获取指定类型的所有模型实例。
    /// </summary>
    /// <typeparam name="TModel">模型类型。</typeparam>
    /// <returns>模型实例列表。</returns>
    public IReadOnlyList<TModel> GetModels<TModel>() where TModel : class, IModel
    {
        return _container.GetAll<TModel>();
    }

    /// <summary>
    ///     获取指定类型的系统实例。
    /// </summary>
    /// <typeparam name="TSystem">系统类型。</typeparam>
    /// <returns>已注册的系统实例。</returns>
    /// <exception cref="InvalidOperationException">未注册系统时抛出。</exception>
    public TSystem GetSystem<TSystem>() where TSystem : class, ISystem
    {
        return _container.GetRequired<TSystem>();
    }

    /// <summary>
    ///     获取指定类型的所有系统实例。
    /// </summary>
    /// <typeparam name="TSystem">系统类型。</typeparam>
    /// <returns>系统实例列表。</returns>
    public IReadOnlyList<TSystem> GetSystems<TSystem>() where TSystem : class, ISystem
    {
        return _container.GetAll<TSystem>();
    }

    /// <summary>
    ///     获取指定类型的工具实例。
    /// </summary>
    /// <typeparam name="TUtility">工具类型。</typeparam>
    /// <returns>已注册的工具实例。</returns>
    /// <exception cref="InvalidOperationException">未注册工具时抛出。</exception>
    public virtual TUtility GetUtility<TUtility>() where TUtility : class, IUtility
    {
        return _container.GetRequired<TUtility>();
    }

    /// <summary>
    ///     获取指定类型的所有工具实例。
    /// </summary>
    /// <typeparam name="TUtility">工具类型。</typeparam>
    /// <returns>工具实例列表。</returns>
    public IReadOnlyList<TUtility> GetUtilities<TUtility>() where TUtility : class, IUtility
    {
        return _container.GetAll<TUtility>();
    }

    /// <summary>
    ///     获取指定类型的所有服务实例，并按优先级排序。
    /// </summary>
    /// <typeparam name="TService">服务类型。</typeparam>
    /// <returns>按优先级排序后的服务实例列表。</returns>
    public IReadOnlyList<TService> GetServicesByPriority<TService>() where TService : class
    {
        return _container.GetAllByPriority<TService>();
    }

    /// <summary>
    ///     获取指定类型的所有系统实例，并按优先级排序。
    /// </summary>
    /// <typeparam name="TSystem">系统类型。</typeparam>
    /// <returns>按优先级排序后的系统实例列表。</returns>
    public IReadOnlyList<TSystem> GetSystemsByPriority<TSystem>() where TSystem : class, ISystem
    {
        return _container.GetAllByPriority<TSystem>();
    }

    /// <summary>
    ///     获取指定类型的所有模型实例，并按优先级排序。
    /// </summary>
    /// <typeparam name="TModel">模型类型。</typeparam>
    /// <returns>按优先级排序后的模型实例列表。</returns>
    public IReadOnlyList<TModel> GetModelsByPriority<TModel>() where TModel : class, IModel
    {
        return _container.GetAllByPriority<TModel>();
    }

    /// <summary>
    ///     获取指定类型的所有工具实例，并按优先级排序。
    /// </summary>
    /// <typeparam name="TUtility">工具类型。</typeparam>
    /// <returns>按优先级排序后的工具实例列表。</returns>
    public IReadOnlyList<TUtility> GetUtilitiesByPriority<TUtility>() where TUtility : class, IUtility
    {
        return _container.GetAllByPriority<TUtility>();
    }

    /// <summary>
    ///     发送无参数事件。
    /// </summary>
    /// <typeparam name="TEvent">事件类型。</typeparam>
    public void SendEvent<TEvent>() where TEvent : new()
    {
    }

    /// <summary>
    ///     发送带参数事件。
    /// </summary>
    /// <typeparam name="TEvent">事件类型。</typeparam>
    /// <param name="e">事件实例。</param>
    public void SendEvent<TEvent>(TEvent e) where TEvent : class
    {
    }

    /// <summary>
    ///     注册事件处理器。
    /// </summary>
    /// <typeparam name="TEvent">事件类型。</typeparam>
    /// <param name="handler">事件处理委托。</param>
    /// <returns>用于测试的空注销句柄。</returns>
    public IUnRegister RegisterEvent<TEvent>(Action<TEvent> handler)
    {
        return new DefaultUnRegister(() => { });
    }

    /// <summary>
    ///     取消注册事件处理器。
    /// </summary>
    /// <typeparam name="TEvent">事件类型。</typeparam>
    /// <param name="onEvent">事件处理委托。</param>
    public void UnRegisterEvent<TEvent>(Action<TEvent> onEvent)
    {
    }

    /// <summary>
    ///     测试桩：异步发送统一 CQRS 请求。
    /// </summary>
    /// <typeparam name="TResponse">响应类型。</typeparam>
    /// <param name="request">要发送的请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>请求响应任务。</returns>
    /// <exception cref="NotSupportedException">该测试桩不支持此成员。</exception>
    public ValueTask<TResponse> SendRequestAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     测试桩：同步发送统一 CQRS 请求。
    /// </summary>
    /// <typeparam name="TResponse">响应类型。</typeparam>
    /// <param name="request">要发送的请求。</param>
    /// <returns>请求响应。</returns>
    /// <exception cref="NotSupportedException">该测试桩不支持此成员。</exception>
    public TResponse SendRequest<TResponse>(IRequest<TResponse> request)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     测试桩：异步发送 CQRS 命令并返回响应。
    /// </summary>
    /// <typeparam name="TResponse">命令响应类型。</typeparam>
    /// <param name="command">要发送的命令。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>命令响应任务。</returns>
    /// <exception cref="NotSupportedException">该测试桩不支持此成员。</exception>
    public ValueTask<TResponse> SendCommandAsync<TResponse>(
        GFramework.Cqrs.Abstractions.Cqrs.Command.ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     测试桩：同步发送 CQRS 命令并返回响应。
    /// </summary>
    /// <typeparam name="TResponse">命令响应类型。</typeparam>
    /// <param name="command">要发送的命令。</param>
    /// <returns>命令响应。</returns>
    /// <exception cref="NotSupportedException">该测试桩不支持此成员。</exception>
    public TResponse SendCommand<TResponse>(GFramework.Cqrs.Abstractions.Cqrs.Command.ICommand<TResponse> command)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     测试桩：异步发送 CQRS 查询并返回结果。
    /// </summary>
    /// <typeparam name="TResponse">查询结果类型。</typeparam>
    /// <param name="query">要发送的查询。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>查询结果任务。</returns>
    /// <exception cref="NotSupportedException">该测试桩不支持此成员。</exception>
    public ValueTask<TResponse> SendQueryAsync<TResponse>(
        GFramework.Cqrs.Abstractions.Cqrs.Query.IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     测试桩：同步发送 CQRS 查询并返回结果。
    /// </summary>
    /// <typeparam name="TResponse">查询结果类型。</typeparam>
    /// <param name="query">要发送的查询。</param>
    /// <returns>查询结果。</returns>
    /// <exception cref="NotSupportedException">该测试桩不支持此成员。</exception>
    public TResponse SendQuery<TResponse>(GFramework.Cqrs.Abstractions.Cqrs.Query.IQuery<TResponse> query)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     测试桩：异步发布 CQRS 通知。
    /// </summary>
    /// <typeparam name="TNotification">通知类型。</typeparam>
    /// <param name="notification">要发布的通知。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>通知发布任务。</returns>
    /// <exception cref="NotSupportedException">该测试桩不支持此成员。</exception>
    public ValueTask PublishAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default) where TNotification : INotification
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     测试桩：创建 CQRS 流式请求响应序列。
    /// </summary>
    /// <typeparam name="TResponse">流式响应元素类型。</typeparam>
    /// <param name="request">流式请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>异步响应流。</returns>
    /// <exception cref="NotSupportedException">该测试桩不支持此成员。</exception>
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     测试桩：异步发送无返回值 CQRS 命令。
    /// </summary>
    /// <typeparam name="TCommand">命令类型。</typeparam>
    /// <param name="command">要发送的命令。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>命令发送任务。</returns>
    /// <exception cref="NotSupportedException">该测试桩不支持此成员。</exception>
    public ValueTask SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : IRequest<Unit>
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     测试桩：异步发送带返回值的 CQRS 请求。
    /// </summary>
    /// <typeparam name="TResponse">响应类型。</typeparam>
    /// <param name="command">要发送的请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>请求响应任务。</returns>
    /// <exception cref="NotSupportedException">该测试桩不支持此成员。</exception>
    public ValueTask<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> command,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     发送旧版命令。
    /// </summary>
    /// <param name="command">命令对象。</param>
    public void SendCommand(ICommand command)
    {
    }

    /// <summary>
    ///     发送旧版带返回值命令。
    /// </summary>
    /// <typeparam name="TResult">返回值类型。</typeparam>
    /// <param name="command">命令对象。</param>
    /// <returns>测试桩默认返回值。</returns>
    public TResult SendCommand<TResult>(ICommand<TResult> command)
    {
        return default!;
    }

    /// <summary>
    ///     异步发送旧版命令。
    /// </summary>
    /// <param name="command">命令对象。</param>
    /// <returns>已完成任务。</returns>
    public Task SendCommandAsync(IAsyncCommand command)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     异步发送旧版带返回值命令。
    /// </summary>
    /// <typeparam name="TResult">返回值类型。</typeparam>
    /// <param name="command">命令对象。</param>
    /// <returns>包含测试桩默认返回值的任务。</returns>
    public Task<TResult> SendCommandAsync<TResult>(IAsyncCommand<TResult> command)
    {
        return Task.FromResult(default(TResult)!);
    }

    /// <summary>
    ///     发送旧版查询请求。
    /// </summary>
    /// <typeparam name="TResult">查询结果类型。</typeparam>
    /// <param name="query">查询对象。</param>
    /// <returns>测试桩默认返回值。</returns>
    public TResult SendQuery<TResult>(IQuery<TResult> query)
    {
        return default!;
    }

    /// <summary>
    ///     异步发送旧版查询请求。
    /// </summary>
    /// <typeparam name="TResult">查询结果类型。</typeparam>
    /// <param name="query">异步查询对象。</param>
    /// <returns>包含测试桩默认返回值的任务。</returns>
    public Task<TResult> SendQueryAsync<TResult>(IAsyncQuery<TResult> query)
    {
        return Task.FromResult(default(TResult)!);
    }

    /// <summary>
    ///     获取当前环境对象。
    /// </summary>
    /// <returns>默认测试环境对象。</returns>
    public IEnvironment GetEnvironment()
    {
        return Environment;
    }
}
