# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-090`
- 当前阶段：`Phase 8`
- 当前 PR 锚点：`PR #326`
- 当前结论：
  - `GFramework.Cqrs` 已完成对外部 `Mediator` 的生产级替代，当前主线已从“是否可替代”转向“仓库内部收口与能力深化顺序”
  - `dispatch/invoker` 生成前移已扩展到 request / stream 路径，`RP-077` 已补齐 request invoker provider gate 与 stream gate 对称的 descriptor / descriptor entry runtime 合同回归
  - `RP-078` 已补齐 mixed fallback metadata 在 runtime 不允许多个 fallback attribute 实例时的单字符串 attribute 回退回归
  - `RP-079` 已补齐 runtime 缺少 generated handler registry interface 时的 generator 静默跳过回归
  - `RP-080` 已将基础 generation gate 回归扩展到 notification handler interface、stream handler interface 与 registry attribute 缺失分支
  - `RP-081` 已继续补齐基础 generation gate 的 logging 与 DI runtime contract 缺失分支
  - 当前 `RP-082` 已补齐基础 generation gate 的 request handler runtime contract 缺失分支
  - `RP-083` 已补齐 mixed direct / reflected-implementation request 与 stream invoker provider 发射顺序回归
  - `RP-084` 已引入独立 `GFramework.Cqrs.Benchmarks` 项目，作为持续吸收 `Mediator` benchmark 组织方式的第一落点
  - `RP-085` 已补齐 stream request benchmark，对齐 `Mediator` messaging benchmark 的第二个核心场景
  - `RP-086` 已补齐 request pipeline `0 / 1 / 4` 数量矩阵，开始把 benchmark 关注点从单纯 messaging steady-state 扩展到行为编排开销
  - `RP-087` 已补齐 request startup benchmark，把 initialization 与 cold-start 维度正式纳入 `GFramework.Cqrs.Benchmarks`
  - 当前 `RP-088` 已补齐 request invoker reflection / generated-provider 对照，开始直接量化 dispatcher 预热 generated descriptor 的收益
  - 当前 `RP-089` 已补齐 stream invoker reflection / generated-provider 对照，使 generated descriptor 预热收益从 request 扩展到 stream 路径
  - 当前 `RP-090` 已收敛 `PR #326` benchmark review：统一 benchmark 最小宿主构建、冻结 GFramework 容器、限制 MediatR 扫描范围，并恢复 request startup cold-start 对照
  - `ai-plan` active 入口现以 `PR #326` 和 `RP-090` 为唯一权威恢复锚点；`PR #323`、`PR #307` 与其他更早阶段细节均以下方归档或说明为准

## 当前活跃事实

- 当前分支对应 `PR #326`，状态为 `OPEN`
- latest-head review 已从 benchmark 运行级缺陷收敛到剩余文档入口与是否继续接受 benchmark 语义细化的判断
- benchmark 场景现统一通过 `BenchmarkHostFactory` 构建最小宿主：GFramework 侧在 runtime 分发前显式 `Freeze()` 容器，MediatR 侧只扫描当前场景需要的 handler / behavior 类型
- `RequestStartupBenchmarks` 已恢复 `ColdStart_GFrameworkCqrs` 结果产出，不再命中 `No CQRS request handler registered`
- 已新增手动触发的 benchmark workflow；默认只验证 benchmark 项目 Release build，只有显式提供过滤器时才执行 BenchmarkDotNet 运行
- 远端 `CTRF` 最新汇总为 `2274/2274` passed
- `MegaLinter` 当前只暴露 `dotnet-format` 的 `Restore operation failed` 环境噪音，尚未提供本地仍成立的文件级格式诊断

## 当前风险

- 顶层 `GFramework.sln` / `GFramework.csproj` 在 WSL 下仍可能受 Windows NuGet fallback 配置影响，完整 solution 级验证成本高于模块级验证
- `RequestStartupBenchmarks` 为了量化真正的单次 cold-start，引入了 `InvocationCount=1` / `UnrollFactor=1` 的专用 job；该配置会触发 BenchmarkDotNet 的 `MinIterationTime` 提示，后续若要做稳定基线比较，还需要决定是否引入批量外层循环或自定义 cold-start harness
- 仓库内部仍保留旧 `Command` / `Query` API、`LegacyICqrsRuntime` alias 与部分历史命名语义，后续若不继续分批收口，容易混淆“对外替代已完成”与“内部收口未完成”
- 若继续扩大 generated invoker 覆盖面，需要持续区分“可静态表达的合同”与 `PreciseReflectedRegistrationSpec` 等仍需保守回退的场景

## 最近权威验证

- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestStartupBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：`ColdStart_GFrameworkCqrs` 已恢复出数，最新本地输出约 `220-292 us`，MediatR 对照约 `575-616 us`；当前仅剩 BenchmarkDotNet 对单次 cold-start 场景的 `MinIterationTime` 提示
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：确认冻结后的 GFramework 最小宿主与受限扫描的 MediatR 最小宿主均可完成 steady-state request 对照
- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
  - 备注：用于验证新增手动 benchmark workflow 依赖的 benchmark 项目入口仍可在 Release 下编译
- `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --format json --json-output <temporary-json-output>`
  - 结果：通过
  - 备注：确认当前分支对应 `PR #326`，本轮剩余 open AI feedback 主要集中在 benchmark 对照语义与 `ai-plan` 结构收敛
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过
- `dotnet build GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Emits_Request_Invoker_Provider_Metadata_In_Stable_Order_For_Mixed_Direct_And_Reflected_Implementations|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Emits_Stream_Invoker_Provider_Metadata_In_Stable_Order_For_Mixed_Direct_And_Reflected_Implementations"`
  - 结果：通过，`2/2` passed
- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
  - 备注：先后覆盖 `StreamingBenchmarks`、`RequestPipelineBenchmarks`、`RequestStartupBenchmarks`、`RequestInvokerBenchmarks` 与 `StreamInvokerBenchmarks` 的引入后复核
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestStartupBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：部分通过
  - 备注：`Initialization_MediatR` 与 `ColdStart_MediatR` 已可实际运行；`ColdStart_GFrameworkCqrs` 仍因 `No CQRS request handler registered` 无法产出完整对照
- `GIT_DIR=<worktree-git-dir> GIT_WORK_TREE=<worktree-root> python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行，避免脚本内部 plain `git ls-files` 误判仓库上下文
- `git diff --check`
  - 结果：通过
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Provider_Interface|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Descriptor_Enumerator|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Descriptor_Type|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Descriptor_Entry_Type|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Emits_Stream_Invoker_Provider_Metadata_When_Runtime_Contract_Is_Available"`
  - 结果：通过，`5/5` passed
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Request_Invoker_Provider_Metadata_When_Runtime_Lacks_Request_Descriptor_Type|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Request_Invoker_Provider_Metadata_When_Runtime_Lacks_Request_Descriptor_Entry_Type|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Request_Invoker_Provider_Metadata_When_Runtime_Lacks_Request_Provider_Interface|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Request_Invoker_Provider_Metadata_When_Runtime_Lacks_Request_Descriptor_Enumerator"`
  - 结果：通过，`4/4` passed
- `dotnet build GFramework.Cqrs.SourceGenerators/GFramework.Cqrs.SourceGenerators.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行，避免脚本内部 plain `git ls-files` 误判仓库上下文
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Emits_String_Fallback_Metadata_For_Mixed_Fallback_When_Runtime_Disallows_Multiple_Fallback_Attributes"`
  - 结果：通过，`1/1` passed
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Generate_Registry_When_Runtime_Lacks_Handler_Registry_Interface"`
  - 结果：通过，`1/1` passed
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Generate_Registry_When_Runtime_Lacks_Required_Generation_Contract"`
  - 结果：通过，`4/4` passed
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Generate_Registry_When_Runtime_Lacks_Required_Generation_Contract"`
  - 结果：通过，`6/6` passed
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Generate_Registry_When_Runtime_Lacks_Required_Generation_Contract"`
  - 结果：通过，`7/7` passed
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过

## 下一推荐步骤

1. 继续处理 `PR #326` 的剩余 review 收尾，优先保持 benchmark 对照语义与 `ai-plan` active 入口一致
2. 决定是否继续细化 `RequestStartupBenchmarks` 的 cold-start harness，降低 `InvocationCount=1` 带来的 `MinIterationTime` 提示噪音
3. 若需要在 CI 中手动复核 benchmark，优先使用新增 workflow 的 `benchmark_filter` 输入按场景筛选，避免默认运行整个 benchmark 矩阵

## 活跃文档

- 历史跟踪归档：[cqrs-rewrite-history-through-rp043.md](../archive/todos/cqrs-rewrite-history-through-rp043.md)
- 验证历史归档：[cqrs-rewrite-validation-history-through-rp062.md](../archive/todos/cqrs-rewrite-validation-history-through-rp062.md)
- `RP-063` 至 `RP-074` 验证归档：[cqrs-rewrite-validation-history-rp063-through-rp074.md](../archive/todos/cqrs-rewrite-validation-history-rp063-through-rp074.md)
- `RP-062` 至 `RP-076` trace 归档：[cqrs-rewrite-history-rp062-through-rp076.md](../archive/traces/cqrs-rewrite-history-rp062-through-rp076.md)
- CQRS 与 Mediator 评估归档：[cqrs-vs-mediator-assessment-rp063.md](../archive/todos/cqrs-vs-mediator-assessment-rp063.md)
- 历史 trace 归档：[cqrs-rewrite-history-through-rp043.md](../archive/traces/cqrs-rewrite-history-through-rp043.md)
- `RP-046` 至 `RP-061` trace 归档：[cqrs-rewrite-history-rp046-through-rp061.md](../archive/traces/cqrs-rewrite-history-rp046-through-rp061.md)

## 说明

- `PR #261`、`PR #302`、`PR #305`、`PR #307` 及更早阶段的详细过程已不再作为 active 恢复入口；如需追溯，以对应归档文件或历史 trace 段落为准
- active tracking 仅保留当前恢复点、当前风险、最近权威验证与下一推荐步骤，避免 `boot` 落到历史阶段细节
