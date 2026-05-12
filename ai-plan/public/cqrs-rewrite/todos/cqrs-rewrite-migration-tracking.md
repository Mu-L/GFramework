<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-140`
- 当前阶段：`Phase 8`
- 当前 PR 锚点：`PR #349（已于 2026-05-12 合并到 origin/main）`
- 当前结论：
  - 本轮按 `$gframework-batch-boot 50` 恢复后，先重新确认基线仍为
    `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)`，当前已提交 branch diff 为 `14 files`，仍远低于 `50 files`
    阈值；是否继续的主停止信号仍是 context-budget / reviewability，而不是 branch-size 预算。
  - 主线程结合本地抽样核对与两个 explorer 子代理的只读盘点后，确认当前不应再继续按“benchmark XML `<returns>` 批量缺口”扩批：
    - README 一致性盘点成立，`GFramework.Cqrs.Benchmarks/README.md` 的 startup coverage / 解释边界仍可收紧
    - benchmark XML 缺口盘点存在明显误报；代表文件中的 class / benchmark 方法 `<summary>` 与 `<returns>` 已实际存在
    - 因此本轮不接受新的大范围 XML 收口波次，避免把上下文预算消耗在错误候选上
  - 本轮主线程将 `RequestLifetimeBenchmarks` 的 request lifetime 矩阵扩展为 baseline、`GFramework.Cqrs`、NuGet `Mediator` 与 `MediatR` 四组对照：
    - `BenchmarkRequest` 与 `BenchmarkRequestHandler` 增补 `Mediator` 契约实现
    - `Mediator` 宿主按 `Singleton / Scoped / Transient` 三种编译期常量分支生成，满足 source generator 的配置约束
    - `Scoped` 路径通过显式 `CreateScope()` 解析 generated `Mediator.Mediator`，保持与现有 request scoped 对照相同的作用域边界
  - 本轮同时收口两处 benchmark XML 文档漂移：
    - `RequestBenchmarks` 的 handler 说明补齐 NuGet `Mediator` 契约事实
    - `NotificationBenchmarks` 的类说明与 handler 说明补齐 NuGet `Mediator` 对照事实
  - 当前决定在该 parity + docs 收口后停在自然边界：
    - branch-size 仍低于 `50 files`
    - 但下一批低风险候选已不再清晰；继续开波次的收益低于评审与上下文成本
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
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestLifetimeBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/NotificationBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/README.md`
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前基线：
  - `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)`
  - 当前已提交 branch diff：`14 files`
  - 当前分支比 `origin/main` 多 `5` 个提交：`f346110a`、`a016e3d4`、`ab422b05`、`555c7c07`、`c32a1ec4`
  - 当前工作面已收口为 request lifetime parity、两处 benchmark XML 文档修正与对应 `README` / `ai-plan` 同步
- 本轮提交：
  - `f346110a` `feat(cqrs-benchmarks): 补齐 stream startup 的 Mediator 对照路径`
  - `ab422b05` `docs(cqrs-benchmarks): 补齐 request benchmark 返回值注释`
  - `555c7c07` `docs(cqrs-benchmarks): 补齐 request benchmark 返回值文档`
  - `c32a1ec4` `docs(cqrs-benchmarks): 补齐stream与notification基准返回值文档`

## 当前风险

- `StreamStartupBenchmarks` 的 `Mediator` parity 目前只做了编译验证，尚未单独执行 benchmark 作业确认 startup 矩阵运行结果。
- `StreamLifetimeBenchmarks` 仍缺 `Mediator` parity；虽然 `RequestLifetimeBenchmarks` 已证明 `Mediator` 的 compile-time lifetime 可通过常量分支接入，但 stream 侧仍需要额外处理 `IAsyncEnumerable<T>` 与作用域覆盖整个枚举周期的组合边界。
- benchmark XML 盘点若再次依赖粗糙脚本或只读 inventory，仍有把已存在文档误记为缺口的风险；后续若再开 XML 波次，必须先用主线程抽样核对代表文件。
- 本轮已在 request lifetime parity 与文档漂移收口后主动停批次；若后续恢复，优先先做 `StreamStartupBenchmarks` smoke run 或更明确的 parity / docs 候选，而不是继续机械扩张 XML 批次。

## 最近权威验证

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
  - 备注：确认 `RequestLifetimeBenchmarks` 的 NuGet `Mediator` lifetime parity 可在当前生成器约束下编译通过
- `python3 scripts/license-header.py --check`
  - 结果：通过
- `$gframework-pr-review`
  - 结果：`PR #349` 已关闭；latest-head review open thread 经本地核对仅剩 `StreamingBenchmarks.Stream_MediatR()` 的 XML 文档缺口仍成立
- `git --git-dir=/mnt/f/gewuyou/System/Documents/WorkSpace/GameDev/GFramework/.git/worktrees/GFramework-cqrs --work-tree=/mnt/f/gewuyou/System/Documents/WorkSpace/GameDev/GFramework-WorkTree/GFramework-cqrs diff -- GFramework.Cqrs.Benchmarks/Messaging/RequestLifetimeBenchmarks.cs GFramework.Cqrs.Benchmarks/Messaging/RequestBenchmarks.cs GFramework.Cqrs.Benchmarks/Messaging/NotificationBenchmarks.cs`
  - 结果：通过
  - 备注：确认本轮代码面收敛在 request lifetime parity 与两处 XML 文档漂移修正

## 下一推荐步骤

1. 若后续继续 benchmark 波次，优先单独执行 `StreamStartupBenchmarks` 的最小 smoke run，验证新加 `Mediator` startup 路径可运行。
2. 若后续评估 `StreamLifetimeBenchmarks` 的 `Mediator` parity，先复用本轮 request lifetime 的“编译期常量 lifetime 分支”经验，再判断是否值得引入新的 scoped stream helper。
3. 若后续再开 XML / docs 批次，先由主线程逐文件核对代表样本，不要直接沿用误报 inventory 扩批。

## 活跃文档

- 当前 active tracking：`ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
- 当前 active trace：`ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前历史归档：
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`

## 说明

- `RP-131` 及之前的长历史验证、阶段流水与旧恢复点说明已迁移到新的 `archive/` 文件，不再继续堆叠在 active 入口。
- active tracking 现在只保留当前恢复点所需的最小事实、风险、权威验证与下一步，供 `boot` 与后续 PR review 快速恢复。
