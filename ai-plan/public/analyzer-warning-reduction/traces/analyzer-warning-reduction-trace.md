# Analyzer Warning Reduction 追踪

## 2026-04-28 — RP-089

### 阶段：复核 `PR #300` 最新 review 真值并收敛仍然成立的本地差异

- 触发背景：
  - 用户再次执行 `$gframework-pr-review`
  - `fetch_current_pr_review.py --json-output /tmp/current-pr-review.json` 返回 `PR #300`，latest head 仍显示 `6` 条 CodeRabbit open threads；其中 `Task.CompletedTask` 强转与 failed-test 信号已是 stale，本地仍需处理的是 `TestArchitectureContextV3` 事件语义、`RegistryInitializationHookBase.OnPhase` XML 异常文档、`CapturingLoggerFactoryProvider.MinLevel` 同步，以及 active trace 过长问题
- 主线程实施：
  - 将 `TestArchitectureContextV3` 的事件发送/注册/注销统一接入共享 `EventBus`，避免静默 no-op
  - 为 `RegistryInitializationHookBase.OnPhase` 补充 `ArgumentNullException` 异常契约
  - 让 `CapturingLoggerFactoryProvider.MinLevel` 与 `CreateLogger` 共享同一把锁，并新增回归测试覆盖更新后的最小级别行为
  - 将 active trace 的 `RP-083` ~ `RP-088` 迁移到 [analyzer-warning-reduction-history-rp083-rp088.md](../archive/traces/analyzer-warning-reduction-history-rp083-rp088.md)，恢复单一恢复入口
- 验证里程碑：
  - `dotnet build GFramework.Core/GFramework.Core.csproj -c Release`
    - 结果：成功；`0 Warning(s)`、`0 Error(s)`
  - `dotnet build GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release`
    - 结果：成功；`0 Warning(s)`、`0 Error(s)`
  - `dotnet build GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release`
    - 结果：成功；`125 Warning(s)`、`0 Error(s)`；warning 仍全部集中在既有 `Mediator/*` 文件
  - `dotnet test GFramework.Core.Tests/GFramework.Core.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~TestArchitectureContextBehaviorTests|FullyQualifiedName~RegistryInitializationHookBaseTests"`
    - 结果：成功；`11` 通过、`0` 失败
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --no-build --filter "FullyQualifiedName~CqrsHandlerRegistrarTests"`
    - 结果：成功；`12` 通过、`0` 失败

## 活跃风险

- `GFramework.Cqrs.Tests/Mediator/*` 仍保留 `125` 条既有 warning。
  - 缓解措施：保持为下一轮独立 warning reduction 波次，不把当前 PR review follow-up 扩展到 `Mediator/*` 写集。
- GitHub PR 上的 open threads 在本地提交前仍可能显示为未关闭。
  - 缓解措施：以当前工作树和定向验证作为真值，推送后再让 PR 线程重新比对最新 head。

## 下一步

1. 提交本轮 `PR #300` follow-up 与 `ai-plan` 同步。
2. 若继续推进 warning reduction，下一轮单独规划 `Mediator/*` 切片。

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
