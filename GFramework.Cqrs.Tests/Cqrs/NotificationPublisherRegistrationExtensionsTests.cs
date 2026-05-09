// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Core.Architectures;
using GFramework.Core.Abstractions.Logging;
using GFramework.Core.Ioc;
using GFramework.Core.Logging;
using GFramework.Cqrs.Abstractions.Cqrs;
using GFramework.Cqrs.Extensions;
using GFramework.Cqrs.Notification;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     验证 notification publisher 组合根注册扩展的关键行为。
/// </summary>
[TestFixture]
internal sealed class NotificationPublisherRegistrationExtensionsTests
{
    /// <summary>
    ///     验证显式注册内置 <see cref="TaskWhenAllNotificationPublisher" /> 后，
    ///     标准 runtime 基础设施会复用该策略并继续调度所有处理器。
    /// </summary>
    [Test]
    public async Task UseTaskWhenAllNotificationPublisher_Should_Be_Used_By_Default_Runtime_Infrastructure()
    {
        LoggerFactoryResolver.Provider = new ConsoleLoggerFactoryProvider();

        var trailingHandler = new RecordingNotificationHandler();
        var container = new MicrosoftDiContainer();
        container.UseTaskWhenAllNotificationPublisher();
        container.Register<INotificationHandler<TestNotification>>(new ThrowingNotificationHandler());
        container.Register<INotificationHandler<TestNotification>>(trailingHandler);
        CqrsTestRuntime.RegisterInfrastructure(container);
        container.Freeze();

        var context = new ArchitectureContext(container);
        var publishTask = context.PublishAsync(new TestNotification()).AsTask();

        try
        {
            await publishTask.ConfigureAwait(false);
        }
        catch (Exception)
        {
            // `TaskWhenAll` 策略会在所有处理器都结束后聚合失败；这里仅消费异常并继续断言第二个处理器已执行。
        }

        Assert.That(trailingHandler.WasInvoked, Is.True);
        Assert.That(publishTask.Exception, Is.Not.Null);
    }

    /// <summary>
    ///     验证显式注册内置 <see cref="SequentialNotificationPublisher" /> 后，
    ///     默认 runtime 基础设施会保留“首个失败立即停止后续处理器”的顺序语义。
    /// </summary>
    [Test]
    public void UseSequentialNotificationPublisher_Should_Preserve_Stop_On_First_Failure_Semantics()
    {
        LoggerFactoryResolver.Provider = new ConsoleLoggerFactoryProvider();

        var trailingHandler = new RecordingNotificationHandler();
        var container = new MicrosoftDiContainer();
        container.UseSequentialNotificationPublisher();
        container.Register<INotificationHandler<TestNotification>>(new ThrowingNotificationHandler());
        container.Register<INotificationHandler<TestNotification>>(trailingHandler);
        CqrsTestRuntime.RegisterInfrastructure(container);
        container.Freeze();

        var context = new ArchitectureContext(container);

        Assert.That(
            async () => await context.PublishAsync(new TestNotification()).ConfigureAwait(false),
            Throws.InvalidOperationException.With.Message.EqualTo("boom"));
        Assert.That(trailingHandler.WasInvoked, Is.False);
        Assert.That(container.GetRequired<INotificationPublisher>(), Is.TypeOf<SequentialNotificationPublisher>());
    }

    /// <summary>
    ///     验证显式传入实例的组合根注册入口会把同一个 publisher 实例绑定到容器。
    /// </summary>
    [Test]
    public void UseNotificationPublisher_Instance_Overload_Should_Register_Same_Instance()
    {
        var container = new MicrosoftDiContainer();
        var publisher = new TrackingNotificationPublisher();

        var returnedContainer = container.UseNotificationPublisher(publisher);

        Assert.That(returnedContainer, Is.SameAs(container));
        Assert.That(container.Get<INotificationPublisher>(), Is.SameAs(publisher));
    }

    /// <summary>
    ///     验证泛型组合根注册入口会把指定的 publisher 类型注册为容器内唯一的单例策略。
    /// </summary>
    [Test]
    public void UseNotificationPublisher_Generic_Overload_Should_Register_Configured_Type()
    {
        var container = new MicrosoftDiContainer();

        var returnedContainer = container.UseNotificationPublisher<TrackingNotificationPublisher>();
        container.Freeze();

        Assert.That(returnedContainer, Is.SameAs(container));
        Assert.That(container.HasRegistration(typeof(INotificationPublisher)), Is.True);
        Assert.That(container.GetRequired<INotificationPublisher>(), Is.TypeOf<TrackingNotificationPublisher>());
        Assert.That(container.GetRequired<INotificationPublisher>(), Is.SameAs(container.GetRequired<INotificationPublisher>()));
    }

    /// <summary>
    ///     验证组合根扩展会阻止重复 notification publisher 注册，避免 runtime 创建阶段才暴露歧义。
    /// </summary>
    [Test]
    public void UseNotificationPublisher_Should_Throw_When_NotificationPublisher_Already_Registered()
    {
        var container = new MicrosoftDiContainer();
        container.UseTaskWhenAllNotificationPublisher();

        Assert.That(
            () => container.UseNotificationPublisher(new TrackingNotificationPublisher()),
            Throws.InvalidOperationException.With.Message.Contains(nameof(INotificationPublisher)));
    }

    /// <summary>
    ///     验证当容器已存在 notification publisher 注册时，泛型组合根入口也会拒绝重复策略声明。
    /// </summary>
    [Test]
    public void UseNotificationPublisher_Generic_Overload_Should_Throw_When_NotificationPublisher_Already_Registered()
    {
        var container = new MicrosoftDiContainer();
        container.UseSequentialNotificationPublisher();

        Assert.That(
            () => container.UseNotificationPublisher<TrackingNotificationPublisher>(),
            Throws.InvalidOperationException.With.Message.Contains(nameof(INotificationPublisher)));
    }

    /// <summary>
    ///     为本组测试提供最小 notification 类型。
    /// </summary>
    private sealed record TestNotification : INotification;

    /// <summary>
    ///     记录自己是否被执行的测试处理器。
    /// </summary>
    private sealed class RecordingNotificationHandler : INotificationHandler<TestNotification>
    {
        /// <summary>
        ///     获取当前处理器是否至少执行过一次。
        /// </summary>
        public bool WasInvoked { get; private set; }

        /// <summary>
        ///     记录执行痕迹并立刻完成。
        /// </summary>
        public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            cancellationToken.ThrowIfCancellationRequested();
            WasInvoked = true;
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    ///     始终抛出异常的测试处理器，用于验证并行策略不会因为首个失败而停止其余处理器。
    /// </summary>
    private sealed class ThrowingNotificationHandler : INotificationHandler<TestNotification>
    {
        /// <summary>
        ///     始终抛出测试异常。
        /// </summary>
        public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            cancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException("boom");
        }
    }

    /// <summary>
    ///     用于验证实例注册重载是否保留原对象身份的测试发布器。
    /// </summary>
    private sealed class TrackingNotificationPublisher : INotificationPublisher
    {
        /// <summary>
        ///     直接完成当前 publish 调用。
        /// </summary>
        public ValueTask PublishAsync<TNotification>(
            NotificationPublishContext<TNotification> context,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            ArgumentNullException.ThrowIfNull(context);
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }
    }
}
