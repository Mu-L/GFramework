<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-136`
- 当前阶段：`Phase 8`
- 当前 PR 锚点：`PR #349`
- 当前结论：
  - 本轮先按 `$gframework-pr-review` 重新确认当前分支最新 GitHub 上下文，确认 `feat/cqrs-optimization` 在 `2026-05-12` 已切到 `PR #349`，不再沿用旧 tracking 中的 `PR #348` 锚点。
  - 随后按 `$gframework-batch-boot 50` 持续协调多波 non-conflicting subagent，基线固定为
    `origin/main @ ef4d3d5d (2026-05-11 17:33:43 +0800)`。
  - 当前 branch 相对基线的累计 diff 约为 `9 files / 1111 lines`；本轮停点由
    `context-budget / reviewability` 决定，而不是 `50 files` 阈值。
  - `PR #349` latest-head review 当前确认仍成立的项只有：
    - `StreamPipelineBenchmarks` 三个公开 benchmark 方法补齐 `<returns>` XML 契约
    - `StreamingBenchmarks.Stream_Mediator` 补齐 `<returns>` XML 契约
    - `CqrsNotificationPublisherTests` 中 fallback publisher 缓存回归测试去掉误导性的“第二次解析返回其它 publisher”分支
    - active tracking / trace 的当前 PR 锚点与下一步入口同步到 `PR #349`
  - tests 侧已补齐并提交：
    - `CqrsRegistrationServiceTests`：补空输入、空项过滤、稳定键排序与跨调用跳过边界
    - `CqrsHandlerRegistrarTests` 与 `CqrsHandlerRegistrarFallbackFailureTests`：
      补 abstract registry 与缺少无参构造器 registry 的回退 / 抛错覆盖
    - `CqrsNotificationPublisherTests`：补“零 publisher 回退到默认顺序发布器并缓存”回归
  - benchmark 侧已补齐并提交：
    - `StreamPipelineBenchmarks`
    - `StreamingBenchmarks` 的 steady-state `Mediator` 对照
    - `GFramework.Cqrs.Benchmarks/README.md` 的 stream coverage / gap 同步
  - 本轮未修改 `GFramework.Cqrs` 运行时代码；notification fallback 与 generated registry 激活守卫均由新回归证明现有实现已满足预期。

## 当前活跃事实

- 当前分支：`feat/cqrs-optimization`
- 当前 PR：`PR #349`
- 当前写面：
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamPipelineBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/README.md`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsHandlerRegistrarFallbackFailureTests.cs`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsHandlerRegistrarTests.cs`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsNotificationPublisherTests.cs`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsRegistrationServiceTests.cs`
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前基线：
  - `origin/main @ ef4d3d5d (2026-05-11 17:33:43 +0800)`
  - 本轮 batch 启动前，分支相对基线的累计 diff 为 `0 files / 0 lines`
  - 当前自然停点时，累计 diff 约为 `9 files / 1111 lines`
- 本轮提交：
  - `ef3cfdc4` `test(cqrs): 补充注册服务边界测试`
  - `bcfecd3c` `test(cqrs): 补充 registrar 激活失败分支测试`
  - `59cab567` `test(cqrs-benchmarks): 新增 stream pipeline benchmark 覆盖`
  - `010b7028` `test(cqrs): 补充通知回退回归覆盖`
  - `ae1c3b89` `test(cqrs-benchmarks): 补齐 stream steady-state Mediator 对照`

## 当前风险

- 分支已累积 5 个窄切片提交；若继续在同一 turn 扩 benchmark + docs，reviewability 会明显下降。
- 新增 benchmark 目前只做了编译验证，尚未执行 `StreamPipelineBenchmarks` 或更新后的 `StreamingBenchmarks` 实际作业。
- `ef3cfdc4` 的 commit body 含字面 `\n`；若后续要整理历史，需要在显式允许的前提下单独处理提交格式。

## 最近权威验证

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsNotificationPublisherTests"`
  - 结果：通过，`Passed: 9, Failed: 0`
- `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsRegistrationServiceTests|FullyQualifiedName~CqrsHandlerRegistrarTests|FullyQualifiedName~CqrsHandlerRegistrarFallbackFailureTests|FullyQualifiedName~CqrsNotificationPublisherTests"`
  - 结果：通过，`Passed: 36, Failed: 0`
- `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Benchmarks/Messaging/StreamPipelineBenchmarks.cs GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs GFramework.Cqrs.Benchmarks/README.md GFramework.Cqrs.Tests/Cqrs/CqrsHandlerRegistrarFallbackFailureTests.cs GFramework.Cqrs.Tests/Cqrs/CqrsHandlerRegistrarTests.cs GFramework.Cqrs.Tests/Cqrs/CqrsNotificationPublisherTests.cs GFramework.Cqrs.Tests/Cqrs/CqrsRegistrationServiceTests.cs ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
  - 结果：通过
- `git diff --check origin/main...HEAD`
  - 结果：通过

## 下一推荐步骤

1. 再次运行 `$gframework-pr-review`，复核 `PR #349` latest-head open thread 是否已随着当前修复提交收敛。
2. 若继续扩 benchmark，优先在 `StreamLifetimeBenchmarks` 或 `StreamStartupBenchmarks` 中补单文件 `Mediator` parity，而不是并行扩多个矩阵。
3. 若切回文档收尾，把 `GFramework.Cqrs/README.md`、`docs/zh-CN/core/command.md`、`docs/zh-CN/core/query.md` 作为单独一波 docs-only 切片处理。

## 活跃文档

- 当前 active tracking：`ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
- 当前 active trace：`ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前历史归档：
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`

## 说明

- `RP-131` 及之前的长历史验证、阶段流水与旧恢复点说明已迁移到新的 `archive/` 文件，不再继续堆叠在 active 入口。
- active tracking 现在只保留当前恢复点所需的最小事实、风险、权威验证与下一步，供 `boot` 与后续 PR review 快速恢复。
