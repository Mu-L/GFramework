<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-142`
- 当前阶段：`Phase 8`
- 当前 PR 锚点：`PR #350（MERGED，2026-05-13）`
- 当前结论：
  - 本轮按 `$gframework-batch-boot 50` 恢复后，先核对本地仓库真值，确认 `feat/cqrs-optimization` 已与
    `origin/main` 指向同一合并提交 `4837aa2a`，此前 tracking 中的 `PR #350（OPEN）`、`14 files` 等事实已过期。
  - 当前 topic 的 benchmark runtime 修正、XML 文档补齐与 `README` 边界收口已经随 `PR #350` 合并进入
    `origin/main`，不再存在可继续扩批的活动写面。
  - 这轮收口不再继续新增 benchmark 或测试切片，而是把 public recovery 入口刷新为“已合并、无 branch diff、等待下一轮新任务”的状态，
    避免后续 `boot` 落回已完成的 PR 上下文。
  - 刷新提交 `01360bb4` 落地后，当前 branch-wide 停止原因仍不是 `50 files` 阈值，而是语义边界已经完成：
    - `origin/main...HEAD` 的累计 diff 为 `2 files / 107 lines`
    - 当前工作树干净
    - 唯一新增 diff 只来自 `cqrs-rewrite` 的 public recovery 文档刷新
    - 继续在同一 topic 上机械扩批不会产生新的低风险、可验证切片

## 当前活跃事实

- 当前分支：`feat/cqrs-optimization`
- 当前 HEAD / 基线：`origin/main @ 4837aa2a (2026-05-12 20:37:56 +0800)`
- 当前 PR：`PR #350（已合并到 origin/main）`
- 当前写面：
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前基线：
  - `origin/main @ 4837aa2a (2026-05-12 20:37:56 +0800)`
  - 当前已提交 branch diff：`2 files / 107 lines`
  - `origin/main...HEAD` 提交差异：`0 behind / 1 ahead`
- 当前工作面已收口为 public recovery 文档刷新；当前工作树干净，CQRS benchmark 代码不再处于活跃修改状态
- 最近已合并提交：
  - `2dd9435c` `fix(cqrs-benchmarks): 修正Mediator基准运行时配置`
  - `e3532fc2` `feat(cqrs-benchmarks): 补齐request生命周期的Mediator对照`
  - `092946e9` `docs(cqrs-benchmarks): 同步startup基准文档边界`
- 当前恢复提交：
  - `01360bb4` `chore(cqrs-rewrite): 刷新PR350合并后的恢复入口`

## 当前风险

- 后续若再次评估 `StreamLifetimeBenchmarks` 或 request lifetime 的 `Mediator` parity，仍必须采用独立 compile-time config
  或独立 benchmark 工程，而不是在同一份 source-generated 产物上切换 runtime `ServiceLifetime`。
- 如果 `feat/cqrs-optimization` 继续承载新的 CQRS 任务而不先分支，public recovery 入口会把“已合并的 PR #350”与下一轮新工作混在一起。
- 若未来再开 benchmark XML / docs 波次，仍需要主线程先抽样核对代表文件，避免重复接受误报 inventory。

## 最近权威验证

- `git rev-list --left-right --count origin/main...HEAD`
  - 结果：通过
  - 备注：在恢复刷新提交前返回 `0 0`，确认 `PR #350` 已完整合入 `origin/main`
- `git diff --name-only origin/main...HEAD | wc -l`
  - 结果：通过
  - 备注：刷新提交后返回 `2`，确认当前 branch diff 只剩两份 `ai-plan` recovery 文档
- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
  - 备注：作为当前 recovery 刷新任务的最小 build validation，继续确认 benchmark 工程在合并后保持可编译

## 下一推荐步骤

1. 若要继续 CQRS 主题的新一轮实现，先把当前 recovery 刷新提交合并或重放到新的 topic branch，再从最新 `origin/main` 继续，而不是复用已完成的 `PR #350` 上下文。
2. 若后续重新打开 `Mediator` 生命周期 parity 工作，优先设计独立 compile-time config / 独立 benchmark 工程，并把该设计单独记录到新的 tracking phase。
3. 若只是恢复本 worktree 继续其他 topic，可把 `cqrs-rewrite` 视为“当前已在自然停点完成”的历史入口，不再默认把 benchmark README / XML 清扫当作活跃批处理目标。

## 活跃文档

- 当前 active tracking：`ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
- 当前 active trace：`ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前历史归档：
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`

## 说明

- `RP-131` 及之前的长历史验证、阶段流水与旧恢复点说明已迁移到新的 `archive/` 文件，不再继续堆叠在 active 入口。
- active tracking 现在只保留当前恢复点所需的最小事实、风险、权威验证与下一步，供 `boot` 与后续 PR review 快速恢复。
