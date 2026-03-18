# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GFramework 是面向游戏开发的模块化 C# 框架，核心能力与引擎解耦。灵感参考 QFramework，在模块边界和可扩展性方面持续重构。

## Build & Test Commands

```bash
# 构建整个解决方案
dotnet build GFramework.sln -c Release

# 运行全部测试
dotnet test GFramework.sln -c Release

# 运行单个测试项目
dotnet test GFramework.Core.Tests -c Release
dotnet test GFramework.Game.Tests -c Release
dotnet test GFramework.SourceGenerators.Tests -c Release
dotnet test GFramework.Ecs.Arch.Tests -c Release

# 运行单个测试方法（NUnit filter）
dotnet test GFramework.Core.Tests -c Release --filter "FullyQualifiedName~CommandExecutorTests.Execute"

# 命名规范验证（CI 中使用）
bash scripts/validate-csharp-naming.sh
```

## Module Dependency Graph

```
GFramework (meta package) ─→ Core + Game
GFramework.Core ─→ Core.Abstractions
GFramework.Game ─→ Game.Abstractions, Core, Core.Abstractions
GFramework.Godot ─→ Core, Game, Core.Abstractions, Game.Abstractions
GFramework.Ecs.Arch ─→ Ecs.Arch.Abstractions, Core, Core.Abstractions
GFramework.SourceGenerators ─→ SourceGenerators.Common, SourceGenerators.Abstractions
```

- **Abstractions projects** (netstandard2.1): 只含接口定义，零实现依赖
- **Core/Game** (net8.0;net9.0;net10.0): 平台无关实现
- **Godot**: Godot 引擎集成层
- **SourceGenerators** (netstandard2.1): Roslyn 增量生成器

## Architecture Pattern

框架核心采用 Architecture / Model / System / Utility 四层结构：

- **IArchitecture**: 顶层容器，管理生命周期（Init → Ready → Destroy）、注册 Model/System/Utility
- **IContextAware**: 统一上下文访问接口，所有组件通过 `SetContext(IArchitectureContext)` 获得对 Architecture 服务的引用
- **IModel**: 数据层（状态管理），继承 IContextAware
- **ISystem**: 业务逻辑层，继承 IContextAware
- **IUtility**: 无状态工具层

关键实现类：`GFramework.Core/Architectures/Architecture.cs`（主流程编排）

## Key Patterns

**CQRS**: Command/Query 分离，支持同步与异步。Mediator 模式通过 `Mediator.SourceGenerator` 实现。

**EventBus**: 类型安全事件总线，支持优先级、过滤器、弱引用订阅。`IEventBus.Send<T>()` / `Register<T>(handler)` →
`IUnRegister`。

**BindableProperty**: 响应式属性绑定，`IBindableProperty<T>.Value` 变更自动触发 `OnValueChanged`。

**Coroutine**: 帧驱动协程系统，`IYieldInstruction` + `CoroutineScheduler`，提供 WaitForSeconds/WaitForEvent/WaitForTask
等指令。

**IoC**: 通过 `MicrosoftDiContainer` 封装 `Microsoft.Extensions.DependencyInjection`。

**Service Modules**: `IServiceModule` 模式用于向 Architecture 注册内置服务（EventBus、CommandExecutor、QueryExecutor 等）。

## Code Conventions

- **命名空间**: `GFramework.{Module}.{Feature}` (PascalCase)，CI 通过 `scripts/validate-csharp-naming.sh` 强制校验
- **ImplicitUsings: disabled** — 所有 using 必须显式声明
- **Nullable: enabled**
- **LangVersion: preview**
- **GenerateDocumentationFile: true** — 公共 API 需要 XML 文档注释
- **Analyzers**: Meziantou.Analyzer 在构建时强制代码规范

## Testing

- **Framework**: NUnit 4.x + Moq
- **测试结构**: 镜像源码目录（如 `Core.Tests/Command/` 对应 `Core/Command/`）
- **基类**: `ArchitectureTestsBase<T>` 提供 Architecture 初始化/销毁模板；`SyncTestArchitecture` /
  `AsyncTestArchitecture` 用于集成测试
- **Target frameworks**: net8.0;net10.0

## Source Generators

四个生成器，均为 Roslyn 增量源码生成器：

- `LoggerGenerator` (`[Log]`): 自动生成 ILogger 字段和日志方法
- `PriorityGenerator` (`[Priority]`): 生成优先级比较实现
- `EnumExtensionsGenerator` (`[GenerateEnumExtensions]`): 枚举扩展方法
- `ContextAwareGenerator` (`[ContextAware]`): 自动实现 IContextAware 接口

测试使用快照验证（Verify + snapshot files）。

## Documentation

VitePress 站点位于 `docs/`，内容为中文 (`docs/zh-CN/`)。修改文档后本地预览：

```bash
cd docs && bun install && bun run dev
```
