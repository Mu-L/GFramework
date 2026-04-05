using GFramework.Core.Abstractions.Coroutine;
using GFramework.Core.Coroutine;
using GFramework.Core.Coroutine.Instructions;

namespace GFramework.Core.Tests.Coroutine;

/// <summary>
///     协程调度器增强行为测试。
/// </summary>
[TestFixture]
public sealed class CoroutineSchedulerAdvancedTests
{
    /// <summary>
    ///     验证 WaitForSecondsRealtime 使用独立的真实时间源推进。
    /// </summary>
    [Test]
    public void WaitForSecondsRealtime_Should_Use_Realtime_TimeSource_When_Provided()
    {
        var scaledTime = new FakeTimeSource();
        var realtimeTime = new FakeTimeSource();
        var scheduler = new CoroutineScheduler(
            scaledTime,
            realtimeTimeSource: realtimeTime);
        var completed = false;

        IEnumerator<IYieldInstruction> Coroutine()
        {
            yield return new WaitForSecondsRealtime(1.0);
            completed = true;
        }

        scheduler.Run(Coroutine());

        scaledTime.Advance(0.1);
        realtimeTime.Advance(0.4);
        scheduler.Update();

        Assert.That(completed, Is.False);
        Assert.That(scheduler.ActiveCoroutineCount, Is.EqualTo(1));

        scaledTime.Advance(0.1);
        realtimeTime.Advance(0.6);
        scheduler.Update();

        Assert.That(completed, Is.True);
        Assert.That(scheduler.ActiveCoroutineCount, Is.EqualTo(0));
    }

    /// <summary>
    ///     验证固定更新等待指令仅在固定阶段调度器中推进。
    /// </summary>
    [Test]
    public void WaitForFixedUpdate_Should_Only_Advance_On_FixedUpdate_Scheduler()
    {
        var defaultTime = new FakeTimeSource();
        var fixedTime = new FakeTimeSource();
        var defaultScheduler = new CoroutineScheduler(defaultTime);
        var fixedScheduler = new CoroutineScheduler(
            fixedTime,
            executionStage: CoroutineExecutionStage.FixedUpdate);
        var defaultCompleted = false;
        var fixedCompleted = false;

        IEnumerator<IYieldInstruction> DefaultCoroutine()
        {
            yield return new WaitForFixedUpdate();
            defaultCompleted = true;
        }

        IEnumerator<IYieldInstruction> FixedCoroutine()
        {
            yield return new WaitForFixedUpdate();
            fixedCompleted = true;
        }

        defaultScheduler.Run(DefaultCoroutine());
        fixedScheduler.Run(FixedCoroutine());

        defaultTime.Advance(0.1);
        fixedTime.Advance(0.1);
        defaultScheduler.Update();
        fixedScheduler.Update();

        Assert.That(defaultCompleted, Is.False);
        Assert.That(defaultScheduler.ActiveCoroutineCount, Is.EqualTo(1));
        Assert.That(fixedCompleted, Is.True);
        Assert.That(fixedScheduler.ActiveCoroutineCount, Is.EqualTo(0));
    }

    /// <summary>
    ///     验证取消令牌会在下一次调度循环中终止协程并记录结果。
    /// </summary>
    [Test]
    public async Task CancellationToken_Should_Cancel_Coroutine_On_Next_Update()
    {
        var timeSource = new FakeTimeSource();
        var scheduler = new CoroutineScheduler(timeSource);
        using var cancellationTokenSource = new CancellationTokenSource();

        IEnumerator<IYieldInstruction> Coroutine()
        {
            yield return new Delay(10);
        }

        var handle = scheduler.Run(Coroutine(), cancellationToken: cancellationTokenSource.Token);
        cancellationTokenSource.Cancel();

        timeSource.Advance(0.1);
        scheduler.Update();

        var status = await scheduler.WaitForCompletionAsync(handle);

        Assert.That(scheduler.IsCoroutineAlive(handle), Is.False);
        Assert.That(status, Is.EqualTo(CoroutineCompletionStatus.Cancelled));
    }

    /// <summary>
    ///     验证调度器可以暴露活跃协程快照。
    /// </summary>
    [Test]
    public void TryGetSnapshot_Should_Return_Current_Waiting_Instruction_And_Stage()
    {
        var timeSource = new FakeTimeSource();
        var scheduler = new CoroutineScheduler(
            timeSource,
            executionStage: CoroutineExecutionStage.EndOfFrame);

        IEnumerator<IYieldInstruction> Coroutine()
        {
            yield return new WaitForEndOfFrame();
        }

        var handle = scheduler.Run(Coroutine(), tag: "ui", group: "frame-end");

        var found = scheduler.TryGetSnapshot(handle, out var snapshot);

        Assert.That(found, Is.True);
        Assert.That(snapshot.Handle, Is.EqualTo(handle));
        Assert.That(snapshot.Tag, Is.EqualTo("ui"));
        Assert.That(snapshot.Group, Is.EqualTo("frame-end"));
        Assert.That(snapshot.IsWaiting, Is.True);
        Assert.That(snapshot.WaitingInstructionType, Is.EqualTo(typeof(WaitForEndOfFrame)));
        Assert.That(snapshot.ExecutionStage, Is.EqualTo(CoroutineExecutionStage.EndOfFrame));
    }

    /// <summary>
    ///     验证异常结束的协程会记录为 Faulted。
    /// </summary>
    [Test]
    public async Task WaitForCompletionAsync_Should_Return_Faulted_For_Failing_Coroutine()
    {
        var timeSource = new FakeTimeSource();
        var scheduler = new CoroutineScheduler(timeSource);

        IEnumerator<IYieldInstruction> Coroutine()
        {
            yield return new WaitOneFrame();
            throw new InvalidOperationException("boom");
#pragma warning disable CS0162
            yield break;
#pragma warning restore CS0162
        }

        var handle = scheduler.Run(Coroutine());
        timeSource.Advance(0.1);
        scheduler.Update();
        timeSource.Advance(0.1);
        scheduler.Update();

        var status = await scheduler.WaitForCompletionAsync(handle);

        Assert.That(status, Is.EqualTo(CoroutineCompletionStatus.Faulted));
    }
}