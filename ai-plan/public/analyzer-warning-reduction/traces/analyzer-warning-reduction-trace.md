# Analyzer Warning Reduction 追踪

## 2026-04-24 — RP-051

### 阶段：`GFramework.Godot.SourceGenerators.Tests` warning 清零

- 触发背景：
  - 用户要求直接运行 `dotnet clean`，不再添加额外 shell 包装；solution-level `dotnet clean` 仍然在 `ValidateSolutionConfiguration` 阶段失败
  - 直接执行仓库根目录 `dotnet build` 成功，并输出 `1184 warning(s)`，说明当前真实热点已从 `GFramework.Godot.SourceGenerators` 转移到对应测试项目
- 主线程实施：
  - 以 `GFramework.Godot.SourceGenerators.Tests` 为独立批次，先确认该项目本地基线为 `24 warning(s)`
  - 在 `BindNodeSignalGeneratorTests.cs`、`AutoSceneGeneratorTests.cs`、`AutoUiPageGeneratorTests.cs`、`GetNodeGeneratorTests.cs`、`AutoRegisterExportedCollectionsGeneratorTests.cs`、`GodotProjectMetadataGeneratorTests.cs` 中抽取共享 source / diagnostic helper，压缩重复长方法
  - 在 `Core/GeneratorTest.cs` 中补充 `ConfigureAwait(false)`，清除项目内唯一 `MA0004`
  - 把 `GFramework.Godot.SourceGenerators.Tests` 项目 warning 从 `24` 降到 `0`
- 验证里程碑：
  - `dotnet build`
    - 结果：成功；`1184 Warning(s)`、`0 Error(s)`
  - `dotnet build GFramework.Godot.SourceGenerators.Tests/GFramework.Godot.SourceGenerators.Tests.csproj`
    - 初始结果：成功；`24 Warning(s)`、`0 Error(s)`
    - 第一批（`BindNodeSignal` + `GeneratorTest`）后：`16 Warning(s)`
    - 第二批（`AutoScene` / `AutoUiPage` / `GetNode`）后：`8 Warning(s)`
    - 第三批（`Registration` / `Project`）后：`1 Warning(s)`
    - 收尾修复后：成功；`0 Warning(s)`、`0 Error(s)`
  - `dotnet build GFramework.Godot.SourceGenerators.Tests/GFramework.Godot.SourceGenerators.Tests.csproj -c Release`
    - 结果：成功；`0 Warning(s)`、`0 Error(s)`
  - `dotnet test GFramework.Godot.SourceGenerators.Tests/GFramework.Godot.SourceGenerators.Tests.csproj -c Release --no-build`
    - 结果：成功；`Passed: 48`、`Failed: 0`
- 当前结论：
  - `GFramework.Godot.SourceGenerators.Tests` 已在 `Debug` / `Release` 构建下达到 `0 warning(s)`
  - 按 `origin/main` merge-base 计算并只纳入当前暂存批次时，累计分支 diff 为 `23` 个文件，低于 `$gframework-batch-boot 75` 的主停止阈值
  - 仓库根目录 `dotnet clean` 仍无法稳定产出新的 clean 基线，需要在下一轮单独排查
  - 当前 worktree 已有与本批次无关的既有改动；提交时必须只暂存 analyzer warning reduction 相关文件

## 2026-04-24 — RP-050

### 阶段：clean-build 基线修正与 `GFramework.Godot.SourceGenerators` 切片清零

- 触发背景：
  - 用户确认之前的 `0 Warning(s)` 来自增量构建假阴性；只有先 `dotnet clean` 再 `dotnet build`，warning 才会重新出现
  - 用户给出 clean solution build 的真实结果：`Build succeeded with 1193 warning(s)`
- 主线程实施：
  - 纠正当前 topic 的 active todo / trace，把 clean build 作为新的 warning 检查真值
  - 在 `BindNodeSignalGenerator.cs`、`GetNodeGenerator.cs`、`GodotProjectMetadataGenerator.cs` 中完成分阶段方法抽取与字符串比较修正
  - 在 `Registration/AutoRegisterExportedCollectionsGenerator.cs` 中拆分 `TryCreateRegistration`，清除最后一个 `MA0051`
  - 更新 `AGENTS.md`，明确 warning 检查必须先 `dotnet clean` 再 `dotnet build`
- 验证里程碑：
  - `dotnet clean GFramework.Godot.SourceGenerators/GFramework.Godot.SourceGenerators.csproj -c Release`
    - 结果：成功；`0 Warning(s)`、`0 Error(s)`
  - `dotnet build GFramework.Godot.SourceGenerators/GFramework.Godot.SourceGenerators.csproj -c Release`
    - 首次验证：成功；`1 Warning(s)`，剩余 `Registration/AutoRegisterExportedCollectionsGenerator.cs(182,25)` `MA0051`
    - 修复后复验：成功；`0 Warning(s)`、`0 Error(s)`
- 当前结论：
  - `GFramework.Godot.SourceGenerators` 已在 clean `Release` build 下从 9 个 warning 降到 0 个 warning
  - 整仓库 warning 基线仍以用户确认的 clean solution build `1193 warning(s)` 为准
  - 下一轮应继续从 clean solution build 输出中选择新的低风险热点

## Archive Context

- 当前轮次归档：
  - [analyzer-warning-reduction-history-rp042-rp048.md](../archive/todos/analyzer-warning-reduction-history-rp042-rp048.md)
  - [analyzer-warning-reduction-history-rp042-rp048.md](../archive/traces/analyzer-warning-reduction-history-rp042-rp048.md)
- 历史跟踪归档：
  - [analyzer-warning-reduction-history-rp001.md](../archive/todos/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/todos/analyzer-warning-reduction-history-rp002-rp041.md)
- 历史 trace 归档：
  - [analyzer-warning-reduction-history-rp001.md](../archive/traces/analyzer-warning-reduction-history-rp001.md)
  - [analyzer-warning-reduction-history-rp002-rp041.md](../archive/traces/analyzer-warning-reduction-history-rp002-rp041.md)
