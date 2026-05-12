<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-137`
- 当前阶段：`Phase 8`
- 当前 PR 锚点：`PR #349（已于 2026-05-12 合并到 origin/main）`
- 当前结论：
  - 本轮恢复时先按 `$gframework-pr-review` 复核 `PR #349` latest-head review，确认该 PR 已关闭且合并到
    `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)`，旧 tracking 中基于 `ef4d3d5d` 的 branch-diff 度量已失效。
  - latest-head 残余 open thread 中，实际仍成立的项只剩：
    - `StreamingBenchmarks.Stream_MediatR()` 缺少 `<returns>` XML 契约
  - 其余线程经本地核对已判定为 stale：
    - `StreamPipelineBenchmarks.Stream_Baseline` 的 `<returns>` 已存在
    - `CqrsNotificationPublisherTests` 的 fallback publisher 缓存回归已改成“再次解析立即失败”
    - active tracking / trace 已同步到 `PR #349`
  - 本轮继续按 `$gframework-batch-boot 50` 协调 subagent，围绕 benchmark 文档与 startup parity 做两波窄切片：
    - 已提交 `f346110a`：`StreamStartupBenchmarks` 补 `Mediator` startup parity
    - 待收尾提交：`StreamingBenchmarks` 的 XML 文档补齐与 `GFramework.Cqrs.Benchmarks/README.md` 的 stream startup / gap 同步
  - tests 侧此前已补齐并提交：
    - `CqrsRegistrationServiceTests`：补空输入、空项过滤、稳定键排序与跨调用跳过边界
    - `CqrsHandlerRegistrarTests` 与 `CqrsHandlerRegistrarFallbackFailureTests`：
      补 abstract registry 与缺少无参构造器 registry 的回退 / 抛错覆盖
    - `CqrsNotificationPublisherTests`：补“零 publisher 回退到默认顺序发布器并缓存”回归
  - benchmark 侧此前已补齐并提交：
    - `StreamPipelineBenchmarks`
    - `StreamingBenchmarks` 的 steady-state `Mediator` 对照
    - `GFramework.Cqrs.Benchmarks/README.md` 的 stream coverage / gap 同步
    - `StreamStartupBenchmarks` 的 `Mediator` initialization / cold-start 对照
  - 本轮未修改 `GFramework.Cqrs` 运行时代码；notification fallback 与 generated registry 激活守卫均由新回归证明现有实现已满足预期。

## 当前活跃事实

- 当前分支：`feat/cqrs-optimization`
- 当前 PR：`PR #349（已合并；当前分支暂无新的公开 PR）`
- 当前写面：
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamStartupBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/README.md`
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前基线：
  - `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)`
  - 当前已提交 branch diff：`1 file / 60 lines`
  - 当前工作树尚有 2 个未提交收尾文件：`StreamingBenchmarks.cs`、`GFramework.Cqrs.Benchmarks/README.md`
- 本轮提交：
  - `f346110a` `feat(cqrs-benchmarks): 补齐 stream startup 的 Mediator 对照路径`

## 当前风险

- `StreamStartupBenchmarks` 的 `Mediator` parity 目前只做了编译验证，尚未单独执行 benchmark 作业确认 startup 矩阵运行结果。
- `StreamLifetimeBenchmarks` 仍缺 `Mediator` parity；该项涉及 `BenchmarkHostFactory` 与 compile-time lifetime 形状，不再是本轮低风险切片。
- 当前 worktree 仍有 2 个未提交文档/注释收尾文件；若不在同轮提交，下一次 `boot` 会同时面对已提交 benchmark 扩展与未提交文档漂移。

## 最近权威验证

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `python3 scripts/license-header.py --check`
  - 结果：通过
- `$gframework-pr-review`
  - 结果：`PR #349` 已关闭；latest-head review open thread 经本地核对仅剩 `StreamingBenchmarks.Stream_MediatR()` 的 XML 文档缺口仍成立

## 下一推荐步骤

1. 先提交 `StreamingBenchmarks.cs`、`GFramework.Cqrs.Benchmarks/README.md` 与本 tracking / trace 收尾，回到干净工作树。
2. 若继续 benchmark 波次，优先单独执行 `StreamStartupBenchmarks` 的最小 smoke run，验证新加 `Mediator` startup 路径可运行。
3. 若后续还要扩 stream parity，把 `StreamLifetimeBenchmarks` 视为跨文件设计任务，而不是继续按“单文件低风险切片”处理。

## 活跃文档

- 当前 active tracking：`ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
- 当前 active trace：`ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前历史归档：
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`

## 说明

- `RP-131` 及之前的长历史验证、阶段流水与旧恢复点说明已迁移到新的 `archive/` 文件，不再继续堆叠在 active 入口。
- active tracking 现在只保留当前恢复点所需的最小事实、风险、权威验证与下一步，供 `boot` 与后续 PR review 快速恢复。
