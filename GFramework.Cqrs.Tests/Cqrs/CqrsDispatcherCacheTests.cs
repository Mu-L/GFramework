using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Architectures;
using GFramework.Core.Ioc;
using GFramework.Core.Logging;
using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     验证 CQRS dispatcher 会缓存热路径中的 dispatch binding。
/// </summary>
[TestFixture]
internal sealed class CqrsDispatcherCacheTests
{
    /// <summary>
    ///     初始化测试上下文。
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        LoggerFactoryResolver.Provider = new ConsoleLoggerFactoryProvider();
        _container = new MicrosoftDiContainer();
        _container.RegisterCqrsPipelineBehavior<DispatcherPipelineCacheBehavior>();

        CqrsTestRuntime.RegisterHandlers(
            _container,
            typeof(CqrsDispatcherCacheTests).Assembly,
            typeof(ArchitectureContext).Assembly);

        _container.Freeze();
        _context = new ArchitectureContext(_container);
        ClearDispatcherCaches();
    }

    /// <summary>
    ///     清理测试上下文引用。
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _context = null;
        _container = null;
    }

    private MicrosoftDiContainer? _container;
    private ArchitectureContext? _context;

    /// <summary>
    ///     验证相同消息类型重复分发时，不会重复扩张 dispatch binding 缓存。
    /// </summary>
    [Test]
    public async Task Dispatcher_Should_Cache_Dispatch_Bindings_After_First_Dispatch()
    {
        var notificationBindings = GetCacheField("NotificationDispatchBindings");
        var requestBindings = GetGenericCacheField("RequestDispatchBindingCache`1", typeof(int), "Bindings");
        var streamBindings = GetCacheField("StreamDispatchBindings");

        var notificationBefore = notificationBindings.Count;
        var requestBefore = requestBindings.Count;
        var streamBefore = streamBindings.Count;

        await _context!.SendRequestAsync(new DispatcherCacheRequest());
        await _context.SendRequestAsync(new DispatcherPipelineCacheRequest());
        await _context.PublishAsync(new DispatcherCacheNotification());
        await DrainAsync(_context.CreateStream(new DispatcherCacheStreamRequest()));

        var notificationAfterFirstDispatch = notificationBindings.Count;
        var requestAfterFirstDispatch = requestBindings.Count;
        var streamAfterFirstDispatch = streamBindings.Count;

        await _context.SendRequestAsync(new DispatcherCacheRequest());
        await _context.SendRequestAsync(new DispatcherPipelineCacheRequest());
        await _context.PublishAsync(new DispatcherCacheNotification());
        await DrainAsync(_context.CreateStream(new DispatcherCacheStreamRequest()));

        Assert.Multiple(() =>
        {
            Assert.That(notificationAfterFirstDispatch, Is.EqualTo(notificationBefore + 1));
            Assert.That(requestAfterFirstDispatch, Is.EqualTo(requestBefore + 2));
            Assert.That(streamAfterFirstDispatch, Is.EqualTo(streamBefore + 1));

            Assert.That(notificationBindings.Count, Is.EqualTo(notificationAfterFirstDispatch));
            Assert.That(requestBindings.Count, Is.EqualTo(requestAfterFirstDispatch));
            Assert.That(streamBindings.Count, Is.EqualTo(streamAfterFirstDispatch));
        });
    }

    /// <summary>
    ///     验证 request dispatch binding 会按响应类型分别缓存，避免不同响应类型共用 object 结果桥接。
    /// </summary>
    [Test]
    public async Task Dispatcher_Should_Cache_Request_Dispatch_Bindings_Per_Response_Type()
    {
        var intRequestBindings = GetGenericCacheField("RequestDispatchBindingCache`1", typeof(int), "Bindings");
        var stringRequestBindings = GetGenericCacheField("RequestDispatchBindingCache`1", typeof(string), "Bindings");

        var intBefore = intRequestBindings.Count;
        var stringBefore = stringRequestBindings.Count;

        await _context!.SendRequestAsync(new DispatcherCacheRequest());
        await _context.SendRequestAsync(new DispatcherStringCacheRequest());

        var intAfterFirstDispatch = intRequestBindings.Count;
        var stringAfterFirstDispatch = stringRequestBindings.Count;

        await _context.SendRequestAsync(new DispatcherCacheRequest());
        await _context.SendRequestAsync(new DispatcherStringCacheRequest());

        Assert.Multiple(() =>
        {
            Assert.That(intAfterFirstDispatch, Is.EqualTo(intBefore + 1));
            Assert.That(stringAfterFirstDispatch, Is.EqualTo(stringBefore + 1));
            Assert.That(intRequestBindings.Count, Is.EqualTo(intAfterFirstDispatch));
            Assert.That(stringRequestBindings.Count, Is.EqualTo(stringAfterFirstDispatch));
        });
    }

    /// <summary>
    ///     通过反射读取 dispatcher 的静态缓存字典。
    /// </summary>
    private static IDictionary GetCacheField(string fieldName)
    {
        var dispatcherType = GetDispatcherType();
        var field = dispatcherType.GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(field, Is.Not.Null, $"Missing dispatcher cache field {fieldName}.");

        return field!.GetValue(null) as IDictionary
               ?? throw new InvalidOperationException(
                   $"Dispatcher cache field {fieldName} does not implement IDictionary.");
    }

    /// <summary>
    ///     清空本测试依赖的 dispatcher 静态缓存，避免跨用例共享进程级状态导致断言漂移。
    /// </summary>
    private static void ClearDispatcherCaches()
    {
        GetCacheField("NotificationDispatchBindings").Clear();
        GetCacheField("StreamDispatchBindings").Clear();
        GetGenericCacheField("RequestDispatchBindingCache`1", typeof(int), "Bindings").Clear();
        GetGenericCacheField("RequestDispatchBindingCache`1", typeof(string), "Bindings").Clear();
    }

    /// <summary>
    ///     通过反射读取 dispatcher 嵌套泛型缓存类型上的静态缓存字典。
    /// </summary>
    private static IDictionary GetGenericCacheField(string nestedTypeName, Type genericTypeArgument, string fieldName)
    {
        var nestedGenericType = GetDispatcherType().GetNestedType(
            nestedTypeName,
            BindingFlags.NonPublic);

        Assert.That(nestedGenericType, Is.Not.Null, $"Missing dispatcher nested cache type {nestedTypeName}.");

        var closedNestedType = nestedGenericType!.MakeGenericType(genericTypeArgument);
        var field = closedNestedType.GetField(
            fieldName,
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.That(
            field,
            Is.Not.Null,
            $"Missing dispatcher nested cache field {nestedTypeName}.{fieldName} for {genericTypeArgument.FullName}.");

        return field!.GetValue(null) as IDictionary
               ?? throw new InvalidOperationException(
                   $"Dispatcher nested cache field {nestedTypeName}.{fieldName} does not implement IDictionary.");
    }

    /// <summary>
    ///     获取 CQRS dispatcher 运行时类型。
    /// </summary>
    private static Type GetDispatcherType()
    {
        return typeof(CqrsReflectionFallbackAttribute).Assembly
            .GetType("GFramework.Cqrs.Internal.CqrsDispatcher", throwOnError: true)!;
    }

    /// <summary>
    ///     消费整个异步流，确保建流路径被真实执行。
    /// </summary>
    private static async Task DrainAsync<T>(IAsyncEnumerable<T> stream)
    {
        await foreach (var _ in stream)
        {
        }
    }
}

/// <summary>
///     用于验证 request 服务类型缓存的测试请求。
/// </summary>
internal sealed record DispatcherCacheRequest : IRequest<int>;

/// <summary>
///     用于验证 notification 服务类型缓存的测试通知。
/// </summary>
internal sealed record DispatcherCacheNotification : INotification;

/// <summary>
///     用于验证 stream 服务类型缓存的测试请求。
/// </summary>
internal sealed record DispatcherCacheStreamRequest : IStreamRequest<int>;

/// <summary>
///     用于验证 pipeline invoker 缓存的测试请求。
/// </summary>
internal sealed record DispatcherPipelineCacheRequest : IRequest<int>;

/// <summary>
///     用于验证按响应类型分层 request invoker 缓存的测试请求。
/// </summary>
internal sealed record DispatcherStringCacheRequest : IRequest<string>;

/// <summary>
///     处理 <see cref="DispatcherCacheRequest" />。
/// </summary>
internal sealed class DispatcherCacheRequestHandler : IRequestHandler<DispatcherCacheRequest, int>
{
    /// <summary>
    ///     返回固定结果，供缓存测试验证 dispatcher 请求路径。
    /// </summary>
    public ValueTask<int> Handle(DispatcherCacheRequest request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(1);
    }
}

/// <summary>
///     处理 <see cref="DispatcherCacheNotification" />。
/// </summary>
internal sealed class DispatcherCacheNotificationHandler : INotificationHandler<DispatcherCacheNotification>
{
    /// <summary>
    ///     消费通知，不执行额外副作用。
    /// </summary>
    public ValueTask Handle(DispatcherCacheNotification notification, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}

/// <summary>
///     处理 <see cref="DispatcherCacheStreamRequest" />。
/// </summary>
internal sealed class DispatcherCacheStreamHandler : IStreamRequestHandler<DispatcherCacheStreamRequest, int>
{
    /// <summary>
    ///     返回一个最小流，供缓存测试命中 stream 分发路径。
    /// </summary>
    public async IAsyncEnumerable<int> Handle(
        DispatcherCacheStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return 1;
        await Task.CompletedTask;
    }
}

/// <summary>
///     处理 <see cref="DispatcherPipelineCacheRequest" />。
/// </summary>
internal sealed class DispatcherPipelineCacheRequestHandler : IRequestHandler<DispatcherPipelineCacheRequest, int>
{
    /// <summary>
    ///     返回固定结果，供 pipeline 缓存测试使用。
    /// </summary>
    public ValueTask<int> Handle(DispatcherPipelineCacheRequest request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(2);
    }
}

/// <summary>
///     处理 <see cref="DispatcherStringCacheRequest" />。
/// </summary>
internal sealed class DispatcherStringCacheRequestHandler : IRequestHandler<DispatcherStringCacheRequest, string>
{
    /// <summary>
    ///     返回固定字符串，供按响应类型缓存测试验证 string 路径。
    /// </summary>
    public ValueTask<string> Handle(DispatcherStringCacheRequest request, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult("dispatcher-cache");
    }
}

/// <summary>
///     为 <see cref="DispatcherPipelineCacheRequest" /> 提供最小 pipeline 行为，
///     用于命中 dispatcher 的 pipeline invoker 缓存分支。
/// </summary>
internal sealed class DispatcherPipelineCacheBehavior : IPipelineBehavior<DispatcherPipelineCacheRequest, int>
{
    /// <summary>
    ///     直接转发到下一个处理器。
    /// </summary>
    public ValueTask<int> Handle(
        DispatcherPipelineCacheRequest request,
        MessageHandlerDelegate<DispatcherPipelineCacheRequest, int> next,
        CancellationToken cancellationToken)
    {
        return next(request, cancellationToken);
    }
}
