# Analyzer Warning Reduction 跟踪

## 目标

继续以“直接看构建输出、直接修构建 warning”为原则推进当前分支，并保持 active recovery 文档只保留当前真值。

## 当前恢复点

- 恢复点编号：`ANALYZER-WARNING-REDUCTION-RP-055`
- 当前阶段：`Phase 55`
- 当前焦点：
  - `2026-04-24` 本轮先纠正了 batch stop-condition 的计算口径：应使用 `origin/main` 与 `HEAD` 的 merge-base 分支 diff，而不是工作树 diff
  - 在该正确口径下，`RP-054` 提交后的真实 branch 体积是 `23` 个文件、`603` 行；当前这批提交后的投影体积是 `26` 个文件、`691` 行，仍低于 `$gframework-batch-boot 75`
  - 本轮已完成 `ArchitectureConfigIntegrationTests`、`GameConfigBootstrapTests`、`JsonSerializerTests` 的小热点清理，并顺手补齐 `YamlConfigLoaderAllOfTests` / `PersistenceTests` 的残余 warning
  - 当前仍在 `GFramework.Game.Tests` 内推进，但剩余热点已经越来越集中到 `YamlConfigLoaderTests.cs` 与 `GeneratedConfigConsumerIntegrationTests.cs` 这类高上下文文件

## 当前活跃事实

- 之前记录的 plain `dotnet build` `0 Warning(s)` 属于增量构建假阴性，不能再作为 warning 检查真值
- 仓库根目录 `dotnet clean GFramework.sln -c Release` 仍在 `ValidateSolutionConfiguration` 阶段失败，项目级 `dotnet clean GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release` 也未能稳定提供 clean 基线
- 当前整仓最近一次直接观测值仍是 `dotnet build GFramework.sln -c Release` 的 `116 warning(s)`
- `dotnet build GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release` 已从上一批入口的 `116 warning(s)` 继续收敛到本轮收尾的 `63 warning(s)`
- 本轮已验证 `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~ArchitectureConfigIntegrationTests|FullyQualifiedName~GameConfigBootstrapTests|FullyQualifiedName~JsonSerializerTests"`，结果为 `Passed: 19`
- `GFramework.Game.Tests` 当前剩余 warning 主要集中在未触碰的 `YamlConfigLoaderTests.cs`、`GeneratedConfigConsumerIntegrationTests.cs`，以及少量未处理的 `GameConfigBootstrapTests` 之外热点

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
  - 结果：成功；`63 Warning(s)`、`0 Error(s)`
- `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~ArchitectureConfigIntegrationTests|FullyQualifiedName~GameConfigBootstrapTests|FullyQualifiedName~JsonSerializerTests"`
  - 结果：成功；`Passed: 19`、`Failed: 0`

## 下一步建议

1. 提交当前 `GFramework.Game.Tests` 小热点批次与 `RP-055` tracking 更新，继续保持只纳入本 topic 相关文件
2. 下一轮若继续 warning reduction，应优先决定是否接受进入 `YamlConfigLoaderTests.cs` 的高上下文批次
