---
title: ECS 系统集成
description: GFramework 当前 ECS 模块族的包边界、采用顺序与源码阅读入口。
---

# ECS 系统集成

GFramework 当前仓库内已经交付并持续维护的 ECS 模块族是 `Ecs.Arch`。它分成运行时实现包
`GFramework.Ecs.Arch` 和契约包 `GFramework.Ecs.Arch.Abstractions`，分别覆盖默认装配能力与共享边界约定。

## 当前模块族

| 包 | 适用场景 | 你会得到什么 | 继续阅读 |
| --- | --- | --- | --- |
| `GFramework.Ecs.Arch` | 需要默认运行时、`UseArch(...)` 装配入口、`World` 注册和系统适配基类 | `ArchEcsModule`、`ArchSystemAdapter<T>`、`ArchExtensions.UseArch(...)`、示例组件与系统 | [Arch ECS 集成](./arch.md) |
| `GFramework.Ecs.Arch.Abstractions` | 只想让共享宿主循环、测试替身或扩展模块依赖最小契约，而不引入默认运行时 | `IArchEcsModule`、`IArchSystemAdapter<T>`、`ArchOptions` 契约对象 | [ECS 抽象层说明](../abstractions/ecs-arch-abstractions.md) |

## 最小采用路径

### 1. 选择包边界

- 需要默认实现时安装 `GeWuYou.GFramework.Ecs.Arch`
- 只需要契约时安装 `GeWuYou.GFramework.Ecs.Arch.Abstractions`

### 2. 在 `Initialize()` 前显式接入运行时

`UseArch(...)` 通过 `ArchitectureModuleRegistry` 注册服务模块。按当前源码与集成测试，它应在架构实例调用
`Initialize()` 之前完成。

```csharp
using Arch.Core;
using GFramework.Core.Architectures;
using GFramework.Ecs.Arch.Abstractions;
using GFramework.Ecs.Arch.Extensions;

public sealed class GameArchitecture : Architecture
{
    public GameArchitecture() : base(new ArchitectureConfiguration())
    {
    }

    protected override void OnInitialize()
    {
        RegisterSystem<MovementSystem>();
    }
}

var architecture = new GameArchitecture()
    .UseArch(options =>
    {
        options.WorldCapacity = 2048;
        options.Priority = 50;
    });

architecture.Initialize();

var world = architecture.Context.GetService<World>();
var ecsModule = architecture.Context.GetService<IArchEcsModule>();
```

### 3. 让 ECS 系统继承 `ArchSystemAdapter<float>`

```csharp
using Arch.Core;
using GFramework.Ecs.Arch;
using GFramework.Ecs.Arch.Components;

public sealed class MovementSystem : ArchSystemAdapter<float>
{
    private QueryDescription _query;

    protected override void OnArchInitialize()
    {
        _query = new QueryDescription()
            .WithAll<Position, Velocity>();
    }

    protected override void OnUpdate(in float deltaTime)
    {
        var frameDelta = deltaTime;

        World.Query(in _query, (ref Position position, ref Velocity velocity) =>
        {
            position.X += velocity.X * frameDelta;
            position.Y += velocity.Y * frameDelta;
        });
    }
}
```

### 4. 由宿主循环驱动更新

`IArchEcsModule` 继承自 `IServiceModule`，负责初始化和销毁；真正的帧更新通过 `Update(float deltaTime)` 显式触发。

```csharp
using GFramework.Ecs.Arch.Abstractions;

public sealed class GameLoop
{
    private readonly IArchEcsModule _ecsModule;

    public GameLoop(IArchEcsModule ecsModule)
    {
        _ecsModule = ecsModule;
    }

    public void Tick(float deltaTime)
    {
        _ecsModule.Update(deltaTime);
    }
}
```

## 阅读顺序

1. 先看本页，确认自己要的是运行时包还是契约包
2. 需要默认实现时继续读[Arch ECS 集成](./arch.md)
3. 只想保留共享边界时继续读[ECS 抽象层说明](../abstractions/ecs-arch-abstractions.md)
4. 统一查阅模块说明、专题页与 XML 入口时回到[API 参考](../api-reference/index.md)

## 源码阅读入口

如果你要从栏目入口页回到源码和 XML 文档，建议按下面的入口阅读：

| 包 | 类型族 | 代表类型 | 建议先确认什么 |
| --- | --- | --- | --- |
| `GFramework.Ecs.Arch` | 运行时装配与模块生命周期 | `ArchExtensions`、`ArchEcsModule` | `UseArch(...)` 的接入时机、`World` 注册、模块优先级 |
| `GFramework.Ecs.Arch` | 系统桥接层 | `ArchSystemAdapter<T>` | GFramework `ISystem` 生命周期如何桥接到 Arch `ISystem<T>` |
| `GFramework.Ecs.Arch` | 示例组件与系统 | `Position`、`Velocity`、`MovementSystem` | 查询写法、组件布局和最小可运行示例 |
| `GFramework.Ecs.Arch.Abstractions` | 契约与配置对象 | `IArchEcsModule`、`IArchSystemAdapter<T>`、`ArchOptions` | 共享宿主循环、测试替身和跨程序集配置边界 |

## 模块边界

- 当前仓库面向读者提供的 ECS 采用路径就是 `Ecs.Arch` 模块族；如果你要接入 ECS，可以直接按本页列出的两个包选择。
- `GFramework.Ecs.Arch.Abstractions` 适合共享宿主循环、测试替身或扩展模块，`GFramework.Ecs.Arch` 提供默认装配与运行时实现。
- 需要进一步查看包目录、源码结构或安装说明时，可继续阅读[Arch ECS 集成](./arch.md)、[ECS 抽象层说明](../abstractions/ecs-arch-abstractions.md)和对应模块说明页。
