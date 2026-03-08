# GFramework.Ecs.Arch

GFramework 的 Arch ECS 集成包，提供开箱即用的 ECS（Entity Component System）支持。

## 特性

- 🎯 **显式集成** - 符合 .NET 生态习惯的显式注册方式
- 🔌 **零依赖** - 不使用时，Core 包无 Arch 依赖
- 🎯 **类型安全** - 完整的类型系统和编译时检查
- ⚡ **高性能** - 基于 Arch ECS 的高性能实现
- 🔧 **易扩展** - 简单的系统适配器模式

## 快速开始

### 1. 安装包

```bash
dotnet add package GeWuYou.GFramework.Ecs.Arch
```

### 2. 注册 ECS 模块

```csharp
// 在架构初始化时添加 Arch ECS 支持
var architecture = new GameArchitecture(config)
    .UseArch();  // 添加 ECS 支持

architecture.Initialize();
```

### 3. 带配置的注册

```csharp
var architecture = new GameArchitecture(config)
    .UseArch(options =>
    {
        options.WorldCapacity = 2000;
        options.EnableStatistics = true;
        options.Priority = 50;
    });

architecture.Initialize();
```

```csharp
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Position(float x, float y)
{
    public float X { get; set; } = x;
    public float Y { get; set; } = y;
}

[StructLayout(LayoutKind.Sequential)]
public struct Velocity(float x, float y)
{
    public float X { get; set; } = x;
    public float Y { get; set; } = y;
}
```

### 5. 创建系统

```csharp
using Arch.Core;
using GFramework.Ecs.Arch;

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
        World.Query(in _query, (ref Position pos, ref Velocity vel) =>
        {
            pos.X += vel.X * deltaTime;
            pos.Y += vel.Y * deltaTime;
        });
    }
}
```

### 6. 注册系统

```csharp
public class MyArchitecture : Architecture
{
    protected override void OnRegisterSystem(IIocContainer container)
    {
        container.Register<MovementSystem>();
    }
}
```

### 7. 创建实体

```csharp
var world = this.GetService<World>();
var entity = world.Create(
    new Position(0, 0),
    new Velocity(1, 1)
);
```

### 8. 更新系统

```csharp
var ecsModule = this.GetService<IArchEcsModule>();
ecsModule.Update(deltaTime);
```

## 配置选项

### 代码配置

```csharp
var architecture = new GameArchitecture(config)
    .UseArch(options =>
    {
        options.WorldCapacity = 2000;
        options.EnableStatistics = true;
        options.Priority = 50;
    });
```

### 配置说明

- `WorldCapacity` - World 初始容量（默认：1000）
- `EnableStatistics` - 是否启用统计信息（默认：false）
- `Priority` - 模块优先级（默认：50）

## 架构说明

### 显式注册模式

本包采用 .NET 生态标准的显式注册模式，基于架构实例：

**优点：**

- ✅ 符合 .NET 生态习惯
- ✅ 显式、可控
- ✅ 易于测试和调试
- ✅ 支持配置
- ✅ 支持链式调用
- ✅ 避免"魔法"行为

**使用方式：**
```csharp
// 在架构初始化时添加
var architecture = new GameArchitecture(config)
    .UseArch();  // 显式注册

architecture.Initialize();
```

详见：[INTEGRATION_PATTERN.md](INTEGRATION_PATTERN.md)

### 系统适配器

`ArchSystemAdapter<T>` 桥接 Arch.System.ISystem<T> 到 GFramework 架构：

- 自动获取 World 实例
- 集成到框架生命周期
- 支持上下文感知（Context-Aware）

### 生命周期

1. **注册阶段** - 模块自动注册到架构
2. **初始化阶段** - 创建 World，初始化系统
3. **运行阶段** - 每帧调用 Update
4. **销毁阶段** - 清理资源，销毁 World

## 示例

完整示例请参考 `GFramework.Ecs.Arch.Tests` 项目。

## 依赖

- GFramework.Core >= 1.0.0
- Arch >= 2.1.0
- Arch.System >= 1.1.0

## 许可证

MIT License
