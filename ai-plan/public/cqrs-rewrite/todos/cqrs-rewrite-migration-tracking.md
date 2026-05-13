<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-143`
- 当前阶段：`Phase 8`
- 当前 PR 锚点：`PR #350（MERGED，2026-05-13）`
- 当前结论：
  - 在用户允许 subagent 后，本轮按 `context-budget 优先、reviewability 次之、50 files 仅作粗阈值` 重开了一波小型 multi-agent 批处理，
    只接受单文件或窄文件组的 docs/test 切片。
  - 已接受的低风险切片：
    - `GFramework.Cqrs.Benchmarks/Messaging/RequestLifetimeBenchmarks.cs`
      - 修正两处 XML 文档缩进异常
    - `GFramework.Cqrs.Benchmarks/Messaging/NotificationFanOutBenchmarks.cs`
      - 类级摘要明确当前 `GFramework.CQRS` fan-out 对照包含默认顺序发布器与内置 `TaskWhenAllNotificationPublisher`
    - `GFramework.Cqrs.Benchmarks/Messaging/RequestBenchmarks.cs`
    - `GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs`
      - 将 `Mediator` reader-facing 注释统一为 “NuGet `Mediator` 的 source-generated concrete mediator”
    - `GFramework.Cqrs.Tests/Cqrs/CqrsDispatcherContextValidationTests.cs`
      - 补一条 stream 缺失 handler 的失败语义回归，固定当前分支在调用点同步抛 `InvalidOperationException`
  - worker 在 `CqrsGeneratedRequestInvokerProviderTests.cs` 补 request/generated + pipeline 对称测试后，主线程确认这不是环境噪音，而是命中了真实运行时缺口：
    - request 路径在接入 `IPipelineBehavior<,>` 后，会退回 `_handler.Handle(...)`
    - 因此 generated request invoker 无法像 stream 路径那样在 pipeline 末端继续保持优先
  - 主线程已在 `GFramework.Cqrs/Internal/CqrsDispatcher.cs` 收口该缺口：
    - request pipeline executor 现在显式复用当前 binding 缓存的 `RequestInvoker`
    - generated request invoker provider 在接入 pipeline 后保持与无 pipeline 路径一致的调用语义
  - 当前 stop decision：
    - 不再继续下一波
    - 原因不是 `50 files` 阈值耗尽；当前 accepted scope 仍然很小
    - 停止原因是当前上下文已接近本轮安全预算，而剩余候选只剩 descriptor / factory / publisher 类小测试，继续扩批收益明显下降

## 当前活跃事实

- 当前分支：`feat/cqrs-optimization`
- 当前 HEAD / 基线：`origin/main @ 4837aa2a (2026-05-12 20:37:56 +0800)`
- 当前 PR：`PR #350（已合并到 origin/main）`
- 当前写面：
  - `GFramework.Cqrs.Benchmarks/Messaging/NotificationFanOutBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestLifetimeBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsDispatcherContextValidationTests.cs`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsGeneratedRequestInvokerProviderTests.cs`
  - `GFramework.Cqrs/Internal/CqrsDispatcher.cs`
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前基线：
  - `origin/main @ 4837aa2a (2026-05-12 20:37:56 +0800)`
- 当前 batch working-tree diff：`7` 个源码 / 测试文件
- 当前工作面已收口为 docs/test 小切片 + 1 处 request pipeline runtime 修正；没有重新打开 benchmark 工程设计级改造
- 最近已合并提交：
  - `2dd9435c` `fix(cqrs-benchmarks): 修正Mediator基准运行时配置`
  - `e3532fc2` `feat(cqrs-benchmarks): 补齐request生命周期的Mediator对照`
  - `092946e9` `docs(cqrs-benchmarks): 同步startup基准文档边界`

## 当前风险

- 后续若再次评估 `StreamLifetimeBenchmarks` 或 request lifetime 的 `Mediator` parity，仍必须采用独立 compile-time config
  或独立 benchmark 工程，而不是在同一份 source-generated 产物上切换 runtime `ServiceLifetime`。
- 如果 `feat/cqrs-optimization` 继续承载新的 CQRS 任务而不先分支，public recovery 入口会把“已合并的 PR #350”与下一轮新工作混在一起。
- 若未来再开 benchmark XML / docs 波次，仍需要主线程先抽样核对代表文件，避免重复接受误报 inventory。
- 剩余低风险候选主要是 `NotificationPublisher` / invoker descriptor / `CqrsRuntimeFactory` 的单文件测试；它们不是当前上下文预算下的高收益下一波。

## 最近权威验证

- `git rev-list --left-right --count origin/main...HEAD`
  - 结果：通过
  - 备注：在恢复刷新提交前返回 `0 0`，确认 `PR #350` 已完整合入 `origin/main`
- `git diff --name-only origin/main...HEAD | wc -l`
  - 结果：通过
  - 备注：确认当前 branch diff 始终只覆盖两份 `ai-plan` recovery 文档，没有重新打开 CQRS benchmark 代码写面
- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
  - 备注：确认 benchmark reader-facing 文档收口后工程仍保持 `Release` 可编译
- `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
  - 备注：确认 request pipeline 复用 generated invoker 的 runtime 修正未引入编译回归
- `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsGeneratedRequestInvokerProviderTests"`
  - 结果：通过，`Passed: 28, Failed: 0`
  - 备注：确认 generated request invoker 在接入 request pipeline 后仍保持优先，并通过新增回归测试锁定
- `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsDispatcherContextValidationTests"`
  - 结果：通过，`Passed: 7, Failed: 0`
  - 备注：确认 stream 缺失 handler 的失败语义回归与既有上下文校验测试仍全部通过

## 下一推荐步骤

1. 若继续 CQRS 主题的低风险硬化，优先从 `NotificationPublisher`、invoker descriptor、`CqrsRuntimeFactory` 三类单文件测试候选中再挑一条，不要重新打开 benchmark 设计级议题。
2. 若后续重新打开 `Mediator` 生命周期 parity 工作，优先设计独立 compile-time config / 独立 benchmark 工程，并把该设计单独记录到新的 tracking phase。
3. 若只是恢复其他 topic，可把当前 `cqrs-rewrite` active 入口视为“本轮已在上下文预算前的自然停点完成”。

## 活跃文档

- 当前 active tracking：`ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
- 当前 active trace：`ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前历史归档：
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`

## 说明

- `RP-131` 及之前的长历史验证、阶段流水与旧恢复点说明已迁移到新的 `archive/` 文件，不再继续堆叠在 active 入口。
- active tracking 现在只保留当前恢复点所需的最小事实、风险、权威验证与下一步，供 `boot` 与后续 PR review 快速恢复。
