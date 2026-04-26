# Analyzer Warning Reduction 跟踪

## 目标

继续以“直接看构建输出、直接修构建 warning”为原则推进当前分支，并保持 active recovery 文档只保留当前真值。

## 当前恢复点

- 恢复点编号：`ANALYZER-WARNING-REDUCTION-RP-075`
- 当前阶段：`Phase 75`
- 当前焦点：
  - `2026-04-27` 按 `$gframework-batch-boot 50` 完成第一轮并行 warning 清理集成，当前先收口已验证改动，再进入下一轮低风险 slice
  - 当前轮次已重新确认 `origin/main` 基线与 `HEAD` 同为 `617e0bf`，已提交 branch diff 现为 `12 / 50` files、`192` changed lines
  - 当前主线程已整合 4 个 subagent 的首轮结果，并补修 `SaveRepository.cs` / `SceneRouterBase.cs` 的 touched-file 残留 warning；下一轮继续优先处理单文件、低耦合 warning

## 当前活跃事实

- 当前 `origin/main` 基线提交为 `617e0bf`（`2026-04-26T12:17:15+08:00`）。
- 提权后的直接仓库根验证当前确认为：
  - `dotnet clean`
    - 结果：成功；此前沙箱内缺失 Windows fallback package folder 的 clean 失败属于环境噪音，不是仓库真值
  - `dotnet build`
    - 最新结果：成功；`430 Warning(s)`、`0 Error(s)`
  - `dotnet build GFramework.sln -c Release`
    - 最新结果：成功；`147 Warning(s)`、`0 Error(s)`
- 当前分支 stop-condition 指标（已提交 `HEAD`）：
  - `git diff --name-only refs/remotes/origin/main...HEAD | wc -l`
    - 最新结果：`12`
  - `git diff --numstat refs/remotes/origin/main...HEAD`
    - 最新结果：`192` changed lines
- 当前批次已完成的 warning slice：
  - `GFramework.Core` 事件 / 状态 / 属性 / 协程统计中的 `MA0158` 专用锁迁移
  - `GFramework.Game/Data` 中 `DataRepository`、`UnifiedSettingsDataRepository`、`SaveRepository` 的 `ConfigureAwait` / 比较器 / 专用锁修正
  - `GFramework.Game/Scene/SceneRouterBase.cs` 与 `GFramework.Game/UI/UiRouterBase.cs` 中的显式上下文 / 参数名 / 比较器修正
- 当前批次待提交的集成改动：
  - `GFramework.Core` / `GFramework.Cqrs` 第二组 `MA0158` 专用锁迁移
  - `ai-plan/public/analyzer-warning-reduction/**` 的恢复点同步
- 当前批次验证结果：
  - `dotnet clean`
    - 最新主线程结果：提权直接执行成功，确认为当前权威 clean 基线
  - `dotnet build`
    - 最新主线程结果：提权直接构建成功；`430 Warning(s)`、`0 Error(s)`
  - `dotnet build GFramework.sln -c Release`
    - 最新主线程结果：提权直接构建成功；`147 Warning(s)`、`0 Error(s)`

## 当前风险

- `GFramework.Game/Config/YamlConfigSchemaValidator*.cs` 仍然聚集多类高耦合 warning。
  - 缓解措施：本轮先避开该热点，只清理低风险且 ownership 清晰的文件集合。
- `MA0158` 迁移涉及 `net8.0` / `net9.0` / `net10.0` 多目标兼容。
  - 缓解措施：复用 `StoreSelection.cs` 已存在的 `#if NET9_0_OR_GREATER` 专用锁模式，不在 `net8.0` 引入不兼容 API。
- 本轮会并行使用多个 subagent，存在交叉修改风险。
  - 缓解措施：每个 worker 仅拥有互不重叠的文件集合，并要求保留其他 agent 的并发更改。

## 活跃文档

- 当前轮次归档：
  - [analyzer-warning-reduction-history-rp042-rp048.md](../archive/todos/analyzer-warning-reduction-history-rp042-rp048.md)
- 历史跟踪归档：
  - [analyzer-warning-reduction-history-rp001.md](../archive/todos/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/todos/analyzer-warning-reduction-history-rp002-rp041.md)
- 历史 trace 归档：
  - [analyzer-warning-reduction-history-rp062-rp071.md](../archive/traces/analyzer-warning-reduction-history-rp062-rp071.md)
  - [analyzer-warning-reduction-history-rp001.md](../archive/traces/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/traces/analyzer-warning-reduction-history-rp002-rp041.md)
  - [analyzer-warning-reduction-history-rp042-rp048.md](../archive/traces/analyzer-warning-reduction-history-rp042-rp048.md)

## 验证说明

- 权威验证结果统一维护在“当前活跃事实”和“当前批次验证结果”。
- 后续若刷新构建或 PR review 真值，只更新上述权威区块，不在本节重复抄录。

## 下一步建议

1. 提交当前工作树里第二组锁迁移、`SaveRepository` / `SceneRouterBase` 补修与 `ai-plan` 同步，避免把两个批次混在同一组未提交改动里。
2. 提交后重新计算 branch diff；若仍明显低于 `$gframework-batch-boot 50`，继续下发 2-3 个 `worker` subagent，优先处理 `SettingsModel.cs`、`RouterBase.cs`、`UiInteractionProfiles.cs` 等低风险单文件 warning。
3. 若后续 branch diff 接近阈值，或剩余候选 slice 只剩 `YamlConfigSchemaValidator*` 这类高耦合热点，则在新的恢复点收口并等待下一轮。
