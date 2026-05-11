<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-132`
- 当前阶段：`Phase 8`
- 当前 PR 锚点：`PR #347`
- 当前结论：
  - 已用 `$gframework-pr-review` 重新抓取并复核 `PR #347` 的 latest-head review，当前仍成立的代码问题已收口到
    `GFramework.Cqrs.Benchmarks` 单模块与 `ai-plan/public/cqrs-rewrite/**` 恢复入口。
  - `Program.cs` 现在按“任意已生效的 artifacts 隔离配置”而不是“仅命令行 `--artifacts-suffix`”决定是否重启隔离宿主；
    同时新增“目标宿主目录不得等于或嵌套在当前宿主输出目录内”的防守式校验，避免 `host/host/...` 递归膨胀。
  - `RequestLifetimeBenchmarks` 与 `StreamLifetimeBenchmarks` 的 `Scoped` 路径改为复用单个 scoped runtime /
    dispatcher，只在每次 benchmark 调用时显式创建并释放真实 DI scope，避免把 runtime 构造常量成本混进生命周期矩阵。
  - `ScopedBenchmarkContainer` 已补齐只读适配语义与作用域租约的 XML 合同说明，避免 PR review 再次停留在“公开成员文档不完整”。
  - active tracking / trace 已完成瘦身：历史长流水迁移到 `archive/` 新文件，当前 active 入口只保留恢复点、风险、
    权威验证与下一步。

## 当前活跃事实

- 当前分支：`feat/cqrs-optimization`
- 当前 PR：`PR #347`
- 当前写面：
  - `GFramework.Cqrs.Benchmarks/Program.cs`
  - `GFramework.Cqrs.Benchmarks/README.md`
  - `GFramework.Cqrs.Benchmarks/Messaging/BenchmarkHostFactory.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestLifetimeBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/ScopedBenchmarkContainer.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamLifetimeBenchmarks.cs`
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`
- 当前基线：
  - `RequestLifetimeBenchmarks.SendRequest_GFrameworkCqrs` short-job 当前约为
    `Singleton 52.69 ns / 32 B`、`Transient 57.88 ns / 56 B`、`Scoped 144.72 ns / 368 B`
  - `StreamLifetimeBenchmarks.Stream_GFramework*` short-job 当前约为
    `Scoped + FirstItem 266.7~267.0 ns / 792 B`、
    `Scoped + DrainAll 331.6~332.2 ns / 856 B`
  - 两条并发 smoke 均已落到独立的
    `BenchmarkDotNet.Artifacts/pr347-req-scoped/host/...` 与
    `BenchmarkDotNet.Artifacts/pr347-stream-scoped/host/...`

## 当前风险

- `Program.cs` 的“嵌套目标目录保护”只覆盖当前宿主目录与隔离宿主目录关系；若后续再扩展更多自定义 artifacts 入口，
  仍需保持同一层防守式校验，避免配置分叉。
- `ScopedBenchmarkContainer` 现在明确禁止重叠 active scope；若后续 benchmark 引入同一 runtime 的并行枚举或嵌套调用，
  需要新的宿主模型，不能直接突破当前只读适配器的约束。
- 本轮 benchmark 结果仍是 `job short + 1 iteration` smoke，用于证明路径正确与相对量级，不应用作稳定性能结论。

## 最近权威验证

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr347-req-scoped --filter "*RequestLifetimeBenchmarks.SendRequest_GFrameworkCqrs*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：`Singleton 52.69 ns / 32 B`、`Transient 57.88 ns / 56 B`、`Scoped 144.72 ns / 368 B`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr347-stream-scoped --filter "*StreamLifetimeBenchmarks.Stream_GFramework*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：`Scoped + FirstItem` 约为 `266.7~267.0 ns / 792 B`，`Scoped + DrainAll` 约为 `331.6~332.2 ns / 856 B`

## 下一推荐步骤

1. 再次运行 `$gframework-pr-review` 复核 `PR #347` latest-head open thread 是否已随本轮 head 收敛。
2. 若 review 已清空，继续留在 `GFramework.Cqrs.Benchmarks` 单模块推进下一批 benchmark 对照，而不是立即扩散到 runtime。
3. 若 review 仍保留 benchmark 相关线程，优先区分 stale 与新增结论，再决定是否需要新的 scoped-host 或 artifacts 入口修补。

## 活跃文档

- 当前 active tracking：`ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
- 当前 active trace：`ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前历史归档：
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`

## 说明

- `RP-131` 及之前的长历史验证、阶段流水与旧恢复点说明已迁移到新的 `archive/` 文件，不再继续堆叠在 active 入口。
- active tracking 现在只保留当前恢复点所需的最小事实、风险、权威验证与下一步，供 `boot` 与后续 PR review 快速恢复。
