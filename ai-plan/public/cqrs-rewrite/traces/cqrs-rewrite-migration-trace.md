# CQRS 重写迁移追踪

## 2026-05-09

### 阶段：request 零管道 behavior presence cache（CQRS-REWRITE-RP-122）

- 延续 `$gframework-batch-boot 50`，本轮在 `RP-121` 把 notification 线阶段性收口后，重新回到 request steady-state 常量开销，并接受并行 explorer 的共同结论：下一刀应继续减少每次 `SendAsync(...)` 必经的通用查询，而不是回头优化 `HasRegistration(Type)` 内部实现或重试已证伪的 `IContextAware` 类型缓存
- 本轮主线程决策：
  - 只改 `GFramework.Cqrs/Internal/CqrsDispatcher.cs` 与 `GFramework.Cqrs.Tests/Cqrs/CqrsDispatcherCacheTests.cs`，不同时打开 scoped benchmark 宿主或 notification 新公开 API 两条线
  - 为 `CqrsDispatcher` 新增 `_requestBehaviorPresenceCache`，按闭合 `IPipelineBehavior<,>` 服务类型缓存“当前 dispatcher 的容器里是否存在该 request behavior 注册”
  - 保持优化面只覆盖 request `0 pipeline` 热路径；stream 对称缓存与 scoped host benchmark 继续留到后续独立批次
  - 在 `CqrsDispatcherCacheTests` 新增实例级回归，明确“同容器多个 `ArchitectureContext` 解析到同一个 runtime/dispatcher，会共享该缓存；另一独立容器创建的 dispatcher 不共享该缓存”
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsDispatcherCacheTests"`
    - 结果：通过，`11/11` passed
    - 备注：新增回归首轮曾因错误假设“不同 `ArchitectureContext` 必定对应不同 dispatcher”而失败；修正为“同容器共享 runtime、独立容器不共享缓存”后稳定通过
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*RequestBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：默认 request steady-state 当前约为 baseline `5.876 ns / 32 B`、`Mediator` `5.275 ns / 32 B`、`GFramework.Cqrs` `51.717 ns / 32 B`、`MediatR` `56.108 ns / 232 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*RequestLifetimeBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：首次与 `RequestBenchmarks` 并行触发时，BenchmarkDotNet 自动生成项目目录发生 `.nuget.g.props already exists` 冲突；改为串行重跑同一命令后，`Singleton` 下 baseline / `GFramework.Cqrs` / `MediatR` 约 `5.720 ns / 52.490 ns / 56.890 ns`，`Transient` 下约 `5.814 ns / 57.746 ns / 55.545 ns`
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs/Internal/CqrsDispatcher.cs GFramework.Cqrs.Tests/Cqrs/CqrsDispatcherCacheTests.cs`
    - 结果：通过
- 本轮结论：
  - request `0 pipeline` 常量路径再次被压短，默认 steady-state request 与 `Singleton` lifetime 均继续快于当前 `MediatR` short-job 基线
  - `Transient` 仍略慢于 `MediatR`，但相较更早轮次已明显收敛；下一轮若继续 request 热点，更值得继续减少 steady-state 必经路径，或切到 explorer 建议的 `request scoped host + compile-time lifetime` 对齐线，而不是继续打磨已收益有限的 `HasRegistration(Type)` 内部细节

### 阶段：标准架构启动路径 notification publisher 回归（CQRS-REWRITE-RP-121）

- 延续 `$gframework-batch-boot 50`，本轮没有继续扩 notification runtime 语义，而是先给 `RP-120` 刚修复的默认接线补一条更贴近生产的架构启动回归
- 本轮主线程决策：
  - 保持写面只落在 `GFramework.Core.Tests/Architectures/ArchitectureModulesBehaviorTests.cs`，不再改动 `GFramework.Cqrs` / `GFramework.Core` 运行时代码
  - 通过 `Architecture.Configurator` 注册依赖容器 probe 的自定义 `INotificationPublisher`，并在 `OnInitialize()` 显式接入额外程序集 notification handler，验证默认 `Architecture.InitializeAsync()` 路径最终 publish 时不会退回默认顺序策略
  - 用现有 `AdditionalAssemblyNotificationHandlerRegistry` 测试桩承载 handler 执行观察，把本轮信号收敛到“标准架构启动路径是否真正复用自定义 publisher”
- 本轮权威验证：
  - `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ArchitectureModulesBehaviorTests"`
    - 结果：通过，`5/5` passed
  - `python3 scripts/license-header.py --check --paths GFramework.Core.Tests/Architectures/ArchitectureModulesBehaviorTests.cs`
    - 结果：通过
- 本轮结论：
  - 标准 `Architecture.InitializeAsync()` 启动路径现在也被回归锁住：通过 `Configurator` 声明的自定义 `INotificationPublisher` 会在真实 publish 路径里被复用，不会再被 `CqrsRuntimeModule` 创建 runtime 时静默短路成默认顺序发布器
  - notification 线当前已形成“组合根入口 -> 默认接线修复 -> 标准架构启动回归”的闭环；下一轮若继续留在该方向，更合理的是重新评估产品面是否真的需要第三种仓库内置策略，而不是继续堆同层级回归

### 阶段：notification publisher 默认接线修复（CQRS-REWRITE-RP-120）

- 延续 `$gframework-batch-boot 50`，本轮沿着 `RP-119` 的 notification publisher 组合根回归继续向下追，发现这不是单纯的文档或测试补洞，而是默认 runtime 接线存在真实时序缺陷
- 本轮主线程决策：
  - 保持修复面收敛在 notification publisher 单线，不把问题扩散到 request dispatch 热路径或无关模块
  - 让 `CqrsRuntimeFactory.CreateRuntime(...)` 不再在工厂层把 `null` publisher 立即替换成 `SequentialNotificationPublisher`，改由 `CqrsDispatcher` 在真正 publish 时优先复用显式实例或容器内唯一注册策略，最后才回退到默认顺序发布器
  - 同步移除 `CqrsRuntimeModule` 与 `GFramework.Tests.Common/CqrsTestRuntime` 里对 `container.Get<INotificationPublisher>()` 的预解析，避免冻结前可见性再次把策略短路掉
  - 在 `NotificationPublisherRegistrationExtensionsTests` 新增“publisher 依赖容器内探针服务”的真实采用回归，并重新验证 `UseTaskWhenAllNotificationPublisher()` 在默认基础设施路径里会继续调度所有处理器
- 本轮权威验证：
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~NotificationPublisherRegistrationExtensionsTests"`
    - 结果：通过，`7/7` passed
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs/Internal/CqrsDispatcher.cs GFramework.Cqrs/CqrsRuntimeFactory.cs GFramework.Core/Services/Modules/CqrsRuntimeModule.cs GFramework.Tests.Common/CqrsTestRuntime.cs GFramework.Cqrs.Tests/Cqrs/NotificationPublisherRegistrationExtensionsTests.cs`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
- 本轮结论：
  - `UseTaskWhenAllNotificationPublisher()` 与 `UseNotificationPublisher<TPublisher>()` 现在不再只是“能注册进容器”，而是能真正穿过默认 runtime 基础设施参与 publish 路径
  - 本轮属于完整的语义修复批次，应在提交后再决定是否继续 notification 线或切回 request steady-state 热点

### 阶段：notification publisher 泛型组合根入口收口（CQRS-REWRITE-RP-119）

- 延续 `$gframework-batch-boot 50`，本轮在 `feat/cqrs-optimization` 已与 `origin/main` 对齐后，没有直接重开 request dispatch 热路径实验，而是先选择 notification publisher 线上一个更小、可直接评审的采用面切片
- 本轮主线程决策：
  - 保持 `GFramework.Cqrs` runtime 代码不变，只补 `UseNotificationPublisher<TPublisher>()` 的组合根回归与用户文档说明
  - 在 `NotificationPublisherRegistrationExtensionsTests` 新增两条 targeted 回归，确认泛型重载会注册唯一单例策略，且在容器已存在 `INotificationPublisher` 时同样会拒绝重复声明
  - 在 `GFramework.Cqrs/README.md` 与 `docs/zh-CN/core/cqrs.md` 把自定义入口统一写成 `UseNotificationPublisher(...)` / `UseNotificationPublisher<TPublisher>()`，并明确实例重载与泛型重载的生命周期边界
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~NotificationPublisherRegistrationExtensionsTests"`
    - 结果：通过，`6/6` passed
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Tests/Cqrs/NotificationPublisherRegistrationExtensionsTests.cs GFramework.Cqrs/README.md docs/zh-CN/core/cqrs.md ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
- 本轮结论：
  - notification publisher 的组合根采用面现在不再默认读者只能“手里先有一个实例”；文档与回归都已明确容器托管型自定义 publisher 的标准入口
  - 这批仍然保持在低风险、单模块、易评审边界内，适合在完成验证后直接收口为新的恢复点

## 2026-05-08

### 阶段：PR #342 latest-head review 收口（CQRS-REWRITE-RP-118）

- 使用 `$gframework-pr-review` 抓取 `feat/cqrs-optimization` 当前公开 PR，并确认当前锚点已从 `PR #341` 更新为 `PR #342`
- 本轮 latest-head review 结论：
  - `CodeRabbit` 当前仍成立的是 `NotificationFanOutBenchmarks.cs` 中 MediatR 显式 `Handle(...)` 直接返回 `Task.CompletedTask`，导致该对照组绕过共享 `HandleCore(...)` 的空值 / 取消校验
  - `CodeRabbit` 对 `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md` 的两条评论也成立：当前恢复点锚点仍写 `PR #341`，且“最近权威验证”里的 fan-out 数值属于更早轮次，需要显式标注历史来源
  - `Greptile` 额外指出 `GFramework.Cqrs/README.md` 与 `docs/zh-CN/core/cqrs.md` 里 `UseTaskWhenAllNotificationPublisher()` 示例包含多余 `using GFramework.Cqrs.Notification;`；这条在当前 head 仍成立
  - MegaLinter 仍报告 `dotnet-format` restore 失败，但这属于 CI 环境 restore 噪声，不是当前 diff 的格式违规；README 的 MD058 空行问题仍需在本地直接修复
- 本轮主线程决策：
  - 让 `NotificationFanOutBenchmarks` 的四个 MediatR handler 显式转发到 `HandleCore(notification, cancellationToken).AsTask()`，保持与 baseline、`GFramework.Cqrs` 和 NuGet `Mediator` 分支一致的前置检查
  - 在 `GFramework.Cqrs/README.md` 修复表格前后空行，并删除 README / 中文文档中 `UseTaskWhenAllNotificationPublisher()` 示例的多余 `using`
  - 把 `cqrs-rewrite` tracking 当前恢复点推进到 `RP-118`，同步 `PR #342` 锚点，并把早期 fan-out 数值显式标成 `历史基线（RP-112）`
- 本轮权威验证：
  - `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --format json --json-output /tmp/current-pr-review.json`
    - 结果：通过
    - 备注：确认当前分支对应 `PR #342`；CodeRabbit 当前 `4` 条 actionable comments 与 Greptile `3` 条 open thread 已作为本轮本地复核输入
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*NotificationFanOutBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：本轮对称化 MediatR handler 后，fixed `4 handler` fan-out 对照约为 `Mediator` `3.598 ns / 0 B`、baseline `7.033 ns / 0 B`、`MediatR` `257.533 ns / 1256 B`、`GFramework.Cqrs` 顺序 `409.557 ns / 408 B`、`TaskWhenAll` `484.531 ns / 496 B`
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Benchmarks/Messaging/NotificationFanOutBenchmarks.cs GFramework.Cqrs/README.md docs/zh-CN/core/cqrs.md ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
    - 备注：仅剩 `GFramework.sln` 的历史 CRLF 提示，无本轮新增 diff 格式问题

### 阶段：notification publisher 采用矩阵文档收口（CQRS-REWRITE-RP-117）

- 延续 `$gframework-batch-boot 50`，本轮没有继续把自动批处理推到新的 runtime seam，而是先按 tracking 建议复核“notification 线是否还缺采用边界文档”：
  - 当前分支相对 `origin/main`（`7ca21af9`, `2026-05-08 16:12:20 +0800`）的累计 branch diff 约为 `12 files`，仍明显低于 `50` 文件阈值
  - 主线程先短试了一刀 request dispatch 热路径微优化：把 dispatcher 中“运行时类型是否实现 `IContextAware`”改成弱键缓存，并按性能治理规则复跑 `RequestBenchmarks` 与 `RequestLifetimeBenchmarks`
  - 复跑结果表明这条假设没有正收益：默认 steady-state request 回到约 `71.824 ns / 32 B`，`Singleton / Transient` lifetime 约为 `73.191 ns / 32 B` 与 `80.468 ns / 56 B`，因此本轮在同一提交前已完全撤回该运行时代码实验，不把负收益热点带进后续恢复点
- 本轮主线程决策：
  - 保持 `GFramework.Cqrs` runtime 与测试代码不变，只更新 `GFramework.Cqrs/README.md` 与 `docs/zh-CN/core/cqrs.md`
  - 把 `SequentialNotificationPublisher`、`TaskWhenAllNotificationPublisher` 与 `UseNotificationPublisher(...)` 自定义实例三条路径收口到同一张策略矩阵
  - 在用户文档里明确 `TaskWhenAllNotificationPublisher` 是“并行完成 + 聚合失败”语义策略，而不是 fixed fan-out publish 的性能开关
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsDispatcherCacheTests|FullyQualifiedName~CqrsDispatcherContextValidationTests"`
    - 结果：通过，`17/17` passed
    - 备注：首轮与 build 并行触发时出现 `MSB3026` 单次复制重试告警，但同一命令最终稳定通过，未形成代码失败
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*RequestBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：用于否决本轮已撤回的热点假设；默认 steady-state request 对照约为 baseline `5.853 ns / 32 B`、`Mediator` `6.256 ns / 32 B`、`MediatR` `53.401 ns / 232 B`、`GFramework.Cqrs` `71.824 ns / 32 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*RequestLifetimeBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：用于否决本轮已撤回的热点假设；`Singleton` 下 baseline / `MediatR` / `GFramework.Cqrs` 约 `5.259 ns / 58.415 ns / 73.191 ns`，`Transient` 下约 `4.914 ns / 57.150 ns / 80.468 ns`
- 本轮结论：
  - notification publisher 公开入口现在不仅有显式顺序 / 并行 API，也有更直接的策略选择矩阵；读者不再需要从分散段落里拼装“什么时候该选哪条策略”
  - request dispatch 热路径的下一轮探索应显式绕开“类型级 `IContextAware` 判定缓存”这一条已验证无收益的方向，把 context budget 留给更可能影响 steady-state 的热点
  - 当前仍可继续自动推进，但若再开一批 runtime 性能实验，应放在新的自然批次里，避免把已否决假设和新热点混在同一评审单元中

### 阶段：公开顺序 notification publisher 策略（CQRS-REWRITE-RP-116）

- 延续 `$gframework-batch-boot 50`，本轮继续留在 notification publisher 配置面，但不再新增第三方 benchmark 或 runtime seam：
  - 当前分支相对 `origin/main`（`7ca21af9`, `2026-05-08 16:12:20 +0800`）的累计 branch diff 在 `RP-115` 提交后约为 `11 files`，明显低于 `50` 文件阈值
  - `RP-115` 已把采用路径收口到显式组合根扩展，但当前仍只有 `TaskWhenAllNotificationPublisher` 是公开内置策略；默认顺序语义仍主要靠“未注册时的隐式回退”表达
- 本轮主线程决策：
  - 新增公开 `GFramework.Cqrs/Notification/SequentialNotificationPublisher.cs`，并让 `CqrsRuntimeFactory` 默认回退直接使用这条公开顺序策略
  - 删除 `GFramework.Cqrs/Internal/SequentialNotificationPublisher.cs` 的内部副本，避免默认顺序语义同时存在“内部实现”和“公开实现”两套类型来源
  - 为 `NotificationPublisherRegistrationExtensions` 增加 `UseSequentialNotificationPublisher()`，并在回归与用户文档中把“显式顺序策略”与“显式并行策略”作为对称选择面呈现
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~NotificationPublisherRegistrationExtensionsTests|FullyQualifiedName~CqrsNotificationPublisherTests"`
    - 结果：通过，`10/10` passed
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs/Notification/SequentialNotificationPublisher.cs GFramework.Cqrs/CqrsRuntimeFactory.cs GFramework.Cqrs/Extensions/NotificationPublisherRegistrationExtensions.cs GFramework.Cqrs.Tests/Cqrs/NotificationPublisherRegistrationExtensionsTests.cs GFramework.Cqrs/README.md docs/zh-CN/core/cqrs.md ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
- 本轮结论：
  - notification publisher 的公开配置面现已从“一个显式策略 + 一个隐式默认回退”收口成两条对称的内置策略选择：`UseSequentialNotificationPublisher()` 与 `UseTaskWhenAllNotificationPublisher()`
  - 若后续继续 notification 线，更合理的下一刀会是补更细的采用文档或新的策略语义，而不是继续让顺序 / 并行这两条基础选择停留在隐式约定上

### 阶段：notification publisher 组合根配置面（CQRS-REWRITE-RP-115）

- 延续 `$gframework-batch-boot 50`，本轮不再回到 benchmark 宿主，而是沿着 `RP-114` 已明确的性能/语义事实继续收口用户接入缺口：
  - 当前分支相对 `origin/main`（`7ca21af9`, `2026-05-08 16:12:20 +0800`）的累计 branch diff 在启动时仍为 `9 files`，明显低于 `50` 文件阈值
  - `RP-113` / `RP-114` 已证明内置 `TaskWhenAllNotificationPublisher` 的价值主要是语义补齐，但当前用户若要采用它，仍需知道 `INotificationPublisher` 的底层注册细节
- 本轮主线程决策：
  - 新增 `GFramework.Cqrs/Extensions/NotificationPublisherRegistrationExtensions.cs`，提供 `UseNotificationPublisher(...)`、`UseNotificationPublisher<TPublisher>()` 与 `UseTaskWhenAllNotificationPublisher()` 三个显式组合根入口
  - 在 `GFramework.Cqrs.Tests/Cqrs/NotificationPublisherRegistrationExtensionsTests.cs` 补齐回归，确认默认 runtime 基础设施会复用 `UseTaskWhenAllNotificationPublisher()`，且重复策略注册会在组合根阶段被显式阻止
  - 更新 `GFramework.Cqrs/README.md` 与 `docs/zh-CN/core/cqrs.md`，把推荐用法改成组合根扩展，并把 `RP-114` 的 benchmark 结论翻译成用户可用的采用边界
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~NotificationPublisherRegistrationExtensionsTests|FullyQualifiedName~CqrsNotificationPublisherTests"`
    - 结果：通过，`9/9` passed
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs/Extensions/NotificationPublisherRegistrationExtensions.cs GFramework.Cqrs.Tests/Cqrs/NotificationPublisherRegistrationExtensionsTests.cs GFramework.Cqrs/README.md docs/zh-CN/core/cqrs.md ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
- 本轮结论：
  - 这一批已把 notification publisher 的采用路径从“理解内部 seam”收口成“在组合根里显式选择策略”，并让重复策略注册在配置阶段就得到清晰失败信号
  - 若后续仍继续 notification 线，更合理的下一刀会是补第二个内置策略或更细的采用文档，而不是继续要求用户手写容器底层注册

### 阶段：`TaskWhenAll` notification publisher fan-out benchmark（CQRS-REWRITE-RP-114）

- 延续 `$gframework-batch-boot 50`，本轮不再扩新的 notification runtime 能力，而是沿着 `RP-113` 刚落地的内置并行 publisher 继续补验证口径：
  - 当前分支相对 `origin/main`（`7ca21af9`, `2026-05-08 16:12:20 +0800`）的累计 branch diff 启动时为 `9 files`，明显低于 `50` 文件阈值
  - `RP-112` 只量化了默认顺序发布器的 fixed `4 handler` fan-out 成本；`RP-113` 已把 `TaskWhenAllNotificationPublisher` 引入 production runtime，但还没有 benchmark 说明“能力差距收口后，代价是多少”
- 本轮主线程决策：
  - 在 `GFramework.Cqrs.Benchmarks/Messaging/NotificationFanOutBenchmarks.cs` 同时保留 `baseline`、默认顺序 `GFramework.Cqrs`、内置 `TaskWhenAllNotificationPublisher`、NuGet `Mediator` concrete runtime 与 `MediatR` 五组对照
  - 复用同一个冻结 `MicrosoftDiContainer` 创建两个 `ICqrsRuntime`，确保变量集中在 notification publisher 策略，而不是 handler 注册或容器形状差异
  - 更新 `GFramework.Cqrs.Benchmarks/README.md` 与 active tracking，使默认恢复入口直接记录新的 benchmark 口径
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*NotificationFanOutBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：fixed `4 handler` fan-out 对照当前约为 baseline `7.424 ns / 0 B`、`Mediator` `3.854 ns / 0 B`、`MediatR` `225.940 ns / 1256 B`、`GFramework.Cqrs` 默认顺序发布器 `427.453 ns / 408 B`、内置 `TaskWhenAllNotificationPublisher` `472.574 ns / 496 B`
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Benchmarks/Messaging/NotificationFanOutBenchmarks.cs GFramework.Cqrs.Benchmarks/README.md ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
- 本轮结论：
  - 当前 benchmark 说明 `TaskWhenAllNotificationPublisher` 的主要价值是补齐“等待全部处理器并聚合异常”的 notification 语义，而不是在 fixed `4 handler` fan-out steady-state 下带来吞吐收益；它比默认顺序发布器额外增加了约 `45 ns` 与 `88 B`
  - 这组结果足以支持后续把 notification 线的重心转回 API 配置面、使用边界与文档语义，而不是继续机械堆新的 runtime seam 或期待 `TaskWhenAll` 自带性能红利
  - 当前 turn 仍可继续自动推进，但默认停止规则仍以“上下文预算优先、单批可评审边界次之”为准

### 阶段：内置 `TaskWhenAll` notification publisher（CQRS-REWRITE-RP-113）

- 延续 `$gframework-batch-boot 50`，本轮不再继续堆 notification benchmark 维度，而是直接把上一批已经量化清楚的 capability gap 收口到 runtime：
  - `RP-111` / `RP-112` 已证明当前 notification publish 无论单处理器还是固定 fan-out，都和 `Mediator` 的 publish strategy 能力差距相关，而不只是“缺 benchmark”
  - 当前分支相对 `origin/main` 的累计 branch diff 仍明显低于 `50` 文件阈值，因此适合用一个单模块、可回归、可文档化的能力切片继续自动推进
- 本轮主线程决策：
  - 新增 `GFramework.Cqrs/Notification/TaskWhenAllNotificationPublisher.cs`，提供公开内置并行 notification publisher，并把“同步抛出的处理器异常也收敛到返回任务中”作为实现约束
  - 在 `GFramework.Cqrs.Tests/Cqrs/CqrsNotificationPublisherTests.cs` 补齐针对新策略的回归，确认它不会像默认顺序发布器那样在首个失败处停止其余处理器
  - 更新 `GFramework.Cqrs/README.md` 与 `docs/zh-CN/core/cqrs.md`，写明切换方式，以及“不保证顺序 / 等待全部处理器完成 / 统一暴露异常或取消结果”的采用边界
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsNotificationPublisherTests"`
    - 结果：通过
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs/Notification/TaskWhenAllNotificationPublisher.cs GFramework.Cqrs.Tests/Cqrs/CqrsNotificationPublisherTests.cs GFramework.Cqrs/README.md docs/zh-CN/core/cqrs.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
- 本轮结论：
  - `GFramework.Cqrs` 现在不再只有“自定义 seam”这一种 notification publisher 扩展方式，而是先提供了一个仓库维护的内置并行策略，开始实质缩小和 `Mediator` 在 publisher strategy 上的能力差距
  - 这批改动保持默认顺序语义不变，因此风险主要落在“新策略的异常聚合和用户理解边界”，已通过测试和文档同步收口
  - 当前可以继续自动推进，但更合理的下一批应优先补新策略的 benchmark 或继续评估 notification publisher 配置面，而不是回头重复扩更多 fan-out benchmark

### 阶段：notification fan-out publish benchmark（CQRS-REWRITE-RP-112）

- 延续 `$gframework-batch-boot 50`，本轮没有直接切入 notification runtime 或 publisher strategy，而是先补齐固定 `4 handler` 的 fan-out publish 对照：
  - `RP-111` 已量化单处理器 notification publish，但还缺“同一路径在固定多处理器 fan-out 时是否保持同级差距”的事实
  - 继续机械扩充 `HandlerCount` 参数矩阵会把 `Mediator` compile-time 处理器集合、MediatR 扫描过滤与 benchmark 注册变量混在一起；固定 `4 handler` 场景更容易保持三方对照口径稳定
- 本轮主线程决策：
  - 新增 `GFramework.Cqrs.Benchmarks/Messaging/NotificationFanOutBenchmarks.cs`，固定 4 个 handler，比对 baseline、`GFramework.Cqrs`、NuGet `Mediator` concrete runtime 与 `MediatR` 的 publish 开销
  - 让 baseline 直接顺序调用 4 个 handler，避免把 fan-out 的额外调用成本误归因为框架 dispatch 自身
  - 更新 `GFramework.Cqrs.Benchmarks/README.md`，明确 notification benchmark 现在同时覆盖单处理器与固定 4 处理器 fan-out 场景
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*NotificationFanOutBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：历史基线（`RP-112`）固定 `4 handler` notification fan-out 对照约为 baseline `8.302 ns / 0 B`、`Mediator` `4.314 ns / 0 B`、`MediatR` `230.304 ns / 1256 B`、`GFramework.Cqrs` `434.413 ns / 408 B`
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Benchmarks/Messaging/NotificationFanOutBenchmarks.cs GFramework.Cqrs.Benchmarks/README.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
    - 备注：仅剩 `GFramework.sln` 的历史 CRLF 提示，无本轮新增 diff 格式问题
- 本轮结论：
  - notification 路径现在已同时具备“单处理器 publish”与“固定 4 处理器 fan-out publish”两条三方对照基线，足以支撑后续是否值得切进 publisher strategy 或 runtime 热点
  - 当前更有价值的下一步不是继续横向堆更多 fan-out 场景，而是转向 publisher strategy / 异常语义，或回到 request dispatch 常量开销这类更可能产生真实运行时收益的切片
  - 在 branch diff 仍明显低于阈值时可以继续自动推进，但应把“上下文预算接近约 80%”继续视为优先停止信号

### 阶段：notification publish 补齐 `Mediator` concrete runtime 对照（CQRS-REWRITE-RP-111）

- 延续 `$gframework-batch-boot 50`，本轮重新按 skill 规则复核 branch diff 基线与容量：
  - `origin/main` = `7ca21af9`，提交时间 `2026-05-08 16:12:20 +0800`
  - 本地 `main` = `c2d22285`，已落后于 remote-tracking ref，因此不作为本轮 baseline
  - 当前分支 `feat/cqrs-optimization` 与 `origin/main` 的累计 branch diff 为 `0 files / 0 lines`
  - 当前工作树干净，且上一个自然批次 `RP-110` 已并入 `origin/main`；因此本轮不是“续做未提交热路径”，而是基于 active topic 重新选择下一块低风险 CQRS benchmark 切片
- 本轮接受的只读探索结论：
  - `NotificationBenchmarks` 仍停留在 `GFramework.Cqrs` vs `MediatR` 的双方对照，缺少 request steady-state 已具备的 `Mediator` concrete runtime 高性能参照物
  - 对 notification 路径直接补 generated invoker/provider 的性价比不高：dispatcher 当前对 notification 反射委托已按消息类型弱缓存，steady-state publish 的主要差距不在“每次都反射”
  - 因此本轮更高信号、边界更清晰的切片是先补 benchmark 对照口径，而不是为了对称性新增一层 runtime seam
- 本轮主线程决策：
  - 在 `GFramework.Cqrs.Benchmarks/Messaging/NotificationBenchmarks.cs` 新增 `Mediator` concrete runtime 宿主、`PublishNotification_Mediator()` benchmark 方法，以及对应的 `Mediator.INotification` / `Mediator.INotificationHandler<T>` 合同实现
  - 保持现有 `GFramework.Cqrs` 与 `MediatR` notification publish 路径不变，只扩充对照组，确保这批仍然是单模块、低风险、可直接评审的 benchmark 收口
  - 更新 `GFramework.Cqrs.Benchmarks/README.md` 与 active tracking，使 notification 场景的公开说明和恢复入口都反映新的三方对照事实
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*NotificationBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：notification publish 三方对照当前约为 `Mediator` `1.108 ns / 0 B`、`MediatR` `97.173 ns / 416 B`、`GFramework.Cqrs` `291.582 ns / 392 B`
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Benchmarks/Messaging/NotificationBenchmarks.cs GFramework.Cqrs.Benchmarks/README.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
    - 备注：仅剩 `GFramework.sln` 的历史 CRLF 提示，无本轮新增 diff 格式问题
- 本轮结论：
  - notification 场景现在也拥有了与 request steady-state 对称的 `Mediator` concrete runtime 参照物，后续再讨论 `notification publisher` 策略或 runtime 热点时，不再只能拿 `MediatR` 做外部对照
  - 当前最值得保留的结论不是“立刻给 notification 也上 generated invoker/provider”，而是 `GFramework.Cqrs` 单处理器 publish 相对 `Mediator` 与 `MediatR` 的量级差距已经被量化出来，可为后续是否继续压 notification 路径提供依据
  - 本轮到这里属于新的自然批次边界；下一轮若继续沿用 `$gframework-batch-boot 50`，更适合从多处理器 publish / publisher strategy 或更高价值的 request 常量开销热点里再选一块，而不是在同一 turn 里继续堆 notification 基准扩展

### 阶段：PR #341 latest-head review 尾声收口（CQRS-REWRITE-RP-110）

- 再次使用 `$gframework-pr-review` 抓取 `PR #341` latest-head review，确认当前 open thread 已收敛到：
  - `BenchmarkHostFactory.cs` 的 legacy runtime alias 防守式类型检查 thread，但当前 head 已存在 `RegisterLegacyRuntimeAlias(...)` 的显式类型校验与实际类型信息异常，属于 GitHub 未 resolve 的 stale thread
  - `RequestBenchmarks.cs` / `CqrsDispatcher.cs` 的 Greptile thread，对应“程序集级 registry 扩散”与“faulted ValueTask 失败语义”均已在当前 head 修复，属于 stale thread
  - 仍然成立且值得当前收口的只剩 `CqrsDispatcherContextValidationTests.cs` 的 strict mock 脆弱性，以及本 trace 中 `本轮下一步` 与 `本轮权威验证` 重复的问题
- 本轮主线程决策：
  - 为 `SendAsync_Should_Return_Faulted_ValueTask_When_Handler_Is_Missing()` 补齐 `HasRegistration(...)` 与 `GetAll(...)` 的防御性 mock，降低该测试对 dispatcher 内部检查顺序的隐式耦合
  - 删除 `RP-109` 记录中重复 `本轮权威验证` 的 `本轮下一步` 段落，保持默认恢复入口只保留仍有价值的恢复信息
- 本轮权威验证：
  - `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --format json --json-output /tmp/current-pr-review.json`
    - 结果：通过
    - 备注：确认当前分支对应 `PR #341`；latest-head 当前仍显示 `CodeRabbit 2` / `Greptile 2` open thread，但其中运行时/benchmark 两条已在本地失效
  - `dotnet build GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
    - 备注：并行验证首轮曾因 `build` 与 `test` 同时写入同一输出 DLL 触发 `MSB3026` 单次复制重试；改为串行重跑同一命令后稳定通过
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsDispatcherContextValidationTests"`
    - 结果：通过，`6/6` passed
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Tests/Cqrs/CqrsDispatcherContextValidationTests.cs`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
    - 备注：仅剩 `GFramework.sln` 的历史 CRLF 提示，无本轮新增 diff 格式问题

### 阶段：PR #341 latest-head review 收口（CQRS-REWRITE-RP-109）

- 使用 `$gframework-pr-review` 抓取 `feat/cqrs-optimization` 当前公开 PR，并确认当前锚点已从 `PR #340` 更新为 `PR #341`
- 本轮 latest-head review 结论：
  - `CodeRabbit` 仍有 `BenchmarkHostFactory.cs` 的 legacy runtime 硬转型、`StreamLifetimeBenchmarks.cs` 的注释缺口，以及 `.agents/skills/gframework-batch-boot/SKILL.md` 的 `MD005` 缩进问题
  - `Greptile` 指出的两条仍然成立：benchmark 项目里通过 `RegisterCqrsHandlersFromAssembly(typeof(...).Assembly)` 会把同程序集的其他 generated registry 一并激活，扩大 benchmark 宿主的服务索引基线；`CqrsDispatcher.SendAsync(...)` 直接去掉 `async/await` 后也把原本的 faulted-`ValueTask` 失败语义改成了同步抛出
- 本轮主线程决策：
  - 在 `GFramework.Cqrs.Internal.CqrsHandlerRegistrar` 新增 direct generated-registry 激活入口，并通过 `InternalsVisibleTo` 暴露给 `GFramework.Cqrs.Benchmarks`，让 benchmark 宿主只激活当前场景的 generated registry
  - 把 `RequestBenchmarks`、`RequestPipelineBenchmarks`、`StreamingBenchmarks`、`StreamLifetimeBenchmarks` 以及 request/stream invoker benchmark 的 generated 宿主全部切到定向 registry 接线，避免同程序集其他 registry 扩大冻结索引和 descriptor 预热基线
  - 在 `BenchmarkHostFactory` 里用防守式类型检查注册 legacy runtime alias，并补充 stream lifetime runtime 二次创建的注释
  - 让 `CqrsDispatcher.SendAsync(...)` 通过 `ValueTask.FromException<TResponse>(...)` 恢复旧的 faulted-`ValueTask` 失败语义，同时保留成功路径的 direct-return 热路径
  - 补齐 `CqrsGeneratedRequestInvokerProviderTests` 与 `CqrsDispatcherContextValidationTests` 的 targeted 回归，并顺手修正 batch boot skill 的 markdown 缩进
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`1 warning / 0 error`
    - 备注：仅出现 `MSB3026` 单次复制重试告警，随后成功产出 `net10.0` 目标；未出现编译失败或新增代码警告
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsDispatcherContextValidationTests|FullyQualifiedName~CqrsGeneratedRequestInvokerProviderTests"`
    - 结果：通过，`24/24` passed
    - 备注：首轮并行验证时因与 build 同时运行触发 MSBuild 输出文件锁竞争；改为串行重跑同一命令后稳定通过
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs/Properties/AssemblyInfo.cs GFramework.Cqrs/Internal/CqrsHandlerRegistrar.cs GFramework.Cqrs/Internal/CqrsDispatcher.cs GFramework.Cqrs.Benchmarks/Messaging/BenchmarkHostFactory.cs GFramework.Cqrs.Benchmarks/Messaging/RequestBenchmarks.cs GFramework.Cqrs.Benchmarks/Messaging/RequestPipelineBenchmarks.cs GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs GFramework.Cqrs.Benchmarks/Messaging/StreamLifetimeBenchmarks.cs GFramework.Cqrs.Benchmarks/Messaging/RequestInvokerBenchmarks.cs GFramework.Cqrs.Benchmarks/Messaging/StreamInvokerBenchmarks.cs GFramework.Cqrs.Tests/Cqrs/CqrsDispatcherContextValidationTests.cs GFramework.Cqrs.Tests/Cqrs/CqrsGeneratedRequestInvokerProviderTests.cs`
    - 结果：通过
    - 备注：仓库脚本默认内部调用未绑定 worktree 的 `git ls-files`，因此本轮按修改文件列表显式 `--paths` 校验
  - `git diff --check`
    - 结果：通过

### 阶段：stream handler 生命周期矩阵 benchmark（CQRS-REWRITE-RP-108）

- 延续 `$gframework-batch-boot 50`，本轮继续使用 `origin/main` 作为 branch diff 基线，并先复核：
  - `origin/main` = `4d6dbba6`，提交时间 `2026-05-08 11:13:33 +0800`
  - 当前分支 `feat/cqrs-optimization` 相对 `origin/main` 的累计 branch diff 为 `14 files / 507 lines`
  - 当前 turn 虽然仍低于 `50 files` 阈值，但已加载多轮 recovery / benchmark 输出；因此只允许再推进一个单模块、低风险 benchmark 切片
- 本轮接受的只读探索结论：
  - `RequestLifetimeBenchmarks` 已覆盖 request 的 `Singleton / Transient` 生命周期矩阵，但 stream 侧仍缺少对称的 handler 生命周期对照
  - `StreamingBenchmarks` 已在 `RP-107` 切到 generated-provider 宿主，适合作为 stream 生命周期矩阵的宿主基础；继续退回纯反射路径会让“生命周期变量”和“descriptor 路径变量”混在一起
  - 如果让 generated registry 顺手注册默认单例 handler，会破坏生命周期矩阵的变量控制，因此 registry 只能暴露 descriptor，不能抢先锁死 handler 生命周期
- 本轮主线程决策：
  - 新增 `StreamLifetimeBenchmarks`，对齐 request 生命周期矩阵，只比较 `Singleton / Transient` 两档，继续明确把 `Scoped` 留给未来显式 scoped host
  - 新增 `GeneratedStreamLifetimeBenchmarkRegistry`，只提供 handwritten generated stream invoker descriptor，不直接注册 handler
  - 让 `StreamLifetimeBenchmarks` 使用 `RegisterCqrsHandlersFromAssembly(typeof(StreamLifetimeBenchmarks).Assembly)` 建立 generated-provider 宿主，再显式按 benchmark 参数注册 `Singleton / Transient` handler 生命周期
  - 更新 `GFramework.Cqrs.Benchmarks/README.md`，把 stream 生命周期矩阵列为已覆盖场景，并从“后续扩展方向”里移除这项待办
- 本轮验证过程的重要补充：
  - 首次并行触发 `RequestBenchmarks` / `RequestLifetimeBenchmarks` / `StreamLifetimeBenchmarks` 时，在同一 autogenerated BenchmarkDotNet 目录下复现了文件已存在冲突与 bootstrap 异常；这是 benchmark 基础设施层面的并行目录竞争，不是代码缺陷
  - 改为串行重跑后三组 benchmark 全部稳定通过，因此本轮将“BenchmarkDotNet 在当前仓库里不应并行运行多条 `dotnet run --project ... --filter ...` 会话”视为有效执行约束
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Benchmarks/Messaging/GeneratedStreamLifetimeBenchmarkRegistry.cs GFramework.Cqrs.Benchmarks/Messaging/StreamLifetimeBenchmarks.cs GFramework.Cqrs.Benchmarks/README.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：steady-state request 对照约为 baseline `5.336 ns / 32 B`、`Mediator` `5.564 ns / 32 B`、`MediatR` `53.307 ns / 232 B`、`GFramework.Cqrs` `64.745 ns / 32 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestLifetimeBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：`Singleton` 下 baseline / `MediatR` / `GFramework.Cqrs` 约 `4.309 ns / 51.923 ns / 67.981 ns`；`Transient` 下约 `5.029 ns / 54.435 ns / 76.437 ns`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*StreamLifetimeBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：`Singleton` 下 baseline / `GFramework.Cqrs` / `MediatR` 约 `80.144 ns / 137.515 ns / 229.242 ns`，`Transient` 下约 `77.198 ns / 144.998 ns / 228.185 ns`
- 本轮结论：
  - stream 生命周期矩阵现在已与 request 生命周期矩阵对称，且继续沿用 generated-provider 宿主路径，没有把变量退化回纯反射 binding
  - `GFramework.Cqrs` 在 stream `Singleton / Transient` 两档下都明显快于 `MediatR`，同时保持接近 baseline 的分配规模；`Transient` 仅从 `240 B` 小幅增至 `264 B`
  - 真正的停止依据仍是上下文预算安全。虽然 branch diff 只有 `14 files`，但当前 turn 已包含多轮 benchmark 输出和恢复文档，因此本批提交后应主动停止
  - 下一轮若继续性能线，更值得优先看 notification publish 或更高价值的 request 常量开销热点，而不是继续做同层级 benchmark 宿主补齐

### 阶段：默认 stream benchmark 吸收 generated provider 宿主（CQRS-REWRITE-RP-107）

- 延续 `$gframework-batch-boot 50`，但本轮按用户新增要求把默认停止依据改为“AI 上下文预算优先，建议在预计接近约 80% 安全上下文占用前收口”；在真正落代码前先复核：
  - `origin/main` = `4d6dbba6`，提交时间 `2026-05-08 11:13:33 +0800`
  - 当前分支 `feat/cqrs-optimization` 相对 `origin/main` 的累计 branch diff 为 `10 files / 507 lines`
  - 当前 turn 已加载 `AGENTS.md`、`gframework-batch-boot` / `gframework-boot`、active tracking/trace、上一轮 benchmark 结果与多次 validation 输出，因此继续一个自然批次可以接受，但不应在本次提交后继续无界循环
- 本轮接受的只读探索结论：
  - 默认 request / request pipeline 宿主都已吸收 generated provider，但 `StreamingBenchmarks` 仍停在“直接注册单个 stream handler”的旧宿主路径，口径与 `StreamInvokerBenchmarks` / 默认 request 组不对称
  - 默认 stream steady-state 场景已经足够独立，适合用一份新的 handwritten generated stream registry 最小化收口，而不用再修改 runtime 语义
  - 用户要求把停止条件从 changed files 改成 AI 上下文预算，因此 skill 文档本身也属于这一批必须一起落下的恢复边界更新
- 本轮主线程决策：
  - 新增 `GeneratedDefaultStreamingBenchmarkRegistry`，用 handwritten generated registry + `ICqrsStreamInvokerProvider` + `IEnumeratesCqrsStreamInvokerDescriptors` 为 `StreamingBenchmarks.BenchmarkStreamRequest` 提供真实的 generated stream invoker descriptor
  - 让 `StreamingBenchmarks` 改用 `RegisterCqrsHandlersFromAssembly(typeof(StreamingBenchmarks).Assembly)` 建容器，并在 `Setup/Cleanup` 前后显式清理 dispatcher 静态缓存
  - 更新 `GFramework.Cqrs.Benchmarks/README.md`，明确默认 stream steady-state benchmark 也已接上 handwritten generated stream invoker provider
  - 更新 `.agents/skills/gframework-batch-boot/SKILL.md` 与 `.agents/skills/gframework-boot/SKILL.md`，明确“上下文预算接近约 80% 时优先停止，branch diff 文件/行数只作次级仓库范围信号”
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Benchmarks/Messaging/GeneratedDefaultStreamingBenchmarkRegistry.cs GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs GFramework.Cqrs.Benchmarks/README.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：steady-state request 对照约为 baseline `5.608 ns / 32 B`、`Mediator` `5.445 ns / 32 B`、`MediatR` `57.071 ns / 232 B`、`GFramework.Cqrs` `64.825 ns / 32 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestLifetimeBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：`Singleton` 下 baseline / `MediatR` / `GFramework.Cqrs` 约 `4.446 ns / 51.331 ns / 69.275 ns`；`Transient` 下约 `4.918 ns / 56.382 ns / 74.301 ns`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*StreamingBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：默认 stream steady-state 对照约为 baseline `5.535 ns / 32 B`、`MediatR` `59.499 ns / 232 B`、`GFramework.Cqrs` `66.778 ns / 32 B`
- 本轮结论：
  - 默认 stream steady-state benchmark 现在也已切到 generated-provider 宿主路径，request / pipeline / stream 三个默认宿主场景的 benchmark 口径终于对齐
  - `StreamingBenchmarks` 的 `GFramework.Cqrs` 结果约 `66.778 ns / 32 B`，仍慢于 `MediatR`，但没有新增分配或明显回退，说明这次宿主收口是低风险可接受的
  - 更重要的是，默认停止依据已从“branch diff 文件数是否触顶”改成“AI 上下文预算是否接近约 80%”；结合当前 turn 已加载的大量 recovery/validation/benchmark 输出，本次提交后应主动停止，而不是继续机械扩批
  - 下一轮若继续性能线，应从 `RP-107` 恢复点重新进入，并优先挑选新的高价值热点族，而不是沿着当前 turn 再追加更多同类宿主收口

### 阶段：request pipeline benchmark 吸收 generated provider 宿主（CQRS-REWRITE-RP-106）

- 延续 `$gframework-batch-boot 50`，本轮基于 `RP-105` 已验证的默认 request 宿主接线继续推进，并先复核 branch diff 基线：
  - `origin/main` = `4d6dbba6`，提交时间 `2026-05-08 11:13:33 +0800`
  - 当前分支 `feat/cqrs-optimization` 相对 `origin/main` 的累计 branch diff 为 `8 files / 358 lines`
  - 当前工作树待提交改动只集中在 `RequestPipelineBenchmarks`、对应 handwritten generated registry 与 benchmark `README`，因此继续自动推进下一批 pipeline 宿主收口
- 本轮接受的只读探索结论：
  - `RP-105` 已证明“让默认 request 宿主真实接上 generated request invoker provider”能稳定压低 steady-state request，因此 pipeline benchmark 仍保留旧的“直接注册单个 handler”路径会让口径不对齐
  - 之前已被 benchmark 否决的“总是 `GetAll(Type)` 做零 pipeline 探测”不应回头重试；下一刀更合理的是把 pipeline benchmark 也切到真实程序集注册入口
  - `RequestPipelineBenchmarks` 只需要补一份与 `RequestBenchmarks` 对称的 handwritten generated registry，就能最小化改动并保持 runtime 语义不变
- 本轮主线程决策：
  - 新增 `GeneratedRequestPipelineBenchmarkRegistry`，用 handwritten generated registry + `ICqrsRequestInvokerProvider` + `IEnumeratesCqrsRequestInvokerDescriptors` 为 `RequestPipelineBenchmarks.BenchmarkRequest` 提供真实的 generated request invoker descriptor
  - 让 `RequestPipelineBenchmarks` 改用 `RegisterCqrsHandlersFromAssembly(typeof(RequestPipelineBenchmarks).Assembly)` 建容器，只把 pipeline 行为数量矩阵保留在 benchmark 自己的显式注册里
  - 更新 `GFramework.Cqrs.Benchmarks/README.md`，明确 request pipeline benchmark 也已接上 handwritten generated request invoker provider
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Benchmarks/Messaging/GeneratedRequestPipelineBenchmarkRegistry.cs GFramework.Cqrs.Benchmarks/Messaging/RequestPipelineBenchmarks.cs GFramework.Cqrs.Benchmarks/README.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：steady-state request 对照约为 baseline `5.680 ns / 32 B`、`Mediator` `6.565 ns / 32 B`、`MediatR` `54.737 ns / 232 B`、`GFramework.Cqrs` `63.644 ns / 32 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestLifetimeBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：`Singleton` 下 `GFramework.Cqrs` / `MediatR` 约 `69.896 ns / 32 B` vs `57.469 ns / 232 B`；`Transient` 下约 `72.880 ns / 56 B` vs `55.106 ns / 232 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestPipelineBenchmarks.SendRequest_GFrameworkCqrs*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：第一次短跑为 `PipelineCount=0` `64.928 ns / 32 B`、`PipelineCount=1` `366.468 ns / 536 B`、`PipelineCount=4` `547.800 ns / 896 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestPipelineBenchmarks.SendRequest_GFrameworkCqrs*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：复跑确认后为 `PipelineCount=0` `64.755 ns / 32 B`、`PipelineCount=1` `353.141 ns / 536 B`、`PipelineCount=4` `555.083 ns / 896 B`
- 本轮结论：
  - request pipeline benchmark 现在已与默认 request steady-state 使用同一条 generated-provider 宿主接线路径，后续再看 `0 / 1 / 4` 行为矩阵时不再混入“默认 request 已吸收 generated invoker，而 pipeline 还停在纯反射宿主”的口径偏差
  - `0 pipeline` steady-state 继续下探到约 `64.755 ns / 32 B`，与 `RP-105` 的默认 request benchmark 收敛方向一致，说明这条宿主接线收益能稳定复用到 pipeline benchmark
  - `1 pipeline` 与 `4 pipeline` 结果在当前 short job 配置下存在噪音，但没有出现清晰的新增分配或显著退化；因此本轮适合作为低风险宿主收口批次接受
  - 下一批若继续沿用 `$gframework-batch-boot 50`，应优先查看 request lifetime、stream 或 notification benchmark 中是否还存在未吸收 generated-provider 宿主收益的对称切片，而不是回头重试已被 benchmark 否决的 runtime 微优化

### 阶段：默认 request benchmark 吸收 generated provider 宿主（CQRS-REWRITE-RP-105）

- 延续 `$gframework-batch-boot 50`，本轮先确认失败试验已手工回退回 `RP-104` 的已验证状态，再重新评估“默认 request 路径继续逼近 source-generated `Mediator`”的下一刀
- 本轮接受的只读探索结论：
  - 继续在 `CqrsDispatcher` 或 `MicrosoftDiContainer` 上堆叠同层级微优化的性价比已经下降，而且上一轮“总是 `GetAll(Type)`”的试验已被 benchmark 明确否决
  - 默认 `RequestBenchmarks` 虽然已包含 `Mediator` 对照，但当前 GFramework 组仍只注册了单个 handler 实例，没有走 `RegisterCqrsHandlersFromAssembly(...)` + generated registry/provider 的真实宿主接线路径
  - `RequestInvokerBenchmarks` 已证明 generated request invoker provider 路径比纯反射 binding 更接近目标，因此下一批最小切片应先把这条收益吸收到默认 steady-state request benchmark
- 本轮主线程决策：
  - 在 `BenchmarkHostFactory` 内补齐 benchmark 最小宿主的 CQRS 基础设施预接线：runtime、legacy alias、registrar、registration service
  - 新增 `GeneratedDefaultRequestBenchmarkRegistry`，用 handwritten generated registry + `ICqrsRequestInvokerProvider` + `IEnumeratesCqrsRequestInvokerDescriptors` 为 `RequestBenchmarks.BenchmarkRequest` 提供真实的 generated request invoker descriptor
  - 让 `RequestBenchmarks` 改用 `RegisterCqrsHandlersFromAssembly(typeof(RequestBenchmarks).Assembly)` 建容器，并在 `Setup/Cleanup` 前后显式清理 dispatcher 静态缓存，避免前一组 benchmark 污染默认 request steady-state 结果
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `python3 scripts/license-header.py --check --paths GFramework.Cqrs.Benchmarks/Messaging/GeneratedDefaultRequestBenchmarkRegistry.cs GFramework.Cqrs.Benchmarks/Messaging/BenchmarkHostFactory.cs GFramework.Cqrs.Benchmarks/Messaging/RequestBenchmarks.cs GFramework.Cqrs.Benchmarks/README.md`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：steady-state request 对照约为 baseline `5.013 ns / 32 B`、`Mediator` `5.747 ns / 32 B`、`MediatR` `51.588 ns / 232 B`、`GFramework.Cqrs` `65.296 ns / 32 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestLifetimeBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：`Singleton` 下 `GFramework.Cqrs` / `MediatR` 约 `68.772 ns / 32 B` vs `48.177 ns / 232 B`；`Transient` 下约 `73.157 ns / 56 B` vs `51.753 ns / 232 B`
- 本轮结论：
  - 默认 request benchmark 现在终于测到了“默认宿主已吸收 generated request invoker provider”后的真实 steady-state，而不再只是纯反射 request binding
  - 这条宿主层收口在不改 runtime 语义的前提下，把 `GFramework.Cqrs` steady-state request 从约 `70.298 ns` 再压到约 `65.296 ns`
  - lifetime 矩阵也同步改善到 `68.772 ns / 73.157 ns`，说明默认 request 宿主吸收 generated provider 不只是 benchmark 口径变化，而是对常见 handler 生命周期也有稳定收益
  - 下一批若继续沿用 `$gframework-batch-boot 50`，应优先转向 pipeline 路径或 handler 解析热路径中仍未吸收 generated/provider 收益的常量开销，而不是回头重试已被否决的 `GetAll(Type)` 零行为探测方案

### 阶段：request 热路径继续收口（CQRS-REWRITE-RP-104）

- 延续 `$gframework-batch-boot 50`，本轮先重新按 `origin/main` 复核 branch diff 基线：
  - `origin/main` = `4d6dbba6`，提交时间 `2026-05-08 11:13:33 +0800`
  - 当前分支 `feat/cqrs-optimization` 相对 `origin/main` 的累计 branch diff 仍为 `0 files / 0 lines`
  - 当前工作树在真正落代码前只有活跃文档更新，仍明显低于 `$gframework-batch-boot 50` 的文件阈值，因此继续自动推进下一批 request 热路径收口
- 本轮接受的只读探索结论：
  - `RequestBenchmarks` / `RequestInvokerBenchmarks` 的下一个低风险热点仍在“每次发送都必经的容器查询与短生命周期对象创建”，不是重新回到更高风险的语义层重构
  - 候选优先级排序为：`SendAsync` 自身状态机开销、`HasRegistration + GetAll` / 服务键扫描，以及 pipeline continuation 的临时对象
- 本轮主线程决策：
  - 先以最小行为改动切第一刀：把 `CqrsDispatcher.SendAsync(...)` 从 `async/await` 改为 direct-return `ValueTask`，让零 pipeline request 常见路径不再为 dispatcher 自身生成额外状态机
  - 在第一刀验证通过且 benchmark 明显改善后，再切第二刀：让 `MicrosoftDiContainer.HasRegistration(Type)` 在冻结后复用预构建的服务键索引，而不是每次线性扫描全部 `ServiceDescriptor`
  - 第二刀完成后停止继续叠第三刀，因为当前批次已经能清晰区分“有效收益”和“无回退但收益不明显”的因果，不再为了追逐更小常量开销降低评审清晰度
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --filter "FullyQualifiedName~MicrosoftDiContainerTests"`
    - 结果：通过，`52/52` passed
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~CqrsDispatcherCacheTests|FullyQualifiedName~CqrsDispatcherContextValidationTests"`
    - 结果：通过，`14/14` passed
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*RequestBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：最新 steady-state request 对照约为 baseline `6.141 ns / 32 B`、`Mediator` `6.674 ns / 32 B`、`MediatR` `61.803 ns / 232 B`、`GFramework.Cqrs` `70.298 ns / 32 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*RequestLifetimeBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：最新 lifetime request 对照约为 `Singleton` 下 baseline / `MediatR` / `GFramework.Cqrs` = `4.706 ns / 52.197 ns / 73.005 ns`，`Transient` 下 = `4.571 ns / 50.175 ns / 74.757 ns`
  - `env GIT_DIR=... GIT_WORK_TREE=... python3 scripts/license-header.py --check`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
    - 备注：仍仅有 `GFramework.sln` 的历史 CRLF 警告，无本轮新增格式问题
- 本轮结论：
  - 第一刀有效：`CqrsDispatcher.SendAsync(...)` 的 direct-return `ValueTask` 把 `GFramework.Cqrs` steady-state request 从 `RP-103` 记录的约 `83.823 ns` 压到约 `70.298 ns`
  - 第二刀保守有效：冻结后 `HasRegistration(Type)` 索引化没有带来同量级的可见收益，但也没有造成功能回退、额外分配或测试破坏
  - 下一批若继续压 request hot path，应优先评估默认 request 路径吸收 generated invoker/provider，而不是继续围绕同层级容器存在性判断做微调

### 阶段：PR #340 latest-head review 收口（CQRS-REWRITE-RP-103）

- 使用 `$gframework-pr-review` 抓取 `feat/cqrs-optimization` 当前公开 PR，并确认当前锚点已从 `PR #339` 更新为 `PR #340`
- 本轮 latest-head review 结论：
  - `CodeRabbit` 当前显示 `2` 个 nitpick / open thread，`Greptile` 显示 `2` 个 open thread；`MegaLinter` 仍只有 `dotnet-format restore` 环境噪音
  - `CTRF` 当前显示 `2321` 项测试中 `1` 项失败，失败用例为 `CreateStream_Should_Throw_When_Stream_Pipeline_Behavior_Context_Does_Not_Implement_IArchitectureContext`
  - 失败原因不是业务断言退化，而是 `CqrsDispatcher` 新增 `HasRegistration(Type)` fast-path 后，严格 mock 的上下文校验测试没有同步配置该调用，导致在命中上下文注入失败断言前先抛出 `Moq.MockException`
  - `MicrosoftDiContainer.HasRegistration(Type)` 当前实现用 `requestedType.IsAssignableFrom(registeredServiceType)` 判断命中，会把“仅以实现类型自身注册”的服务误判成接口服务键也已注册，这与 `Get(Type)` / `GetAll(Type)` 的服务键语义不一致，属于仍成立的运行时缺陷
  - `IIocContainer.HasRegistration(Type)` XML 文档缺少异常契约；`docs/zh-CN/core/ioc.md` 也还未解释该新公开入口的用途与语义边界；`BenchmarkHostFactory` / `RequestBenchmarks` 中仍残留旧 `ai-libs/Mediator` 注释或隐式共享 handler 合同，属于仍成立的文档/维护性问题
- 本轮主线程决策：
  - 在 `CqrsDispatcherContextValidationTests` 为受影响的 request / stream pipeline mock 显式补 `HasRegistration(Type)` 配置，确保上下文失败语义测试不会被 strict mock 噪音短路
  - 把 `MicrosoftDiContainer.CanSatisfyServiceType(...)` 收窄为“服务键完全命中”或“开放泛型服务键可闭合到目标类型”，并新增回归覆盖“仅按具体实现类型自注册时，接口服务键应返回 false”
  - 为 `IIocContainer.HasRegistration(Type)` 补 `<exception>` / `<remarks>`，并在 `docs/zh-CN/core/ioc.md` 新增用户接入说明，明确该入口按服务键而不是按可赋值关系判断可见性
  - 更新 benchmark 相关注释到 NuGet `Mediator` 语义，并为 `BenchmarkRequestHandler` 增补显式 `Mediator.IRequestHandler<,>` 实现，降低未来升级时的契约漂移诊断成本
- 本轮权威验证：
  - `dotnet build GFramework.Core.Abstractions/GFramework.Core.Abstractions.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~MicrosoftDiContainerTests"`
    - 结果：通过，`52/52` passed
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~CqrsDispatcherContextValidationTests"`
    - 结果：通过，`4/4` passed
  - `env GIT_DIR=... GIT_WORK_TREE=... python3 scripts/license-header.py --check`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
    - 备注：仅剩 `GFramework.sln` 的历史 CRLF 警告，无本轮新增格式问题
- 下一步：
  - 关注 `PR #340` 重新索引后的 latest-head open thread 是否随本轮提交自然收敛，尤其是 `HasRegistration(Type)` 相关 runtime / docs 线程
  - 若后续继续压 request hot path，可从 `CqrsDispatcher` 默认 request 路径与 generated invoker/provider 的进一步吸收空间继续下钻

### 阶段：性能回归门槛收紧与 benchmark 产物忽略收口（CQRS-REWRITE-RP-102）

- 延续 `RP-101` 后的 benchmark 基线，本轮没有继续改 runtime 热路径，而是先把性能治理规则补齐，避免后续优化波次出现“功能通过但 steady-state request 变慢”的回退
- 本轮主线程决策：
  - 将 `BenchmarkDotNet.Artifacts/` 加入仓库 `.gitignore`，避免本地 benchmark 生成目录反复污染工作树
  - 在 `GFramework.Cqrs.Benchmarks/README.md` 明确写下新的默认回归门槛：只要改动触达 request dispatch、DI 热路径、invoker/provider、pipeline 或 benchmark 宿主，就必须至少复跑 `RequestBenchmarks.SendRequest_*` 与 `RequestLifetimeBenchmarks.SendRequest_*`
  - 在 `cqrs-rewrite` active tracking 中把当前阶段目标升级为“持续逼近 source-generated `Mediator`，并至少稳定超过反射版 `MediatR`”，不再只把 benchmark 当成观察工具，而是作为性能收口阶段的验收门槛
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：按新门槛复跑后，steady-state request 对照约为 baseline `5.300 ns / 32 B`、`Mediator` `4.964 ns / 32 B`、`MediatR` `57.993 ns / 232 B`、`GFramework.Cqrs` `83.823 ns / 32 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestLifetimeBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：按新门槛复跑后，`Singleton` 下 `GFramework.Cqrs` / `MediatR` 约 `83.183 ns / 32 B` vs `60.915 ns / 232 B`；`Transient` 下约 `86.243 ns / 56 B` vs `59.644 ns / 232 B`
  - `env GIT_DIR=... GIT_WORK_TREE=... python3 scripts/license-header.py --check`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
- 本轮结论：
  - `BenchmarkDotNet.Artifacts/` 现在不再是工作树噪音源
  - request benchmark 已从“偶尔人工观察”升级为 CQRS 性能波次的默认回归门槛
  - 当前离“至少超过反射版 `MediatR`”还有明确差距，所以下一批优化必须围绕 request steady-state 常量开销继续下钻，而不是只增加更多 benchmark 维度

### 阶段：request 热路径 benchmark 收口与 NuGet `Mediator` 对照补齐（CQRS-REWRITE-RP-101）

- 延续 `$gframework-batch-boot 50`，本轮先按 `origin/main` 复核 branch diff 基线：
  - `origin/main` = `5dc2dd25`，提交时间 `2026-05-08 09:08:37 +0800`
  - 当前分支 `feat/cqrs-optimization` 相对 `origin/main` 的累计 branch diff 仍为 `0 files / 0 lines`
  - 当前工作树仅新增 9 个跟踪文件修改，另有 `BenchmarkDotNet.Artifacts/` 本地生成输出未纳入提交范围，仍明显低于 `$gframework-batch-boot 50` 的文件阈值
- 用户新增的 benchmark 诉求有两部分：
  - 解释 `BenchmarkDotNet.Artifacts/results` 里为什么 `GFramework.Cqrs` request 路径表现显著差于对照组
  - 把 `martinothamar/Mediator` 加入 benchmark 对照，但必须使用官方 NuGet 包，不允许接本地 `ai-libs/Mediator` project reference
- 本轮主线程先回到 runtime hot path 与 benchmark 宿主做最小成本排查，确认旧坏值的两个主要根因：
  - `CqrsDispatcher.SendAsync(...)` / `CreateStream(...)` 在 `0 pipeline` 场景下仍无条件执行 `container.GetAll(dispatchBinding.BehaviorType)`，即使根本没有行为注册，也会多走一次容器解析与空集合分配
  - `MicrosoftDiContainer.Get(Type)` / `GetAll<T>()` / `GetAll(Type)` 在 debug logging 关闭时仍会先构造日志字符串，导致 benchmark 默认 `Fatal` 级别下仍持续产生无效分配
- 本轮主线程决策：
  - 为 `IIocContainer` 新增不激活实例的 `HasRegistration(Type)`，并由 `MicrosoftDiContainer` 提供支持开放泛型匹配的非激活查询实现
  - 让 `CqrsDispatcher` 在 request / stream 的 `0 pipeline` 场景先走 `HasRegistration(...)` fast-path；没有行为注册时直接调用已准备好的 request / stream invoker，不再解析空行为列表
  - 为 `MicrosoftDiContainer` 的热路径查询补 `IsDebugEnabled()` 守卫，避免 benchmark 常态配置下的无效日志字符串构造
  - 在 benchmark 项目中通过 NuGet 接入 `Mediator.Abstractions` 与 `Mediator.SourceGenerator` `3.0.2`，并让 `RequestBenchmarks` 使用 source-generated concrete `Mediator.Mediator` 作为新对照组
  - 保持 `ai-libs/Mediator` 只作为本地源码 / README 参考资料，不参与编译或项目引用
- 本轮新增 / 更新的验证与回归覆盖：
  - `GFramework.Core.Tests/Ioc/MicrosoftDiContainerTests.cs` 新增 `HasRegistration(...)` 回归，覆盖“无匹配注册返回 false”与“开放泛型注册可满足封闭请求行为类型”两个分支
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestBenchmarks.cs` 现在同时对照 baseline / `Mediator` / `MediatR` / `GFramework.Cqrs`
  - `GFramework.Cqrs.Benchmarks/Messaging/BenchmarkHostFactory.cs` 新增 `CreateMediatorServiceProvider(...)`，统一最小宿主构建方式
- 本轮权威验证：
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --filter "FullyQualifiedName~MicrosoftDiContainerTests"`
    - 结果：通过，`51/51` passed
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：steady-state request 对照约为 baseline `5.969 ns / 32 B`、`Mediator` `6.242 ns / 32 B`、`MediatR` `53.818 ns / 232 B`、`GFramework.Cqrs` `85.504 ns / 32 B`
  - `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestLifetimeBenchmarks.SendRequest_*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
    - 结果：通过
    - 备注：`Singleton` 下 `GFramework.Cqrs` 从旧值 `301.731 ns / 440 B` 收敛到 `84.066 ns / 32 B`；`Transient` 下从旧值 `287.863 ns / 464 B` 收敛到 `90.652 ns / 56 B`
- 本轮结论：
  - `GFramework.Cqrs` 之前“垫底很多”的主要原因不是抽象层级本身，而是 request 热路径残留了两个可避免的分配热点：空 pipeline 解析与禁用日志下的字符串构造
  - 收口后，`GFramework.Cqrs` 仍慢于 `MediatR` 与 source-generated `Mediator`，但已经去掉了旧 benchmark 中最明显的异常分配和 300ns 级退化
  - 下一批若继续沿用 `$gframework-batch-boot 50` 压 request steady-state，最值得优先评估的是让默认 request 路径进一步吸收 generated invoker/provider 的收益，而不是继续扩大更多横向对照项

## 2026-05-07

### 阶段：PR #339 stream pipeline seam review 收口（CQRS-REWRITE-RP-100）

- 使用 `$gframework-pr-review` 抓取 `feat/cqrs-optimization` 当前公开 PR，并确认已从历史 `PR #334` 进入新的 `PR #339`
- 本轮 latest-head review 结论：
  - `CodeRabbit` 当前显示 `2` 个 open thread 与 `2` 个 nitpick，失败检查为 `0`，GitHub Test Reporter 汇总仍为全绿
  - `GFramework.Cqrs.Tests/Cqrs/CqrsDispatcherCacheTests.cs` 的 test-name 拼写 thread 已在当前 head 失效，本地代码已经是 `Per_Behavior_Count`
  - `GFramework.Core.Abstractions/Ioc/IIocContainer.cs` 的 `RegisterCqrsStreamPipelineBehavior<TBehavior>()` 仍缺 `<exception>` / `<remarks>` 契约说明，属于仍成立的文档缺口
  - `GFramework.Cqrs/Internal/CqrsDispatcher.cs` 的 `StreamPipelineInvocation.GetContinuation(...)` 缺少 request 对称路径已有的线程模型说明，属于仍成立的并发语义文档缺口
  - `GFramework.Core/Ioc/MicrosoftDiContainer.cs` 的 request / stream CQRS 行为注册逻辑完全重复，属于仍成立的维护性问题
- 本轮主线程决策：
  - 在 `IIocContainer` 补齐流式行为注册入口的 `<exception>` / `<remarks>`，并把相同契约补到 `IArchitecture`、`Architecture` 与 `ArchitectureModules`，避免公开入口文档漂移
  - 为 `StreamPipelineInvocation.GetContinuation(...)` 补齐“仅假定单次建流链顺序推进；并发 `next()` 时可能重复创建等价 continuation，但不会跨建流共享实例”的说明
  - 在 `MicrosoftDiContainer` 抽取 `RegisterCqrsPipelineBehaviorCore(...)`，统一 request / stream 行为注册的开放泛型、封闭接口枚举、错误消息与日志路径
  - 顺手修复 `dotnet format` 在当前 branch diff 内实际命中的 `GFramework.Cqrs/ICqrsRequestInvokerProvider.cs` XML 缩进问题；不处理未触达历史文件上的 `CHARSET` 提示
- 本轮权威验证：
  - `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/current-pr-review.json`
    - 结果：通过
  - `dotnet format GFramework.Cqrs/GFramework.Cqrs.csproj --verify-no-changes`
    - 结果：发现当前 diff 内 `GFramework.Cqrs/ICqrsRequestInvokerProvider.cs` 的空白格式问题；其余 `CHARSET` 提示集中在未触达的历史文件
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Core.Abstractions/GFramework.Core.Abstractions.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~CqrsDispatcherCacheTests"`
    - 结果：通过，`10/10` passed
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ArchitectureModulesBehaviorTests"`
    - 结果：通过，`4/4` passed
  - `env GIT_DIR=... GIT_WORK_TREE=... python3 scripts/license-header.py --check`
    - 结果：通过
  - `git diff --check`
    - 结果：通过

### 阶段：stream pipeline seam 收口（CQRS-REWRITE-RP-099）

- 延续 `$gframework-batch-boot 50`，主线程先按 `origin/main` 评估 branch diff 容量，并在 `stream pipeline` 与 `notification publisher` 两个独立切片中选择更贴近 active gap 的下一批目标
- 只读 subagent 结论已被接受：
  - `notification publisher` 已有稳定 seam、默认顺序实现与专门回归，缺口主要在“更多内置策略”
  - `stream pipeline` 仍缺独立 contract、注册入口与 runtime executor，对应缺口在 public docs 与 active tracking 中都已显式列出
- 本轮主线程决策：
  - 为 `GFramework.Cqrs.Abstractions` 新增 `IStreamPipelineBehavior<,>` 与 `StreamMessageHandlerDelegate<,>`
  - 为 `IIocContainer`、`IArchitecture`、`Architecture`、`ArchitectureModules` 与 `MicrosoftDiContainer` 新增 `RegisterCqrsStreamPipelineBehavior<TBehavior>()`
  - 为 `CqrsDispatcher` 的 `CreateStream(...)` 路径补齐 stream behavior 解析、上下文注入，以及按 behaviorCount 缓存的 stream pipeline executor 形状
  - 保持语义边界清晰：本轮 stream pipeline 只包裹单次 `CreateStream(...)` 建流，不扩展到每个元素的逐项 middleware 语义
  - 让 generated stream invoker provider 与 stream pipeline seam 共存，并补齐“generated invoker 仍命中、行为链仍生效”的回归
- 本轮新增 / 更新的测试方向：
  - `CqrsDispatcherCacheTests`：stream pipeline executor 缓存、顺序稳定性、上下文重新注入
  - `CqrsDispatcherContextValidationTests`：stream behavior 需要 `IArchitectureContext` 时的显式失败语义
  - `CqrsGeneratedRequestInvokerProviderTests`：generated stream invoker 与 stream behavior 并存时仍优先消费 generated descriptor
  - `ArchitectureModulesBehaviorTests`：公开 `RegisterCqrsStreamPipelineBehavior<TBehavior>()` 冒烟回归
- 文档收口：
  - `GFramework.Cqrs/README.md` 现在显式说明 stream behavior 的建流级作用域
  - `docs/zh-CN/core/cqrs.md` 现在区分 request pipeline 与 stream pipeline 两个注册入口，并从“仍缺 stream pipeline seam”的能力差距列表中移除该项
- 当前立即下一步：
  - 运行 `GFramework.Cqrs` / `GFramework.Cqrs.Tests` / `GFramework.Core.Tests` 的 Release build 与 targeted tests
  - 刷新 `origin/main...HEAD` 的 branch diff files / lines 指标
  - 若验证通过，再补 license / diff check 与自动提交
- 本轮权威验证：
  - `dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~CqrsDispatcherCacheTests|FullyQualifiedName~CqrsGeneratedRequestInvokerProviderTests|FullyQualifiedName~CqrsDispatcherContextValidationTests"`
    - 结果：通过，`31/31` passed
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ArchitectureModulesBehaviorTests"`
    - 结果：通过，`4/4` passed
  - `env GIT_DIR=... GIT_WORK_TREE=... python3 scripts/license-header.py --check`
    - 结果：通过
  - `git diff --check`
    - 结果：通过
  - `origin/main...HEAD`
    - 结果：`0 files / 0 lines`

### 阶段：PR #334 latest-head helper 异常边界收口（CQRS-REWRITE-RP-098）

- 再次使用 `$gframework-pr-review` 抓取 `feat/cqrs-optimization` 对应的 `PR #334` latest-head review，并重新核对 `/tmp/current-pr-review.json` 中最新 open thread：
  - 当前公开 PR 仍为 `PR #334`
  - `CodeRabbit` 最新 review 在 `2026-05-07T12:20:24Z` 为 `APPROVED`
  - latest-head 当前显示 `CodeRabbit 10` / `Greptile 5` 个 open thread
- 本轮逐条回到本地代码后，确认大多数 open thread 仍是 stale 状态；唯一继续成立的问题集中在 `LegacyCqrsDispatchHelper.TryResolveDispatchContext(...)`：
  - 该 helper 之前会把 `IContextAware.GetContext()` 抛出的任意 `InvalidOperationException` 都吞掉并回退到 legacy 直执行
  - 这会把真实运行时故障误判为“上下文未就绪”，导致 bridge 路径悄悄绕过统一 runtime，退化为难以诊断的行为差异
- 本轮主线程决策：
  - 将异常过滤收窄为只接受两类缺上下文信号：`Architecture context has not been set...` 与 `No active architecture context is currently bound.`
  - 其他 `InvalidOperationException` 一律继续向上传播，避免掩盖容器、生命周期或自定义 `GetContext()` 内的真实错误
  - 在 `CommandExecutorTests` 中新增两条回归：一条验证缺上下文时仍会 fallback 到 legacy 直执行；一条验证意外 `InvalidOperationException` 不会被 bridge 逻辑静默吞掉
  - 同步刷新 `cqrs-rewrite` active tracking，把本轮修复记录为新的恢复锚点 `RP-098`
- 本轮权威验证：
  - `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/current-pr-review.json`
    - 结果：通过
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~CommandExecutorTests|FullyQualifiedName~QueryExecutorTests"`
    - 结果：通过，`25/25` passed
  - `python3 scripts/license-header.py --check`
    - 结果：通过
  - `git diff --check`
    - 结果：通过

### 阶段：PR #334 nitpick 测试收尾（CQRS-REWRITE-RP-097）

- 继续处理 `PR #334` latest-head review 中仍值得本地吸收的轻量 nitpick，范围限定在 legacy bridge 测试可观察性与测试替身诊断质量：
  - `AsyncQueryExecutorTests.SendAsync_Should_Bridge_Through_Runtime_And_Preserve_Context` 标题声明“保留上下文”，但此前只断言了返回值与 bridge request 类型
  - `CommandExecutorTests.Send_WithResult_Should_Bridge_Through_Runtime_And_Preserve_Context` 同样缺少可观察的上下文注入断言
  - `RecordingCqrsRuntime` 直接强转响应对象，若测试工厂回错类型，失败信息不够聚焦
- 本轮主线程决策：
  - 为两个 “Preserve_Context” 用例补齐 `ObservedContext` 与 `expectedContext` 的同一实例断言，使测试标题、注释与断言对象保持一致
  - 让 `RecordingCqrsRuntime` 通过私有 helper 显式执行响应类型还原；当工厂返回 `null` 或错误装箱类型时，抛出包含 request 类型与期望/实际响应类型的 `InvalidOperationException`
  - 同步刷新 `cqrs-rewrite` active tracking，把本轮 nitpick 收敛与验证结果记录为新的恢复锚点 `RP-097`
- 本轮权威验证：
  - `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --filter "FullyQualifiedName~CommandExecutorTests|FullyQualifiedName~AsyncQueryExecutorTests"`
    - 结果：通过，`19/19` passed
  - `python3 scripts/license-header.py --check`
    - 结果：通过
  - `git diff --check`
    - 结果：通过

### 阶段：PR #334 latest-head review 复核（CQRS-REWRITE-RP-096）

- 再次使用 `$gframework-pr-review` 抓取 `feat/cqrs-optimization` 对应的 `PR #334` latest-head review，并读取 `/tmp/current-pr-review.json` 中的 `review_agents`、`latest_commit_review`、`megalinter_report` 与 `test_reports`
- 本轮复核结论：
  - 当前公开 PR 为 `PR #334`，head commit 为 `dc3bd3744e2ceaa557ef03bc991fc88daedb460b`
  - `CodeRabbit` latest review 在 `2026-05-07T11:46:42Z` 已是 `APPROVED`，但 latest-head 仍显示 `10` 个 open thread；`Greptile` 仍显示 `3` 个 open thread
  - 逐条回到本地代码后，相关修复已在当前分支落地：`ArchitectureBootstrapper` 已自动扫描 `typeof(ArchitectureContext).Assembly`；`ArchitectureContextTests` / `ArchitectureModulesBehaviorTests` 已标注 `NonParallelizable` 并保证资源释放；`LegacyAsync*DispatchRequestHandler` 已统一补 `ThrowIfCancellationRequested()` + `WaitAsync(cancellationToken)`；`QueryExecutor` / legacy bridge request XML 文档与 `docs/zh-CN/core/command.md` fallback 说明也已齐备
  - 远端 CTRF 最新测试汇总为 `2311/2311 passed`（run `#1079`），`MegaLinter` 仅剩 `dotnet-format` restore failed 的环境噪音，没有新的文件级诊断
- 主线程决策：
  - 不再为这些 stale open thread 追加新的本地代码改动，避免重复修补已吸收的问题
  - 仅更新 `cqrs-rewrite` active tracking/trace，把“当前剩余差异主要是 GitHub thread 状态滞后”记录为最新权威事实
- 本轮权威验证：
  - `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --format json --json-output /tmp/current-pr-review.json`
    - 结果：通过
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`

### 阶段：PR #334 legacy bridge sync follow-up（CQRS-REWRITE-RP-095）

- 再次使用 `$gframework-pr-review` 抓取 `feat/cqrs-optimization` 对应的 `PR #334` latest-head review，并只保留本地复核后仍成立的问题：
  - `QueryExecutor` / `CommandExecutor` 新增的同步 bridge 仍直接阻塞 `ICqrsRuntime.SendAsync(...)`，在调用方存在 `SynchronizationContext` 时容易放大 sync-over-async 死锁面
  - `QueryExecutor` / `CommandExecutor` / `AsyncQueryExecutor` 各自保留一份相同的 dispatch-context 解析逻辑，仍有漂移风险
  - `ArchitectureContextTests` 的 bridge fixture 依然共享静态 tracker 且未显式声明非并行；冻结容器所有权也未交还给调用方释放
  - `LegacyAsyncCommandDispatchRequestHandler` 仍未沿用另两个 async bridge handler 的取消可见性模式
- 本轮主线程决策：
  - 新增 `GFramework.Core/Cqrs/LegacyCqrsDispatchHelper.cs`，统一收口 legacy bridge 的 dispatch-context 解析，以及同步 bridge 对 `ICqrsRuntime.SendAsync(...)` 的线程池隔离等待
  - 将 `QueryExecutor`、`CommandExecutor`、`AsyncQueryExecutor` 的重复 helper 改为复用共享 helper，并把 `ArchitectureContext` 的同步 CQRS 包装入口一并切换到同一阻塞策略，避免留下半修状态
  - 为 `ICqrsRuntime.SendAsync(...)` 补充 `<remarks>`，显式说明 legacy 同步入口会在后台线程上等待该异步契约，处理链路不应依赖调用方 `SynchronizationContext`
  - 把 `ArchitectureContextTests`、`ArchitectureModulesBehaviorTests` 标记为 `NonParallelizable`，并让 `CreateFrozenBridgeContext(...)` 把冻结容器通过 `out` 参数返还给每个测试在 `finally` 中释放
  - 为 `LegacyAsyncCommandDispatchRequestHandler` 增补 `ThrowIfCancellationRequested()` + `WaitAsync(cancellationToken)`，与另外两个 async bridge handler 保持一致
  - 新增回归测试覆盖同步 bridge 的 `SynchronizationContext` 隔离、legacy async command handler 的取消语义，以及 async/sync bridge request 的 request-type 命中
- 本轮权威验证：
  - `env GIT_DIR=... GIT_WORK_TREE=... python3 scripts/license-header.py --check`
    - 结果：通过
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ArchitectureContextTests|FullyQualifiedName~ArchitectureModulesBehaviorTests|FullyQualifiedName~CommandExecutorTests|FullyQualifiedName~QueryExecutorTests|FullyQualifiedName~AsyncQueryExecutorTests|FullyQualifiedName~LegacyAsyncCommandDispatchRequestHandlerTests"`
    - 结果：通过，`54/54` passed

### 阶段：PR #334 legacy bridge / 文档 review 收尾（CQRS-REWRITE-RP-094）

- 使用 `$gframework-pr-review` 抓取当前分支公开 PR，确认 `feat/cqrs-optimization` 当前对应 `PR #334`
- latest-head open AI review 复核后，主线程接受并执行的修复集中在六类：
  - `GFramework.Core.Tests/Architectures/ArchitectureContextTests.cs` 通过字符串字面量反射实例化内部 bridge handler，维护成本高且不利于 rename-safe 重构
  - `ArchitectureModulesBehaviorTests` 在断言失败路径下未保证 `DestroyAsync()` 执行，且 `TearDown` 未重置 `LegacyBridgePipelineTracker`
  - `LegacyBridgePipelineTracker` 以静态共享计数器记录 bridge pipeline 命中，但未文档化线程安全语义，且用字符串匹配类型名识别 bridge request
  - `LegacyAsyncQueryDispatchRequestHandler` / `LegacyAsyncCommandResultDispatchRequestHandler` 丢弃了 runtime 传入的 `CancellationToken`
  - `CommandExecutorModule` / `QueryExecutorModule` / `AsyncQueryExecutorModule` 依赖 `container.Get<ICqrsRuntime>()` 的隐式注册顺序，但此前既未显式失败，也未写进 API 契约
  - 多个 legacy bridge request / docs 页面仍缺 XML 文档或回退边界说明
- 本轮主线程决策：
  - 为 `GFramework.Core` 新增 `Properties/AssemblyInfo.cs`，用 `InternalsVisibleTo("GFramework.Core.Tests")` 让测试直接实例化内部 handler
  - 把 `ArchitectureContextTests.RegisterLegacyBridgeHandlers` 改成显式构造 6 个 handler，移除字符串反射装配
  - 为 bridge 相关测试补 `TearDown` 清理和 `try/finally` 销毁，减少失败路径资源泄露
  - 为 `LegacyBridgePipelineTracker` 增补 `<remarks>`，并改用 `typeof(LegacyCqrsDispatchRequestBase).IsAssignableFrom(requestType)` 识别 bridge request
  - 为 `LegacyAsyncQueryDispatchRequestHandler` / `LegacyAsyncCommandResultDispatchRequestHandler` 加入预取消检查与 `WaitAsync(cancellationToken)`
  - 将三个 executor module 改为 `GetRequired<ICqrsRuntime>()`，同时在 XML 文档中显式声明 `CqrsRuntimeModule` 的前置注册约束
  - 为 `CommandExecutor` / `QueryExecutor` / `AsyncQueryExecutor` 的 dispatch-context helper 增加 `[MemberNotNullWhen]`，收敛重复 `_runtime is not null` 判空与 null-forgiving
  - 补齐 legacy bridge request / handler 的 XML 文档，以及 `docs/zh-CN/core/command.md`、`context.md` 的 fallback 边界说明
- 本轮没有跟进的 thread：
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestLifetimeBenchmarks.cs` 的 `sealed` 建议属于低价值性能/风格提示，不影响 `PR #334` 的行为正确性
  - 若 review 在 GitHub 重新索引前仍显示旧 thread，下一轮以最新 head commit 再次抓取为准，不在本地重复造改动
- 本轮权威验证：
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：通过，`0 warning / 0 error`
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ArchitectureContextTests|FullyQualifiedName~ArchitectureModulesBehaviorTests|FullyQualifiedName~CommandExecutorTests|FullyQualifiedName~QueryExecutorTests|FullyQualifiedName~AsyncQueryExecutorTests"`
    - 结果：通过，`48/48` passed
  - `env GIT_DIR=... GIT_WORK_TREE=... python3 scripts/license-header.py --check`
    - 结果：通过
  - `git diff --check`
    - 结果：通过

### 阶段：legacy Core CQRS -> GFramework.Cqrs bridge（CQRS-REWRITE-RP-093）

- 延续 `$gframework-batch-boot 50`，本轮明确不只盯 benchmark，而是同时处理两个目标：
  - 复核 `ai-libs/Mediator` 还有哪些能力尚未被 `GFramework.Cqrs` 吸收
  - 验证 `GFramework.Core` 的简单 `Command` / `Query` 兼容入口能否在不改外部用法的前提下，底层统一改走 `GFramework.Cqrs`
- 主线程先完成 `GFramework.Core` bridge 实现收尾与测试修正：
  - `ArchitectureContext` 的 legacy `SendCommand(...)` / `SendQuery(...)` / `SendQueryAsync(...)` 现在会创建内部 bridge request，并直接通过统一 `ICqrsRuntime` 分发
  - `CommandExecutor`、`QueryExecutor`、`AsyncQueryExecutor` 在解析到 runtime 且目标对象可提供架构上下文时，也会复用同一条 bridge/runtime 路径
  - 为避免破坏不依赖容器的旧测试，执行器仍保留“未接入 runtime 时直接执行”的回退语义
- 新增 `GFramework.Core/Cqrs/Legacy*DispatchRequest*.cs` 与对应 handler，把 legacy 命令/查询包装成内部 request：
  - bridge handler 在执行前会显式把当前 `IArchitectureContext` 注入给 `IContextAware` 目标
  - 这让旧调用链在不改 public API 的情况下，也能复用统一 pipeline 与 handler dispatch 语义
- 生产接线结论已经本地复核：
  - `CqrsRuntimeModule` 只注册 runtime / registrar / registration service，本身不直接手工注册 bridge handler
  - 默认生产路径依赖 `ArchitectureBootstrapper.ConfigureServices(...)` 自动调用 `RegisterCqrsHandlersFromAssemblies([architectureType.Assembly, typeof(ArchitectureContext).Assembly])`
  - 因此 `GFramework.Core` 程序集中的 internal bridge handler 会在标准架构初始化阶段自动被扫描和注册，不需要业务侧手工补注册
- 为防止以后有人改坏默认扫描范围，本轮额外补了一条更接近真实启动路径的回归：
  - `ArchitectureModulesBehaviorTests.InitializeAsync_Should_AutoRegister_LegacyBridgeHandlers_For_Default_Core_Assemblies`
  - 该用例通过 `Architecture.Configurator` 注册 open-generic pipeline behavior，然后直接走 `Architecture.InitializeAsync()`，验证旧 `SendCommand` / `SendQuery` 兼容入口能命中统一 pipeline
- 只读 subagent 同步完成 `Mediator` 差距复核，接受的结论是六类未完全吸收能力：
  - `IMediator` / `ISender` / `IPublisher` 风格 facade
  - telemetry / tracing / metrics
  - stream pipeline
  - notification publisher 策略
  - 生成器配置与诊断公开面
  - 生命周期 / 缓存公开配置面
- 文档与恢复入口同步更新：
  - `docs/zh-CN/core/context.md`、`command.md`、`query.md`、`cqrs.md`
  - `GFramework.Core/README.md`
  - active tracking / trace 升级到 `RP-093`

### 验证（RP-093）

- `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --filter "FullyQualifiedName~ArchitectureContextTests|FullyQualifiedName~CommandExecutorTests|FullyQualifiedName~QueryExecutorTests|FullyQualifiedName~AsyncQueryExecutorTests"`
  - 结果：通过，`45/45` passed
- `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
  - 结果：通过，`1644/1644` passed
- `env GIT_DIR=... GIT_WORK_TREE=... python3 scripts/license-header.py --check`
  - 结果：通过
- `git diff --check`
  - 结果：通过

### 当前下一步（RP-093）

1. 若继续沿用 `$gframework-batch-boot 50`，优先从 `stream pipeline` 或 `notification publisher` 策略中切一块对齐 `Mediator`
2. 若要继续收敛 public seam，下一批优先设计 facade，而不是继续扩大 `ArchitectureContext` 的兼容职责

### 阶段：request handler 生命周期矩阵 benchmark（CQRS-REWRITE-RP-092）

- 使用 `$gframework-batch-boot 50` 启动本轮批次，并按技能要求先复核 `origin/main` 基线与 branch diff：
  - `origin/main` = `2c58d8b6`，提交时间 `2026-05-07 13:24:46 +0800`
  - 本地 `main` = `c2d22285`，已落后于 remote-tracking ref，因此不作为本轮 batch baseline
  - 当前 `feat/cqrs-optimization` 相对 `origin/main` 的累计 branch diff 在开工前为 `0 files / 0 lines`
- 本轮批次目标：继续推进 `GFramework.Cqrs.Benchmarks`，补一个独立、低风险、可单项目 Release 验证的 request 生命周期对照切片
- 主线程先复核现有 benchmark 宿主与 runtime 解析路径后确认：
  - `RequestBenchmarks` 与 `StreamingBenchmarks` 当前都固定使用单根容器宿主
  - `MicrosoftDiContainer` 虽支持 `RegisterScoped` / `CreateScope()`，但当前 `CqrsDispatcher` 的 steady-state benchmark 路径直接从根容器解析 handler
  - 因此若直接把 `Scoped` 注册加入现有 benchmark，会把“根作用域下的 scoped 解析”误当成公平对照，语义不成立
- 本轮决策：
  - 新增 `Messaging/RequestLifetimeBenchmarks.cs`
  - 生命周期矩阵只覆盖 `Singleton / Transient`
  - 在 XML 文档与 README 中显式注明：`Scoped` 需要等未来具备真实显式作用域边界的 benchmark host 后再比较
- 已修改：
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestLifetimeBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/README.md`
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 预期结果：
  - `GFramework.Cqrs.Benchmarks` 不再只覆盖“有无 generated provider / startup / pipeline”的维度，也开始覆盖 request steady-state 下的 handler 生命周期成本差异
  - benchmark 设计继续保持“只加入语义公平的矩阵”，避免把作用域模型不对称的结论写进基线

### 验证（RP-092）

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release --no-build -- --filter "*RequestLifetimeBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过（沙箱外权威结果）
  - 备注：当前 agent 沙箱内执行同一 benchmark 会在 BenchmarkDotNet 自动生成 bootstrap 阶段失败；切换到沙箱外后，`restore/build` 自举与 6 个 benchmark case 全部通过
  - 备注：`Singleton` 下 baseline / MediatR / GFramework 分别约 `5.633 ns / 58.687 ns / 301.731 ns`
  - 备注：`Transient` 下 baseline / MediatR / GFramework 分别约 `5.044 ns / 52.274 ns / 287.863 ns`
- `env GIT_DIR=... GIT_WORK_TREE=... python3 scripts/license-header.py --check`
  - 结果：通过
- `git diff --check`
  - 结果：通过

### 当前下一步（RP-092）

1. 若 branch diff 仍明显低于 `$gframework-batch-boot 50` 阈值，下一批优先补 `stream handler` 生命周期矩阵，保持 request / stream benchmark 维度对称
2. 若准备扩到 `Scoped` 生命周期，先为 benchmark host 设计真实显式作用域基线，再进入运行时对照

## 2026-05-06

### 阶段：PR #331 review 收尾补丁（CQRS-REWRITE-RP-091）

- 使用 `$gframework-pr-review` 拉取当前分支 `fix/package-validation-guard` 对应的 `PR #331` latest-head review 后，主线程只保留本地复核仍成立的问题：
  - `.github/workflows/ci.yml` 的 `dotnet pack` 步骤缺少 `--no-build`，会在已完成 solution `Build` 后重复编译整仓库
  - `scripts/validate-packed-modules.sh` 使用 GNU `find -printf`，在 macOS / BSD `find` 下无法运行
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md` 的 active PR 锚点仍写成 `待创建`，与当前公开 PR 状态不一致
- 本轮决策：
  - `ci.yml` 的 pack 步骤显式补上 `--no-build`，使其与前置 `Build` 步骤形成单次编译链路
  - 共享包校验脚本改为使用 `find ... -exec basename {} \;`，避免依赖 GNU-only 选项
  - active tracking 同步到 `PR #331`，并把这轮 PR review 的剩余问题描述更新为当前已核验的真实范围
- 预期结果：
  - PR workflow 的 pack 阶段不再对同一 solution 重复编译
  - `validate-packed-modules.sh` 可在 GNU / BSD `find` 环境下保持相同行为
  - `cqrs-rewrite` active 恢复入口继续与当前公开 PR 保持一致

### 阶段：benchmark 发布面隔离与包清单校验前移（CQRS-REWRITE-RP-091）

- 针对 tag 发布中出现的 `GFramework.Cqrs.Benchmarks` 异常包名单，本轮先复核 benchmark 项目与 solution pack 的本地事实：
  - `GFramework.Cqrs.Benchmarks.csproj` 已包含 `IsPackable=false` 与 `GeneratePackageOnBuild=false`
  - 本地执行 `dotnet pack GFramework.sln -c Release --no-restore -o /tmp/gframework-sln-pack-probe -p:IncludeSymbols=false` 时，产物仅包含 14 个预期发布包
  - 因此本轮不把 benchmark 包加入发布白名单，而是把“benchmark 永不发布”与“PR 前置完整包名单校验”同时固化
- 本轮决策：
  - 为 `GFramework.Cqrs.Benchmarks` 补充注释，明确其 benchmark-only 的发布边界
  - 新增 `scripts/validate-packed-modules.sh`，集中维护预期包集合与实际 `.nupkg` diff 逻辑
  - `publish.yml` 改为调用共享脚本，避免发布工作流与 PR 工作流各自维护一份包名单
  - `ci.yml` 新增 solution `dotnet pack` 与 packed modules 校验，把异常发布包从 tag 发布前移到普通 PR 阶段
- 预期结果：
  - benchmark / example / tooling 一类新项目若意外进入发布面，会先在 PR 失败，而不是等到 tag 发布
  - 发布与 PR 使用同一份包名单规则，减少后续名单漂移
  - `GFramework.Cqrs.Benchmarks` 继续只服务于 benchmark workflow，不进入 NuGet / GitHub Packages

### 阶段：benchmark 对照宿主收敛与 startup cold-start 恢复（CQRS-REWRITE-RP-090）

- 使用 `$gframework-pr-review` 拉取 `PR #326` latest-head review 后，主线程确认仍有效的 benchmark 反馈集中在三类问题：
  - `RequestBenchmarks` 的 GFramework / MediatR handler 生命周期不对齐
  - `RequestStartupBenchmarks` 把容器构建、程序集扫描范围和缓存清理阶段混在一起，导致 cold-start 对照不公平
  - benchmark 工程里的 `MicrosoftDiContainer` 多处以 `ImplementationType` 方式注册 handler，但未在 runtime 分发前 `Freeze()`，首次真实解析路径存在隐藏失败风险
- 本轮本地复核的关键根因：
  - `MicrosoftDiContainer.Get(Type)` 在未冻结时只读取 `ImplementationInstance`，不会实例化 `ImplementationType`
  - `ColdStart_GFrameworkCqrs` 清空 dispatcher 静态缓存后，首次发送必须走真实 handler 解析，因此会稳定触发 `No CQRS request handler registered`
  - 多个 benchmark 同时采用“手工 MediatR 注册 + `RegisterServicesFromAssembly(...)` 全程序集扫描”，容易把无关 handler / behavior 一并纳入对照，且存在重复注册漂移
- 本轮决策：
  - 新增 `Messaging/BenchmarkHostFactory.cs`，统一 benchmark 最小宿主构建规则
  - GFramework benchmark 宿主统一先注册再 `Freeze()`，保证 steady-state 与 cold-start 都走真实可解析容器
  - MediatR benchmark 宿主统一通过 `TypeEvaluator` 限制到当前场景所需 handler / behavior 类型，保留正常 `AddMediatR` 组装路径，同时移除全程序集扫描噪音
  - `RequestStartupBenchmarks` 采用专用 `ColdStart` job，设置 `InvocationCount=1` 与 `WithUnrollFactor(1)`，并把 dispatcher cache reset 放到 `IterationSetup`
- 已修改的 benchmark 范围：
  - `RequestBenchmarks`
  - `RequestPipelineBenchmarks`
  - `RequestStartupBenchmarks`
  - `StreamingBenchmarks`
  - `NotificationBenchmarks`
  - `RequestInvokerBenchmarks`
  - `StreamInvokerBenchmarks`
- 结果：
  - `ColdStart_GFrameworkCqrs` 已恢复出有效结果，不再出现 `No CQRS request handler registered`
  - `RequestBenchmarks`、`RequestStartupBenchmarks` 在本地均可实际运行
  - `RequestStartupBenchmarks` 目前仍会收到 BenchmarkDotNet 对单次 cold-start 场景的 `MinIterationTime` 提示；这是测量形状带来的工具提示，不再是运行级失败

### 验证（RP-090）

- `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/gframework-current-pr-review.json`
  - 结果：通过
  - 备注：确认当前分支对应 `PR #326`，仍有效的 open AI feedback 集中在 benchmark 对照语义与 active 文档收敛
- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestStartupBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：`ColdStart_GFrameworkCqrs` 已恢复，最新本地输出约 `220-292 us`，`ColdStart_MediatR` 约 `575-616 us`
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：通过
  - 备注：steady-state request 对照可正常运行，未再触发 MediatR 重复注册或 GFramework 首次解析失败

### 阶段：PR #326 review 收尾补丁（CQRS-REWRITE-RP-090）

- 再次使用 `$gframework-pr-review` 复核 `PR #326` latest-head open threads 后，主线程确认本轮仍成立且适合在当前 PR 内收敛的问题集中在四类：
  - `.github/workflows/benchmark.yml` 的 `benchmark_filter` 直接插值到 shell，存在 workflow_dispatch 输入注入风险
  - `RequestInvokerBenchmarks` 与 `StreamInvokerBenchmarks` 的 MediatR handler 生命周期仍为 `Singleton`，与 GFramework 反射 / generated 路径的 transient 语义不一致
  - `RequestPipelineBenchmarks` 未在场景切换前后清理 dispatcher 缓存，且四个空 pipeline behavior 类型仍使用非法的分号类声明
  - `ai-plan/public/cqrs-rewrite` active 文档仍保留旧失败结论与重复日期标题，和“active 入口只保留最新权威恢复点”的约束不一致
- 本轮刻意未扩展处理的 review：
  - `MicrosoftDiContainer` 的释放契约建议会扩大到核心 Ioc 接口与全仓库生命周期语义，不适合作为 benchmark review 顺手改动
  - `RequestStartupBenchmarks` 的“手工单点注册 vs 受限程序集扫描”差异目前属于有意保留的最小宿主模型，代码注释已明确该设计边界
- 已修改：
  - `.github/workflows/benchmark.yml`
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestInvokerBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestPipelineBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamInvokerBenchmarks.cs`
  - `ai-plan/public/cqrs-rewrite/todos/cqrs-rewrite-migration-tracking.md`
  - `ai-plan/public/cqrs-rewrite/traces/cqrs-rewrite-migration-trace.md`
- 预期结果：
  - 手动 benchmark workflow 的过滤器输入不再直接参与 shell 解析
  - request / stream invoker 三路对照的 handler 生命周期重新回到同一基线
  - request pipeline benchmark 在 `0 / 1 / 4` 场景切换时不再复用旧 dispatcher cache
  - active tracking / trace 更符合 boot 恢复入口所要求的“只保留最新权威结论”形状

## 2026-04-30

### 阶段：历史 PR #307 active 入口收敛（CQRS-REWRITE-RP-076）

- 继续沿用 `$gframework-pr-review` 对 `PR #307` 做 latest-head triage，本轮只处理仍成立的 `ai-plan` 恢复入口问题
- 主线程确认当前远端权威信号：
  - 当时分支对应 `PR #307`，状态为 `OPEN`
  - 远端 `CTRF` 最新汇总为 `2247/2247` passed
  - `MegaLinter` 仅剩 `dotnet-format` 的 `Restore operation failed` 环境噪音
  - 仍未闭环的 review 重点集中在 `cqrs-rewrite` active tracking / trace 仍保留过多历史锚点，而非新的运行时代码缺陷
- 本轮决策：
  - 将 active tracking 收敛为单一恢复入口，只保留 `RP-076`、`PR #307`、活跃风险、最近权威验证与下一推荐步骤
  - 将 active trace 收敛为当前阶段的关键事实与决策，不再在默认恢复入口中保留 `RP-062` 之后的长阶段流水账
  - 新增 `archive/traces/cqrs-rewrite-history-rp062-through-rp076.md` 承接 `RP-062` 至 `RP-076` 的详细 trace 历史，保持旧阶段仍可追溯

### 验证（RP-076）

- `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --format json --json-output <temporary-json-output>`
  - 结果：通过
  - 备注：确认 `PR #307` 的当前 review 重点已收敛到 `ai-plan` 文档收尾
- `dotnet build GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Provider_Interface|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Descriptor_Enumerator|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Descriptor_Type|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Descriptor_Entry_Type|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Emits_Stream_Invoker_Provider_Metadata_When_Runtime_Contract_Is_Available"`
  - 结果：通过，`5/5` passed

### 当前下一步（RP-076）

1. 当时继续按 `PR #307` 的 latest-head review 收尾，优先保持 active tracking 与 active trace 的单一锚点一致
2. 若继续推进代码切片，先复核 request 侧是否仍存在与 stream invoker gate 对称的生成合同遗漏
3. 进入下一批前继续使用最小 Release build 或 targeted test 作为权威验证，避免把环境噪音误判为代码问题

## 2026-05-04

### 阶段：request invoker provider gate 对称回归（CQRS-REWRITE-RP-077）

- 使用 `$gframework-batch-boot 25` 继续 `feat/cqrs-optimization` 的 CQRS 收口批次
- 批次目标：在 branch diff 相对 `origin/main` 接近 `25` 个文件前，补齐低风险的 generator 合同回归切片
- 本轮先确认当前 worktree 已无 `local-plan` 遗留恢复入口，随后转入 `cqrs-rewrite` 的 request / stream invoker provider gate 对称性复核
- 结论：
  - 生产代码已经同时检查 request provider、enumerator、descriptor 与 descriptor entry 四项 runtime 合同
  - request 侧测试只覆盖缺少 provider / enumerator，缺少 descriptor / descriptor entry 的回归覆盖落后于 stream 侧
- 已补齐：
  - `Does_Not_Emit_Request_Invoker_Provider_Metadata_When_Runtime_Lacks_Request_Descriptor_Type`
  - `Does_Not_Emit_Request_Invoker_Provider_Metadata_When_Runtime_Lacks_Request_Descriptor_Entry_Type`
  - source emission XML 文档同步说明 provider gate 依赖完整 descriptor / descriptor entry 合同

### 验证（RP-077）

- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Request_Invoker_Provider_Metadata_When_Runtime_Lacks_Request_Descriptor_Type|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Request_Invoker_Provider_Metadata_When_Runtime_Lacks_Request_Descriptor_Entry_Type|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Request_Invoker_Provider_Metadata_When_Runtime_Lacks_Request_Provider_Interface|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Request_Invoker_Provider_Metadata_When_Runtime_Lacks_Request_Descriptor_Enumerator"`
  - 结果：通过，`4/4` passed
- `dotnet build GFramework.Cqrs.SourceGenerators/GFramework.Cqrs.SourceGenerators.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行，避免脚本内部 plain `git ls-files` 误判仓库上下文
- `git diff --check`
  - 结果：通过

### 当前下一步（RP-077）

1. 继续使用 `origin/main` 作为 `$gframework-batch-boot 25` 的基线，复算 branch diff 后决定是否还能接下一批
2. 若继续推进代码切片，优先查找 request / stream invoker provider runtime 合同之外的同类对称测试缺口

### 阶段：mixed fallback attribute usage 回归（CQRS-REWRITE-RP-078）

- 继续沿用 `$gframework-batch-boot 25`，当前 branch diff 仍低于阈值
- 复核 fallback metadata runtime contract 后确认：
  - mixed fallback 在 runtime 允许多个 fallback attribute 实例时已有直接 `Type` + 字符串拆分回归
  - runtime 同时支持 `params Type[]` / `params string[]` 但不允许多个 fallback attribute 实例时，缺少锁定“整体回退到单个字符串 attribute”的回归
- 已补齐：
  - `Emits_String_Fallback_Metadata_For_Mixed_Fallback_When_Runtime_Disallows_Multiple_Fallback_Attributes`
  - `ReplaceAttributeUsageForType` 测试辅助方法，用于构造 runtime attribute usage 变体而不复制大型 source fixture

### 验证（RP-078）

- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Emits_String_Fallback_Metadata_For_Mixed_Fallback_When_Runtime_Disallows_Multiple_Fallback_Attributes"`
  - 结果：通过，`1/1` passed
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过

### 当前下一步（RP-078）

1. 继续复算 branch diff vs `origin/main`，若仍低于 `25` 个文件可继续下一批
2. 下一批优先查看 fallback metadata 与 generated invoker provider 之外是否还有同类 runtime contract gate 回归缺口

### 阶段：基础 generated registry contract gate 回归（CQRS-REWRITE-RP-079）

- 继续沿用 `$gframework-batch-boot 25`，当前 branch diff 仍低于阈值
- 复核 generator 基础启用条件后确认：缺少 `ICqrsHandlerRegistry` 时，runtime 不具备承载 generated registry 的基础接口合同，应整体跳过发射
- 已补齐：
  - `Does_Not_Generate_Registry_When_Runtime_Lacks_Handler_Registry_Interface`

### 验证（RP-079）

- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Generate_Registry_When_Runtime_Lacks_Handler_Registry_Interface"`
  - 结果：通过，`1/1` passed
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过

### 当前下一步（RP-079）

1. 继续复算 branch diff vs `origin/main`，若仍低于 `25` 个文件可继续下一批
2. 下一批优先复核基础 generation gate 中其他必需 runtime contracts 是否也需要同类回归覆盖

### 阶段：基础 generated registry contract gate 扩展回归（CQRS-REWRITE-RP-080）

- 将 `RP-079` 的单一 handler registry interface 缺失回归扩展为基础 generation gate 参数化测试
- 已补齐缺失分支：
  - `ICqrsHandlerRegistry`
  - `INotificationHandler<TNotification>`
  - `IStreamRequestHandler<TRequest, TResponse>`
  - `CqrsHandlerRegistryAttribute`
- stream handler interface 变体采用类型重命名构造 runtime metadata miss，避免删除命名空间尾部单行接口时引入输入编译错误

### 验证（RP-080）

- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Generate_Registry_When_Runtime_Lacks_Required_Generation_Contract"`
  - 结果：通过，`4/4` passed
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过

### 当前下一步（RP-080）

1. 继续复算 branch diff vs `origin/main`，若仍低于 `25` 个文件可继续下一批
2. 下一批优先复核基础 generation gate 中 logging / DI 依赖是否已有合适的输入编译安全回归覆盖方式

### 阶段：基础 generated registry external contract gate 回归（CQRS-REWRITE-RP-081）

- 延续 `RP-080` 的参数化基础 generation gate 测试，将外部 logging / DI 依赖也纳入同一组静默跳过回归
- 已补齐缺失分支：
  - `GFramework.Core.Abstractions.Logging.ILogger`
  - `Microsoft.Extensions.DependencyInjection.IServiceCollection`
- 两个变体均通过类型重命名构造 runtime metadata miss，保持输入源码可编译，避免把依赖缺失测试误写成编译失败测试

### 验证（RP-081）

- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Generate_Registry_When_Runtime_Lacks_Required_Generation_Contract"`
  - 结果：通过，`6/6` passed
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过

### 当前下一步（RP-081）

1. 继续复算 branch diff vs `origin/main`，若仍低于 `25` 个文件可继续下一批
2. 下一批优先复核基础 generation gate 中 request handler contract 与 handler registry attribute 以外是否还有可安全构造的缺失分支

### 阶段：基础 generated registry request handler gate 回归（CQRS-REWRITE-RP-082）

- 延续 `RP-081` 的基础 generation gate 参数化测试，补齐 `IRequestHandler<TRequest,TResponse>` 缺失分支
- 该变体同样通过类型重命名构造 runtime metadata miss，保持输入源码可编译
- 至此基础 generation gate 中可安全构造的缺失分支已覆盖：
  - request handler interface
  - notification handler interface
  - stream handler interface
  - handler registry interface
  - handler registry attribute
  - logging interface
  - DI service collection interface

### 验证（RP-082）

- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Generate_Registry_When_Runtime_Lacks_Required_Generation_Contract"`
  - 结果：通过，`7/7` passed
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `git diff --check`
  - 结果：通过

### 当前下一步（RP-082）

1. 继续复算 branch diff vs `origin/main`，若仍低于 `25` 个文件可继续下一批
2. 下一批优先复核基础 generation gate 之外的 runtime contract 或 fallback selection 分支；基础 gate 的可安全构造缺失分支已覆盖

### 阶段：PR #323 review 锚点收敛（CQRS-REWRITE-RP-082）

- 使用 `$gframework-pr-review` 重新拉取当前分支 PR review payload，确认当前分支对应 `PR #323`，状态为 `OPEN`
- 本轮 latest-head open AI thread 仅指出 active tracking 中仍保留 `PR #307` 作为当前 PR 锚点；本地复核后确认该反馈仍成立
- 已将 active tracking 的当前 PR 锚点、活跃事实、最近 PR review 备注和下一推荐步骤统一到 `PR #323`
- `PR #307` 仅保留为历史 PR 说明和较早 trace 段落，不再作为 active 恢复入口

### 验证（PR #323 review）

- `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --format json --json-output <temporary-json-output>`
  - 结果：通过
  - 备注：确认 `PR #323` 只有 1 个 CodeRabbit open thread，指向 active tracking 的 PR 锚点漂移
- 远端 `CTRF` 最新汇总为 `2274/2274` passed
- `MegaLinter` 当前仅报告 `dotnet-format` 的 `Restore operation failed` 环境噪音，未提供本地仍成立的文件级格式诊断
- `git diff --check`
  - 结果：通过
- `python3 scripts/license-header.py --check`
  - 结果：通过
  - 备注：当前 WSL worktree 需要显式绑定 `GIT_DIR` / `GIT_WORK_TREE` 后运行
- `dotnet build GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`

### 当前下一步（PR #323 review）

1. 若 review 重新触发后仍有 latest-head open thread，继续以 `PR #323` 为当前唯一 PR 恢复锚点复核
2. 后续若继续推进代码切片，优先复核基础 generation gate 之外的 runtime contract 或 fallback selection 分支

## 2026-05-06（RP-083 ~ RP-089）

### 阶段：mixed invoker provider 排序回归（CQRS-REWRITE-RP-083）

- 使用 `$gframework-batch-boot 50` 继续 `feat/cqrs-optimization` 的 CQRS 收口批次
- 批次目标：在 branch diff 相对 `origin/main` 接近 `50` 个文件前，继续补齐低风险的 generator runtime contract / emission 回归
- 本轮基线选择：
  - `origin/main a8c6c11e`，committer date `2026-05-05 13:14:24 +0800`
  - `main a8c6c11e`，committer date `2026-05-05 13:14:24 +0800`
  - 当前分支 `feat/cqrs-optimization a8c6c11e`，committer date `2026-05-05 13:14:24 +0800`
- 启动时 branch diff vs `origin/main` 为 `0` files / `0` lines，因此继续选择低风险测试回归切片
- 本轮复核 `CreateGeneratedRegistrySourceShape` 与 invoker emission 路径后确认：
  - 现有测试已覆盖 request / stream provider 的单一 direct 场景、单一 reflected-implementation 场景、precise reflected 跳过边界，以及各项 runtime contract 缺失分支
  - 尚未锁定“同一 registry 同时包含 direct registration 与 reflected-implementation registration”时的 descriptor 顺序与 `Invoke*HandlerN` 编号稳定性
- 已补齐：
  - `Emits_Request_Invoker_Provider_Metadata_In_Stable_Order_For_Mixed_Direct_And_Reflected_Implementations`
  - `Emits_Stream_Invoker_Provider_Metadata_In_Stable_Order_For_Mixed_Direct_And_Reflected_Implementations`
  - 两组 source fixture：`MixedRequestInvokerProviderSource`、`MixedStreamInvokerProviderSource`
- 通过新增回归，显式锁定以下约束：
  - provider descriptor 条目按稳定实现排序输出
  - `InvokeRequestHandler0/1` 与 `InvokeStreamHandler0/1` 的方法编号随 emission 顺序连续增长
  - 隐藏实现类型不会破坏 direct registration 与 reflected-implementation registration 的混合发射

### 验证（RP-083）

- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Emits_Request_Invoker_Provider_Metadata_In_Stable_Order_For_Mixed_Direct_And_Reflected_Implementations|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Emits_Stream_Invoker_Provider_Metadata_In_Stable_Order_For_Mixed_Direct_And_Reflected_Implementations"`
  - 结果：通过，`2/2` passed

### 当前 stop-condition 度量（RP-083）

- primary metric：branch diff files vs `origin/main`
- 当前说明：active batch 尚未提交时，基于 `HEAD` 的 branch diff 仍显示 `0` files / `0` lines；提交本批后再以新 `HEAD` 复算累计 branch diff

### 当前下一步（RP-083）

1. 提交本轮 mixed invoker provider 排序回归后，复算 branch diff vs `origin/main`，确认 `50` 文件阈值仍有充足余量
2. 若继续推进代码切片，优先复核 invoker provider 之外的 runtime contract 或 fallback selection 分支

### 阶段：benchmark 基础设施引入（CQRS-REWRITE-RP-084）

- 用户明确将当前长期分支目标上提为：系统性吸收 `ai-libs/Mediator` 的实现思路与设计哲学，并将可取部分纳入 `GFramework.Cqrs`
- 本轮据此调整批次目标，不再把关注点收缩到单个 generator 回归，而是建立能持续比较和吸收设计差异的 benchmark 基础设施
- 参考 `ai-libs/Mediator` 的 benchmark 设计后，本轮采纳的核心结构包括：
  - 独立 benchmark 项目壳，而非扩展现有 NUnit 测试项目
  - 共享 `Fixture` 输出并校验场景配置
  - `Request` / `Notification` 两个 messaging 场景作为首批最小落点
  - 自定义列 `CustomColumn`，为后续矩阵扩展保留可读结果标签
- 本轮新增：
  - `GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj`
  - `GFramework.Cqrs.Benchmarks/Program.cs`
  - `GFramework.Cqrs.Benchmarks/CustomColumn.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/Fixture.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/BenchmarkContext.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/NotificationBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/README.md`
- 设计取舍：
  - 使用最小 `ICqrsContext` marker，避免把完整 `ArchitectureContext` 初始化成本混入 steady-state dispatch
  - 直接复用 `GFramework.Cqrs.CqrsRuntimeFactory` 与 `MicrosoftDiContainer`，让基准聚焦于 runtime dispatch / publish
  - 外部对照组先接入 `MediatR`，保持与 `Mediator` benchmark 的对照哲学一致；但本轮仍只做最小 request / notification 场景
  - 暂不把 source generator benchmark、cold-start 独立工程或完整 pipeline / stream 矩阵一起引入，避免首批 scope 失控
- 兼容性修正：
  - 在根 `GFramework.csproj` 中显式排除 `GFramework.Cqrs.Benchmarks/**`，避免 meta-package 意外编译 benchmark 源码
  - 将 benchmark 项目加入 `GFramework.sln`，保持仓库级工作流完整

### 验证（RP-084）

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `GIT_DIR=<worktree-git-dir> GIT_WORK_TREE=<worktree-root> python3 scripts/license-header.py --check`
  - 结果：通过
- `git diff --check`
  - 结果：通过

### 当前 stop-condition 度量（RP-084）

- primary metric：branch diff files vs `origin/main`
- 当前说明：本轮仍在 `50` 文件阈值以内，可继续按 benchmark 场景或 CQRS runtime 对照能力分批推进

### 当前下一步（RP-084）

1. 继续扩展 `GFramework.Cqrs.Benchmarks`，优先补齐 pipeline、stream、cold-start 与 generated invoker provider 对照场景
2. 当后续有具体 runtime 优化切片时，用该 benchmark 项目验证是否真正吸收到了 `Mediator` 的低开销 dispatch 设计收益

### 阶段：stream request benchmark 对照（CQRS-REWRITE-RP-085）

- 继续沿用 `$gframework-batch-boot 50`，当前 branch diff 相对 `origin/main` 仍明显低于阈值
- 在 `RP-084` 已建立独立 benchmark 项目后，本轮优先补齐 `ai-libs/Mediator/benchmarks/Mediator.Benchmarks/Messaging/StreamingBenchmarks.cs` 对应的最小 stream 场景
- 选择 stream 作为第二批 benchmark 的原因：
  - 已有独立的 `CreateStream` runtime 路径和单独的 stream invoker provider 元数据契约
  - 与 `Mediator` 的 messaging benchmark 分层直接对应
  - 不需要像 pipeline / cold-start 那样先进一步澄清运行时或宿主边界
- 本轮新增：
  - `GFramework.Cqrs.Benchmarks/Messaging/StreamingBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/README.md` 中的 stream 场景说明
- 设计约束：
  - 保持与前一批一致的三路对照：`Baseline`、`GFramework.Cqrs`、`MediatR`
  - 基准测量“完整枚举 3 个元素”的全量消费成本，而不是只测创建异步枚举器
  - 使用最小 `ICqrsContext` marker，继续避免把完整 `ArchitectureContext` 初始化成本混入 steady-state stream dispatch
- 结论：
  - 当前 benchmark 项目已经覆盖 `Request`、`Notification`、`StreamRequest` 三个核心 messaging steady-state 场景
  - 下一批更适合转向 request pipeline 数量矩阵或 cold-start / initialization，而不是继续扩同层次的 messaging 基线

### 验证（RP-085）

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `GIT_DIR=<worktree-git-dir> GIT_WORK_TREE=<worktree-root> python3 scripts/license-header.py --check`
  - 结果：通过
- `git diff --check`
  - 结果：通过

### 当前 stop-condition 度量（RP-085）

- primary metric：branch diff files vs `origin/main`
- 当前说明：新增 stream benchmark 后仍处于 `50` 文件阈值以内，适合继续下一批 request pipeline 或 cold-start 场景

### 当前下一步（RP-085）

1. 继续扩展 `GFramework.Cqrs.Benchmarks`，优先补齐 request pipeline 数量矩阵，随后再评估 cold-start / initialization
2. 当需要验证 generated invoker provider 的实际收益时，把 request benchmark 扩展为 reflection / generated provider 对照，而不是只停留在框架间对比

### 阶段：request pipeline 数量矩阵（CQRS-REWRITE-RP-086）

- 继续沿用 `$gframework-batch-boot 50`，当前 branch diff 相对 `origin/main` 仍明显低于阈值
- 本轮把 benchmark 关注点从单纯 messaging steady-state 扩展到 request pipeline 编排行为，原因是：
  - `ai-libs/Mediator` 的对照价值已经不只在 request / notification / stream 三个入口本身，还在 pipeline 包装策略与生命周期取舍
  - `GFramework.Cqrs.Internal.CqrsDispatcher` 已按 `behaviorCount` 缓存 `RequestPipelineExecutor<TResponse>` 形状，因此单独量化 `0 / 1 / 4` 个行为的 steady-state 开销有直接信息密度
- 本轮新增：
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestPipelineBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/README.md` 中的 request pipeline 场景说明
- 设计取舍：
  - 采用 `0 / 1 / 4` 个 pipeline 行为，而不是立即扩到更大的参数空间，先锁定最有代表性的无行为 / 少量行为 / 常见多行为矩阵
  - 使用最小 no-op 行为族，不引入日志、计时或上下文刷新逻辑，避免把测量结果污染成业务行为成本
  - `GFramework.Cqrs` 与 `MediatR` 侧都只注册当前 benchmark 请求对应的闭合行为类型，确保矩阵反映编排成本而非程序集扫描差异
- 接受的只读 subagent 结论：
  - 下一批 benchmark 继续优先考虑 `cold-start / initialization` 与 `generated provider` 对照，而不是立即照搬 `Mediator` 的 large-project 维度
  - 当前 `GFramework.Cqrs.Benchmarks` 仍未接入 `Mediator` 包和 `GFramework.Cqrs.SourceGenerators`，因此本轮不扩成 `Mediator_IMediator` / generated-provider 对照，避免 scope 失控
- 结论：
  - 当前 benchmark 项目已经覆盖 `Request`、`Notification`、`StreamRequest` 与 `RequestPipeline`
  - 后续若要继续贴近 `Mediator` 的 comparison benchmark，最值得优先补的是 initialization / first-hit 与 generated invoker provider，而不是继续横向堆更多 steady-state messaging 入口

### 验证（RP-086）

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`

### 当前 stop-condition 度量（RP-086）

- primary metric：branch diff files vs `origin/main`
- 当前说明：提交前基于 `HEAD` 的 branch diff 仍为 `14` files，距离 `50` 文件阈值仍有明显余量

### 当前下一步（RP-086）

1. 提交本轮 request pipeline benchmark 后，继续扩展 `GFramework.Cqrs.Benchmarks`，优先补齐 initialization / cold-start 场景
2. 当需要验证 dispatcher 预热与 source generator 收益时，引入 generated invoker provider 对照，并评估是否同时接入 `Mediator` concrete runtime 作为更贴近设计哲学的外部参照

### 阶段：request startup 基线（CQRS-REWRITE-RP-087）

- 继续沿用 `$gframework-batch-boot 50`，当前 branch diff 相对 `origin/main` 仍明显低于阈值
- 本轮目标：把 benchmark 从 steady-state dispatch 再向前推进一层，补齐与 `ai-libs/Mediator/benchmarks/Mediator.Benchmarks/Messaging/Comparison/*` 更接近的 startup 维度
- 本轮新增：
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestStartupBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/README.md` 中的 startup 场景说明
- 设计取舍：
  - `Initialization` 只测“从已配置宿主解析/创建 runtime 句柄”的成本，不把完整架构初始化混入 benchmark
  - `ColdStart` 只测新宿主上的首次 request send；`GFramework.Cqrs` 侧在每次 benchmark 前通过反射清空 dispatcher 静态缓存，避免把热缓存误当 first-hit
  - `ColdStart_MediatR` 改为真正 `await` 完任务后再释放 `ServiceProvider`，以满足 `Meziantou.Analyzer` 对资源生命周期的要求，并避免 benchmark 本身含有错误宿主释放语义
- 结论：
  - 当前 benchmark 项目已经覆盖 `Request`、`Notification`、`StreamRequest`、`RequestPipeline`、`RequestStartup`
  - 后续若继续贴近 `Mediator` comparison benchmark，下一批最有价值的是 generated invoker provider、registration / service lifetime 与 concrete runtime 外部对照，而不是继续只加同层 steady-state case

### 验证（RP-087）

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`

### 当前 stop-condition 度量（RP-087）

- primary metric：branch diff files vs `origin/main`
- 当前说明：提交前 branch diff 仍远低于 `50` 文件阈值，可继续下一批 benchmark 或低风险 runtime 对照切片

### 当前下一步（RP-087）

1. 提交本轮 request startup benchmark 后，继续扩展 `GFramework.Cqrs.Benchmarks`，优先评估 generated invoker provider 与 registration / service lifetime 矩阵
2. 若要更贴近 `Mediator` 的 comparison benchmark 设计哲学，评估是否在 benchmark 项目中同时接入 `Mediator` concrete runtime 对照，而不只保留 `MediatR`

### 阶段：request invoker reflection / generated 对照（CQRS-REWRITE-RP-088）

- 继续沿用 `$gframework-batch-boot 50`，当前 branch diff 相对 `origin/main` 仍明显低于阈值
- 本轮目标：不再只比较 `GFramework.Cqrs` 与 `MediatR` 的外层框架差异，而是开始直接量化 `GFramework.Cqrs` 内部 reflection request binding 与 generated invoker provider 路径的 steady-state 差异
- 本轮新增：
  - `GFramework.Cqrs.Benchmarks/Messaging/RequestInvokerBenchmarks.cs`
  - `GFramework.Cqrs.Benchmarks/Messaging/GeneratedRequestInvokerBenchmarkRegistry.cs`
  - `GFramework.Cqrs.Benchmarks/README.md` 中的 generated invoker 场景说明
- 设计取舍：
  - 采用 benchmark 内手写的 generated registry/provider“等价物”，而不是当轮就把真实 `GFramework.Cqrs.SourceGenerators` 接到 benchmark 项目中，目的是先走通真实的 registrar -> descriptor 预热 -> dispatcher generated path，同时把写入面控制在低风险范围
  - generated 对照使用程序集级 `CqrsHandlerRegistryAttribute` + `ICqrsRequestInvokerProvider` + `IEnumeratesCqrsRequestInvokerDescriptors`，确保运行时语义与生产路径一致
  - 在 benchmark 生命周期前后清理 dispatcher 静态缓存，避免 generated descriptor 预热状态跨场景泄漏，污染 reflection 对照
- 结论：
  - 当前 benchmark 项目已经能区分 `GFramework.Cqrs` 的 reflection request 路径、generated request 路径与 `MediatR` 外部对照
  - 后续若继续贴近 `Mediator` comparison benchmark，下一批更适合扩到 registration / service lifetime、stream generated provider，或再决定是否接入 `Mediator` concrete runtime

### 验证（RP-088）

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`

### 当前 stop-condition 度量（RP-088）

- primary metric：branch diff files vs `origin/main`
- 当前说明：提交前 branch diff 仍远低于 `50` 文件阈值，可继续推进下一批 benchmark 对照切片

### 当前下一步（RP-088）

1. 提交本轮 request invoker benchmark 后，继续扩展 `GFramework.Cqrs.Benchmarks`，优先评估 registration / service lifetime 或 stream generated provider

### 阶段：stream invoker reflection / generated 对照（CQRS-REWRITE-RP-089）

- 使用 `$gframework-batch-boot 30` 继续 `feat/cqrs-optimization` 的 CQRS 收口批次
- 本轮基线选择：
  - `origin/main c01abac0`，committer date `2026-05-06 09:40:08 +0800`
  - `main a8c6c11e`，committer date `2026-05-05 13:14:24 +0800`
- 启动时 branch diff vs `origin/main` 为 `18` files / `2100` lines，低于 `30` 文件阈值，因此继续选择单模块、低风险 benchmark 切片
- 复核 `GFramework.Cqrs.Benchmarks` 与 `ai-libs/Mediator/benchmarks` 后确认：
  - `RP-088` 已把 generated descriptor 预热收益量化到 request dispatch 路径
  - stream benchmark 仍停留在 direct handler / reflection runtime / `MediatR` 三路对照，尚未量化 generated stream invoker provider 的收益
  - 虽然 `Mediator` 参考基准大量使用 service lifetime 矩阵，但当前 `GFramework.Cqrs.Benchmarks` 尚未建立对称的 scoped host 模式；直接扩 lifetime 会引入超出本批风险预算的宿主语义变化
- 本轮因此优先选择 request 对称切片，而不是 service lifetime 扩展：
  - 新增 `Messaging/StreamInvokerBenchmarks.cs`
  - 新增 `Messaging/GeneratedStreamInvokerBenchmarkRegistry.cs`
  - 更新 `GFramework.Cqrs.Benchmarks/README.md`
- 设计约束：
  - 继续沿用 handwritten generated registry/provider 模式，避免把 benchmark 基础设施与真实 source-generator 输出耦合
  - 复用与 `RP-088` 相同的 dispatcher 缓存清理策略，确保 reflection / generated 路径对照不受静态缓存残留污染
  - 使用统一的异步枚举体工厂，让三组 stream handler 共享同一枚举成本基线，把变量收敛到 invoker/provider 接线路径

### 当前下一步（RP-089）

1. 完成本轮 benchmark 项目 Release build、license header 检查与 diff 校验后，更新 active tracking 的权威验证列表
2. 若 branch diff 仍明显低于 `30` 文件阈值，可继续评估 notification publish strategy 或更贴近 `Mediator` concrete runtime 的单批对照
3. 若要继续贴近 `Mediator` 的 comparison benchmark 设计哲学，评估是否把 `Mediator` concrete runtime 本身接入 benchmark 项目，而不是长期只保留 `MediatR`

### 验证（RP-089）

- `dotnet build GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `GIT_DIR=<worktree-git-dir> GIT_WORK_TREE=<worktree-root> python3 scripts/license-header.py --check`
  - 结果：通过
- `git diff --check`
  - 结果：通过
- `dotnet run --project GFramework.Cqrs.Benchmarks/GFramework.Cqrs.Benchmarks.csproj -c Release -- --filter "*RequestStartupBenchmarks*" --job short --warmupCount 1 --iterationCount 1 --launchCount 1`
  - 结果：部分通过；`MediatR` startup benchmark 已恢复真实测量，`ColdStart_GFrameworkCqrs` 仍因 `No CQRS request handler registered` 失败

### 阶段：手动 benchmark workflow（CQRS-REWRITE-RP-089）

- 新增 `.github/workflows/benchmark.yml`，提供仅 `workflow_dispatch` 触发的 benchmark 入口
- workflow 默认只执行 `GFramework.Cqrs.Benchmarks` 的 Release build，避免在当前已知 `RequestStartupBenchmarks` 残留未清时默认运行失败
- 只有在手动输入 `benchmark_filter` 时才执行 BenchmarkDotNet，并上传 `BenchmarkDotNet.Artifacts` 供后续比较
