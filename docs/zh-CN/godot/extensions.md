---
title: Godot 扩展方法
description: 以当前 GFramework.Godot.Extensions 源码为准，说明路径、Node、signal 和 unregister 扩展的真实成员与边界。
---

# Godot 扩展方法

`GFramework.Godot.Extensions` 当前并不是“覆盖所有 Godot 节点操作”的万能层。按源码看，它实际公开的扩展主要只有四组：

- `GodotPathExtensions`
- `NodeExtensions`
- `SignalFluentExtensions`
- `UnRegisterExtension`

这页的重点是识别这些扩展各自解决什么问题，以及哪些能力属于单一运行时辅助，而不是统一入口。

## 当前公开入口

### `GodotPathExtensions`

这组扩展只负责判断 Godot 虚拟路径前缀：

- `IsUserPath(this string path)`
- `IsResPath(this string path)`
- `IsGodotPath(this string path)`

它们不做文件访问，也不解析目录结构，只是用字符串前缀判断 `user://` 和 `res://`。

```csharp
using GFramework.Godot.Extensions;

if ("user://save.json".IsUserPath())
{
}

if ("res://config/gameplay.yaml".IsGodotPath())
{
}
```

### `NodeExtensions`

`NodeExtensions` 是当前扩展集合里体量最大的部分，但职责仍然比较具体，主要分成下面几类。

#### 生命周期与有效性辅助

- `QueueFreeX(this Node? node)`
- `FreeX(this Node? node)`
- `WaitUntilReadyAsync(this Node node)`
- `WaitUntilReady(this Node node, Action callback)`
- `IsValidNode(this Node? node)`
- `IsInvalidNode(this Node? node)`

这里最容易写偏的地方有两个：

- `QueueFreeX()` / `FreeX()` 会先检查 null、实例是否仍有效、是否已经进入删除队列
- `IsValidNode()` 不只要求实例还活着，还要求节点已经在 `SceneTree` 里；单纯 `new` 出来但还没挂树的节点会返回 `false`

#### 节点访问与装配辅助

- `FindChildX<T>(...)`
- `GetOrCreateNode<T>(...)`
- `AddChildXAsync(...)`
- `GetParentX<T>()`
- `GetRootNodeX()`
- `ForEachChild<T>(...)`
- `OfType<T>()`

这几组方法更偏“少量常用装配动作”，不是完整查询 DSL。

特别是 `GetOrCreateNode<T>(string path)` 的当前实现要注意：

1. 先尝试 `GetNodeOrNull<T>(path)`
2. 如果没找到，就 `new T()`
3. 把新节点直接 `AddChild(...)` 到当前节点
4. 再把 `created.Name = path`

它不会按斜杠路径逐级创建中间节点，所以不要把它当成层级化路径构建器。

#### 输入、暂停与调试辅助

- `SetInputAsHandled()`
- `Paused(bool paused = true)`
- `DisableInput()`
- `EnableInput()`
- `LogNodePath()`
- `PrintTreeX(string indent = "")`
- `SafeCallDeferred(string method)`

这些方法都很薄，基本是在现有 `Viewport` / `SceneTree` / `CallDeferred(...)` 上做便捷包装，没有额外状态机。

### `SignalFluentExtensions`

`SignalFluentExtensions` 只提供一个入口：

- `Signal(this GodotObject @object, StringName signal)`

它把目标对象和 signal 名称包装成 `SignalBuilder`。具体连接语义请看 [Godot 信号系统](./signal.md)。

### `UnRegisterExtension`

`UnRegisterExtension` 当前也只有一个公开方法：

- `UnRegisterWhenNodeExitTree(this IUnRegister unRegister, Node node)`

它做的事情很明确：把 `unRegister.UnRegister` 挂到 `node.TreeExiting` 上。这样框架侧的订阅句柄就能跟 Godot 节点生命周期对齐。

```csharp
IUnRegister subscription = eventBus.Subscribe<SettingsChangedEvent>(OnSettingsChanged);
subscription.UnRegisterWhenNodeExitTree(this);
```

它不会接管普通 Godot signal 的断开逻辑，也不会帮你推断别的释放时机。

## 最小接入路径

### 1. 节点进入树之后再做装配

如果你的节点可能在 `_Ready()` 前就被访问，先用 `WaitUntilReadyAsync()`：

```csharp
using GFramework.Godot.Extensions;
using GFramework.Godot.Extensions.Signal;
using Godot;

public partial class SettingsPanel : Control
{
    public override async void _Ready()
    {
        await this.WaitUntilReadyAsync();

        var applyButton = FindChildX<Button>("ApplyButton");
        applyButton?.Signal(Button.SignalName.Pressed)
                    .To(Callable.From(OnApplyPressed));
    }

    private void OnApplyPressed()
    {
        this.SetInputAsHandled();
    }
}
```

### 2. 框架订阅和节点生命周期一起收尾

当订阅句柄实现了 `IUnRegister`，可以把释放时机绑到节点退出树：

```csharp
public override void _Ready()
{
    IUnRegister subscription = _eventBus.Subscribe<SettingsChangedEvent>(OnSettingsChanged);
    subscription.UnRegisterWhenNodeExitTree(this);
}
```

这比在多个 `_ExitTree()` / `Dispose()` 分支里手写解绑更稳定，也更符合当前扩展的职责边界。

### 3. 只在需要时使用 signal fluent API

`Signal(...)` 属于扩展集合的一部分，但它已经有独立页面。实践上可以这样分工：

- 节点查找、ready 等待、输入处理：`NodeExtensions`
- 动态 signal 绑定：`Signal(...)`
- 框架订阅释放：`UnRegisterWhenNodeExitTree(...)`
- 路径前缀判断：`GodotPathExtensions`

## 当前边界

- 当前 `NodeExtensions` 不提供 `GetNodeX()`、`CreateSignalBuilder()` 这类额外包装 API
- 它不是 router、scene factory、UI factory 或生成器的替代层
- `GetOrCreateNode<T>()` 只会创建一个直接子节点，不会递归补整条路径
- `SafeCallDeferred(...)` 只有在 `IsValidNode()` 为 `true` 时才会调用；节点未入树时不会执行
- `UnRegisterWhenNodeExitTree(...)` 只针对实现了 `IUnRegister` 的框架订阅句柄，不会自动处理 Godot 原生 `Connect(...)`
- 协程辅助扩展在 `GFramework.Godot.Coroutine` 命名空间，不属于这组 `Extensions` 页面要覆盖的核心范围

## 继续阅读

- [Godot 运行时集成](./index.md)
- [Godot 信号系统](./signal.md)
- [Godot 场景系统](./scene.md)
- [Godot UI 系统](./ui.md)
