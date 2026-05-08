// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GFramework.Core.Abstractions.Ioc;
using GFramework.Core.Abstractions.Logging;
using GFramework.Cqrs.Abstractions.Cqrs;
using GFramework.Cqrs.Cqrs;
using GFramework.Cqrs.Tests.Logging;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     验证默认 dispatcher 在上下文注入前置条件不满足时的失败语义。
/// </summary>
[TestFixture]
internal sealed class CqrsDispatcherContextValidationTests
{
    /// <summary>
    ///     验证当 request handler 需要上下文注入、但当前 CQRS 上下文不实现 <see cref="GFramework.Core.Abstractions.Architectures.IArchitectureContext" /> 时，
    ///     dispatcher 会在调用前显式失败。
    /// </summary>
    [Test]
    public void SendAsync_Should_Throw_When_Context_Does_Not_Implement_IArchitectureContext()
    {
        var runtime = CreateRuntime(
            container =>
            {
                container
                    .Setup(currentContainer => currentContainer.Get(typeof(IRequestHandler<ContextAwareRequest, int>)))
                    .Returns(new ContextAwareRequestHandler());
                container
                    .Setup(currentContainer => currentContainer.HasRegistration(typeof(IPipelineBehavior<ContextAwareRequest, int>)))
                    .Returns(false);
                container
                    .Setup(currentContainer => currentContainer.GetAll(typeof(IPipelineBehavior<ContextAwareRequest, int>)))
                    .Returns(Array.Empty<object>());
            });

        Assert.That(
            async () => await runtime.SendAsync(new FakeCqrsContext(), new ContextAwareRequest()).ConfigureAwait(false),
            Throws.InvalidOperationException.With.Message.Contains("does not implement IArchitectureContext"));
    }

    /// <summary>
    ///     验证 request 上下文校验失败时，<see cref="GFramework.Cqrs.Abstractions.Cqrs.ICqrsRuntime.SendAsync{TResponse}" />
    ///     不会在调用点同步抛出，而是返回一个 faulted <see cref="ValueTask{TResult}" /> 保持既有异步失败语义。
    /// </summary>
    [Test]
    public void SendAsync_Should_Return_Faulted_ValueTask_When_Context_Preparation_Fails()
    {
        var runtime = CreateRuntime(
            container =>
            {
                container
                    .Setup(currentContainer => currentContainer.Get(typeof(IRequestHandler<ContextAwareRequest, int>)))
                    .Returns(new ContextAwareRequestHandler());
                container
                    .Setup(currentContainer => currentContainer.HasRegistration(typeof(IPipelineBehavior<ContextAwareRequest, int>)))
                    .Returns(false);
            });

        ValueTask<int> dispatch = default;
        Assert.That(
            () => { dispatch = runtime.SendAsync(new FakeCqrsContext(), new ContextAwareRequest()); },
            Throws.Nothing);
        Assert.That(
            async () => await dispatch.ConfigureAwait(false),
            Throws.InvalidOperationException.With.Message.Contains("does not implement IArchitectureContext"));
    }

    /// <summary>
    ///     验证 request handler 缺失时，dispatcher 仍返回 faulted <see cref="ValueTask{TResult}" />，
    ///     而不是在调用点同步抛出异常。
    /// </summary>
    [Test]
    public void SendAsync_Should_Return_Faulted_ValueTask_When_Handler_Is_Missing()
    {
        var runtime = CreateRuntime(
            container =>
            {
                container
                    .Setup(currentContainer => currentContainer.Get(typeof(IRequestHandler<ContextAwareRequest, int>)))
                    .Returns((object?)null);
            });

        ValueTask<int> dispatch = default;
        Assert.That(
            () => { dispatch = runtime.SendAsync(new FakeCqrsContext(), new ContextAwareRequest()); },
            Throws.Nothing);
        Assert.That(
            async () => await dispatch.ConfigureAwait(false),
            Throws.InvalidOperationException.With.Message.Contains("No CQRS request handler registered"));
    }

    /// <summary>
    ///     验证当 notification handler 需要上下文注入、但当前 CQRS 上下文不实现 <see cref="GFramework.Core.Abstractions.Architectures.IArchitectureContext" /> 时，
    ///     dispatcher 会在发布前显式失败。
    /// </summary>
    [Test]
    public void PublishAsync_Should_Throw_When_Context_Does_Not_Implement_IArchitectureContext()
    {
        var runtime = CreateRuntime(
            container =>
            {
                container
                    .Setup(currentContainer => currentContainer.GetAll(typeof(INotificationHandler<ContextAwareNotification>)))
                    .Returns([new ContextAwareNotificationHandler()]);
            });

        Assert.That(
            async () => await runtime.PublishAsync(new FakeCqrsContext(), new ContextAwareNotification()).ConfigureAwait(false),
            Throws.InvalidOperationException.With.Message.Contains("does not implement IArchitectureContext"));
    }

    /// <summary>
    ///     验证当 stream handler 需要上下文注入、但当前 CQRS 上下文不实现 <see cref="GFramework.Core.Abstractions.Architectures.IArchitectureContext" /> 时，
    ///     dispatcher 会在建流前显式失败。
    /// </summary>
    [Test]
    public void CreateStream_Should_Throw_When_Context_Does_Not_Implement_IArchitectureContext()
    {
        var runtime = CreateRuntime(
            container =>
            {
                container
                    .Setup(currentContainer => currentContainer.Get(typeof(IStreamRequestHandler<ContextAwareStreamRequest, int>)))
                    .Returns(new ContextAwareStreamHandler());
                container
                    .Setup(currentContainer => currentContainer.HasRegistration(typeof(IStreamPipelineBehavior<ContextAwareStreamRequest, int>)))
                    .Returns(false);
                container
                    .Setup(currentContainer => currentContainer.GetAll(typeof(IStreamPipelineBehavior<ContextAwareStreamRequest, int>)))
                    .Returns(Array.Empty<object>());
            });

        Assert.That(
            () => runtime.CreateStream(new FakeCqrsContext(), new ContextAwareStreamRequest()),
            Throws.InvalidOperationException.With.Message.Contains("does not implement IArchitectureContext"));
    }

    /// <summary>
    ///     验证当 stream pipeline behavior 需要上下文注入、但当前 CQRS 上下文不实现
    ///     <see cref="GFramework.Core.Abstractions.Architectures.IArchitectureContext" /> 时，
    ///     dispatcher 会在建流前显式失败。
    /// </summary>
    [Test]
    public void CreateStream_Should_Throw_When_Stream_Pipeline_Behavior_Context_Does_Not_Implement_IArchitectureContext()
    {
        var runtime = CreateRuntime(
            container =>
            {
                container
                    .Setup(currentContainer => currentContainer.Get(typeof(IStreamRequestHandler<ContextAwareStreamRequest, int>)))
                    .Returns(new PassthroughStreamHandler());
                container
                    .Setup(currentContainer => currentContainer.HasRegistration(typeof(IStreamPipelineBehavior<ContextAwareStreamRequest, int>)))
                    .Returns(true);
                container
                    .Setup(currentContainer => currentContainer.GetAll(typeof(IStreamPipelineBehavior<ContextAwareStreamRequest, int>)))
                    .Returns([new ContextAwareStreamBehavior()]);
            });

        Assert.That(
            () => runtime.CreateStream(new FakeCqrsContext(), new ContextAwareStreamRequest()),
            Throws.InvalidOperationException.With.Message.Contains("does not implement IArchitectureContext"));
    }

    /// <summary>
    ///     创建一个只满足当前测试最小依赖面的 dispatcher runtime。
    /// </summary>
    /// <param name="configureContainer">对容器 mock 的额外配置。</param>
    /// <returns>默认 CQRS runtime。</returns>
    private static GFramework.Cqrs.Abstractions.Cqrs.ICqrsRuntime CreateRuntime(
        Action<Mock<IIocContainer>> configureContainer)
    {
        var container = new Mock<IIocContainer>(MockBehavior.Strict);
        var logger = new TestLogger("CqrsDispatcherContextValidationTests", LogLevel.Debug);

        configureContainer(container);
        return CqrsRuntimeFactory.CreateRuntime(container.Object, logger);
    }

    /// <summary>
    ///     为失败语义测试提供最小 CQRS 上下文标记，但故意不实现架构上下文能力。
    /// </summary>
    private sealed class FakeCqrsContext : ICqrsContext
    {
    }

    /// <summary>
    ///     为 request 上下文校验提供最小测试请求。
    /// </summary>
    private sealed record ContextAwareRequest : IRequest<int>;

    /// <summary>
    ///     为 notification 上下文校验提供最小测试通知。
    /// </summary>
    private sealed record ContextAwareNotification : INotification;

    /// <summary>
    ///     为 stream 上下文校验提供最小测试请求。
    /// </summary>
    private sealed record ContextAwareStreamRequest : IStreamRequest<int>;

    /// <summary>
    ///     为 request 上下文校验提供需要注入架构上下文的最小 handler。
    /// </summary>
    private sealed class ContextAwareRequestHandler : CqrsContextAwareHandlerBase, IRequestHandler<ContextAwareRequest, int>
    {
        /// <summary>
        ///     返回固定结果；当前测试只关心调用前的上下文校验。
        /// </summary>
        /// <param name="request">当前请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>固定整型结果。</returns>
        public ValueTask<int> Handle(ContextAwareRequest request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(1);
        }
    }

    /// <summary>
    ///     为 notification 上下文校验提供需要注入架构上下文的最小 handler。
    /// </summary>
    private sealed class ContextAwareNotificationHandler
        : CqrsContextAwareHandlerBase,
            INotificationHandler<ContextAwareNotification>
    {
        /// <summary>
        ///     返回已完成任务；当前测试只关心调用前的上下文校验。
        /// </summary>
        /// <param name="notification">当前通知。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>已完成任务。</returns>
        public ValueTask Handle(ContextAwareNotification notification, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    ///     为 stream 上下文校验提供需要注入架构上下文的最小 handler。
    /// </summary>
    private sealed class ContextAwareStreamHandler
        : CqrsContextAwareHandlerBase,
            IStreamRequestHandler<ContextAwareStreamRequest, int>
    {
        /// <summary>
        ///     返回一个最小流；当前测试只关心建流前的上下文校验。
        /// </summary>
        /// <param name="request">当前流请求。</param>
        /// <param name="cancellationToken">取消枚举时使用的取消令牌。</param>
        /// <returns>包含单个固定元素的异步流。</returns>
        public async IAsyncEnumerable<int> Handle(
            ContextAwareStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return 1;
            await ValueTask.CompletedTask.ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     为 stream behavior 上下文校验提供不依赖上下文注入的最小 handler。
    /// </summary>
    private sealed class PassthroughStreamHandler : IStreamRequestHandler<ContextAwareStreamRequest, int>
    {
        /// <summary>
        ///     返回一个最小流；当前测试只关心 behavior 注入前的上下文校验。
        /// </summary>
        /// <param name="request">当前流请求。</param>
        /// <param name="cancellationToken">取消枚举时使用的取消令牌。</param>
        /// <returns>包含单个固定元素的异步流。</returns>
        public async IAsyncEnumerable<int> Handle(
            ContextAwareStreamRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return 1;
            await ValueTask.CompletedTask.ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     为 stream behavior 上下文校验提供需要注入架构上下文的最小 behavior。
    /// </summary>
    private sealed class ContextAwareStreamBehavior
        : CqrsContextAwareHandlerBase,
            IStreamPipelineBehavior<ContextAwareStreamRequest, int>
    {
        /// <summary>
        ///     直接转发到下一个处理阶段；当前测试只关心调用前的上下文校验。
        /// </summary>
        /// <param name="message">当前流式请求。</param>
        /// <param name="next">下一个处理阶段。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>下游处理阶段返回的异步流。</returns>
        public IAsyncEnumerable<int> Handle(
            ContextAwareStreamRequest message,
            StreamMessageHandlerDelegate<ContextAwareStreamRequest, int> next,
            CancellationToken cancellationToken)
        {
            return next(message, cancellationToken);
        }
    }
}
