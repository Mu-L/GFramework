# Input System Godot Integration 跟踪

## 目标

在 `GFramework.Game.Abstractions`、`GFramework.Game` 与 `GFramework.Godot` 之间建立统一输入抽象、默认动作绑定运行时与
Godot `InputMap` 适配，优先服务 UI 语义动作桥接和绑定重映射能力。

## 当前恢复点

- 恢复点编号：`INPUT-GODOT-RP-001`
- 当前阶段：`Phase 1`
- 当前焦点：
  - 已新增 `GFramework.Game.Abstractions.Input` 契约，覆盖动作绑定描述、快照、设备上下文与 UI 输入桥接接口
  - 已新增 `GFramework.Game.Input` 默认运行时，覆盖纯托管绑定存储、设备上下文持有者和逻辑动作到 `UiInputAction` 的桥接
  - 已新增 `GFramework.Godot.Input` 适配层，覆盖 `InputMap` 绑定读写与 descriptor-based backend 桥接
  - 已补 `Game.Tests` 与 `Godot.Tests` 的新增回归，并补 `docs/zh-CN/game/input.md` 与 `docs/zh-CN/godot/input.md`
  - 已处理 PR `#346` 的首轮 review follow-up，修复只读查询污染快照与 Godot 导入快照残留绑定问题，并补齐 README / XML / `ai-plan` 收尾

## 当前状态摘要

- 统一输入抽象已建立，但当前仍聚焦动作绑定和 UI 输入桥接，不尝试覆盖完整 gameplay input runtime
- `GodotInputBindingStore` 当前把 `InputMap` 默认绑定和主绑定替换接到框架抽象，允许导出 / 导入 `InputBindingSnapshot`
- `InputBindingStore.GetBindings(...)` 已改为纯读取语义，不再因查询缺失动作而把空条目带进导出快照
- `GodotInputBindingStore.ImportSnapshot(...)` 已改为快照级覆盖语义，会清空快照中未出现动作的后端绑定
- `GodotInputMapBackend.ResetAction(...)` / `ResetAll()` 已对齐默认快照替换语义，运行时新增动作在全量重置后不会残留在 `InputMap`
- `project.godot -> InputActions` 生成器链路保持不变，新的输入系统直接复用动作名常量，而不是替代它

## 当前风险

- Godot 原生 `InputEvent` 对象在普通 `dotnet test` 宿主中的可测性仍有限
  - 缓解措施：当前 `Godot.Tests` 只覆盖纯托管 backend 桥接语义，原生 `InputMap` 行为由 `GFramework.Godot` Release build 兜底验证
- 当前 `UiInputActionMap` 只内置 `ui_accept` / `ui_cancel` 等最小别名集
  - 缓解措施：后续如需更大动作表，由项目层自定义 `IUiInputActionMap`

## 验证说明

- `dotnet build GFramework.Game.Abstractions/GFramework.Game.Abstractions.csproj -c Release --no-restore -m:1 -nodeReuse:false`
  - 结果：通过
- `dotnet build GFramework.Game/GFramework.Game.csproj -c Release --no-restore -m:1 -nodeReuse:false`
  - 结果：通过
- `dotnet build GFramework.Godot/GFramework.Godot.csproj -c Release --no-restore -m:1 -nodeReuse:false`
  - 结果：通过
- `dotnet test GFramework.Game.Tests/GFramework.Game.Tests.csproj -c Release --filter "FullyQualifiedName~InputBindingStoreTests|FullyQualifiedName~UiInputDispatcherTests" -m:1 -p:RestoreFallbackFolders= -nodeReuse:false`
  - 结果：通过
- `dotnet test GFramework.Godot.Tests/GFramework.Godot.Tests.csproj -c Release --filter "FullyQualifiedName~GodotInputBindingStoreTests" -m:1 -p:RestoreFallbackFolders= -nodeReuse:false`
  - 结果：通过
- `python3 scripts/license-header.py --check --paths GFramework.Game.Abstractions/README.md GFramework.Game.Tests/Input/InputBindingStoreTests.cs GFramework.Game/Input/InputBindingStore.cs GFramework.Game/Input/InputDeviceTracker.cs GFramework.Game/Input/UiInputDispatcher.cs GFramework.Game/README.md GFramework.Godot.Tests/Input/GodotInputBindingStoreTests.cs GFramework.Godot/Input/GodotInputBindingStore.cs GFramework.Godot/Input/GodotInputMapBackend.cs GFramework.Godot/Input/IGodotInputMapBackend.cs ai-plan/public/input-system-godot-integration/traces/input-system-godot-integration-trace.md`
  - 结果：通过（All supported files include an Apache-2.0 license header.）
- `dotnet build GFramework.Game/GFramework.Game.csproj -c Release -m:1 -nodeReuse:false`
  - 结果：通过（0 warning, 0 error）
- `dotnet build GFramework.Godot/GFramework.Godot.csproj -c Release -m:1 -nodeReuse:false`
  - 结果：通过（0 warning, 0 error）

## 下一步

1. 若继续处理 PR review，可再单独评估值对象切换到 `record` 是否值得进入同一个 PR
2. 若继续扩展输入系统，优先补更多逻辑动作与 gameplay 输入场景，而不是先扩面到品牌图标、震动预设或平台文案
3. 若要增强 Godot 宿主覆盖，优先补真实 `InputMap` / `InputEvent` 集成测试宿主，而不是把更多原生对象直接放进普通 `dotnet test`
