# Analyzer Warning Reduction 追踪

## 2026-04-27 — RP-081

### 阶段：核实 PR `#295` 的剩余 nitpick，并补齐脚本解析回归测试

- 触发背景：
  - 用户再次执行 `$gframework-pr-review`，需要根据当前 PR `#295` 的 latest-head review 继续核实哪些反馈仍需在本地处理
  - 远端 review 显示 `1` 条 open thread 与 `2` 条 nitpick，需要区分 stale finding 与仍然成立的本地问题
- 主线程实施：
  - 复核 `/tmp/current-pr-review.json` 与本地 `AsyncExtensionsTests.cs`，确认 open thread 指向的 `nameof(parameterName)` 问题已在现有代码中修复，属于 stale finding
  - 删除 `GFramework.Core.Tests/Extensions/AsyncExtensionsTests.cs` 中 `WithRetry_Should_Respect_ShouldRetry_Predicate` 的冗余 `Task.Delay(50)`，将测试改回同步断言路径
  - 调整 `.agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py` 的 `parse_failed_test_details`，允许 failed-test HTML 表格在 `Name` / `Failure Message` 后追加额外列
  - 新增 `.agents/skills/gframework-pr-review/scripts/test_fetch_current_pr_review.py`，以 `unittest` 覆盖“尾随额外列不影响前两列提取”的回归场景
- 验证里程碑：
  - `python3 .agents/skills/gframework-pr-review/scripts/test_fetch_current_pr_review.py`
    - 结果：成功；`Ran 1 test in 0.000s`, `OK`
  - `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --section tests --json-output /tmp/current-pr-review-postfix.json`
    - 结果：成功；真实 PR 评论抓取仍显示 `2` 份测试报告，失败测试名与 failure message 摘要保持可见
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --filter "FullyQualifiedName~WithRetry_Should_Respect_ShouldRetry_Predicate"`
    - 结果：成功；`Failed: 0, Passed: 1, Skipped: 0, Total: 1`
- 当前结论：
  - 本轮 latest-head review 中只有 `AsyncExtensionsTests` 冗余等待与 failed-test 表格尾随列容错性两个 nitpick 仍与本地代码一致，现已修复
  - `ThrowShouldNotRetry` 的 `ParamName` open thread 属于 stale finding，本地代码已经符合预期，只需等待新提交进入远端后复核 thread 状态

## 活跃风险

- PR 上的 latest-head review thread 与测试报告仍需要等新提交进入远端后再复核。
  - 缓解措施：提交并推送后重新执行 `$gframework-pr-review`，只以新的 latest-head 和 test report 为准。
- `YamlConfigSchemaValidator*`、`YamlConfigLoader.cs` 与 `MA0048` 拆分仍是下一波次的高耦合候选。
  - 缓解措施：保持本轮边界只处理 PR review nitpick follow-up，不顺手扩展 warning reduction 范围。

## 下一步

1. 完成本轮提交。
2. 推送后重新执行 `$gframework-pr-review`，确认 PR `#295` 的 stale open thread 与 nitpick 是否已刷新。

## 历史归档指针

- 最新 trace 归档：
  - [analyzer-warning-reduction-history-rp073-rp078.md](../archive/traces/analyzer-warning-reduction-history-rp073-rp078.md)
  - [analyzer-warning-reduction-history-rp062-rp071.md](../archive/traces/analyzer-warning-reduction-history-rp062-rp071.md)
- 历史 todo 归档：
  - [analyzer-warning-reduction-history-rp074-rp078.md](../archive/todos/analyzer-warning-reduction-history-rp074-rp078.md)
  - [analyzer-warning-reduction-history-rp042-rp048.md](../archive/todos/analyzer-warning-reduction-history-rp042-rp048.md)
- 早期归档：
  - [analyzer-warning-reduction-history-rp001.md](../archive/traces/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/traces/analyzer-warning-reduction-history-rp002-rp041.md)
  - [analyzer-warning-reduction-history-rp042-rp048.md](../archive/traces/analyzer-warning-reduction-history-rp042-rp048.md)
  - [analyzer-warning-reduction-history-rp001.md](../archive/todos/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/todos/analyzer-warning-reduction-history-rp002-rp041.md)
