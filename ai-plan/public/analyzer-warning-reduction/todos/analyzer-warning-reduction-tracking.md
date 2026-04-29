# Analyzer Warning Reduction 跟踪

## 目标

继续以“直接看构建输出、直接修构建 warning”为原则推进当前分支，并保持 active recovery 文档只保留当前真值。

## 当前恢复点

- 恢复点编号：`ANALYZER-WARNING-REDUCTION-RP-093`
- 当前阶段：`Phase 93`
- 当前焦点：
  - `2026-04-29` 使用 `$gframework-batch-boot 50` 从 clean build warning 基线继续分批清理 analyzer warnings
  - 已接受三个 worker 的 `GFramework.Cqrs.Tests/Mediator/*` 独立切片，三个 Mediator 测试文件的 warning 已清零
  - 主线程补齐 `YamlConfigSchemaValidator` 运行时正则 timeout 与 ordinal 字符串比较，先收掉低风险 `MA0009` / `MA0006`
  - 当前停止条件为相对 `origin/main` 接近 `50` 个变更文件；本轮尚未接近阈值，下一批可继续处理 `GFramework.Game/Config/YamlConfigSchemaValidator*`

## 当前活跃事实

- 当前 `origin/main` 基线提交为 `0e32dab`（`2026-04-28T17:15:47+08:00`）。
- 当前直接验证结果：
  - `dotnet clean -p:RestoreFallbackFolders= -v:quiet`
    - 最新结果：成功；标准 `dotnet clean` 仍会先命中当前 WSL 环境的 Windows NuGet fallback 目录，已按既有环境口径先执行 `dotnet restore GFramework.sln -p:RestoreFallbackFolders= --disable-parallel` 后清理
  - `dotnet build -p:RestoreFallbackFolders= -clp:WarningsOnly -v:minimal -m:1 -nodeReuse:false`
    - 最新结果：成功；`75` warnings、`0` errors；warning 从本轮基线 `236` 降到 `75`
  - `dotnet build GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release -p:RestoreFallbackFolders= -m:1 -nodeReuse:false -clp:Summary`
    - 最新结果：成功；`0 Warning(s)`、`0 Error(s)`
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --no-build -p:RestoreFallbackFolders= -m:1 -nodeReuse:false --filter "FullyQualifiedName~Mediator"`
    - 最新结果：成功；`45` 通过、`0` 失败
  - `dotnet build GFramework.Game/GFramework.Game.csproj -c Release -p:RestoreFallbackFolders= -m:1 -nodeReuse:false -clp:Summary`
    - 最新结果：成功；`75 Warning(s)`、`0 Error(s)`；剩余均为 `YamlConfigSchemaValidator*` 的 `MA0048` / `MA0051`
  - `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release -p:RestoreFallbackFolders= -m:1 -nodeReuse:false --filter "FullyQualifiedName~YamlConfigLoaderTests|FullyQualifiedName~YamlConfigSchemaValidatorTests"`
    - 最新结果：成功；`80` 通过、`0` 失败
- 当前批次摘要：
  - 当前已提交分支相对 `origin/main...HEAD` 包含 `3` 个变更文件；本次主线程待提交的 `GFramework.Game/Config/YamlConfigSchemaValidator.cs` 与 `ai-plan` 文档会把累计变更推到约 `6` 个文件，低于 `50` 个文件阈值
  - 已完成 worker 切片：
    - `ed269d4`：`MediatorArchitectureIntegrationTests.cs`，清理 `MA0048` / `MA0004` / `MA0016`
    - `121df44`：`MediatorAdvancedFeaturesTests.cs`，清理 `MA0048` / `MA0004` / `MA0015`
    - `9109eec`：`MediatorComprehensiveTests.cs`，清理 `MA0048` / `MA0004` / `MA0016` / `MA0002` / `MA0015`
  - 主线程切片：`YamlConfigSchemaValidator.cs` 正则 timeout 与 ordinal equality，清理 `MA0009` / `MA0006`

## 当前风险

- `GFramework.Game/Config/YamlConfigSchemaValidator*` 仍然是仓库根 warning 热点，剩余 `45` 条 `MA0048` 与 `30` 条 `MA0051`。
  - 缓解措施：下一批优先把 `YamlConfigSchemaValidator.cs` 末尾 schema model 类型拆到独立文件，再评估 `MA0051` 方法拆分。
- 标准 `dotnet clean` 在当前 WSL 环境仍会读取失效的 Windows fallback package folder。
  - 缓解措施：本主题验证继续沿用 `-p:RestoreFallbackFolders=`，必要时先执行 solution restore 刷新 Linux 侧资产。

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
- `GFramework.Cqrs.Tests` 的当前受影响项目 Release 构建已清零，并通过 Mediator 定向测试回归。
- `GFramework.Game` 当前低风险正则 / 字符串比较切片通过 Release 构建与 config 定向测试；剩余 warning 属于拆文件与复杂度拆分。
- `git diff --check` 结果为空，说明本轮新增改动没有引入新的尾随空格或冲突标记。
- warning reduction 的仓库级真值以同轮 `dotnet build`、定向 `dotnet test` 与 `git diff --check` 为准，并与 trace 中的验证里程碑保持一致。

## 下一步建议

1. 提交主线程 `YamlConfigSchemaValidator` 正则安全补丁与本轮 `ai-plan` 同步。
2. 继续下一批 `GFramework.Game/Config/YamlConfigSchemaValidator.cs` 末尾 schema model 类型拆文件，目标清理 `MA0048`。
3. 再评估 `YamlConfigSchemaValidator.ObjectKeywords.cs` 与主 validator 的 `MA0051` 方法拆分，避免单批触碰过多高耦合逻辑。
