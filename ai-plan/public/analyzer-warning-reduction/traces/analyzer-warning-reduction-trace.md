# Analyzer Warning Reduction 追踪

## 2026-04-28 — RP-090

### 阶段：复核 `PR #300` 最新 review 真值并补齐事件 API 回归覆盖

- 触发背景：
  - 用户再次执行 `$gframework-pr-review`
  - `fetch_current_pr_review.py --json-output /tmp/gframework-current-pr-review.json` 返回 `PR #300`，latest head 仍显示 `6` 条 CodeRabbit open threads；本地复核后，`Task.CompletedTask` 强转、`RegisterLifecycleHook` 语义、`TestResourceLoader` 文档与 `PartialGeneratedNotificationHandlerRegistry` XML 契约都已在当前 head 上成立，唯一仍未锁住的是事件 API 回归覆盖
- 主线程实施：
  - 为 `TestArchitectureContext` 与 `TestArchitectureContextV3` 新增共享测试数据源，补齐 `SendEvent` / `RegisterEvent` / `UnRegisterEvent` 的空参数异常契约
  - 新增 `UnRegisterEvent_Should_Stop_Dispatch` 回归测试，防止后续把注销路径退化成 `no-op`
  - 整理 `TestResourceLoader.cs` 的命名空间缩进，避免当前修改继续叠加局部格式噪音
- 验证里程碑：
  - `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
    - 结果：成功；`0 Warning(s)`、`0 Error(s)`
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~TestArchitectureContextBehaviorTests"`
    - 结果：成功；`9` 通过、`0` 失败
  - `dotnet format GFramework.Core.Tests/GFramework.Core.Tests.csproj --verify-no-changes --no-restore`
    - 结果：失败；输出落在 `ObjectExtensionsTests.cs`、多处 `FINALNEWLINE` 与若干 `CHARSET` 基线文件，均不属于本轮写集
  - `git diff --check`
    - 结果：成功；无新增 whitespace / conflict-marker 问题

## 活跃风险

- GitHub PR 上的 open threads 在本地提交前仍可能显示为未关闭。
  - 缓解措施：以当前工作树和定向验证作为真值，推送后再让 PR 线程重新比对最新 head。
- `GFramework.Core.Tests` 项目当前存在独立于本轮改动的 `dotnet format` 基线。
  - 缓解措施：保持为后续单独格式治理切片，不在当前 PR review follow-up 中扩写。

## 下一步

1. 提交本轮 `PR #300` follow-up 与 `ai-plan` 同步。
2. 若继续收口 PR 线程，单独评估是否接受 `TestArchitectureContext` / `TestArchitectureContextV3` 的共享 helper nitpick。

## 历史归档指针

- 最新 trace 归档：
  - [analyzer-warning-reduction-history-rp083-rp088.md](../archive/traces/analyzer-warning-reduction-history-rp083-rp088.md)
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
