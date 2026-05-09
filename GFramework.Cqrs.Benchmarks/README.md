# GFramework.Cqrs.Benchmarks

该模块承载 `GFramework.Cqrs` 的独立性能基准工程，用于持续比较运行时 dispatch、publish、cold-start 与后续 generator / pipeline 收口的成本变化。

## 目的

- 为 `GFramework.Cqrs` 建立独立于 NUnit 集成测试的 BenchmarkDotNet 基线
- 参考 `ai-libs/Mediator/benchmarks` 的场景组织方式，逐步补齐 request、notification、stream 与初始化成本对比
- 为后续吸收 `Mediator` 的 dispatch 设计、fixture 组织和对比矩阵提供可重复验证入口

## 当前内容

- `Program.cs`
  - benchmark 命令行入口
- `Messaging/Fixture.cs`
  - 运行前输出并校验场景配置
- `Messaging/RequestBenchmarks.cs`
  - direct handler、NuGet `Mediator` source-generated concrete path、已接上 handwritten generated request invoker provider 的默认 `GFramework.Cqrs` runtime 与 `MediatR` 的 request steady-state dispatch 对比
- `Messaging/RequestLifetimeBenchmarks.cs`
  - `Singleton / Transient` 两类 handler 生命周期下，direct handler、已对齐 generated-provider 宿主接线的默认 `GFramework.Cqrs` runtime 与 `MediatR` 的 request steady-state dispatch 对比
- `Messaging/StreamLifetimeBenchmarks.cs`
  - `Singleton / Transient` 两类 handler 生命周期下，direct handler、`GFramework.Cqrs` reflection stream binding、接上 generated stream registry 的 `GFramework.Cqrs` runtime 与 `MediatR` 的 stream 完整枚举分层对照
- `Messaging/RequestPipelineBenchmarks.cs`
  - `0 / 1 / 4` 个 pipeline 行为下，direct handler、已接上 handwritten generated request invoker provider 的 `GFramework.Cqrs` runtime 与 `MediatR` 的 request steady-state dispatch 对比
- `Messaging/RequestStartupBenchmarks.cs`
  - `Initialization` 与 `ColdStart` 两组 request startup 成本对比，补齐与 `Mediator` comparison benchmark 更接近的 startup 维度
- `Messaging/RequestInvokerBenchmarks.cs`
  - direct handler、`GFramework.Cqrs` reflection runtime、handwritten generated-invoker runtime 与 `MediatR` 的 request steady-state dispatch 对比
- `Messaging/StreamInvokerBenchmarks.cs`
  - direct handler、`GFramework.Cqrs` reflection runtime、handwritten generated-invoker runtime 与 `MediatR` 的 stream 完整枚举对比
- `Messaging/NotificationBenchmarks.cs`
  - `GFramework.Cqrs` runtime、NuGet `Mediator` source-generated concrete path 与 `MediatR` 的单处理器 notification publish 对比
- `Messaging/NotificationFanOutBenchmarks.cs`
  - fixed `4 handler` notification fan-out 的 baseline、`GFramework.Cqrs` 默认顺序发布器、内置 `TaskWhenAllNotificationPublisher`、NuGet `Mediator` source-generated concrete path 与 `MediatR` publish 对比
- `Messaging/StreamingBenchmarks.cs`
  - direct handler、已接上 handwritten generated stream invoker provider 的 `GFramework.Cqrs` runtime 与 `MediatR` 的 stream request 完整枚举对比

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
```

## 当前约束

- `BenchmarkDotNet.Artifacts/` 属于本地生成输出，默认加入仓库忽略，不作为常规提交内容
- `RequestLifetimeBenchmarks` 现在复用与默认 generated-provider 路径一致的 benchmark 宿主接线；它比较的是生命周期切换后的 handler 解析与 dispatch 成本，不单独引入另一套 runtime 发现口径
- `StreamLifetimeBenchmarks` 现在按 direct handler、`GFramework.Cqrs` reflection、`GFramework.Cqrs` generated、`MediatR` 四层口径组织，用于把 stream 生命周期成本与 binding 路径成本拆开观察
- 只要变更影响 `GFramework.Cqrs` request dispatch、DI 解析热路径、invoker/provider、pipeline 或 benchmark 宿主，就应至少复跑能覆盖该路径的过滤场景；request 热路径通常先看：
  - `RequestBenchmarks.SendRequest_*`
  - `RequestLifetimeBenchmarks.SendRequest_*`
- 只要变更影响 stream dispatch、建流绑定或相关宿主接线，就应补跑：
  - `StreamingBenchmarks.Stream_*`
  - `StreamLifetimeBenchmarks.Stream_*`
- 当前性能目标不是超过 source-generated `Mediator`，而是让默认 request steady-state 路径尽量接近它，并至少稳定快于基于反射 / 扫描的 `MediatR`

## 后续扩展方向

- request / stream 的真实 source-generator 产物与 handwritten generated provider 对照
- `Mediator` 的 transient / scoped compile-time lifetime 矩阵对照
- 带真实显式作用域边界的 scoped host 对照
- generated invoker provider 与纯反射 dispatch / 建流对比继续扩展到更多场景
