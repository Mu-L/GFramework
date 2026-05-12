<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-138`
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
  - 在当前恢复点继续推进第 2 波 benchmark XML 契约收口，范围严格限定为公开 benchmark 方法缺失的 `<returns>` 文档：
    - 主线程：`StreamingBenchmarks.cs`、`NotificationBenchmarks.cs` 与 `ai-plan/public/cqrs-rewrite/**`
    - worker：`StreamLifetimeBenchmarks.cs`、`StreamInvokerBenchmarks.cs`、`NotificationFanOutBenchmarks.cs`
  - 启动第 2 波前，当前分支相对 `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)` 的已提交 branch diff 为 `5 files / 177 lines`，远低于 `$gframework-batch-boot 50` 的文件阈值；本轮是否继续主要由 context-budget / reviewability 决定，而不是 branch-size 预算。
  - 第 3 波继续沿同一模式扩到 request 系 benchmark XML 契约收口，并已由 worker 分别提交：
    - `555c7c07`：`RequestBenchmarks.cs`、`RequestPipelineBenchmarks.cs`
    - `ab422b05`：`RequestInvokerBenchmarks.cs`、`RequestLifetimeBenchmarks.cs`
    - 另有 `RequestStartupBenchmarks.cs` 已完成 `<returns>` 收口，待与主线程未提交切片一并收尾
  - 当前已决定在第 3 波后停在自然边界，而不是继续开启第 4 波：
    - branch-size 仍远低于 `50 files`
    - 但剩余候选已不比当前波次更低风险，继续机械扩批会降低 reviewability，并推高当前上下文负担
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
  - `GFramework.Cqrs.Benchmarks/Messaging/NotificationBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/NotificationFanOutBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestStartupBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamInvokerBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamLifetimeBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamStartupBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/README.md`
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前基线：
  - `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)`
  - 当前已提交 branch diff：`9 files / 143 lines`
  - 当前分支比 `origin/main` 多 `4` 个提交：`f346110a`、`a016e3d4`、`ab422b05`、`555c7c07`
  - 当前未提交面由 notification / stream / request-startup 的 XML 契约补齐与 `ai-plan` 恢复点更新构成
- 本轮提交：
  - `f346110a` `feat(cqrs-benchmarks): 补齐 stream startup 的 Mediator 对照路径`
  - `ab422b05` `docs(cqrs-benchmarks): 补齐 request benchmark 返回值注释`
  - `555c7c07` `docs(cqrs-benchmarks): 补齐 request benchmark 返回值文档`

## 当前风险

- `StreamStartupBenchmarks` 的 `Mediator` parity 目前只做了编译验证，尚未单独执行 benchmark 作业确认 startup 矩阵运行结果。
- `StreamLifetimeBenchmarks` 仍缺 `Mediator` parity；该项涉及 `BenchmarkHostFactory` 与 compile-time lifetime 形状，不再是本轮低风险切片。
- 本轮已在 request 系 benchmark XML 契约收口后主动停批次；若后续恢复，优先先提交当前未提交面，再决定是否开启 docs/README 或 smoke-run 下一阶段，而不是继续机械扩张 XML 批次。

## 最近权威验证

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `python3 scripts/license-header.py --check`
  - 结果：通过
- `$gframework-pr-review`
  - 结果：`PR #349` 已关闭；latest-head review open thread 经本地核对仅剩 `StreamingBenchmarks.Stream_MediatR()` 的 XML 文档缺口仍成立

## 下一推荐步骤

1. 串行运行 `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`、`python3 scripts/license-header.py --check --paths ...` 与 `git diff --check`，作为当前自然停点的权威收尾验证。
2. 提交当前未提交的 notification / stream / request-startup XML 契约与 `ai-plan` 更新，回到干净工作树。
3. 若后续继续 benchmark 波次，优先单独执行 `StreamStartupBenchmarks` 的最小 smoke run，验证新加 `Mediator` startup 路径可运行。
4. 若后续还要扩 stream parity，把 `StreamLifetimeBenchmarks` 视为跨文件设计任务，而不是继续按“单文件低风险切片”处理。

## 活跃文档

- 当前 active tracking：`ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
- 当前 active trace：`ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前历史归档：
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`

## 说明

- `RP-131` 及之前的长历史验证、阶段流水与旧恢复点说明已迁移到新的 `archive/` 文件，不再继续堆叠在 active 入口。
- active tracking 现在只保留当前恢复点所需的最小事实、风险、权威验证与下一步，供 `boot` 与后续 PR review 快速恢复。
