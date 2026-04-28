# Analyzer Warning Reduction 跟踪

## 目标

继续以“直接看构建输出、直接修构建 warning”为原则推进当前分支，并保持 active recovery 文档只保留当前真值。

## 当前恢复点

- 恢复点编号：`ANALYZER-WARNING-REDUCTION-RP-091`
- 当前阶段：`Phase 91`
- 当前焦点：
  - `2026-04-28` 继续收口 `PR #300` 的剩余 nitpick：已把 `TestArchitectureContext` / `TestArchitectureContextV3` 的重复 `IArchitectureContext` 测试实现抽到共享基类 `TestArchitectureContextBase`
  - 已将 “本地验证后仍然成立的 nitpick 不得默认降级为可选项” 写入 `.agents/skills/gframework-pr-review/SKILL.md`
  - `dotnet format --verify-no-changes` 的 `GFramework.Core.Tests` 既有 `FINALNEWLINE`、`CHARSET`、`WHITESPACE` 基线仍保持独立，不与本轮共享基类重构混提

## 当前活跃事实

- 当前 `origin/main` 基线提交为 `6cc87a9`（`2026-04-27T20:28:50+08:00`）。
- 当前直接验证结果：
  - `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
    - 最新结果：成功；`0 Warning(s)`、`0 Error(s)`
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~ArchitectureServicesTests|FullyQualifiedName~ContextAwareServiceExtensionsTests|FullyQualifiedName~TestArchitectureContextBehaviorTests|FullyQualifiedName~RegistryInitializationHookBaseTests|FullyQualifiedName~ArchitectureContextTests"`
    - 最新结果：成功；`67` 通过、`0` 失败
- 当前批次摘要：
  - 当前工作树包含 `6` 个已修改文件与 `1` 个新增文件，分别位于 `GFramework.Core.Tests`、`.agents/skills/gframework-pr-review/` 与 `ai-plan/public/analyzer-warning-reduction`
  - 本轮没有触碰 `Mediator/*`、`YamlConfigSchemaValidator*` 或 `GFramework.Core.Tests` 的整项目格式基线波次

## 当前风险

- `dotnet format GFramework.Core.Tests/GFramework.Core.Tests.csproj --verify-no-changes` 当前会命中项目内大量历史格式诊断。
  - 缓解措施：本轮只记录为现存基线，不把 `PR #300` 的 review follow-up 扩展成整项目格式清理。
- `GFramework.Game/Config/YamlConfigSchemaValidator*` 仍然是仓库根 warning 热点，但与本轮 review 修复无交集。
  - 缓解措施：继续保持为独立高耦合波次。

## 活跃文档

- 当前轮次归档：
  - [analyzer-warning-reduction-history-rp083-rp088.md](../archive/traces/analyzer-warning-reduction-history-rp083-rp088.md)
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
- `GFramework.Core.Tests` 的当前受影响项目 Release 构建已清零，并通过对应定向测试回归。
- `git diff --check` 结果为空，说明本轮新增改动没有引入新的尾随空格或冲突标记。
- warning reduction 的仓库级真值只以同轮 `dotnet clean` 后的 `dotnet build` 为准。

## 下一步建议

1. 提交本轮 `PR #300` nitpick follow-up、技能规则更新与 `ai-plan` 同步。
2. 推送后重新执行 `$gframework-pr-review`，确认该 nitpick thread 是否随最新 head 自动收口。
3. 若要清理 `dotnet format` 基线，另开 `GFramework.Core.Tests` 格式治理切片，不与当前 PR review 修复混提。
