<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-134`
- 当前阶段：`Phase 8`
- 当前 PR 锚点：`PR #348`
- 当前结论：
  - 本轮按 `$gframework-batch-boot` 协调多波 non-conflicting subagent，基线固定为
    `origin/main @ 3b2e6899d5ffdcfb634b28f3846f57528fbf9196 (2026-05-11T12:25:00+08:00)`。
  - 本轮停止继续扩 batch 的主信号是 `reviewability / context-budget`，不是 `50` 文件阈值；
    自然停点时累计 branch diff 约为 `12 files`，仍明显低于阈值。
  - CQRS runtime / tests 侧已补齐并提交：
    - `CqrsNotificationPublisherTests` 锁定“多 publisher 报错”与“单 dispatcher 内 publisher 缓存复用”
    - `CqrsGeneratedRequestInvokerProviderTests` 与 `CqrsHandlerRegistrar` 收口 generated descriptor 的异常枚举、
      坏元数据与重复 pair 回退契约
    - `CqrsDispatcherCacheTests` 锁定 request / stream pipeline presence、executor cache 与上下文重新注入组合分支
  - benchmark 侧已补齐并提交：
    - `RequestStartupBenchmarks` 的 `Mediator` startup 对照
    - `StreamStartupBenchmarks`
    - `NotificationStartupBenchmarks`
    - `GFramework.Cqrs.Benchmarks/README.md` 的 current coverage / gap 收口
  - 文档与恢复入口侧已补齐并提交：
    - `GFramework.Cqrs/README.md`
    - `docs/zh-CN/core/cqrs.md`
    - `docs/zh-CN/source-generators/cqrs-handler-registry-generator.md`
    - `ai-plan/public/cqrs-rewrite/archive/**` 顶部导航与跳转约定
  - 当前 `PR #348` latest-head review 再次复核后：
    - 跳过 `NotificationLifetimeBenchmarks.HandlerLifetime` 的 `[GenerateEnumExtensions]` 建议，原因是仓库没有“所有枚举统一生成扩展”的约定，且 benchmark 局部枚举不在该能力的强制范围内
    - 接受并修复 `NotificationLifetimeBenchmarks` 的 scoped 容器释放与公开 XML 文档缺口
    - 接受并修复 `CqrsHandlerRegistrar` 对 generated descriptor 的“先去重后校验”缺陷，并补回归测试锁定“首条无效、后条有效”的同键场景
    - 接受并修复 generated descriptor 校验对 `MethodInfo` 使用 `ReferenceEquals` 的过严比较，改为按方法语义等价匹配
  - 当前尚未提交的收尾切片仅剩：
    - `GFramework.Cqrs.Benchmarks/Messaging/NotificationLifetimeBenchmarks.cs`
    - `GFramework.Cqrs.Tests/Cqrs/CqrsRegistrationServiceTests.cs`
    - `GFramework.Cqrs/README.md`
    - `docs/zh-CN/core/command.md`
    - `docs/zh-CN/core/query.md`
    - 本 tracking / trace 文件本身

## 当前活跃事实

- 当前分支：`feat/cqrs-optimization`
- 当前 PR：`PR #348`
- 当前写面：
  - `GFramework.Cqrs.Benchmarks/README.md`
  - `GFramework.Cqrs.Benchmarks/Messaging/NotificationLifetimeBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/NotificationStartupBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestStartupBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamStartupBenchmarks.cs`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsDispatcherCacheTests.cs`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsGeneratedRequestInvokerProviderTests.cs`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsNotificationPublisherTests.cs`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsRegistrationServiceTests.cs`
  - `GFramework.Cqrs/Internal/CqrsHandlerRegistrar.cs`
  - `GFramework.Cqrs/README.md`
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`
  - `docs/zh-CN/core/command.md`
  - `docs/zh-CN/core/cqrs.md`
  - `docs/zh-CN/core/query.md`
  - `docs/zh-CN/source-generators/cqrs-handler-registry-generator.md`
- 当前基线：
  - 本轮 batch 启动前，分支相对基线的累计 diff 为 `0 files / 0 lines`
  - 当前自然停点时，累计 diff 约为 `12 files`
  - 本轮新增 benchmark smoke 结果：
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

## 当前风险

- `NotificationLifetimeBenchmarks` 当前已跑完整默认作业，但还没并入提交；若继续新开 batch，未提交面会明显降低可审查性。
- `RequestStartup` 的提交 `8990749d` 连带带入了 `CqrsDispatcherCacheTests.cs`；虽然两条切片均有效且已验证通过，但提交边界不再严格对应单个 ownership slice。
- startup 与 lifetime benchmark 的默认作业结果已足以证明路径与相对量级，但 `Initialization_*` 与少量 short-run 结果仍不应直接当成稳定排序结论。

## 最近权威验证

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsRegistrationServiceTests"`
  - 结果：通过，`Passed: 4, Failed: 0`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr347-req-scoped --filter "*RequestLifetimeBenchmarks.SendRequest_GFrameworkCqrs*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：`Singleton 52.69 ns / 32 B`、`Transient 57.88 ns / 56 B`、`Scoped 144.72 ns / 368 B`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr347-stream-scoped --filter "*StreamLifetimeBenchmarks.Stream_GFramework*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：`Scoped + FirstItem` 约为 `266.7~267.0 ns / 792 B`，`Scoped + DrainAll` 约为 `331.6~332.2 ns / 856 B`

## 下一推荐步骤

1. 先提交当前未提交的 `NotificationLifetime + registration fallback tests + CQRS/legacy docs` 收尾切片，回收工作树到干净状态。
2. 再次运行 `$gframework-pr-review`，复核 `PR #348` latest-head open thread 是否已随着本轮多波 head 收敛。
3. 若继续扩 benchmark，优先从 `GFramework.Cqrs.Benchmarks/README.md` 已明确列出的 gap 中选下一个单文件切片，而不是继续扩大 shared infra 改动面。

## 活跃文档

- 当前 active tracking：`ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
- 当前 active trace：`ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前历史归档：
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`

## 说明

- `RP-131` 及之前的长历史验证、阶段流水与旧恢复点说明已迁移到新的 `archive/` 文件，不再继续堆叠在 active 入口。
- active tracking 现在只保留当前恢复点所需的最小事实、风险、权威验证与下一步，供 `boot` 与后续 PR review 快速恢复。
