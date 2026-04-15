using System.Reflection;
using GFramework.Core.Abstractions.Cqrs;
using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Architectures;
using GFramework.Core.Logging;

namespace GFramework.Core.Tests.Architectures;

/// <summary>
///     验证架构初始化阶段可以显式接入默认程序集之外的 CQRS handlers。
/// </summary>
[TestFixture]
public sealed class ArchitectureAdditionalCqrsHandlersTests
{
    /// <summary>
    ///     初始化日志工厂和共享测试状态。
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        LoggerFactoryResolver.Provider = new ConsoleLoggerFactoryProvider();
        GameContext.Clear();
        AdditionalAssemblyNotificationHandler.Reset();
    }

    /// <summary>
    ///     清理测试过程中写入的共享状态。
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        AdditionalAssemblyNotificationHandler.Reset();
        GameContext.Clear();
    }

    /// <summary>
    ///     验证显式声明的额外程序集会在初始化阶段接入当前架构容器。
    /// </summary>
    [Test]
    public async Task RegisterCqrsHandlersFromAssembly_Should_Register_Handlers_From_Explicit_Assembly()
    {
        var generatedAssembly = CreateGeneratedHandlerAssembly();
        var architecture = new AdditionalHandlersTestArchitecture(target =>
            target.RegisterCqrsHandlersFromAssembly(generatedAssembly.Object));

        await architecture.InitializeAsync();
        await architecture.Context.PublishAsync(new AdditionalAssemblyNotification());

        Assert.That(AdditionalAssemblyNotificationHandler.InvocationCount, Is.EqualTo(1));

        await architecture.DestroyAsync();
    }

    /// <summary>
    ///     验证同一额外程序集被重复声明时，不会向容器重复写入相同 handler 映射。
    /// </summary>
    [Test]
    public async Task RegisterCqrsHandlersFromAssembly_Should_Deduplicate_Repeated_Assembly_Registration()
    {
        var generatedAssembly = CreateGeneratedHandlerAssembly();
        var architecture = new AdditionalHandlersTestArchitecture(target =>
        {
            target.RegisterCqrsHandlersFromAssembly(generatedAssembly.Object);
            target.RegisterCqrsHandlersFromAssemblies([generatedAssembly.Object]);
        });

        await architecture.InitializeAsync();
        await architecture.Context.PublishAsync(new AdditionalAssemblyNotification());

        Assert.That(AdditionalAssemblyNotificationHandler.InvocationCount, Is.EqualTo(1));

        await architecture.DestroyAsync();
    }

    /// <summary>
    ///     创建一个仅暴露程序集级 CQRS registry 元数据的 mocked Assembly。
    ///     该测试替身模拟“扩展程序集已经挂接 source-generator，运行时只需显式接入该程序集”的真实路径。
    /// </summary>
    /// <returns>包含程序集级 handler registry 元数据的 mocked Assembly。</returns>
    private static Mock<Assembly> CreateGeneratedHandlerAssembly()
    {
        var generatedAssembly = new Mock<Assembly>();
        generatedAssembly
            .SetupGet(static assembly => assembly.FullName)
            .Returns("GFramework.Core.Tests.Architectures.ExplicitAdditionalHandlers, Version=1.0.0.0");
        generatedAssembly
            .Setup(static assembly => assembly.GetCustomAttributes(typeof(CqrsHandlerRegistryAttribute), false))
            .Returns([new CqrsHandlerRegistryAttribute(typeof(AdditionalAssemblyNotificationHandlerRegistry))]);
        return generatedAssembly;
    }

    /// <summary>
    ///     用于测试额外程序集注册入口的最小架构实现。
    /// </summary>
    private sealed class AdditionalHandlersTestArchitecture(Action<AdditionalHandlersTestArchitecture> configure) :
        Architecture
    {
        /// <summary>
        ///     在初始化阶段执行测试注入的额外 CQRS 程序集接入逻辑。
        /// </summary>
        protected override void OnInitialize()
        {
            configure(this);
        }
    }
}

/// <summary>
///     用于验证额外程序集接入是否成功的测试通知。
/// </summary>
public sealed record AdditionalAssemblyNotification : INotification;

/// <summary>
///     由模拟扩展程序集的生成注册器挂入当前容器的通知处理器。
/// </summary>
public sealed class AdditionalAssemblyNotificationHandler : INotificationHandler<AdditionalAssemblyNotification>
{
    /// <summary>
    ///     获取当前测试进程中该处理器的执行次数。
    /// </summary>
    public static int InvocationCount { get; private set; }

    /// <summary>
    ///     记录一次通知处理，供测试断言显式程序集接入后的运行时行为。
    /// </summary>
    /// <param name="notification">通知实例。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>已完成任务。</returns>
    public ValueTask Handle(AdditionalAssemblyNotification notification, CancellationToken cancellationToken)
    {
        InvocationCount++;
        return ValueTask.CompletedTask;
    }

    /// <summary>
    ///     清理共享计数器，避免测试间相互污染。
    /// </summary>
    public static void Reset()
    {
        InvocationCount = 0;
    }
}

/// <summary>
///     模拟由 source-generator 为扩展程序集生成的 CQRS handler registry。
/// </summary>
internal sealed class AdditionalAssemblyNotificationHandlerRegistry : ICqrsHandlerRegistry
{
    /// <summary>
    ///     将扩展程序集中的通知处理器映射写入服务集合。
    /// </summary>
    /// <param name="services">目标服务集合。</param>
    /// <param name="logger">日志记录器。</param>
    public void Register(IServiceCollection services, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(logger);

        services
            .AddTransient<INotificationHandler<AdditionalAssemblyNotification>,
                AdditionalAssemblyNotificationHandler>();
        logger.Debug(
            $"Registered CQRS handler {typeof(AdditionalAssemblyNotificationHandler).FullName} as {typeof(INotificationHandler<AdditionalAssemblyNotification>).FullName}.");
    }
}
