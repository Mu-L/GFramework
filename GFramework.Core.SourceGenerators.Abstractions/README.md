# GFramework.Core.SourceGenerators.Abstractions

`GFramework.Core.SourceGenerators.Abstractions` 存放 `Core` 侧源码生成器公开使用的 attribute 和最小契约定义。
它不是单独推广的生成器包，而是 `GFramework.Core.SourceGenerators` 的支撑层。

## 目录定位

这里当前主要提供：

- 架构注册相关特性
  - `AutoRegisterModuleAttribute`
  - `RegisterModelAttribute`
  - `RegisterSystemAttribute`
  - `RegisterUtilityAttribute`
- 规则与上下文注入特性
  - `ContextAwareAttribute`
  - `GetModelAttribute`
  - `GetModelsAttribute`
  - `GetSystemAttribute`
  - `GetSystemsAttribute`
  - `GetUtilityAttribute`
  - `GetUtilitiesAttribute`
  - `GetServiceAttribute`
  - `GetServicesAttribute`
  - `GetAllAttribute`
- 其他通用生成器特性
  - `LogAttribute`
  - `PriorityAttribute`
  - `GenerateEnumExtensionsAttribute`

这些类型定义“消费端代码可以写什么 attribute”，而实际生成逻辑和 diagnostics 仍在
`GFramework.Core.SourceGenerators` 中。

## 与相邻模块的关系

- `GFramework.Core.SourceGenerators`
  - 实际读取这里定义的 attribute，并生成代码或 diagnostics。
- `GFramework.SourceGenerators.Common`
  - 为 `Core` 侧生成器提供共享基类和公共约束。

当前目录本身 `IsPackable=false`。对 NuGet 使用者来说，更实际的入口仍然是
`GeWuYou.GFramework.Core.SourceGenerators`；这个 abstractions DLL 会跟随对应 analyzer 包一起交付。

## 什么时候需要读这里

- 你在确认某个 `Core` 侧 attribute 的可用参数和命名
- 你在排查“文档写法”和 attribute 实际公开面是否一致
- 你在扩展 `Core.SourceGenerators`，需要先确认契约层边界

如果你的目标只是开始使用生成器，优先回到 `Core.SourceGenerators` README 和 `source-generators` 栏目。

## 继续阅读

- [Core 源码生成器说明](../GFramework.Core.SourceGenerators/README.md)
- [源码生成器总览](../docs/zh-CN/source-generators/index.md)
- [Context Get 注入生成器](../docs/zh-CN/source-generators/context-get-generator.md)
- [枚举扩展生成器](../docs/zh-CN/source-generators/enum-generator.md)
