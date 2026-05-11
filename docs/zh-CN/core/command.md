---
title: 命令（Command）
description: 说明 GFramework.Core.Command 旧命令体系的兼容定位、可用基类与当前使用约束。
---

# 命令（Command）

本页只说明 `GFramework.Core.Command` 里的旧命令体系。

它仍然被保留，用来兼容存量代码；但如果你在写新功能，优先使用 [cqrs](./cqrs.md) 里的新请求模型。

## 当前仍然可用的基类

旧命令体系当前最常见的三个基类是：

- `AbstractCommand`
  - 无输入、无返回值
- `AbstractCommand<TInput>`
  - 有输入、无返回值
- `AbstractCommand<TInput, TResult>`
  - 有输入、有返回值

当前泛型命令通过构造函数接收输入，而不是依赖 `Input` 可写属性。

## 无输入命令

```csharp
using GFramework.Core.Command;
using GFramework.Core.Extensions;

public sealed class RestoreHealthCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var playerModel = this.GetModel<PlayerModel>();
        playerModel.Health.Value = playerModel.MaxHealth.Value;
        this.SendEvent(new PlayerHealthRestoredEvent());
    }
}
```

发送方式：

```csharp
this.SendCommand(new RestoreHealthCommand());
```

## 带输入命令

旧命令输入类型现在直接复用 CQRS 抽象层里的 `ICommandInput`：

```csharp
using GFramework.Core.Command;
using GFramework.Core.Extensions;
using GFramework.Cqrs.Abstractions.Cqrs.Command;

public sealed record DamagePlayerInput(int Amount) : ICommandInput;

public sealed class DamagePlayerCommand(DamagePlayerInput input)
    : AbstractCommand<DamagePlayerInput>(input)
{
    protected override void OnExecute(DamagePlayerInput input)
    {
        var playerModel = this.GetModel<PlayerModel>();
        playerModel.Health.Value -= input.Amount;
    }
}
```

发送方式：

```csharp
this.SendCommand(new DamagePlayerCommand(new DamagePlayerInput(10)));
```

## 带返回值命令

```csharp
using GFramework.Core.Command;
using GFramework.Core.Extensions;
using GFramework.Cqrs.Abstractions.Cqrs.Command;

public sealed record GetGoldRewardInput(int EnemyLevel) : ICommandInput;

public sealed class GetGoldRewardCommand(GetGoldRewardInput input)
    : AbstractCommand<GetGoldRewardInput, int>(input)
{
    protected override int OnExecute(GetGoldRewardInput input)
    {
        return input.EnemyLevel * 10;
    }
}
```

```csharp
var reward = this.SendCommand(new GetGoldRewardCommand(new GetGoldRewardInput(3)));
```

## 发送入口

旧命令由 `IArchitectureContext` 的兼容入口执行：

- `SendCommand(ICommand)`
- `SendCommand<TResult>(ICommand<TResult>)`
- `SendCommandAsync(IAsyncCommand)`
- `SendCommandAsync<TResult>(IAsyncCommand<TResult>)`

在标准架构启动路径中，这些兼容入口底层已经统一改走 `ICqrsRuntime`。
这意味着历史命令调用链在不改调用方式的前提下，也会复用同一套 pipeline 与上下文注入语义。
只有在你直接 `new CommandExecutor()` 做隔离测试，且没有提供 `ICqrsRuntime` 时，才会回退到 legacy 直接执行；此时不会注入统一 pipeline，也不会额外补上下文桥接链路。

## 兼容入口和 CQRS bridge 的关系

这里可以把旧命令路径理解成“保留旧 API、内部接到新 runtime”：

- 对调用方来说，`SendCommand(...)` / `SendCommandAsync(...)` 仍然是旧命令入口
- 对运行时来说，标准 `Architecture` 路径会把这些旧命令包装成内部 bridge request，再交给 `ICqrsRuntime`
- 对处理过程来说，命令最终会复用当前 CQRS 的 request pipeline 与上下文注入链路，而不是维持一套完全独立的分发栈

因此，兼容入口的意义主要是降低迁移成本，而不是鼓励新模块继续围绕旧执行器设计。

在 `IContextAware` 对象内，通常直接通过扩展使用：

```csharp
using GFramework.Core.Extensions;
```

## 什么时候继续保留旧命令

- 你在维护既有 `Core.Command` 代码
- 你的调用链已经依赖旧 `CommandExecutor`
- 当前改动目标是局部修复，不值得同时做 CQRS 迁移
- 你需要保持现有命令类型、调用入口或测试夹具不变，只希望它们在标准架构下继续工作

这类场景的重点是“让存量代码继续跑”，而不是把旧命令体系当成新模块默认入口。

## 什么时候该开始迁移

如果出现下面这些信号，说明更适合把命令迁到新 CQRS：

- 需要 request / notification / stream 的统一模型
- 需要 pipeline behaviors
- 需要 handler registry 生成器
- 你正在写新的业务模块，而不是维护历史命令代码
- 你希望命令处理逻辑直接落在 `AbstractCommandHandler<,>` 等 CQRS handler 上，而不是继续扩展 `AbstractCommand*`
- 你需要让命令和查询、通知共用同一套注册与调试路径

一个简单判断方法：

- 继续保留旧路径：为了兼容已有 `Command` 类型和调用链
- 迁移到 CQRS：为了给新功能建立统一 request model，而不是继续扩大 legacy 面积

迁移后常见写法见：[cqrs](./cqrs.md)
