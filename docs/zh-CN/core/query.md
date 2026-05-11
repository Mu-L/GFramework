---
title: 查询（Query）
description: 说明 GFramework.Core.Query 旧查询体系的兼容定位、可用基类与当前使用约束。
---

# 查询（Query）

本页说明 `GFramework.Core.Query` 里的旧查询体系。

和旧命令系统一样，它仍然保留用于兼容存量代码；新功能优先使用 [cqrs](./cqrs.md) 中的新查询模型。

## 当前仍然可用的基类

旧查询体系最常见的两个基类是：

- `AbstractQuery<TResult>`
  - 无输入查询
- `AbstractQuery<TInput, TResult>`
  - 带输入查询

当前带输入查询通过构造函数接收输入，不再依赖 `Input` 属性赋值。

## 无输入查询

```csharp
using GFramework.Core.Extensions;
using GFramework.Core.Query;

public sealed class GetPlayerHealthQuery : AbstractQuery<int>
{
    protected override int OnDo()
    {
        return this.GetModel<PlayerModel>().Health.Value;
    }
}
```

发送方式：

```csharp
var health = this.SendQuery(new GetPlayerHealthQuery());
```

## 带输入查询

旧查询输入类型现在直接复用 CQRS 抽象层里的 `IQueryInput`：

```csharp
using GFramework.Core.Extensions;
using GFramework.Core.Query;
using GFramework.Cqrs.Abstractions.Cqrs.Query;

public sealed record GetItemCountInput(string ItemId) : IQueryInput;

public sealed class GetItemCountQuery(GetItemCountInput input)
    : AbstractQuery<GetItemCountInput, int>(input)
{
    protected override int OnDo(GetItemCountInput input)
    {
        var inventoryModel = this.GetModel<InventoryModel>();
        return inventoryModel.GetItemCount(input.ItemId);
    }
}
```

```csharp
var count = this.SendQuery(
    new GetItemCountQuery(new GetItemCountInput("potion")));
```

## 异步查询

上下文仍然保留旧异步查询执行入口：

- `SendQueryAsync(IAsyncQuery<TResult>)`

这主要面向兼容旧 `AsyncQueryExecutor` 路径。文档不再推荐围绕旧 `QueryBus` 设计新功能。

## 发送入口

旧查询的执行入口是：

- `SendQuery<TResult>(IQuery<TResult>)`
- `SendQueryAsync<TResult>(IAsyncQuery<TResult>)`

在标准架构启动路径中，这些兼容入口底层同样会转到统一 `ICqrsRuntime`。
因此历史查询对象仍保持原始 `SendQuery(...)` / `SendQueryAsync(...)` 用法，但会共享新版 request pipeline 与上下文注入链路。
只有在你直接 `new QueryExecutor()` 或 `new AsyncQueryExecutor()` 做隔离测试，且没有提供 `ICqrsRuntime` 时，才会回退到 legacy 直接执行；这时异步查询也不会进入统一 CQRS pipeline。

## 兼容入口和 CQRS bridge 的关系

旧查询页面的重点不是再引入一套新执行模型，而是说明兼容入口现在如何接到 CQRS runtime：

- `SendQuery(...)` / `SendQueryAsync(...)` 仍然是面向存量代码的旧 API
- 标准 `Architecture` 路径会把旧查询包装成内部 bridge request，再交给 `ICqrsRuntime`
- 这让旧查询对象在不改调用方式的前提下，也能共享当前 CQRS 的 pipeline、handler 调度和上下文注入语义

如果你依赖的是 direct executor 测试或隔离运行，那么仍要把它看成 legacy 路径，而不是完整的新 CQRS 使用方式。

在 `IContextAware` 对象内部，通常直接使用 `GFramework.Core.Extensions` 里的扩展：

```csharp
using GFramework.Core.Extensions;
```

## 什么时候继续保留旧查询

- 你在维护现有 `Core.Query` 代码
- 当前代码已经建立在旧查询执行器之上
- 你只想修正局部行为，不想顺手迁移整条调用链
- 你需要保留现有 `AbstractQuery*` 类型与测试入口，只要求标准架构下继续复用统一 runtime

## 什么时候改用 CQRS 查询

如果你正在写新的读取路径，或者已经需要统一读写模型，优先考虑：

- `GFramework.Cqrs.Abstractions.Cqrs.Query.IQuery<TResponse>`
- `AbstractQueryHandler<TQuery, TResponse>`
- `architecture.Context.SendQueryAsync(...)`

原因很简单：新查询路径和命令、通知、流式请求共享同一 dispatcher 与行为管道。

可以按下面的判断来选：

- 继续保留旧路径：为了兼容已有 `Query` 类型、旧执行器或局部修复场景
- 迁移到 CQRS：为了把新的读取能力纳入统一 request model，而不是继续扩大 legacy 查询面

继续阅读：[cqrs](./cqrs.md)
