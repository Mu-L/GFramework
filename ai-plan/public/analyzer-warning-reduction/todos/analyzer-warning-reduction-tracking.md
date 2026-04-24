# Analyzer Warning Reduction 跟踪

## 目标

继续以“直接看构建输出、直接修构建 warning”为原则推进当前分支，并保持 active recovery 文档只保留当前真值。

## 当前恢复点

- 恢复点编号：`ANALYZER-WARNING-REDUCTION-RP-049`
- 当前阶段：`Phase 49`
- 当前焦点：
  - 默认 warning 检查入口已统一为仓库根目录直接执行 `dotnet build`
  - `2026-04-24` 最新一次 plain `dotnet build` 结果为 `Build succeeded.`、`0 Warning(s)`、`0 Error(s)`
  - 当前分支仍为 `fix/analyzer-warning-reduction-batch`，最近相关提交包括 `77e332f` 与 `a98d1cb`
  - 当前工作树除未跟踪的 `.codex` 目录外无活动代码修改

## 当前活跃事实

- 需要修复的对象是 plain `dotnet build` 实际打印出来的 warning，而不是不同 logger / 参数组合下的命令行为差异
- 截至当前恢复点，默认 solution 构建入口没有打印 warning，因此没有可立即切分的 warning-fix 代码切片
- `UnifiedSettingsFile`、`UnifiedSettingsDataRepository`、`LocalizationMap` 与 `CqrsHandlerRegistryGeneratorTests` 的上一轮 warning-reduction 修改已经提交在当前分支历史中

## 当前风险

- active 文档此前过度记录了 batch 停点、构建参数与旧 baseline 细节，容易把恢复重点带偏到“如何检查 warning”而不是“修 warning 本身”
  - 缓解措施：active 文档只保留 plain `dotnet build` 的最新结果与下一步动作，把被替换的细节移入 archive
- 如果后续代码修改重新引入 warning，但没有先从 plain `dotnet build` 输出确认，就容易再次偏离当前分支目标
  - 缓解措施：后续每一轮都先跑 plain `dotnet build`，再按实际打印的 warning 逐项处理

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

- `dotnet build`
  - 结果：成功；`0 Warning(s)`、`0 Error(s)`、`Time Elapsed 00:00:14.97`

## 下一步建议

1. 后续继续当前分支目标时，先跑 plain `dotnet build`，只处理它实际打印出来的 warning
2. 如果下一轮 plain `dotnet build` 仍然保持 `0 Warning(s)`，则当前分支的 build-warning 目标可视为已完成
