<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移追踪

## 当前恢复摘要

- 当前恢复点：`CQRS-REWRITE-RP-144`
- 当前日期：`2026-05-14`
- 当前分支：`feat/cqrs-optimization`
- 当前 PR：`PR #351（OPEN）`
- 当前目标：
  - 用 `$gframework-pr-review` 重新对齐当前 PR 真值
  - 修复 `cqrs-rewrite` active tracking / trace 的过期与膨胀问题
  - 让 `boot` 与后续 PR triage 回到最小可恢复入口
- 历史归档入口：
  - `RP-131` 及之前：`ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`
  - `RP-131` 及之前对应 tracking：`ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`

## 2026-05-14

### 阶段：PR #351 的 active recovery 入口收口（CQRS-REWRITE-RP-144）

- 先执行 `$gframework-pr-review` 抓取当前分支的 GitHub 真值，而不是沿用 active tracking 里过期的 `PR #350` 状态。
- 当前抓取结果：
  - 当前 PR：`#351`
  - 状态：`OPEN`
  - 最新 reviewed commit：`e5b173c29abb4ad2faf211bf8f20fd2075c1945c`
  - failed checks：`0`
  - 测试汇总：`2379 passed / 0 failed`
  - latest-head open threads：`4`
- 4 条 open threads 的本地复核结论：
  - `todos/cqrs-rewrite-migration-tracking.md`
    - `PR #350（MERGED）` 已不再代表当前审查上下文，必须刷新为 `PR #351（OPEN）`
    - “当前 batch working-tree diff：7 个源码 / 测试文件”与“branch diff 只覆盖两份 `ai-plan` 文档”互相冲突，不能继续同时保留
  - `traces/cqrs-rewrite-migration-trace.md`
    - 出现重复日期标题与多余反引号，属于应当立即清掉的 Markdown 结构噪音
    - active trace 已重新膨胀成追加式长日志，不适合继续作为默认恢复入口
- 当前收口动作：
  - 重写 active tracking，使其只保留 `PR #351` 的当前恢复真值、风险、验证与下一步
  - 重写 active trace，使其只保留 `RP-144` 摘要与必要的最近阶段记录
  - 继续把更早阶段细节留在 `archive/`，避免再次扩大 active 入口
- 本轮本地验证：
  - `python3 scripts/license-header.py --check --paths ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - 再次执行 `$gframework-pr-review`
    - 结果：仍显示旧 head 上的 `4` 条 CodeRabbit open threads
    - 说明：这些线程要等本轮 `ai-plan` 修复提交形成新的 PR head 后，才有机会被 GitHub 标记为 stale 或 resolved
- 当前下一步：
  - 提交本轮 `ai-plan` recovery 入口收口
  - 推送后重新执行 `$gframework-pr-review`，确认当前 open threads 是否随新 head 收敛

## 2026-05-13

### 阶段：request pipeline generated invoker 收口（CQRS-REWRITE-RP-143）

- 在用户允许 subagent 后，本轮只接受 docs/test 小切片与 1 处 request pipeline runtime 修正，未重新打开 benchmark 工程设计级改造。
- 本轮关键结论：
  - `CqrsGeneratedRequestInvokerProviderTests.cs` 的新增对称测试证明 request 路径在接入 `IPipelineBehavior<,>` 后，会退回 `_handler.Handle(...)`
  - `GFramework.Cqrs/Internal/CqrsDispatcher.cs` 已把 request pipeline 末端改为继续复用当前 binding 的 `RequestInvoker`
  - generated request invoker provider 在 pipeline 存在时恢复与无 pipeline 路径一致的调用语义
- 当轮验证：
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsGeneratedRequestInvokerProviderTests"`
    - 结果：通过，`Passed: 28, Failed: 0`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsDispatcherContextValidationTests"`
    - 结果：通过，`Passed: 7, Failed: 0`
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
- 历史说明：
  - `RP-143` 的完整执行流水不再保留在 active trace；若后续需要 worker 边界或更细的阶段材料，应把该阶段补入 archive，而不是继续膨胀 active 入口。
