# Analyzer Warning Reduction 追踪

## 2026-04-24 — RP-042

### 阶段：PR #280 review follow-up 与 active 文档归档压缩

- 启动复核：
  - 使用 `gframework-pr-review` 抓取当前分支 PR #280 的 latest-head review threads、MegaLinter 摘要与测试报告
  - 本地核对后确认 `3` 条 open threads 中仍成立的是 `SchemaConfigGeneratorTests` 的冗余 `global::` 返回类型，以及 active tracking 入口过长的问题
- 决策：
  - 对测试代码仅做最小行为无关修正，避免扩大 `GFramework.SourceGenerators.Tests` 写集
  - 将旧 active tracking / trace 的 `RP-002` 到 `RP-041` 详细历史整体迁入 `archive/`，重新建立精简版恢复入口
- 实施调整：
  - 将 `RunAndCollectGeneratedSources(...)` 的返回类型从 `global::System.Collections.Generic.IReadOnlyDictionary<string, string>` 收口为 `IReadOnlyDictionary<string, string>`
  - 归档旧 tracking 到 `archive/todos/analyzer-warning-reduction-history-rp002-rp041.md`
  - 归档旧 trace 到 `archive/traces/analyzer-warning-reduction-history-rp002-rp041.md`
  - 重建 active tracking / trace，只保留当前恢复点、活跃事实、风险、验证与下一步
- 验证结果：
  - `DOTNET_CLI_HOME=/tmp/dotnet-home dotnet restore GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -p:RestoreFallbackFolders=""`
    - 结果：通过；测试项目资产已摆脱失效的 Windows fallback package folder
  - `DOTNET_CLI_HOME=/tmp/dotnet-home dotnet build GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release -t:Rebuild --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false -p:RestoreFallbackFolders="" -nologo -clp:"Summary;WarningsOnly"`
    - 结果：`10 Warning(s)`，`0 Error(s)`；warning 继续全部落在 `CqrsHandlerRegistryGeneratorTests.cs` 的既有 `MA0051` 热点
- 当前结论：
  - PR #280 当前没有 failed-test 回归信号；本轮主要是收口 latest-head review thread 中仍成立的低风险项
  - active 恢复入口已回到可读规模，后续 `boot` 不必再扫描完整阶段历史
- 下一步建议：
  - 提交后重新抓取 PR #280 review，确认 open threads 是否收敛
  - 若 threads 收敛，则回到 `CqrsHandlerRegistryGeneratorTests.cs` 剩余 `MA0051`，或根据目标改切新的 warning 写集

## Archive Context

- 历史跟踪归档：
  - [analyzer-warning-reduction-history-rp001.md](../archive/todos/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/todos/analyzer-warning-reduction-history-rp002-rp041.md)
- 历史 trace 归档：
  - [analyzer-warning-reduction-history-rp001.md](../archive/traces/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/traces/analyzer-warning-reduction-history-rp002-rp041.md)
