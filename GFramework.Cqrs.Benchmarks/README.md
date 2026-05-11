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
  - `Singleton / Scoped / Transient` 三类 handler 生命周期下，direct handler、已对齐 generated-provider 宿主接线的默认 `GFramework.Cqrs` runtime 与 `MediatR` 的 request steady-state dispatch 对比；其中 `Scoped` 通过真实 request 级作用域宿主执行，不再把 scoped handler 退化为根容器解析
- `Messaging/StreamLifetimeBenchmarks.cs`
  - `Singleton / Scoped / Transient` 三类 handler 生命周期下，direct handler、`GFramework.Cqrs` reflection stream binding、接上 generated stream registry 的 `GFramework.Cqrs` runtime 与 `MediatR` 的 stream 分层对照，并同时提供 `FirstItem / DrainAll` 两种观测口径
- `Messaging/RequestPipelineBenchmarks.cs`
  - `0 / 1 / 4` 个 pipeline 行为下，direct handler、已接上 handwritten generated request invoker provider 的 `GFramework.Cqrs` runtime 与 `MediatR` 的 request steady-state dispatch 对比
- `Messaging/RequestStartupBenchmarks.cs`
  - `Initialization` 与 `ColdStart` 两组 request startup 成本对比，补齐与 `Mediator` comparison benchmark 更接近的 startup 维度
- `Messaging/RequestInvokerBenchmarks.cs`
  - direct handler、`GFramework.Cqrs` reflection runtime、handwritten generated-invoker runtime 与 `MediatR` 的 request steady-state dispatch 对比
- `Messaging/StreamInvokerBenchmarks.cs`
  - direct handler、`GFramework.Cqrs` reflection runtime、handwritten generated-invoker runtime 与 `MediatR` 的 stream 对比，并同时提供 `FirstItem / DrainAll` 两种观测口径
- `Messaging/NotificationBenchmarks.cs`
  - `GFramework.Cqrs` runtime、NuGet `Mediator` source-generated concrete path 与 `MediatR` 的单处理器 notification publish 对比
- `Messaging/NotificationFanOutBenchmarks.cs`
  - fixed `4 handler` notification fan-out 的 baseline、`GFramework.Cqrs` 默认顺序发布器、内置 `TaskWhenAllNotificationPublisher`、NuGet `Mediator` source-generated concrete path 与 `MediatR` publish 对比
- `Messaging/StreamingBenchmarks.cs`
  - direct handler、已接上 handwritten generated stream invoker provider 的 `GFramework.Cqrs` runtime 与 `MediatR` 的 stream request 对比，并同时提供 `FirstItem / DrainAll` 两种观测口径

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

如果需要在两个终端里并发复核不同的过滤 benchmark，请为每个进程追加不同的 `--artifacts-suffix <suffix>`，把 `BenchmarkDotNet` auto-generated build 与 artifacts 输出隔离到不同目录；这只是运行入口的目录隔离约定，不是 benchmark 业务逻辑本身的要求。例如：

```bash
dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix req-lifetime-a --filter "*RequestLifetimeBenchmarks.SendRequest_*"
dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix stream-lifetime-b --filter "*StreamLifetimeBenchmarks.Stream_*"
```

## 当前约束

- `BenchmarkDotNet.Artifacts/` 属于本地生成输出，默认加入仓库忽略，不作为常规提交内容
- 当两个带 `--filter` 的 benchmark 进程需要并发运行时，必须为它们分别传入不同的 `--artifacts-suffix <suffix>`，避免多个 `BenchmarkDotNet` 进程写入同一份 auto-generated build / artifacts 目录；这个约束只服务于本地输出隔离，不代表 benchmark 场景之间存在额外业务依赖
- `RequestLifetimeBenchmarks` 现在复用与默认 generated-provider 路径一致的 benchmark 宿主接线；它比较的是生命周期切换后的 handler 解析与 dispatch 成本，不单独引入另一套 runtime 发现口径
- `RequestLifetimeBenchmarks` 的 `Scoped` 场景会在每次 request 分发时显式创建并释放真实 DI 作用域，用来观察 scoped handler 绑定到 request 边界后的解析与 dispatch 成本
- `StreamLifetimeBenchmarks` 现在按 direct handler、`GFramework.Cqrs` reflection、`GFramework.Cqrs` generated、`MediatR` 四层口径组织，并额外区分 `FirstItem` 与 `DrainAll` 两种观测方式，用于把 stream 建流/首个元素成本与完整枚举成本拆开观察
- `StreamingBenchmarks` 与 `StreamInvokerBenchmarks` 都同时暴露 `FirstItem` 与 `DrainAll`；阅读结果时应把它们分别理解为“建流到首个元素”的固定成本观测与“完整枚举整个 stream”的总成本观测
- `StreamInvokerBenchmarks` 当前的 `DrainAll` short-job 输出只适合做 smoke 复核，确认矩阵和路径可以正常跑通；它不应直接写成 reflection、generated 或 `MediatR` 之间的稳定性能结论，若要做排序判断，应复跑默认作业或更完整的 benchmark 批次
- 只要变更影响 `GFramework.Cqrs` request dispatch、DI 解析热路径、invoker/provider、pipeline 或 benchmark 宿主，就应至少复跑能覆盖该路径的过滤场景；request 热路径通常先看：
  - `RequestBenchmarks.SendRequest_*`
  - `RequestLifetimeBenchmarks.SendRequest_*`
- 只要变更影响 stream dispatch、建流绑定或相关宿主接线，就应补跑：
  - `StreamingBenchmarks.Stream_*`
  - `StreamLifetimeBenchmarks.Stream_*`
- 当前性能目标不是超过 source-generated `Mediator`，而是让默认 request steady-state 路径尽量接近它，并至少稳定快于基于反射 / 扫描的 `MediatR`

## 后续扩展方向

- 若继续优化 stream lifetime，可优先复核 `Transient + FirstItem` 下 generated 与 reflection 的小幅差值是否稳定，再决定继续压 generated 宿主的建流瞬时成本，还是把后续对照切回 `StreamInvokerBenchmarks` / `Mediator` concrete runtime 批次
- request / stream 的真实 source-generator 产物与 handwritten generated provider 对照
- `Mediator` 的 transient / scoped compile-time lifetime 矩阵对照
- generated invoker provider 与纯反射 dispatch / 建流对比继续扩展到更多场景
