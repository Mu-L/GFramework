# GFramework.Cqrs.Benchmarks

该模块承载 `GFramework.Cqrs` 的独立性能基准工程，用于在当前 HEAD 上复核 request、stream、notification 的 steady-state 与 startup 成本边界。

## 目的

- 为 `GFramework.Cqrs` 提供独立于测试工程的 BenchmarkDotNet 复核入口
- 让 request、stream、notification 的热路径与 cold-start 变化有可重复的对照矩阵
- 在不引入“未来已存在”假设的前提下，明确当前 benchmark 已覆盖什么、还没有覆盖什么

## 当前 coverage

当前工程已经覆盖以下矩阵：

- request steady-state
  - `Messaging/RequestBenchmarks.cs`
    - direct handler、默认 `GFramework.Cqrs` runtime、NuGet `Mediator` source-generated concrete path、`MediatR`
  - `Messaging/RequestLifetimeBenchmarks.cs`
    - `Singleton / Scoped / Transient` 三类 handler 生命周期下，baseline、默认 generated-provider 宿主接线的 `GFramework.Cqrs` runtime、NuGet `Mediator` source-generated concrete path 与 `MediatR`
  - `Messaging/RequestPipelineBenchmarks.cs`
    - `0 / 1 / 4` 个 pipeline 行为下，baseline、默认 generated-provider 宿主接线的 `GFramework.Cqrs` runtime 与 `MediatR`
  - `Messaging/RequestInvokerBenchmarks.cs`
    - baseline、`GFramework.Cqrs` reflection request binding、`GFramework.Cqrs` generated request invoker、`MediatR`
- request startup
  - `Messaging/RequestStartupBenchmarks.cs`
    - `Initialization` 与 `ColdStart` 两组下，`GFramework.Cqrs`、NuGet `Mediator`、`MediatR`
    - 其中 `GFramework.Cqrs` 路径是“单 handler 最小宿主 + 手工注册”的 startup/cold-start 模型，不包含更大范围的程序集扫描或完整注册协调器接线
- stream steady-state
  - `Messaging/StreamingBenchmarks.cs`
    - baseline、默认 generated-provider 宿主接线的 `GFramework.Cqrs` runtime、NuGet `Mediator` source-generated concrete path 与 `MediatR`
    - 同时提供 `FirstItem` 与 `DrainAll` 两种观测口径
  - `Messaging/StreamLifetimeBenchmarks.cs`
    - `Singleton / Scoped / Transient` 三类 handler 生命周期下，baseline、`GFramework.Cqrs` reflection stream binding、`GFramework.Cqrs` generated stream registry、`MediatR`
    - 同时提供 `FirstItem` 与 `DrainAll` 两种观测口径
  - `Messaging/StreamInvokerBenchmarks.cs`
    - baseline、`GFramework.Cqrs` reflection stream binding、`GFramework.Cqrs` generated stream invoker、`MediatR`
    - 同时提供 `FirstItem` 与 `DrainAll` 两种观测口径
  - `Messaging/StreamPipelineBenchmarks.cs`
    - `0 / 1 / 4` 个 stream pipeline 行为下，baseline、默认 generated-provider 宿主接线的 `GFramework.Cqrs` runtime 与 `MediatR`
    - 同时提供 `FirstItem` 与 `DrainAll` 两种观测口径
- stream startup
  - `Messaging/StreamStartupBenchmarks.cs`
    - `Initialization` 与 `ColdStart` 两组下，覆盖 `MediatR`、`GFramework.Cqrs` reflection、`GFramework.Cqrs` generated、NuGet `Mediator` 四组 initialization/cold-start 对照
    - 其中 `ColdStart` 的边界是“新宿主 + 首个元素命中”，不是完整枚举整个 stream
- notification steady-state
  - `Messaging/NotificationBenchmarks.cs`
    - 单处理器 publish 下，`GFramework.Cqrs` runtime、NuGet `Mediator` source-generated concrete path、`MediatR`
  - `Messaging/NotificationLifetimeBenchmarks.cs`
    - 单处理器 publish 在 `Singleton / Scoped / Transient` 三类 handler 生命周期下的 baseline、`GFramework.Cqrs` 与 `MediatR` 对照
  - `Messaging/NotificationFanOutBenchmarks.cs`
    - 固定 `4 handler` fan-out 下的 baseline、`GFramework.Cqrs` 默认顺序发布器、内置 `TaskWhenAllNotificationPublisher`、NuGet `Mediator`、`MediatR`
- notification startup
  - `Messaging/NotificationStartupBenchmarks.cs`
    - `Initialization` 与 `ColdStart` 两组下，`GFramework.Cqrs`、NuGet `Mediator`、`MediatR`
    - 其中 `GFramework.Cqrs` 路径是“单 handler 最小宿主 + 手工注册”的 startup/cold-start 模型，不包含 fan-out、发布策略变体或更大范围的注册协调逻辑

## 最小使用方式

```bash
dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release
```

上面的命令只验证 benchmark 工程当前可以正常编译。

如需实际运行 benchmark，再执行：

```bash
dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release
```

如需只复核某一类场景，可把 `BenchmarkDotNet` 参数放在 `--` 之后，例如：

```bash
dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*RequestLifetimeBenchmarks.SendRequest_*"
dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*StreamLifetimeBenchmarks.Stream_*"
dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*StreamPipelineBenchmarks.Stream_*"
```

## 并发运行约束

当两个 benchmark 进程需要并发运行时，必须为每个进程追加不同的 `--artifacts-suffix <suffix>`。当前入口会把这个 suffix 解析成独立的 `BenchmarkDotNet.Artifacts/<suffix>/` 目录，并在该目录下复制隔离的 benchmark host，避免多个进程写入同一份 auto-generated build 与 artifacts 输出。

例如：

```bash
dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix req-lifetime-a --filter "*RequestLifetimeBenchmarks.SendRequest_*"
dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix stream-lifetime-b --filter "*StreamLifetimeBenchmarks.Stream_*"
```

如果不并发运行，就不需要额外传入 `--artifacts-suffix`。`BenchmarkDotNet.Artifacts/` 仍然是本地生成输出，默认不作为常规提交内容。

## 结果解读边界

- `RequestLifetimeBenchmarks` 的 `Scoped` 场景会在每次 request 分发时显式创建并释放真实 DI 作用域；它观察的是 scoped handler 的解析与 dispatch 成本，不把 runtime 构造常量成本混入生命周期对照
- `NotificationLifetimeBenchmarks` 的 `Scoped` 场景也采用真实 DI 作用域；它比较的是 publish 路径上的生命周期额外开销，不是根容器解析退化后的近似值
- `StreamingBenchmarks`、`StreamLifetimeBenchmarks`、`StreamInvokerBenchmarks`、`StreamPipelineBenchmarks` 同时暴露 `FirstItem` 与 `DrainAll`
  - `FirstItem` 适合观察“建流到首个元素”的固定成本
  - `DrainAll` 适合观察完整枚举整个 stream 的总成本
- `StreamStartupBenchmarks` 的 `ColdStart` 只推进到首个元素，因此它回答的是“新宿主下首次建流命中”的边界，不回答完整枚举总成本
- `RequestStartupBenchmarks` 与 `NotificationStartupBenchmarks` 的 `GFramework.Cqrs` startup 路径都固定在单 handler、最小宿主、手工注册模型；它们回答的是首次 request / publish 命中的额外成本，不代表程序集扫描或完整注册协调器场景
- 当前 HEAD 没有单独固化的 short-job benchmark 类或 checked-in short-job 结果；如果手动使用 short job / short run 只做 smoke 复核，应把它理解为“确认矩阵与路径能跑通”
- 特别是 `StreamInvokerBenchmarks` 的 `DrainAll` 在 short-job smoke 下不应直接写成 reflection、generated 或 `MediatR` 之间的稳定排序结论；若要比较名次或小幅差值，应复跑默认作业或更完整的批次

## 当前缺口

- 当前没有 stream 生命周期版的 NuGet `Mediator` source-generated concrete path 对照；`StreamLifetimeBenchmarks` 现在只覆盖 `GFramework.Cqrs` 与 `MediatR`
- 当前没有 notification fan-out 的生命周期矩阵；`NotificationFanOutBenchmarks` 只覆盖固定 `4 handler` 的已装配宿主
