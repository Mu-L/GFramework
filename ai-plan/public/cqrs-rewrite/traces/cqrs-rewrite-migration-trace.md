<!--
Copyright (c) 2025-2026 GeWuYou
SPDX-License-Identifier: Apache-2.0
-->

# CQRS 重写迁移追踪

## 2026-05-12

### 阶段：PR #350 的 Mediator runtime 配置收口（CQRS-REWRITE-RP-141）

- 使用 `$gframework-pr-review` 重新抓取当前分支 PR，确认 GitHub 真值已从 active tracking 中过期的 `PR #349` 切换为仍处于 `OPEN` 状态的 `PR #350`。
- 最新 AI review 只剩 1 条 Greptile open thread：
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamStartupBenchmarks.cs:210`
  - 质疑点不是文档，而是 `Mediator` startup / request lifetime 新路径仅编译通过、未实际 smoke-run
- 主线程先按 review 建议做本地 smoke 验证，而不是直接回复线程：
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr350-stream-startup-mediator --filter "*StreamStartupBenchmarks.ColdStart_Mediator*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr350-request-lifetime-mediator --filter "*RequestLifetimeBenchmarks.SendRequest_Mediator*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
- 第一轮 smoke 暴露出 Greptile 线程背后的真实运行时问题，而不是 review 噪音：
  - `StreamStartupBenchmarks.ColdStart_Mediator()` 在 BenchmarkDotNet 自动生成宿主里抛出
    `Invalid configuration detected for Mediator. Generated code for 'Transient' lifetime, but got 'Singleton' lifetime from options.`
  - `RequestLifetimeBenchmarks.SendRequest_Mediator()` 的 `Singleton` / `Scoped` 同样抛出相同异常，只有 `Transient` 分支能实际执行
- 根因判断：
  - `Mediator` 的 DI lifetime 由 source generator 在 benchmark 项目编译期固定
  - 当前项目同时包含默认 `AddMediator()` 和 request lifetime 场景里的 `AddMediator(options => options.ServiceLifetime = ...)`
  - 同一份生成产物在 BenchmarkDotNet 自动生成宿主里因此出现 compile-time lifetime 与 runtime options 不一致
- 主线程修复：
  - `BenchmarkHostFactory.CreateMediatorServiceProvider()` 统一改为显式 `ServiceLifetime.Singleton`
  - `RequestLifetimeBenchmarks` 删除当前无法真实运行的 `SendRequest_Mediator()` 与相关 `Mediator` 生命周期 helper / 契约实现
  - `GFramework.Cqrs.Benchmarks/README.md` 将 request lifetime coverage 收窄为 `GFramework.Cqrs` + `MediatR`，并把 `Mediator` lifetime parity 改记为当前缺口
- 串行验证结果：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr350-stream-startup-mediator-fixed --filter "*StreamStartupBenchmarks.ColdStart_Mediator*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 关键数值：`ColdStart_Mediator ≈ 144.036 us / 69.3 KB`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr350-request-lifetime-fixed-rerun --filter "*RequestLifetimeBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 关键结论：当前 request lifetime 矩阵已收敛为 `9` 项（baseline / `GFramework.Cqrs` / `MediatR` * `Singleton|Scoped|Transient`），不再包含伪 `Mediator` lifetime 条目
- 本轮 stop decision：
  - 不继续把 `Mediator` lifetime parity 硬扩到 request 或 stream lifetime benchmark
  - 原因不是 branch-size；而是 source-generator compile-time config 已明确构成真实边界，继续在同一项目里扩 runtime 切换只会制造新的伪覆盖

### 阶段：request lifetime 的 Mediator parity 与文档漂移收口（CQRS-REWRITE-RP-140）

- 继续按 `$gframework-batch-boot 50` 推进，基线保持为 `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)`。
- 本轮启动时重新测得当前已提交 branch diff 仍为 `14 files / 324 lines`，远低于 `50 files` 阈值；停止与否继续由 context-budget / reviewability 主导。
- 主线程结合两个 explorer 子代理的只读盘点后，接受以下结论：
  - 不再继续按 benchmark XML `<returns>` inventory 机械扩批；粗糙脚本会把“注释位于 `[Benchmark]` 之前”的现有文档误判为缺口
  - `RequestLifetimeBenchmarks` 的 NuGet `Mediator` lifetime parity 是当前仍然真实、且能保持 reviewable 的实现候选
  - `NotificationBenchmarks.cs` 与 `RequestBenchmarks.cs` 仍有两处低风险 XML 文档漂移，均只涉及 NuGet `Mediator` 事实同步
- 本轮主线程实施：
  - `RequestLifetimeBenchmarks.cs`
    - 新增 `GeneratedMediator` 宿主字段、`SendRequest_Mediator()` benchmark 方法与 scoped `Mediator` request helper
    - 将 `BenchmarkRequest` / `BenchmarkRequestHandler` 扩为同时实现 `Mediator` 契约
    - 为 `Mediator` 宿主改用 `Singleton / Scoped / Transient` 三个编译期常量分支，规避 `MSG0007` 对运行时 lifetime 赋值的生成器限制
  - `RequestBenchmarks.cs`
    - 将 handler XML 文档说明补齐为同时实现 `GFramework.CQRS`、NuGet `Mediator` 与 `MediatR`
  - `NotificationBenchmarks.cs`
    - 将类说明与 handler XML 文档说明补齐为同时覆盖 `GFramework.CQRS`、NuGet `Mediator` 与 `MediatR`
  - `GFramework.Cqrs.Benchmarks/README.md`
    - 将 `RequestLifetimeBenchmarks` 的 coverage 更新为包含 NuGet `Mediator` source-generated concrete path
    - 删除“当前没有 request 生命周期下的 NuGet `Mediator` compile-time lifetime 矩阵”这一已过时缺口
- 验证里程碑：
  - 第一次 `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：失败
    - 原因：`RequestLifetimeBenchmarks.cs` 中基于运行时变量写入 `MediatorOptions.ServiceLifetime`，触发 `MSG0007`
  - 主线程修正：
    - 将 `CreateMediatorServiceProvider(HandlerLifetime lifetime)` 收口为 3 个常量分支工厂：
      `CreateSingletonMediatorServiceProvider()`、`CreateScopedMediatorServiceProvider()`、`CreateTransientMediatorServiceProvider()`
  - 第二次 `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
- 当前 stop decision：
  - 不继续开启新的实现波次
  - 原因不是 branch-size 阈值耗尽；当前分支仍只有 `14 files`
  - 停止原因是本轮已经完成一条真实 parity 收口和两处文档漂移修正，继续扩到 `StreamLifetimeBenchmarks` 会显著提高作用域与 review 成本
- 当前下一步：
  - 主线程补跑 `python3 scripts/license-header.py --check --paths ...` 与 `git diff --check`
  - 更新 active tracking / trace 后提交当前 benchmark 代码、README 与 `ai-plan`

### 阶段：README startup coverage 精度同步并停在自然边界（CQRS-REWRITE-RP-139）

- 继续按 `$gframework-batch-boot 50` 推进，基线保持为 `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)`。
- 本轮启动时重新测得当前已提交 branch diff 为 `14 files`，仍远低于 `50 files` 阈值；继续与否的主停止信号仍是
  context-budget / reviewability。
- 本轮主线程先做只读盘点与抽样核对：
  - README 一致性 explorer 结论成立：`GFramework.Cqrs.Benchmarks/README.md` 对 startup coverage / 边界的表述仍可更精确
  - benchmark XML 缺口 explorer 结论未直接接受；主线程抽样检查 `NotificationBenchmarks.cs`、
    `RequestBenchmarks.cs`、`StreamingBenchmarks.cs`、`NotificationStartupBenchmarks.cs` 后确认，
    其 class / benchmark 方法的 `<summary>` 与 `<returns>` 实际已存在，不能继续按“14 个门面文件普遍缺 XML”
    的假设扩批
- 因此本轮 accepted delegated scope 缩成单文件 docs-only worker：
  - `GFramework.Cqrs.Benchmarks/README.md`
    - 把 `StreamStartupBenchmarks` 明确写成 `MediatR`、`GFramework.Cqrs` reflection、
      `GFramework.Cqrs` generated、NuGet `Mediator` 四组 initialization / cold-start 对照
    - 补充 `RequestStartupBenchmarks` 与 `NotificationStartupBenchmarks` 的 `GFramework.Cqrs` startup 路径是
      “单 handler 最小宿主 + 手工注册”模型，不外推到程序集扫描、完整注册协调器、fan-out 或发布策略变体
- worker 回传验收结论：
  - 改动文件未越出 ownership 边界
  - README diff 与代码事实一致，且未引入无法从当前 benchmark 实现验证的表述
- 当前 stop decision：
  - 不继续开启新的 XML 文档波次
  - 原因不是 branch-size 阈值耗尽；当前分支仍只有 `14 files`
  - 停止原因是候选清晰度下降：继续追逐 explorer 误报会降低 reviewability，并无谓增加当前上下文负担
- 当前下一步：
  - 主线程更新 `ai-plan/public/cqrs-rewrite/**`
  - 串行运行 benchmark 工程 build、license-header 与 `git diff --check`
  - 提交 README 与 `ai-plan` 收尾

### 阶段：benchmark XML 契约第 2 波收口（CQRS-REWRITE-RP-138）

- 延续 `$gframework-batch-boot 50`，基线保持为 `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)`。
- 第 2 波启动前，当前分支相对基线的已提交 branch diff 为 `5 files / 177 lines`，明显低于 `50 files` 阈值；本轮继续与否的主停止信号仍是 context-budget / reviewability。
- 主线程本地盘点 `GFramework.Cqrs.Benchmarks/Messaging/*.cs` 的公开 `[Benchmark]` 方法后，确认当前仍有一批与既有收口模式一致的 XML `<returns>` 缺口：
  - `StreamingBenchmarks.Stream_GFrameworkCqrs()`
  - `NotificationBenchmarks` 的 3 个公开 benchmark 方法
  - `NotificationFanOutBenchmarks` 的 5 个公开 benchmark 方法
  - `StreamInvokerBenchmarks` 的 4 个公开 benchmark 方法
  - `StreamLifetimeBenchmarks` 的 4 个公开 benchmark 方法
- 本波 accepted ownership：
  - 主线程
    - `GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs`
    - `GFramework.Cqrs.Benchmarks/Messaging/NotificationBenchmarks.cs`
    - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
    - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
  - worker
    - `StreamLifetimeBenchmarks.cs`
    - `StreamInvokerBenchmarks.cs`
    - `NotificationFanOutBenchmarks.cs`
- worker 回传验收结论：
  - `StreamLifetimeBenchmarks.cs`
    - 只补 `Stream_Baseline`、`Stream_GFrameworkReflection`、`Stream_GFrameworkGenerated`、`Stream_MediatR` 的 `<returns>`
    - worker 自报 `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release` 通过
  - `StreamInvokerBenchmarks.cs`
    - 只补 4 个公开 benchmark 方法的 `<returns>`
    - worker 自报同一条 benchmark 工程 build 通过
  - `NotificationFanOutBenchmarks.cs`
    - 只补 5 个公开 benchmark 方法的 `<returns>`
    - worker 自报 build 遇到 `CS2012`：`obj/Release/net10.0/GFramework.Cqrs.Benchmarks.dll` 被并发进程占用；该失败被判定为并发构建噪音，而不是代码语义问题
- 主线程局部实施：
  - `StreamingBenchmarks.cs`
    - 为 `Stream_GFrameworkCqrs()` 补 `<returns>`
  - `NotificationBenchmarks.cs`
    - 为 `PublishNotification_GFrameworkCqrs()`、`PublishNotification_MediatR()`、`PublishNotification_Mediator()` 补 `<returns>`
- 当前下一步：
  - 主线程串行执行 benchmark 工程 Release build，消除 worker 并发写 `obj/Release` 带来的验证噪音
  - 若串行验证通过，决定是在当前自然停点提交收尾，还是继续 request 侧 XML 契约的下一波低风险批处理

### 阶段：request benchmark XML 契约第 3 波收口后停在自然边界（CQRS-REWRITE-RP-138）

- 第 2 波串行验证通过后，继续用 3 个 worker 扩展 request 系 benchmark 的同类 `<returns>` 收口：
  - `RequestStartupBenchmarks.cs`
  - `RequestBenchmarks.cs` + `RequestPipelineBenchmarks.cs`
  - `RequestInvokerBenchmarks.cs` + `RequestLifetimeBenchmarks.cs`
- worker 回传与 acceptance：
  - `RequestStartupBenchmarks.cs`
    - 只补公开 benchmark 方法缺失的 `<returns>`
    - worker 自报 `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release` 通过
  - `RequestBenchmarks.cs` + `RequestPipelineBenchmarks.cs`
    - 只补公开 benchmark 方法缺失的 `<returns>`
    - worker 自报 build 通过，并已提交：`555c7c07 docs(cqrs-benchmarks): 补齐 request benchmark 返回值文档`
  - `RequestInvokerBenchmarks.cs` + `RequestLifetimeBenchmarks.cs`
    - 只补公开 benchmark 方法缺失的 `<returns>`
    - worker 自报 build 通过，并已提交：`ab422b05 docs(cqrs-benchmarks): 补齐 request benchmark 返回值注释`
- 第 3 波后主线程 stop decision：
  - 不再开启第 4 波 XML 契约批处理
  - 原因不是 branch-size 阈值耗尽；当前分支相对 `origin/main` 仍只有 `9 files / 143 lines`
  - 停止原因是 context-budget / reviewability：剩余候选已不比当前波次更低风险，继续机械扩批收益下降
- 当前下一步：
  - 只做主线程未提交面的串行验证与收尾提交
  - 将干净工作树作为下一次 `boot` 的默认恢复目标

### 阶段：stream startup parity 与文档收尾（CQRS-REWRITE-RP-137）

- 按 `$gframework-batch-boot 50` 恢复后，先重新执行 `$gframework-pr-review`。
- 当前 GitHub 事实：
  - `PR #349` 已关闭并合并到 `origin/main`
  - 基线切换为 `origin/main @ 2b2bec65 (2026-05-12 11:49:39 +0800)`
  - 当前分支相对新基线的已提交 diff 初始为 `0 files / 0 lines`
- latest-head open thread 本地复核：
  - stale
    - `StreamPipelineBenchmarks.Stream_Baseline` 的 `<returns>` 已存在
    - `CqrsNotificationPublisherTests` 的 fallback 缓存安全网已收口
    - trace 的当前 PR / 下一步已同步到 `PR #349`
  - valid
    - `StreamingBenchmarks.Stream_MediatR()` 仍缺 `<returns>` XML 文档
- 第 1 波 accepted delegated scope：
  - `StreamingBenchmarks.cs`
    - worker 补 `Stream_Baseline()` 与 `Stream_MediatR()` 的 `<returns>` XML 契约
    - 主线程验收时确认其中 `Stream_Baseline()` 属于额外收口，不是 latest-head 必修项
  - `StreamStartupBenchmarks.cs`
    - worker 在单文件 ownership 内补 `GeneratedMediator` 宿主字段、setup/cleanup、`Initialization_Mediator()`、`ColdStart_Mediator()`
    - 同文件把 `BenchmarkStreamRequest` / `BenchmarkStreamHandler` 扩成同时支持 `Mediator` stream 合同
    - worker 自主完成并提交：`f346110a feat(cqrs-benchmarks): 补齐 stream startup 的 Mediator 对照路径`
  - `GFramework.Cqrs.Benchmarks/README.md`
    - worker 只收口 `StreamStartupBenchmarks` coverage 与当前 gap 描述，不假设 `StreamLifetimeBenchmarks` 已补 parity
- 主线程验收结论：
  - `StreamLifetimeBenchmarks` 的 `Mediator` parity 被判定为 hard slice，需要 `BenchmarkHostFactory` 与 compile-time lifetime 配套，不再继续作为本 turn 的低风险并行切片
  - 当前自然停点应落在：
    - 已提交的 `StreamStartupBenchmarks` parity
    - 未提交但已验收的 `StreamingBenchmarks.cs` / `README.md` 收尾
    - `ai-plan` 同步到新基线与新恢复点
- 本轮权威验证里程碑：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `python3 scripts/license-header.py --check`
    - 结果：通过
  - `git diff --check`
    - 待当前未提交收尾切片与 `ai-plan` 一并提交前再次运行
- 当前下一步：
  - 提交 `StreamingBenchmarks.cs`、`GFramework.Cqrs.Benchmarks/README.md` 与 `ai-plan/public/cqrs-rewrite/**` 收尾
  - 如需继续 benchmark 波次，优先做 `StreamStartupBenchmarks` 的最小 smoke run，而不是直接展开 `StreamLifetimeBenchmarks`

### 阶段：PR #349 latest-head review 收口（CQRS-REWRITE-RP-136）

- 重新执行 `$gframework-pr-review`，按 GitHub 当前分支状态确认 `feat/cqrs-optimization` 在 `2026-05-12` 对应的是 `PR #349`，不再沿用 active tracking 中的 `PR #348` 锚点。
- 本轮 latest-head open AI thread 复核结论：
  - `StreamPipelineBenchmarks` 的 `Stream_Baseline`、`Stream_GFrameworkCqrs`、`Stream_MediatR` 缺少 `<returns>` XML 契约，接受修复
  - `StreamingBenchmarks.Stream_Mediator` 缺少 `<returns>` XML 契约，接受修复
  - `CqrsNotificationPublisherTests` 的 fallback publisher 缓存回归测试用“第二次解析返回另一个 publisher”充当安全网，和断言消息表达不一致，接受收口为“首次后任何再次解析都直接失败”
  - active tracking / trace 的当前 PR 锚点与下一步入口仍停留在 `PR #348`，接受同步到 `PR #349`
- 本轮主线程实施：
  - `StreamPipelineBenchmarks`
    - 为 3 个公开 benchmark 方法补齐 `<returns>` XML 文档
  - `StreamingBenchmarks`
    - 为 `Stream_Mediator()` 补齐 `<returns>` XML 文档
  - `CqrsNotificationPublisherTests`
    - 把 fallback publisher 缓存回归测试改为“首次返回空数组，后续任何再次解析立即抛 `AssertionException`”，避免测试安全网与失败消息自相矛盾
  - `ai-plan/public/cqrs-rewrite/**`
    - 将 active tracking / trace 的当前 PR 锚点与下一步入口同步到 `PR #349`
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsNotificationPublisherTests"`
    - 结果：通过，`Passed: 9, Failed: 0`
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Benchmarks/Messaging/StreamPipelineBenchmarks.cs GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs GFramework.Cqrs.Tests/Cqrs/CqrsNotificationPublisherTests.cs ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
    - 结果：通过

### 阶段：多波 batch 继续收口（CQRS-REWRITE-RP-135）

- 按 `$gframework-batch-boot 50` 恢复当前 topic，并把基线固定为
  `origin/main @ ef4d3d5d (2026-05-11 17:33:43 +0800)`。
- 启动时确认当前工作树干净，branch diff 为 `0 files / 0 lines`；旧 tracking 中“未提交收尾切片”已不再反映真实仓库状态。
- 第 1 波 accepted delegated scope：
  - `CqrsRegistrationServiceTests`
    - 补空输入不触发 registrar、忽略空项后按稳定程序集键排序并去重、跨调用跳过已注册键时继续处理剩余新程序集
  - `CqrsHandlerRegistrarTests` + `CqrsHandlerRegistrarFallbackFailureTests`
    - 补 abstract registry 与缺少无参构造器 registry 在程序集级回退路径和 direct activation 入口的告警 / 抛错覆盖
  - `StreamPipelineBenchmarks` + `GFramework.Cqrs.Benchmarks/README.md`
    - 新增 `0 / 1 / 4` 个 stream pipeline 行为与 `FirstItem / DrainAll` 观测矩阵
    - README 补齐 stream pipeline coverage、运行示例与 gap 说明
- 第 2 波 accepted delegated scope：
  - `CqrsNotificationPublisherTests`
    - 补“容器未注册 publisher 时回退到 `SequentialNotificationPublisher`，且首次解析后缓存结果”回归
  - `StreamingBenchmarks` + `GFramework.Cqrs.Benchmarks/README.md`
    - 补 steady-state stream 的 `Mediator` 对照
    - README 将 stream steady-state gap 收口为“lifetime / startup 仍缺 `Mediator` parity”
- 主线程验收与修正：
  - 审核 5 个 worker 提交均未越出 ownership 边界
  - 在 `StreamPipelineBenchmarks.cs` 修掉 `git diff --check` 报出的 1 处 trailing whitespace
  - 更新 active tracking / trace 到当前 branch 事实，避免下次 `boot` 继续落到过期恢复点
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsRegistrationServiceTests|FullyQualifiedName~CqrsHandlerRegistrarTests|FullyQualifiedName~CqrsHandlerRegistrarFallbackFailureTests|FullyQualifiedName~CqrsNotificationPublisherTests"`
  - `python3 scripts/license-header.py --check --paths ...`
  - `git diff --check origin/main...HEAD`
- 当前停点判断：
  - 当前 branch diff 约为 `9 files / 1111 lines`
  - 明显低于 `50 files` 阈值
  - 本轮停止信号来自 `context-budget / reviewability`，不是文件预算耗尽
- 当前下一步：
  - 先按需要运行 `$gframework-pr-review`，确认 `PR #349` latest-head open thread 是否已随当前修复提交收敛
  - 若继续扩 benchmark，优先补 `StreamLifetimeBenchmarks` 或 `StreamStartupBenchmarks` 的单文件 `Mediator` parity
  - 若切回文档收尾，把 `GFramework.Cqrs/README.md`、`docs/zh-CN/core/command.md`、`docs/zh-CN/core/query.md` 单独作为 docs-only 下一波

## 2026-05-11

### 阶段：PR #348 latest-head review 再收口（CQRS-REWRITE-RP-134）

- 重新执行 `$gframework-pr-review` 抓取当前分支 `feat/cqrs-optimization` 对应的 `PR #348`
- 本轮 latest-head open AI thread 复核结论：
  - `NotificationLifetimeBenchmarks.HandlerLifetime` 补 `[GenerateEnumExtensions]` 仍判定为泛化误报
    - 仓库没有“产品/benchmark 枚举默认都启用该特性”的现行约定
    - benchmark 项目也未接入 `GFramework.Core.SourceGenerators.Abstractions`，不应为局部对照枚举平白扩大 generator 依赖面
  - `NotificationLifetimeBenchmarks` 的 `_scopedContainer` 释放缺口与公开 benchmark API 的 XML 契约缺口仍成立，接受修复
  - `CqrsHandlerRegistrar` 中 generated descriptor 的“先去重后校验”缺陷仍成立，接受修复并补测试
  - `CqrsHandlerRegistrar` 对 `MethodInfo` 使用 `ReferenceEquals` 的过严比较仍成立，接受修复
  - active tracking / trace 的当前 PR 锚点仍停留在 `PR #347`，接受同步到 `PR #348`
- 本轮主线程实施：
  - `NotificationLifetimeBenchmarks`
    - `Cleanup()` 将 `_scopedContainer` 一并交给 `BenchmarkCleanupHelper.DisposeAll(...)`
    - 为公开 benchmark 方法与公开 handler 方法补齐缺失的 `<returns>` / `<param>` XML 契约
  - `CqrsHandlerRegistrar`
    - request / stream generated descriptor 预热路径改为“先 `TryValidate...`，后写入 `registeredKeys`”
    - descriptor 对齐判断从 `ReferenceEquals(resolvedDescriptor.InvokerMethod, ...)` 调整为 `resolvedDescriptor.InvokerMethod.Equals(...)`
  - `CqrsGeneratedRequestInvokerProviderTests`
    - 新增 request / stream 两个回归用例，锁定“首条同键 descriptor 无效、后条有效时，仍应接受后条有效 generated descriptor”
  - `ai-plan/public/cqrs-rewrite/**`
    - 将 active tracking / trace 的当前 PR 锚点同步到 `PR #348`

### 阶段：PR #347 latest-head review 收口（CQRS-REWRITE-RP-132）

- 使用 `$gframework-pr-review` 重新抓取当前分支 `feat/cqrs-optimization` 对应的 `PR #347`
- 抓取结果显示当前 latest-head 仍有 `CodeRabbit 6` 与 `Greptile 2` open thread，但本地复核后只接受以下仍成立的结论：
  - `Program.cs` 只在命令行 `--artifacts-suffix` 下重启隔离宿主，未覆盖环境变量触发的隔离路径
  - `Program.cs` 缺少“目标隔离宿主目录位于当前宿主输出目录内”的防守式校验
  - `RequestLifetimeBenchmarks` / `StreamLifetimeBenchmarks` 的 `Scoped` 路径每次 benchmark 调用都会新建 runtime，污染生命周期矩阵公平性
  - `ScopedBenchmarkContainer` 的公共/关键成员 XML 契约说明不完整
  - active tracking / trace 已经偏离“快速恢复入口”，需要归档瘦身
- 本轮实现选择：
  - `Program.cs`
    - 把“是否需要重启隔离宿主”统一收敛到 `ArtifactsPath != null`
    - 为隔离宿主目录补充“不得等于或嵌套在当前宿主输出目录内”的校验
  - `BenchmarkHostFactory.cs`
    - scoped request / stream helper 改为复用单个 runtime，只在调用边界进入和释放 scope
  - `ScopedBenchmarkContainer.cs`
    - 从“构造时绑定单次 scope”改为“可重复进入的作用域适配器 + `ScopeLease`”
    - 补齐只读适配器与作用域租约的 XML 合同说明
  - `RequestLifetimeBenchmarks.cs`
    - `Scoped` 路径改为 `GlobalSetup` 时创建单个 scoped runtime
  - `StreamLifetimeBenchmarks.cs`
    - `Scoped` reflection / generated 路径改为 `GlobalSetup` 时各自创建单个 scoped runtime
    - 按语义阶段拆分 `Setup()`，同时清掉 `MA0051`
  - `README.md`
    - 把 `RequestLifetimeBenchmarks` 的 scoped 语义更新为“复用 runtime，仅每次创建真实 scope”
  - `ai-plan/public/cqrs-rewrite/**`
    - 将旧 active tracking / trace 复制归档到
      `archive/todos/cqrs-rewrite-migration-tracking-history-through-rp131.md`
      与
      `archive/traces/cqrs-rewrite-migration-trace-history-through-rp131.md`
    - 重写 active tracking / trace，只保留当前恢复点、关键结论、风险与验证

### 本轮验证

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr347-req-scoped --filter "*RequestLifetimeBenchmarks.SendRequest_GFrameworkCqrs*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：独立宿主目录位于 `BenchmarkDotNet.Artifacts/pr347-req-scoped/host/...`
  - 结果摘要：`Singleton 52.69 ns / 32 B`、`Transient 57.88 ns / 56 B`、`Scoped 144.72 ns / 368 B`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix pr347-stream-scoped --filter "*StreamLifetimeBenchmarks.Stream_GFramework*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：独立宿主目录位于 `BenchmarkDotNet.Artifacts/pr347-stream-scoped/host/...`
  - 结果摘要：
    - `Singleton + FirstItem`：reflection `108.7 ns / 216 B`，generated `110.1 ns / 216 B`
    - `Scoped + FirstItem`：reflection `266.7 ns / 792 B`，generated `267.0 ns / 792 B`
    - `Scoped + DrainAll`：reflection `331.6 ns / 856 B`，generated `332.2 ns / 856 B`

### 当前结论

- `Program.cs` 已不再依赖“命令行后缀”这一单一来源决定宿主隔离，环境变量生效路径也会进入隔离宿主。
- scoped benchmark 的公平性问题已经实质收口：runtime 构造成本不再按每次调用重复计入生命周期矩阵。
- active tracking / trace 已恢复到可供 `boot` 和后续 PR review 快速进入的体量；历史细节仍可通过新的 archive 文件追溯。

### 当前下一步

- 推送本轮变更后，重新运行 `$gframework-pr-review`，确认 `PR #347` 的 latest-head open thread 是否已随着新 head 收敛。

### 阶段：多波 batch 收口与 benchmark / docs 扩面（CQRS-REWRITE-RP-133）

- 按 `$gframework-batch-boot` 启动多波 non-conflicting subagent，基线固定为
  `origin/main @ 3b2e6899d5ffdcfb634b28f3846f57528fbf9196 (2026-05-11T12:25:00+08:00)`。
- 启动前分支累计 diff 为 `0 files / 0 lines`；自然停点时累计 branch diff 约为 `12 files`。
- 主线程把 stop decision 明确交给 `reviewability / context-budget`，没有在仍有文件预算时继续机械追到 `50 files`。
- 本轮 accepted delegated scope：
  - runtime / tests
    - `CqrsNotificationPublisherTests`：补“多 publisher 报错”与“publisher 缓存复用”回归
    - `CqrsGeneratedRequestInvokerProviderTests` + `CqrsHandlerRegistrar`：补 generated descriptor 坏元数据、异常枚举、重复 pair 回退契约
    - `CqrsDispatcherCacheTests`：补 request / stream pipeline presence、executor cache 与上下文重新注入组合分支
    - `CqrsRegistrationServiceTests`：补稳定程序集键 fallback 到 `AssemblyName.Name` / `ToString()` 的回归
  - benchmarks
    - `RequestStartupBenchmarks`：补 `Mediator` startup 对照
    - `StreamStartupBenchmarks`
    - `NotificationStartupBenchmarks`
    - `NotificationLifetimeBenchmarks`
    - `GFramework.Cqrs.Benchmarks/README.md`：收口当前 coverage / gap / smoke 解释边界
  - docs / recovery
    - `GFramework.Cqrs/README.md`
    - `docs/zh-CN/core/cqrs.md`
    - `docs/zh-CN/source-generators/cqrs-handler-registry-generator.md`
    - `docs/zh-CN/core/command.md`
    - `docs/zh-CN/core/query.md`
    - `ai-plan/public/cqrs-rewrite/archive/**` 顶部导航与跳转约定
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsRegistrationServiceTests"`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --artifacts-suffix notif-lifetime --filter "*NotificationLifetimeBenchmarks*"`
- 本轮 benchmark 结果摘要：
  - `RequestStartupBenchmarks`
    - `ColdStart_GFrameworkCqrs 61.648 us / 25336 B`
    - `ColdStart_Mediator 110.867 us / 57872 B`
    - `ColdStart_MediatR 679.103 us / 606256 B`
  - `StreamStartupBenchmarks`
    - `ColdStart_GFrameworkReflection 71.13 us / 25504 B`
    - `ColdStart_GFrameworkGenerated 82.12 us / 28280 B`
    - `ColdStart_MediatR 933.87 us / 678992 B`
  - `NotificationStartupBenchmarks`
    - `ColdStart_GFrameworkCqrs 85.09 us / 24752 B`
    - `ColdStart_Mediator 136.08 us / 62512 B`
    - `ColdStart_MediatR 1.379 ms / 719056 B`
  - `NotificationLifetimeBenchmarks`
    - `Singleton`：`GFramework 295.48 ns / 360 B`，`MediatR 77.99 ns / 288 B`
    - `Scoped`：`GFramework 410.92 ns / 640 B`，`MediatR 213.49 ns / 632 B`
    - `Transient`：`GFramework 311.21 ns / 416 B`，`MediatR 74.36 ns / 288 B`
- 当前收尾判断：
  - branch diff 仍远低于 `50` 文件阈值，但 active 未提交面与 benchmark 运行输出已经足够构成自然 stop boundary
  - 下一步不继续扩 batch，先提交当前收尾切片并回到干净工作树，再按 PR review 结果决定后续波次
