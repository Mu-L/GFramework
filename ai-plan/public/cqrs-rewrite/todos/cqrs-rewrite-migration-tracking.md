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
- 当前 PR 锚点：`PR #350（OPEN，2026-05-12）`
- 当前结论：
  - 本轮按 `$gframework-pr-review` 重新抓取 GitHub 真值后，确认当前公开 PR 不是已合并的 `PR #349`，而是仍处于 `OPEN` 状态的 `PR #350`。
  - 最新 AI review 只有 1 条 Greptile open thread，关注点是：
    - `StreamStartupBenchmarks.ColdStart_Mediator()` 与 `RequestLifetimeBenchmarks.SendRequest_Mediator()` 先前只做了编译验证，未实际 smoke-run
  - 主线程按 review 提示执行最小 benchmark smoke run 后，确认 Greptile 线程不是误报，而是命中了真实运行时缺陷：
    - `StreamStartupBenchmarks.ColdStart_Mediator()` 在 BenchmarkDotNet 自动生成宿主里抛出
      `Invalid configuration detected for Mediator. Generated code for 'Transient' lifetime, but got 'Singleton' lifetime from options.`
    - `RequestLifetimeBenchmarks.SendRequest_Mediator()` 的 `Singleton / Scoped` 也抛出同类异常；只有 `Transient` 变体能跑通
  - 根因确认：
    - NuGet `Mediator` 的 DI lifetime 由 source generator 在 benchmark 项目编译期固定
    - 当前工程同时存在默认 `AddMediator()` 与 request lifetime 场景下的 `AddMediator(options => options.ServiceLifetime = ...)`
    - 这会让同一份生成产物在 BenchmarkDotNet 自动生成宿主中出现 compile-time 形状与 runtime options 不一致
  - 本轮收口策略：
    - `BenchmarkHostFactory.CreateMediatorServiceProvider()` 统一显式固定为 `Singleton` compile-time lifetime
    - `RequestLifetimeBenchmarks` 撤回当前无法真实运行的 `Mediator` 生命周期矩阵，只保留 `GFramework.Cqrs` 与 `MediatR`
    - `GFramework.Cqrs.Benchmarks/README.md` 同步收窄 request lifetime coverage，并把 `Mediator` 生命周期矩阵改记为当前缺口
  - 本轮未修改 `GFramework.Cqrs` 运行时代码；修复面限定在 benchmark 宿主装配与 reader-facing docs。

## 当前活跃事实

- 当前分支：`feat/cqrs-optimization`
- 当前 PR：`PR #349（已合并；当前分支暂无新的公开 PR）`
- 当前 PR：`PR #350（OPEN）`
- 当前写面：
  - `GFramework.Cqrs.Benchmarks/Messaging/BenchmarkHostFactory.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestLifetimeBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/README.md`
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 当前基线：
  - `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)`
  - 当前已提交 branch diff：`14 files`
  - 当前分支比 `origin/main` 多 `5` 个提交：`f346110a`、`a016e3d4`、`ab422b05`、`555c7c07`、`c32a1ec4`
- 当前工作面已收口为 `Mediator` benchmark runtime 配置修正、request lifetime coverage 收窄与对应 `README` / `ai-plan` 同步
- 本轮提交：
  - `f346110a` `feat(cqrs-benchmarks): 补齐 stream startup 的 Mediator 对照路径`
  - `ab422b05` `docs(cqrs-benchmarks): 补齐 request benchmark 返回值注释`
  - `555c7c07` `docs(cqrs-benchmarks): 补齐 request benchmark 返回值文档`
  - `c32a1ec4` `docs(cqrs-benchmarks): 补齐stream与notification基准返回值文档`

## 当前风险

- `StreamLifetimeBenchmarks` 仍缺 `Mediator` parity；如果后续要补，必须采用独立 compile-time config 或独立 benchmark 工程，而不是在当前项目里切换 runtime `ServiceLifetime`。
- `RequestLifetimeBenchmarks` 目前不再覆盖 `Mediator`；若后续要恢复该矩阵，也必须先解决 source-generated lifetime 与 BenchmarkDotNet 自动宿主的编译期塑形边界。
- benchmark XML 盘点若再次依赖粗糙脚本或只读 inventory，仍有把已存在文档误记为缺口的风险；后续若再开 XML 波次，必须先用主线程抽样核对代表文件。
- 当前 PR 的 Greptile open thread 在代码修正后虽已有本地验证证据，但线程本身还未在 GitHub 上回复 / resolve。

## 最近权威验证

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
  - 备注：确认统一 `Mediator` compile-time lifetime 后 benchmark 工程仍可编译
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr350-stream-startup-mediator-fixed --filter "*StreamStartupBenchmarks.ColdStart_Mediator*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：`ColdStart_Mediator` 已在 BenchmarkDotNet 自动生成宿主中实际执行，约 `144.036 us / 69.3 KB`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr350-request-lifetime-fixed-rerun --filter "*RequestLifetimeBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：当前矩阵为 `9` 项（baseline / `GFramework.Cqrs` / `MediatR` * `Singleton|Scoped|Transient`），不再包含伪 `Mediator` lifetime 条目
- `$gframework-pr-review`
  - 结果：确认 `PR #350` open，CodeRabbit 已 `APPROVED`，Greptile 仍有 `1` 条 open thread 指向 `StreamStartupBenchmarks.cs`

## 下一推荐步骤

1. 在 GitHub `PR #350` 回应并 resolve 当前 Greptile 线程，说明 `ColdStart_Mediator` 已补 smoke-run，且 request lifetime 的 `Mediator` 矩阵已按 source-generator 真实约束撤回。
2. 若后续评估 `StreamLifetimeBenchmarks` 或 request lifetime 的 `Mediator` parity，优先设计独立 compile-time config / 独立 benchmark 工程，而不是继续在同一项目里切换 runtime `ServiceLifetime`。
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
