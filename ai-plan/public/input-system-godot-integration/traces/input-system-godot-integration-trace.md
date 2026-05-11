# Input System Godot Integration 追踪

## 2026-05-10

### 阶段：统一输入抽象与 Godot 适配首轮落地（RP-001）

- 创建长分支 `feat/input-system-godot-integration`，并在 `feat/input-system-godot-integration#346`
  上推进独立实现与验证
- 在 `GFramework.Game.Abstractions/Input/` 新增：
  - `InputBindingDescriptor`
  - `InputActionBinding`
  - `InputBindingSnapshot`
  - `InputDeviceContext`
  - `IInputBindingStore`
  - `IInputDeviceTracker`
  - `IUiInputActionMap`
  - `IUiInputDispatcher`
- 在 `GFramework.Game/Input/` 新增：
  - `InputBindingStore`
  - `InputDeviceTracker`
  - `UiInputActionMap`
  - `UiInputDispatcher`
- 在 `GFramework.Godot/Input/` 新增：
  - `GodotInputBindingCodec`
  - `IGodotInputMapBackend`
  - `GodotInputMapBackend`
  - `GodotInputBindingStore`
- 关键设计决策：
  - 保留字符串动作名，直接复用 `InputActions.*` 常量
  - 抽象层只暴露 descriptor / snapshot，不暴露 Godot `InputEvent`
  - Godot backend 改成 descriptor-based contract，避免测试直接依赖原生 `InputEvent` 实例
  - `SetPrimaryBinding(...)` 改为按完整快照回写后端，以保留冲突交换语义
- 新增测试：
  - `GFramework.Game.Tests/Input/InputBindingStoreTests.cs`
  - `GFramework.Game.Tests/Input/UiInputDispatcherTests.cs`
  - `GFramework.Godot.Tests/Input/GodotInputBindingStoreTests.cs`
- 文档更新：
  - 新增 `docs/zh-CN/game/input.md`
  - 新增 `docs/zh-CN/godot/input.md`
  - 更新 `docs/zh-CN/game/index.md`
  - 更新 `docs/zh-CN/godot/index.md`
  - 更新 `docs/zh-CN/tutorials/godot-integration.md`
  - 更新 `GFramework.Game.Abstractions/README.md`
  - 更新 `GFramework.Game/README.md`
  - 更新 `GFramework.Godot/README.md`

### 下一步

1. 若继续推进输入系统，优先定义更多逻辑动作与 gameplay 输入桥接，而不是先扩到宿主品牌文案
2. 若要增强 Godot 验证，单独准备真实 `InputMap` / `InputEvent` 集成宿主，而不是依赖普通 VSTest process

## 2026-05-11

### 阶段：PR #346 review follow-up（RP-001）

- 核对当前分支 PR `#346` 的 CodeRabbit review，确认以下问题仍然适用于本地代码：
  - `InputBindingStore.GetBindings(...)` 读取缺失动作时会隐式创建空条目，并污染 `ExportSnapshot()`
  - `GodotInputBindingStore.ImportSnapshot(...)` 只覆盖快照内动作，未清空后端残留绑定
  - `InputDeviceTracker` / `UiInputDispatcher` / `IGodotInputMapBackend` 的 XML 文档缺少线程或异常契约
  - `GFramework.Game.Abstractions/README.md` 与 `GFramework.Game/README.md` 缺少输入系统文档入口
  - 公开 trace 中仍包含 worktree 目录名，已改为 `feat/input-system-godot-integration#346`
- 本轮未跟进 `InputBindingDescriptor`、`InputActionBinding`、`InputBindingSnapshot`、`InputDeviceContext` 改成 `record` 的 nitpick
  - 原因：这些建议偏向值语义风格统一，不是当前 PR 中已验证的行为缺陷；本轮优先收敛真实回归风险与契约缺口
- 新增回归测试：
  - `InputBindingStoreTests.GetBindings_WhenActionMissing_Should_NotMutateSnapshot`
  - `GodotInputBindingStoreTests.ImportSnapshot_WhenActionMissingFromSnapshot_Should_ClearBackendBindings`
- 验证结果：
  - `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~InputBindingStoreTests|FullyQualifiedName~UiInputDispatcherTests"` 通过
  - `dotnet test GFramework.Godot.Tests/GFramework.Godot.Tests.csproj -c Release --filter "FullyQualifiedName~GodotInputBindingStoreTests"` 通过

### 下一步

1. 运行针对本次改动文件的 license-header 检查并补录结果
2. 如需继续消化 PR review，再单独评估值对象切换到 `record` 是否值得放进同一个 PR
