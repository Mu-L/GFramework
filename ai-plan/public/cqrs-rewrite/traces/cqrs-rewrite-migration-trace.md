<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移追踪

## 2026-05-11

### 阶段：PR #347 latest-head review 收口（CQRS-REWRITE-RP-132）

- 使用 `$gframework-pr-review` 重新抓取当前分支 `feat/cqrs-optimization` 对应的 `PR #347`
- 抓取结果显示当前 latest-head 仍有 `CodeRabbit 6` 与 `Greptile 2` open thread，但本地复核后只接受以下仍成立的结论：
  - `Program.cs` 只在命令行 `--artifacts-suffix` 下重启隔离宿主，未覆盖环境变量触发的隔离路径
  - `Program.cs` 缺少“目标隔离宿主目录位于当前宿主输出目录内”的防守式校验
  - `RequestLifetimeBenchmarks` / `StreamLifetimeBenchmarks` 的 `Scoped` 路径每次 benchmark 调用都会新建 runtime，污染生命周期矩阵公平性
  - `ScopedBenchmarkContainer` 的公共/关键成员 XML 契约说明不完整
  - active tracking / trace 已经偏离“快速恢复入口”，需要归档瘦身
- 本轮实现选择：
  - `Program.cs`
    - 把“是否需要重启隔离宿主”统一收敛到 `ArtifactsPath != null`
    - 为隔离宿主目录补充“不得等于或嵌套在当前宿主输出目录内”的校验
  - `BenchmarkHostFactory.cs`
    - scoped request / stream helper 改为复用单个 runtime，只在调用边界进入和释放 scope
  - `ScopedBenchmarkContainer.cs`
    - 从“构造时绑定单次 scope”改为“可重复进入的作用域适配器 + `ScopeLease`”
    - 补齐只读适配器与作用域租约的 XML 合同说明
  - `RequestLifetimeBenchmarks.cs`
    - `Scoped` 路径改为 `GlobalSetup` 时创建单个 scoped runtime
  - `StreamLifetimeBenchmarks.cs`
    - `Scoped` reflection / generated 路径改为 `GlobalSetup` 时各自创建单个 scoped runtime
    - 按语义阶段拆分 `Setup()`，同时清掉 `MA0051`
  - `README.md`
    - 把 `RequestLifetimeBenchmarks` 的 scoped 语义更新为“复用 runtime，仅每次创建真实 scope”
  - `ai-plan/public/cqrs-rewrite/**`
    - 将旧 active tracking / trace 复制归档到
      `archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
      与
      `archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`
    - 重写 active tracking / trace，只保留当前恢复点、关键结论、风险与验证

### 本轮验证

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr347-req-scoped --filter "*RequestLifetimeBenchmarks.SendRequest_GFrameworkCqrs*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：独立宿主目录位于 `BenchmarkDotNet.Artifacts/pr347-req-scoped/host/...`
  - 结果摘要：`Singleton 52.69 ns / 32 B`、`Transient 57.88 ns / 56 B`、`Scoped 144.72 ns / 368 B`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr347-stream-scoped --filter "*StreamLifetimeBenchmarks.Stream_GFramework*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：独立宿主目录位于 `BenchmarkDotNet.Artifacts/pr347-stream-scoped/host/...`
  - 结果摘要：
    - `Singleton + FirstItem`：reflection `108.7 ns / 216 B`，generated `110.1 ns / 216 B`
    - `Scoped + FirstItem`：reflection `266.7 ns / 792 B`，generated `267.0 ns / 792 B`
    - `Scoped + DrainAll`：reflection `331.6 ns / 856 B`，generated `332.2 ns / 856 B`

### 当前结论

- `Program.cs` 已不再依赖“命令行后缀”这一单一来源决定宿主隔离，环境变量生效路径也会进入隔离宿主。
- scoped benchmark 的公平性问题已经实质收口：runtime 构造成本不再按每次调用重复计入生命周期矩阵。
- active tracking / trace 已恢复到可供 `boot` 和后续 PR review 快速进入的体量；历史细节仍可通过新的 archive 文件追溯。

### 当前下一步

- 推送本轮变更后，重新运行 `$gframework-pr-review`，确认 `PR #347` 的 latest-head open thread 是否已随着新 head 收敛。

### 阶段：多波 batch 收口与 benchmark / docs 扩面（CQRS-REWRITE-RP-133）

- 按 `$gframework-batch-boot` 启动多波 non-conflicting subagent，基线固定为
  `origin/main @ 3b2e6899d5ffdcfb634b28f3846f57528fbf9196 (2026-05-11T12:25:00+08:00)`。
- 启动前分支累计 diff 为 `0 files / 0 lines`；自然停点时累计 branch diff 约为 `12 files`。
- 主线程把 stop decision 明确交给 `reviewability / context-budget`，没有在仍有文件预算时继续机械追到 `50 files`。
- 本轮 accepted delegated scope：
  - runtime / tests
    - `CqrsNotificationPublisherTests`：补“多 publisher 报错”与“publisher 缓存复用”回归
    - `CqrsGeneratedRequestInvokerProviderTests` + `CqrsHandlerRegistrar`：补 generated descriptor 坏元数据、异常枚举、重复 pair 回退契约
    - `CqrsDispatcherCacheTests`：补 request / stream pipeline presence、executor cache 与上下文重新注入组合分支
    - `CqrsRegistrationServiceTests`：补稳定程序集键 fallback 到 `AssemblyName.Name` / `ToString()` 的回归
  - benchmarks
    - `RequestStartupBenchmarks`：补 `Mediator` startup 对照
    - `StreamStartupBenchmarks`
    - `NotificationStartupBenchmarks`
    - `NotificationLifetimeBenchmarks`
    - `GFramework.Cqrs.Benchmarks/README.md`：收口当前 coverage / gap / smoke 解释边界
  - docs / recovery
    - `GFramework.Cqrs/README.md`
    - `docs/zh-CN/core/cqrs.md`
    - `docs/zh-CN/source-generators/cqrs-handler-registry-generator.md`
    - `docs/zh-CN/core/command.md`
    - `docs/zh-CN/core/query.md`
    - `ai-plan/public/cqrs-rewrite/archive/**` 顶部导航与跳转约定
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsRegistrationServiceTests"`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix notif-lifetime --filter "*NotificationLifetimeBenchmarks*"`
- 本轮 benchmark 结果摘要：
  - `RequestStartupBenchmarks`
    - `ColdStart_GFrameworkCqrs 61.648 us / 25336 B`
    - `ColdStart_Mediator 110.867 us / 57872 B`
    - `ColdStart_MediatR 679.103 us / 606256 B`
  - `StreamStartupBenchmarks`
    - `ColdStart_GFrameworkReflection 71.13 us / 25504 B`
    - `ColdStart_GFrameworkGenerated 82.12 us / 28280 B`
    - `ColdStart_MediatR 933.87 us / 678992 B`
  - `NotificationStartupBenchmarks`
    - `ColdStart_GFrameworkCqrs 85.09 us / 24752 B`
    - `ColdStart_Mediator 136.08 us / 62512 B`
    - `ColdStart_MediatR 1.379 ms / 719056 B`
  - `NotificationLifetimeBenchmarks`
    - `Singleton`：`GFramework 295.48 ns / 360 B`，`MediatR 77.99 ns / 288 B`
    - `Scoped`：`GFramework 410.92 ns / 640 B`，`MediatR 213.49 ns / 632 B`
    - `Transient`：`GFramework 311.21 ns / 416 B`，`MediatR 74.36 ns / 288 B`
- 当前收尾判断：
  - branch diff 仍远低于 `50` 文件阈值，但 active 未提交面与 benchmark 运行输出已经足够构成自然 stop boundary
  - 下一步不继续扩 batch，先提交当前收尾切片并回到干净工作树，再按 PR review 结果决定后续波次
