# Analyzer Warning Reduction 跟踪

## 目标

继续以“直接看构建输出、直接修构建 warning”为原则推进当前分支，并保持 active recovery 文档只保留当前真值。

## 当前恢复点

- 恢复点编号：`ANALYZER-WARNING-REDUCTION-RP-061`
- 当前阶段：`Phase 61`
- 当前焦点：
  - `2026-04-25` 继续按 `$gframework-batch-boot 75` 自动推进，并明确允许使用 subagent 处理互不重叠的写集
  - 当前 `HEAD` 为 `67c9359`，基线 `origin/main` 仍为 `9964962`
  - 当前累计 branch diff 相对 `origin/main` 为 `28` 个文件、`903` 行，仍低于 `75 files` 主停止阈值
  - `RP-060` 之后已接受 8 个批次提交：`64c8589`、`4bb8f4f`、`bad6c1b`、`e8eda81`、`3be299e`、`09cbd16`、`9b20a07`、`67c9359`
  - 本轮主线策略已经从“继续深挖 `YamlConfigLoaderTests.cs`”切换为“优先吃新文件的低风险机械型异步断言包装”，以更有效推进 `75 files` 目标
  - 当前正在并行推进 3 个新写集：`ResultExtensionsTests.cs` + `AsyncOperationTests.cs`、`StateMachineSystemTests.cs` + `StateMachineTests.cs`、以及 `ArchitectureConfigIntegrationTests.cs`

## 当前活跃事实

- 之前记录的 plain `dotnet build` `0 Warning(s)` 属于增量构建假阴性，不能再作为 warning 检查真值
- 仓库根目录 `dotnet clean GFramework.sln -c Release` 仍在 `ValidateSolutionConfiguration` 阶段失败，项目级 `dotnet clean GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release` 也未能稳定提供 clean 基线
- 当前整仓最近一次直接观测值仍是 `dotnet build GFramework.sln -c Release` 的 `116 warning(s)`
- `RP-056` 已验证 `GeneratedConfigConsumerIntegrationTests.cs` 不再出现在项目 build warning 输出中
- `RP-057` 已验证 `PersistenceTests.cs` 不再出现在 `dotnet build GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --no-incremental` 的 warning 输出中
- `RP-059` 已验证 `YamlConfigLoaderTests.cs` 中的 `MA0004` 已清零，本轮继续把该文件 4 个纯加载 `MA0051` 热点也降掉了
- `GFramework.Game.Tests` 当前 `--no-incremental Release build` 结果为 `145 Warning(s)`、`0 Error(s)`；最近两批 `YamlConfigLoaderAllOfTests.cs`、`YamlConfigLoaderEnumTests.cs`、`YamlConfigLoaderNegationTests.cs`、`YamlConfigLoaderDependentSchemasTests.cs`、`YamlConfigLoaderIfThenElseTests.cs`、`PersistenceTests.cs` 未引入新增错误
- `GFramework.Game/GFramework.Game.csproj -c Release` 当前最近一次可信结果为 `0 Error(s)`；最近几批 touched files `SettingsSystem.cs`、`ScopedStorage.cs`、`SceneRouterBase.cs`、`FileStorage.cs`、`RouterBase.cs`、`UiRouterBase.cs` 未在主线程复核中暴露新增编译错误
- `GFramework.Godot.Tests/GFramework.Godot.Tests.csproj -c Release` 当前结果为 `0 Warning(s)`、`0 Error(s)`；`AbstractArchitectureModuleInstallationTests.cs` 已通过单测复验
- 当前 `origin/main` 基线提交为 `9964962`（`2026-04-24T23:05:53+08:00`）
- `GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --no-incremental` 当前结果为 `298 Warning(s)`、`0 Error(s)`；`ArchitectureLifecycleBehaviorTests.cs`、`AbstractAsyncCommandTests.cs`、`CommandExecutorTests.cs`、`AsyncKeyLockManagerTests.cs`、`AbstractAsyncQueryTests.cs`、`AsyncQueryExecutorTests.cs`、`AsyncArchitectureTests.cs` 已随 `67c9359` 落地
- 当前累计 branch diff 相对 `origin/main` 为 `28` 个文件、`903` 行；主停止条件仍然是 `75 changed files`

## 当前风险

- 如果后续继续依赖增量 `dotnet build`，容易再次把 warning 数量误判为 0
  - 缓解措施：每轮 warning 检查前先执行 `dotnet clean`，再执行目标 `dotnet build`
- 仓库根目录与 `GFramework.Game.Tests` 的 `dotnet clean` 目前都无法给出新的 clean 基线
  - 缓解措施：后续若继续整仓 warning reduction，需要单独定位 clean 失败原因，或明确继续沿用 direct build 观测值作为临时真值
- 当前 worktree 仍存在未跟踪的 `.codex` 目录
  - 缓解措施：提交当前批次时只暂存 analyzer-warning-reduction 相关源码与 `ai-plan` 文件，避免把工作目录辅助文件混入提交
- `YamlConfigLoaderTests.cs` 剩余切片已经收敛到热重载相关 `MA0051`，继续处理它的单文件收益不再能明显提升 branch diff 文件数
  - 缓解措施：后续优先切回新的单文件热点，只有在缺少低风险新文件时再回到该文件的热重载方法
- 并行 subagent 已经证明能加快批次落地，但主线程仍需逐批复核并统一记录，否则容易让恢复点失真
  - 缓解措施：每轮并行批次完成后先更新 active tracking / trace，再继续下一批
- 并行执行 `dotnet build` 会在共享输出目录上触发 `deps.json` 或 DLL 文件锁，产生与代码无关的假失败
  - 缓解措施：受影响项目的主线程验证统一改为串行 `--no-incremental Release build`，避免把并发 I/O 竞争误判成编译回归

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
  - `RP-059` 当前结果：成功；`203 Warning(s)`、`0 Error(s)`
- `dotnet build GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --no-incremental`
  - `RP-060` 当前结果：成功；`189 Warning(s)`、`0 Error(s)`
- `dotnet build GFramework.Game/GFramework.Game.csproj -c Release`
  - `RP-061` 最近可信结果：成功；`0 Error(s)`；warning 基线仍高，但最近 touched files 未见新增编译失败
- `dotnet build GFramework.Game/GFramework.Game.csproj -c Release`
  - `RP-060` 当前结果：成功；`519 Warning(s)`、`0 Error(s)`
- `dotnet build GFramework.Godot.Tests/GFramework.Godot.Tests.csproj -c Release`
  - `RP-060` 当前结果：成功；`0 Warning(s)`、`0 Error(s)`
- `dotnet build GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --no-incremental`
  - `RP-061` 当前结果：成功；`145 Warning(s)`、`0 Error(s)`
- `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --no-incremental`
  - 首次并行复验：失败；`GenerateDepsFile` 写入 `GFramework.Cqrs.Abstractions.deps.json` 时命中文件锁，属于并发构建副作用
  - 串行复验：成功；`298 Warning(s)`、`0 Error(s)`
- `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~ArchitectureConfigIntegrationTests|FullyQualifiedName~GameConfigBootstrapTests|FullyQualifiedName~JsonSerializerTests"`
  - 结果：成功；`Passed: 19`、`Failed: 0`
- `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~GeneratedConfigConsumerIntegrationTests"`
  - 结果：成功；`Passed: 4`、`Failed: 0`
- `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~UnifiedSettingsDataRepository_SaveAsync_When_Persist_Fails_Should_Keep_Cache_Consistent|FullyQualifiedName~UnifiedSettingsDataRepository_DeleteAsync_When_Persist_Fails_Should_Keep_Cache_Consistent"`
  - 结果：成功；`Passed: 2`、`Failed: 0`
- `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~YamlConfigLoaderTests"`
  - 结果：成功；`Passed: 74`、`Failed: 0`
- `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~YamlConfigTextValidatorTests|FullyQualifiedName~GameConfigBootstrapTests|FullyQualifiedName~YamlConfigLoaderDependentRequiredTests"`
  - 结果：成功；`Passed: 15`、`Failed: 0`
- `dotnet test GFramework.Godot.Tests/GFramework.Godot.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~InstallGodotModuleAsync_ShouldThrowBeforeInvokingModuleInstall_WhenAnchorIsMissing"`
  - 结果：成功；`Passed: 1`、`Failed: 0`

## 下一步建议

1. 等待当前 3 个并行子批次回报，并优先接受新的单文件测试清理提交，把 branch diff 继续向 `75 files` 推进
2. 若这 3 个批次全部落地后仍明显低于阈值，继续按 `rg -n "async \\(\\) => await"` 的剩余结果扩展到新的 `Core.Tests` / `Game.Tests` 低风险文件
