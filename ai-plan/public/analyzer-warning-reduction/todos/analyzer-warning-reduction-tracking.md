# Analyzer Warning Reduction 跟踪

## 目标

继续以“直接看构建输出、直接修构建 warning”为原则推进当前分支，并保持 active recovery 文档只保留当前真值。

## 当前恢复点

- 恢复点编号：`ANALYZER-WARNING-REDUCTION-RP-057`
- 当前阶段：`Phase 57`
- 当前焦点：
  - `2026-04-24` 本轮继续吃掉 `GFramework.Game.Tests` 中仍然独立且低风险的 `PersistenceTests.cs` 残余 `MA0004`
  - 已补齐两个失败缓存一致性测试中的剩余 `.ConfigureAwait(false)`，使 `PersistenceTests.cs` 不再出现在非增量构建 warning 输出中
  - 非增量 `dotnet build GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --no-incremental` 从 `253 Warning(s)` 进一步收敛到 `249 Warning(s)`，剩余热点几乎全部集中在 `YamlConfigLoaderTests.cs`
  - 按当前工作树投影重新计算后，分支体积为 `27` 个文件、`991` 行，仍低于 `$gframework-batch-boot 75`

## 当前活跃事实

- 之前记录的 plain `dotnet build` `0 Warning(s)` 属于增量构建假阴性，不能再作为 warning 检查真值
- 仓库根目录 `dotnet clean GFramework.sln -c Release` 仍在 `ValidateSolutionConfiguration` 阶段失败，项目级 `dotnet clean GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release` 也未能稳定提供 clean 基线
- 当前整仓最近一次直接观测值仍是 `dotnet build GFramework.sln -c Release` 的 `116 warning(s)`
- `RP-056` 已验证 `GeneratedConfigConsumerIntegrationTests.cs` 不再出现在项目 build warning 输出中
- `RP-057` 已验证 `PersistenceTests.cs` 不再出现在 `dotnet build GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --no-incremental` 的 warning 输出中
- 本轮已验证 `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~UnifiedSettingsDataRepository_SaveAsync_When_Persist_Fails_Should_Keep_Cache_Consistent|FullyQualifiedName~UnifiedSettingsDataRepository_DeleteAsync_When_Persist_Fails_Should_Keep_Cache_Consistent"`，结果为 `Passed: 2`
- `GFramework.Game.Tests` 当前剩余热点已经几乎完全集中到 `YamlConfigLoaderTests.cs` 这一高上下文文件

## 当前风险

- 如果后续继续依赖增量 `dotnet build`，容易再次把 warning 数量误判为 0
  - 缓解措施：每轮 warning 检查前先执行 `dotnet clean`，再执行目标 `dotnet build`
- 仓库根目录与 `GFramework.Game.Tests` 的 `dotnet clean` 目前都无法给出新的 clean 基线
  - 缓解措施：后续若继续整仓 warning reduction，需要单独定位 clean 失败原因，或明确继续沿用 direct build 观测值作为临时真值
- 当前 worktree 仍存在未跟踪的 `.codex` 目录
  - 缓解措施：提交当前批次时只暂存 analyzer-warning-reduction 相关源码与 `ai-plan` 文件，避免把工作目录辅助文件混入提交
- 下一轮若继续深入 `GFramework.Game.Tests`，很可能需要进入 `YamlConfigLoaderTests.cs` 这种高上下文大文件
  - 缓解措施：把它单独作为一个明确的新批次处理，不与其它 warning family 混批

## 活跃文档

- 当前轮次归档：
  - [analyzer-warning-reduction-history-rp042-rp048.md](../archive/todos/analyzer-warning-reduction-history-rp042-rp048.md)
- 历史跟踪归档：
  - [analyzer-warning-reduction-history-rp001.md](../archive/todos/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/todos/analyzer-warning-reduction-history-rp002-rp041.md)
- 历史 trace 归档：
  - [analyzer-warning-reduction-history-rp001.md](../archive/traces/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/traces/analyzer-warning-reduction-history-rp002-rp041.md)
  - [analyzer-warning-reduction-history-rp042-rp048.md](../archive/traces/analyzer-warning-reduction-history-rp042-rp048.md)

## 验证说明

- `dotnet clean GFramework.sln -c Release`
  - 结果：失败；停在 solution `ValidateSolutionConfiguration`，`0 Warning(s)`、`0 Error(s)`，未输出更具体的 error 文本
- `dotnet build GFramework.sln -c Release`
  - 结果：成功；`116 Warning(s)`、`0 Error(s)`
- `dotnet clean GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release`
  - 结果：失败；clean 阶段在 MSBuild 清理路径结束前返回 `0 Warning(s)`、`0 Error(s)`，未输出额外错误文本
- `dotnet build GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release`
  - `RP-055` 收尾结果：成功；`63 Warning(s)`、`0 Error(s)`
  - `RP-056` 当前结果：成功；`59 Warning(s)`、`0 Error(s)`
- `dotnet build GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --no-incremental`
  - `RP-057` 热点重排前：成功；`253 Warning(s)`、`0 Error(s)`
  - `RP-057` 当前结果：成功；`249 Warning(s)`、`0 Error(s)`
- `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~ArchitectureConfigIntegrationTests|FullyQualifiedName~GameConfigBootstrapTests|FullyQualifiedName~JsonSerializerTests"`
  - 结果：成功；`Passed: 19`、`Failed: 0`
- `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~GeneratedConfigConsumerIntegrationTests"`
  - 结果：成功；`Passed: 4`、`Failed: 0`
- `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~UnifiedSettingsDataRepository_SaveAsync_When_Persist_Fails_Should_Keep_Cache_Consistent|FullyQualifiedName~UnifiedSettingsDataRepository_DeleteAsync_When_Persist_Fails_Should_Keep_Cache_Consistent"`
  - 结果：成功；`Passed: 2`、`Failed: 0`

## 下一步建议

1. 提交 `PersistenceTests.cs` 与 `RP-057` tracking/trace 更新，继续保持只纳入本 topic 相关文件
2. 若继续 warning reduction，应把 `YamlConfigLoaderTests.cs` 视为独立的高上下文批次，不再与其它文件混批
