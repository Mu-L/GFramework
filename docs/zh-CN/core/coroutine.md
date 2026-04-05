---
title: 协程系统
description: 基于 IEnumerator<IYieldInstruction> 的协程调度系统，支持时间等待、阶段等待、Task 桥接、事件等待与运行时快照查询。
---

# 协程系统

## 概述

`GFramework.Core.Coroutine` 提供一个宿主无关的协程内核。它围绕 `CoroutineScheduler` 工作，统一处理：

- `IEnumerator<IYieldInstruction>` 形式的协程推进
- 时间等待、条件等待、Task 等待与事件等待
- 标签、分组、暂停、恢复与终止
- 取消令牌、完成状态查询与运行快照
- 调度阶段语义，例如默认更新、固定更新和帧结束

Core 协程本身不依赖任何具体引擎；阶段语义是否真实成立，取决于宿主是否为调度器提供了匹配的执行阶段。

## CoroutineScheduler

### 基础创建

```csharp
using GFramework.Core.Abstractions.Coroutine;
using GFramework.Core.Coroutine;

ITimeSource scaledTimeSource = /* 游戏时间 */;
ITimeSource realtimeTimeSource = /* 真实时间，可选 */;

var scheduler = new CoroutineScheduler(
    scaledTimeSource,
    realtimeTimeSource: realtimeTimeSource,
    executionStage: CoroutineExecutionStage.Update);

var handle = scheduler.Run(MyCoroutine(), tag: "bootstrap", group: "loading");

// 在宿主主循环中推进协程
scheduler.Update();
```

构造参数中最重要的两个语义是：

- `realtimeTimeSource`
  - 如果提供，`WaitForSecondsRealtime` 会使用它的 `DeltaTime`
  - 如果不提供，实时等待会退化为使用默认时间源
- `executionStage`
  - `Update`：默认阶段
  - `FixedUpdate`：固定步阶段
  - `EndOfFrame`：帧结束阶段

### 控制与完成状态

```csharp
using var cts = new CancellationTokenSource();

var handle = scheduler.Run(
    LoadResources(),
    tag: "loading",
    group: "bootstrap",
    cancellationToken: cts.Token);

scheduler.Pause(handle);
scheduler.Resume(handle);
scheduler.Kill(handle);

var completionStatus = await scheduler.WaitForCompletionAsync(handle);
```

协程的最终结果由 `CoroutineCompletionStatus` 表示：

- `Completed`
- `Cancelled`
- `Faulted`
- `Unknown`

### 快照与可观测性

```csharp
if (scheduler.TryGetSnapshot(handle, out var snapshot))
{
    Console.WriteLine(snapshot.State);
    Console.WriteLine(snapshot.WaitingInstructionType);
    Console.WriteLine(snapshot.ExecutionStage);
}

var allSnapshots = scheduler.GetActiveSnapshots();
```

快照适合做诊断、调试面板和运行中状态检查。

## IYieldInstruction

协程通过 `yield return IYieldInstruction` 表达等待逻辑：

```csharp
public interface IYieldInstruction
{
    bool IsDone { get; }
    void Update(double deltaTime);
}
```

## 常用等待指令

### 时间与帧

```csharp
yield return new Delay(1.0);
yield return new WaitForSecondsScaled(1.0);
yield return new WaitForSecondsRealtime(1.0);
yield return new WaitOneFrame();
yield return new WaitForNextFrame();
yield return new WaitForFrames(5);
yield return new WaitForFixedUpdate();
yield return new WaitForEndOfFrame();
```

语义说明：

- `Delay` 与 `WaitForSecondsScaled`
  - 使用调度器默认时间源推进
- `WaitForSecondsRealtime`
  - 优先使用调度器的 `realtimeTimeSource`
- `WaitForFixedUpdate`
  - 仅在 `CoroutineExecutionStage.FixedUpdate` 调度器中推进
- `WaitForEndOfFrame`
  - 仅在 `CoroutineExecutionStage.EndOfFrame` 调度器中推进

如果宿主没有提供匹配阶段，这类阶段型等待不会自然完成。

### 条件等待

```csharp
yield return new WaitUntil(() => health > 0);
yield return new WaitWhile(() => isLoading);
yield return new WaitForPredicate(() => hp >= maxHp);
yield return new WaitUntilOrTimeout(() => connected, timeoutSeconds: 5.0);
yield return new WaitForConditionChange(() => isPaused, waitForTransitionTo: true);
```

### Task 桥接

```csharp
using GFramework.Core.Coroutine.Extensions;

Task loadTask = LoadDataAsync();
yield return loadTask.AsCoroutineInstruction();

var handle = scheduler.StartTaskAsCoroutine(LoadDataAsync());
```

### 等待事件

```csharp
using GFramework.Core.Abstractions.Events;
using GFramework.Core.Coroutine.Instructions;

public IEnumerator<IYieldInstruction> WaitForEventExample(IEventBus eventBus)
{
    using var wait = new WaitForEvent<PlayerJoinedEvent>(eventBus);
    yield return wait;

    Console.WriteLine(wait.EventData?.PlayerName);
}
```

## CoroutineHelper

`CoroutineHelper` 提供一组常用简写：

```csharp
yield return CoroutineHelper.WaitForSeconds(1.5);
yield return CoroutineHelper.WaitForOneFrame();
yield return CoroutineHelper.WaitForFrames(10);
yield return CoroutineHelper.WaitUntil(() => isReady);
yield return CoroutineHelper.WaitWhile(() => isLoading);
```

也可以直接生成可运行的协程枚举器：

```csharp
scheduler.Run(CoroutineHelper.DelayedCall(2.0, () => Console.WriteLine("延迟执行")));
scheduler.Run(CoroutineHelper.RepeatCall(1.0, 5, () => Console.WriteLine("重复执行")));
```

## 协程组合

```csharp
public IEnumerator<IYieldInstruction> ParentCoroutine()
{
    yield return new WaitForCoroutine(ChildCoroutine());
}

private IEnumerator<IYieldInstruction> ChildCoroutine()
{
    yield return new Delay(1.0);
}
```

如果需要等待多个顶层协程句柄，可以结合 `WaitForAllCoroutines` 或 `ParallelCoroutines(...)` 使用。

## 建议

- 普通游戏时间等待优先使用 `Delay` 或 `WaitForSecondsScaled`
- 只有宿主提供真实时间源时再使用 `WaitForSecondsRealtime`
- 只有宿主显式区分阶段时才使用 `WaitForFixedUpdate` 与 `WaitForEndOfFrame`
- 需要对接生命周期或外部取消时，优先传入 `CancellationToken`
- 需要诊断线上状态时，优先使用 `TryGetSnapshot(...)` 和 `GetActiveSnapshots()`
