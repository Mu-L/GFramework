# Input System Godot Integration 追踪

## 2026-05-10

### 阶段：统一输入抽象与 Godot 适配首轮落地（RP-001）

- 创建长分支 `feat/input-system-godot-integration`，并在 `GFramework-WorkTree/GFramework-input-system-godot-integration`
  建立独立 worktree
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
