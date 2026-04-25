# Analyzer Warning Reduction 跟踪

## 目标

继续以“直接看构建输出、直接修构建 warning”为原则推进当前分支，并保持 active recovery 文档只保留当前真值。

## 当前恢复点

- 恢复点编号：`ANALYZER-WARNING-REDUCTION-RP-071`
- 当前阶段：`Phase 71`
- 当前焦点：
  - `2026-04-25` 主线程再次按 `$gframework-pr-review` 复核当前分支 PR `#291`，确认 latest-head 仅剩 1 条 open review thread，指向 active todo 中已过时的 `.codex` 风险描述
  - 当前批次只同步 active todo/trace 到 `chore(git)` 之后的新真值：`.codex` 已被 `.gitignore` 排除，`.gitignore` 也应进入“已提交的低风险批次文件”清单
  - `dotnet clean` + `dotnet build` 的直接仓库根基线仍为 `639 Warning(s)`、`0 Error(s)`，因此本轮属于文档真值收口，而不是新的 warning 清理批次
  - CodeRabbit 剩余的 `VersionedMigrationRunner.cs` 上下文一致性建议与 active trace 归档建议仍属 non-blocking nitpick，本轮不扩大写集去吸收可选整理

## 当前活跃事实

- 当前 `origin/main` 基线提交为 `4ad880c`（`2026-04-25T14:35:38+08:00`）。
- 提权后的直接仓库根验证当前确认为：
  - `dotnet clean`
    - 结果：成功；此前沙箱内 “Build FAILED but 0 errors” 的 clean 结果不是仓库真值
  - `dotnet build`
    - 最新结果：成功；`639 Warning(s)`、`0 Error(s)`
- 已提交的低风险批次文件：
  - `AGENTS.md`
  - `.gitignore`
  - `GFramework.Core.Tests/Logging/LogContextTests.cs`
  - `GFramework.Core.Tests/Logging/LoggerTests.cs`
  - `GFramework.Cqrs.Tests/Cqrs/CqrsHandlerRegistrarTests.cs`
  - `GFramework.Cqrs.Tests/Logging/TestLogger.cs`
  - `GFramework.Cqrs.Tests/Mediator/MediatorAdvancedFeaturesTests.cs`
  - `GFramework.Ecs.Arch.Tests/Ecs/EcsAdvancedTests.cs`
  - `GFramework.Game.Tests/Config/GameConfigBootstrapTests.cs`
  - `GFramework.Game.Tests/Config/GeneratedConfigConsumerIntegrationTests.cs`
  - `GFramework.Game.Tests/Config/YamlConfigLoaderTests.cs`
  - `GFramework.Game.Tests/Config/YamlConfigTextValidatorTests.cs`
  - `GFramework.Game/Internal/VersionedMigrationRunner.cs`
  - `ai-plan/public/analyzer-warning-reduction/todos/analyzer-warning-reduction-tracking.md`
  - `ai-plan/public/analyzer-warning-reduction/traces/analyzer-warning-reduction-trace.md`
- 当前批次验证结果：
  - `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/current-pr-review.json`
    - 最新主线程结果：成功；确认 PR `#291` latest-head open review threads 为 `1`，唯一仍成立项为 active todo 中过时的 `.codex` 风险描述
  - `dotnet build`
    - 最新主线程结果：成功；`0 Warning(s)`、`0 Error(s)`；该次为增量 Debug 构建，只作为完成校验，warning 权威基线仍以 `dotnet clean` 后的 `639 Warning(s)` 为准
  - `dotnet build GFramework.Game/GFramework.Game.csproj -c Release`
    - 最新主线程结果：成功；`326 Warning(s)`、`0 Error(s)`
  - `dotnet build GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release`
    - 最新主线程结果：成功；`149 Warning(s)`、`0 Error(s)`
  - `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
    - 上一轮主线程结果：成功；`0 Warning(s)`、`0 Error(s)`

## 当前风险

- active `ai-plan` 之外的历史归档仍保留一部分沙箱内 workaround / 假阻塞记录，且 active trace 中 RP-062 ~ RP-064 的详细历史尚未进一步归档。
  - 缓解措施：本轮已把 `.codex` 风险从 active todo 中收口；后续如单独处理 trace 轻量化，可把该 nitpick 作为独立文档提交。
- `GFramework.Core`、`GFramework.Game`、`GFramework.Core.Tests`、`GFramework.Cqrs.Tests` 仍有较大 warning 基线。
  - 缓解措施：后续批次继续优先挑低风险、少文件、可独立验证的测试与局部逻辑切片。

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

- `dotnet clean`
  - 当前结果：成功；在提权后的直接 shell 中可正常完成仓库根 clean
- `dotnet build`
  - 当前结果：成功；最近一次增量 Debug 构建为 `0 Warning(s)`、`0 Error(s)`，但 warning 权威基线仍以提权后的 `dotnet clean` + `dotnet build` 结果 `639 Warning(s)`、`0 Error(s)` 为准
- `dotnet build GFramework.Game/GFramework.Game.csproj -c Release`
  - 当前结果：成功；`326 Warning(s)`、`0 Error(s)`
- `dotnet build GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release`
  - 当前结果：成功；`149 Warning(s)`、`0 Error(s)`
- `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
  - 当前结果：成功；`0 Warning(s)`、`0 Error(s)`

## 下一步建议

1. 推送包含本轮 active todo/trace 同步的提交后，重新执行 `$gframework-pr-review`，确认 PR `#291` 的最后一条 latest-head open thread 是否已自动收口。
2. 若 PR `#291` 仍只剩 nitpick，继续以当前 `639 Warning(s)` 根基线为恢复点，按 `$gframework-batch-boot 50` 规则挑选下一个 1-3 文件的低风险热点。
3. 后续如需处理文档 nitpick，优先把 active trace 中 RP-062 ~ RP-064 的详细历史归档出默认恢复入口，而不是与 warning 收敛批次混做。
