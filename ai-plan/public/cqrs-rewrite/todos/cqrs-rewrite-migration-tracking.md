<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-144`
- 当前阶段：`Phase 8`
- 当前 PR 锚点：`PR #351（OPEN，2026-05-14）`
- 当前结论：
  - 本轮先按 `$gframework-pr-review` 重新抓取当前分支 PR 真值，确认当前在审 PR 已从旧的 `PR #350（MERGED）` 切换为
    `PR #351（OPEN）`。
  - 当前 PR 没有 failed checks；GitHub Test Reporter 显示 `2379 passed / 0 failed`，当前 CI 没有测试阻塞信号。
  - 最新 head review 只有 `coderabbitai[bot]` 的 `4` 条 open threads，且都落在
    `ai-plan/public/cqrs-rewrite/todos/` 与 `ai-plan/public/cqrs-rewrite/traces/` 两份恢复文件上。
  - 这些线程指向的都是有效治理问题，而不是运行时代码缺陷：
    - active tracking 仍停留在 `PR #350`，与当前 `PR #351` 不一致
    - active tracking 同时保留了“7 个源码/测试文件写面”与“branch diff 只剩两份 `ai-plan` 文档”两套互相冲突的事实
    - active trace 重新膨胀为追加式长日志，且包含 Markdown 结构噪音
  - 当前收口策略是把 `cqrs-rewrite` active tracking / trace 刷回“最小可恢复入口”，并把旧阶段流水继续留在
    `archive/`，避免在 `boot` 与后续 PR review 中继续暴露失真的恢复入口。

## 当前活跃事实

- 当前分支：`feat/cqrs-optimization`
- 当前 HEAD：`e5b173c29abb4ad2faf211bf8f20fd2075c1945c`
- 当前基线：`origin/main @ 4837aa2a (2026-05-12 20:37:56 +0800)`
- 当前 PR：`PR #351（OPEN）`
- 当前 review 焦点：
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前工作面：
  - 只收口 `cqrs-rewrite` 的 public recovery 文档，不重新打开 CQRS benchmark、runtime 或测试工程写面。
- 当前 AI review 真值：
  - CodeRabbit latest review：`CHANGES_REQUESTED`（`2026-05-14T02:21:55Z`）
  - open threads：`4`
  - 已本地复核为有效的主题：
    - PR 锚点过期
    - active tracking 事实冲突
    - active trace 结构噪音
    - active trace 体积过大，应继续瘦身

## 当前风险

- 若继续沿用 `PR #350` 的恢复入口，后续 `boot` 与 PR triage 会把“已合并阶段”误当作当前审查上下文。
- 若 active tracking 同时保留历史阶段事实与当前恢复事实，后续恢复会继续遭遇“当前写面”与“当前 branch diff”互相冲突的问题。
- 若 active trace 再次回到追加式长日志，后续每一轮 PR review 都会优先命中文档治理噪音，而不是当前真正需要验证的 CQRS 代码问题。

## 最近权威验证

- `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/current-pr-review.json`
  - 结果：通过
  - 备注：确认当前分支对应 `PR #351`，并抓到 `4` 条 CodeRabbit open threads、`0` failed checks
- `python3 scripts/license-header.py --check --paths ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
  - 结果：通过
  - 备注：本轮改动的 public recovery 文档均保留 Apache-2.0 头
- `git diff --check`
  - 结果：通过
  - 备注：本轮 `ai-plan` 收口未引入 patch 格式或 trailing whitespace 问题
- `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
  - 备注：满足仓库“完成任务前至少通过一条 build validation”的要求
- GitHub Test Reporter（来自 `PR #351` 的最新 review 数据）
  - 结果：通过
  - 备注：`2379 passed / 0 failed / 38.4s`
- MegaLinter（来自 `PR #351` 的最新 review 数据）
  - 结果：`Success with warnings`
  - 备注：当前高信号问题仍是 `ai-plan` 文档治理收口；未观察到新的 CQRS 运行时代码失败信号

## 下一推荐步骤

1. 先完成 `ai-plan/public/cqrs-rewrite/**` 的 active 入口瘦身与事实刷新，再重新运行最小验证。
2. 若文档收口后 PR 仍有 open threads，再次执行 `$gframework-pr-review`，确认是否只剩 stale review 线程。
3. 只有在文档治理线程清空后，才继续评估是否存在新的 CQRS benchmark / runtime / test 跟进项。

## 活跃文档

- 当前 active tracking：`ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
- 当前 active trace：`ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前历史归档：
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`

## 说明

- `RP-131` 及更早长历史已保留在 `archive/`，active 入口不再重复堆叠旧阶段流水。
- `RP-142` 与 `RP-143` 的细节已压缩为当前恢复点结论与 active trace 摘要，避免继续把 active 入口用作追加式日志。
- 本地再次执行 `$gframework-pr-review` 时，GitHub 仍返回旧 head 上的 `4` 条 open threads；在本轮 `ai-plan` 修复提交推到新 head 前，这些线程仍会继续显示为未解决。
