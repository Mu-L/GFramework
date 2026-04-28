# Analyzer Warning Reduction 跟踪

## 目标

继续以“直接看构建输出、直接修构建 warning”为原则推进当前分支，并保持 active recovery 文档只保留当前真值。

## 当前恢复点

- 恢复点编号：`ANALYZER-WARNING-REDUCTION-RP-087`
- 当前阶段：`Phase 87`
- 当前焦点：
  - `2026-04-28` 已按 `$gframework-batch-boot 50` 先执行仓库根 `dotnet clean` + `dotnet build`，建立本轮权威基线 `288 Warning(s)` / `214` 个唯一位点
  - 本轮已并行收敛 `GameContextTests.cs`、`ArchitectureServicesTests.cs`、`RegistryInitializationHookBaseTests.cs`、`CqrsDispatcherCacheTests.cs` 与 `CqrsHandlerRegistrarTests.cs`
  - 主线程已补齐 `ResourceManagerTests.cs`、`TestEvent.cs`、`LoggerTests.cs`、`ContextProviderTests.cs`、`TestArchitectureBase.cs`、`CommandCoroutineExtensionsTests.cs` 等 `Core.Tests` 零散 warning
  - 当前 `GFramework.Core.Tests` 与 `GFramework.Cqrs.Tests` 的受影响项目 Release 构建都已恢复到 `0 Warning(s)` / `0 Error(s)`
  - 当前仓库根权威基线已从本轮开始时的 `288 Warning(s)` / `214` 个唯一位点下降到 `236 Warning(s)` / `162` 个唯一位点；剩余 warning 只集中在 `Mediator/*` 与 `YamlConfigSchemaValidator*`

## 当前活跃事实

- 当前 `origin/main` 基线提交为 `6cc87a9`（`2026-04-27T20:28:50+08:00`）。
- 当前直接验证结果：
  - `dotnet clean`
    - 最新结果：成功；已刷新本轮 final non-incremental 仓库根基线
  - `dotnet build`
    - 最新结果：成功；`236 Warning(s)`、`0 Error(s)`，唯一位点 `162`
  - `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
    - 最新结果：成功；`0 Warning(s)`、`0 Error(s)`
  - `dotnet build GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release`
    - 最新结果：成功；`0 Warning(s)`、`0 Error(s)`
- 当前批次摘要：
  - 本轮接受并集成 `GameContextTests.cs`、`ArchitectureServicesTests.cs`、`RegistryInitializationHookBaseTests.cs`、`CqrsDispatcherCacheTests.cs`、`CqrsHandlerRegistrarTests.cs` 五个并行 worker 切片
  - 主线程补齐 `Core.Tests` 内剩余零散 warning，使 `GFramework.Core.Tests` 项目级 Release 构建回到 `0 Warning(s)` / `0 Error(s)`
  - 当前 `origin/main...HEAD` 已提交 branch diff 仍为 `21` 个文件；计入当前待提交工作树后的并集 footprint 为 `45 / 50` 个文件，已接近本轮停止线
- 当前建议保留到下一波次的候选：
  - `GFramework.Cqrs.Tests/Mediator/MediatorArchitectureIntegrationTests.cs`、`MediatorComprehensiveTests.cs`、`MediatorAdvancedFeaturesTests.cs` 的高密度 `MA0048` / `MA0004`
  - `GFramework.Game/Config/YamlConfigSchemaValidator.cs` 与 `YamlConfigSchemaValidator.ObjectKeywords.cs` 的高耦合 warning 热点

## 当前风险

- `GFramework.Cqrs.Tests/Mediator/*` 仍有 `94` / `88` / `68` 条输出 warning，属于高 changed-file 风险的 `MA0048` 大波次。
  - 缓解措施：当前 footprint 已到 `45 / 50`，下一轮应在新提交基础上单独规划 `Mediator*` 波次，而不是继续叠在本轮工作树上。
- `YamlConfigSchemaValidator*` 仍然聚集 `222` 条输出 warning，且同时混有 `MA0048`、`MA0009`、`MA0051`、`MA0006`。
  - 缓解措施：保持为独立高耦合波次，不与测试项目拆分混提。

## 活跃文档

- 当前轮次归档：
  - [analyzer-warning-reduction-history-rp074-rp078.md](../archive/todos/analyzer-warning-reduction-history-rp074-rp078.md)
  - [analyzer-warning-reduction-history-rp042-rp048.md](../archive/todos/analyzer-warning-reduction-history-rp042-rp048.md)
- 历史跟踪归档：
  - [analyzer-warning-reduction-history-rp001.md](../archive/todos/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/todos/analyzer-warning-reduction-history-rp002-rp041.md)
- 历史 trace 归档：
  - [analyzer-warning-reduction-history-rp073-rp078.md](../archive/traces/analyzer-warning-reduction-history-rp073-rp078.md)
  - [analyzer-warning-reduction-history-rp062-rp071.md](../archive/traces/analyzer-warning-reduction-history-rp062-rp071.md)
  - [analyzer-warning-reduction-history-rp001.md](../archive/traces/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/traces/analyzer-warning-reduction-history-rp002-rp041.md)
  - [analyzer-warning-reduction-history-rp042-rp048.md](../archive/traces/analyzer-warning-reduction-history-rp042-rp048.md)

## 验证说明

- 权威验证结果统一维护在“当前活跃事实”。
- `GFramework.Core.Tests` 与 `GFramework.Cqrs.Tests` 的当前受影响项目 Release 构建都已在本轮清零，但仓库根 non-incremental 构建仍保留 `Mediator/*` 与 `YamlConfigSchemaValidator*` 既有 warning。
- warning reduction 的仓库级真值只以同轮 `dotnet clean` 后的 `dotnet build` 为准。

## 下一步建议

1. 提交本轮 `Core.Tests` / `Cqrs.Tests` warning reduction 与 `ai-plan` 同步。
2. 下一轮在新提交基础上单独规划 `Mediator/*` 波次，避免在 `45 / 50` footprint 状态继续扩批。
3. 将 `YamlConfigSchemaValidator*` 保持为独立高耦合波次，必要时先由主线程局部切分再决定是否并行。
