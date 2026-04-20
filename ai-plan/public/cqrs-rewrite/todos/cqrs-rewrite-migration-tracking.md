# CQRS 重写迁移跟踪

## 目标

围绕 `GFramework` 当前的双轨 CQRS 现状，继续完成以“去外部依赖、降低反射、收口公开入口”为目标的
CQRS 迁移与收敛。

## 当前恢复点

- 恢复点编号：`CQRS-REWRITE-RP-048`
- 当前阶段：`Phase 8`
- 当前焦点：
  - 当前功能历史已归档，active 跟踪仅保留 `Phase 8` 主线的恢复入口
  - 已完成 generated registry 激活路径收敛：`CqrsHandlerRegistrar` 现优先复用缓存工厂委托，避免重复 `ConstructorInfo.Invoke`
  - 已补充私有无参构造 generated registry 的回归测试，确保兼容现有生成器产物
  - 已补充 pointer 响应类型的 precise runtime type 生成，避免这类 handler 再退回程序集级 reflection fallback
  - 已收紧 function pointer 签名的可直接生成判定，仅在其返回值与参数类型都可安全引用时才走静态注册路径
  - 已为 registrar 的 reflection 注册路径补充 handler-interface 元数据缓存，减少跨容器重复注册时的 `GetInterfaces()` 反射
  - 中期上继续 `Phase 8` 主线：参考 `ai-libs/Mediator`，继续扩大 generator 覆盖，并选择下一个收益明确的 dispatch / invoker 反射收敛点

## 当前状态摘要

- 已完成 `Mediator` 外部依赖移除、CQRS runtime 重建、默认架构接线和显式程序集 handler 注册入口
- 已完成 `GFramework.Cqrs.Abstractions` / `GFramework.Cqrs` 项目骨架与 runtime seam 收敛
- 已完成 handler registry generator 的多轮收敛，当前合法 closed handler contract 已统一收敛到更窄的注册路径
- 已完成一轮公开入口文档与 source-generator 命名空间收口
- 已接入 `$gframework-pr-review`，可直接抓取当前分支对应 PR 的 CodeRabbit 评论、checks 和测试结果

## 当前活跃事实

- `Phase 8` 仍是当前主线，不再回退到 `Phase 7`
- `2026-04-20` 已重新执行 `$gframework-pr-review`：
  - `PR #253` 当前状态为 `CLOSED`
  - latest reviewed commit 仍显示 `1` 条 open thread，但其内容针对的是已过时的 `Phase 7` 恢复建议
  - 当前 active tracking / trace 已统一到 `Phase 8`，因此该 thread 不再作为当前主线阻塞项
- `2026-04-20` 已完成一轮冷启动反射收敛：
  - generated registry 类型首次分析后，会缓存一个可复用的激活工厂，而不是在后续容器注册时重复走 `ConstructorInfo.Invoke`
  - 若运行环境不允许动态方法，仍保留原有的反射激活回退，避免阻塞 generated registry 路径
  - `GFramework.Cqrs.Tests` 已补充“私有无参构造 registry 仍可激活”的回归覆盖
- `2026-04-20` 已完成一轮 generator 覆盖面扩展：
  - `CqrsHandlerRegistryGenerator` 现可为 pointer 类型递归重建 runtime type，并通过 `MakePointerType()` 生成精确 service type
  - function pointer 签名不再默认视为“可直接引用”；只有当返回值与每个参数类型都可从 generated registry 安全引用时，才允许直接生成
  - 含隐藏类型的 function pointer handler 仍会保留原有 fallback / 诊断路径，避免此次覆盖扩展误伤已有回退边界
- `2026-04-20` 已完成一轮 registrar reflection 路径收敛：
  - `CqrsHandlerRegistrar` 现会按 `Type` 弱键缓存已筛选且排序好的 supported handler interface 列表
  - 同一 handler 类型跨容器重复注册时，不再重复执行 `GetInterfaces()` 与支持接口筛选
  - `GFramework.Cqrs.Tests` 已补充 registrar 静态缓存隔离与 supported interface 缓存复用回归
- 当前主线优先级：
  - generator 覆盖面继续扩大
  - dispatch/invoker 反射占比继续下降
  - package / facade / 兼容层继续收口

## 当前风险

- 当前 `dotnet build GFramework.sln -c Release` 在 WSL 环境仍会受顶层 `GFramework.csproj` 的 Windows NuGet fallback 配置影响
- 当前 `GFramework.Cqrs.Tests` 仍直接引用 `GFramework.Core`，说明测试已按模块意图拆分，但 runtime 物理迁移尚未完全切断依赖
- `RegisterMediatorBehavior`、`MediatorCoroutineExtensions` 与 `ContextAwareMediator*Extensions` 仍作为兼容层存在，未来真正移除时仍需单独规划弃用窗口

## 活跃文档

- 模块拆分计划：`ai-plan/migration/CQRS_MODULE_SPLIT_PLAN.md`
- 历史跟踪归档：[cqrs-rewrite-history-through-rp043.md](../archive/todos/cqrs-rewrite-history-through-rp043.md)
- 历史 trace 归档：[cqrs-rewrite-history-through-rp043.md](../archive/traces/cqrs-rewrite-history-through-rp043.md)

## 验证说明

- `RP-043` 之前的详细阶段记录、定向验证命令和阶段性决策均已移入主题内归档
- active 跟踪文件只保留当前恢复点、当前活跃事实、风险和下一步，避免 `boot` 在默认入口中重复扫描 1000+ 行历史 trace
- `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --no-restore -p:RestoreFallbackFolders= -m:1 -nodeReuse:false`
  - 结果：通过
  - 备注：`63/63` 测试通过；当前沙箱限制了 MSBuild named pipe，验证需在提权环境下运行
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --no-restore -p:RestoreFallbackFolders= -m:1 -nodeReuse:false --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests"`
  - 结果：通过
  - 备注：`14/14` 测试通过；本轮覆盖 pointer precise registration 与 function pointer fallback 边界
- `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --no-restore -p:RestoreFallbackFolders= -m:1 -nodeReuse:false --filter "FullyQualifiedName~GFramework.Cqrs.Tests.Cqrs.CqrsHandlerRegistrarTests"`
  - 结果：通过
  - 备注：`10/10` 测试通过；本轮覆盖 registrar 的 supported handler interface 缓存

## 下一步

1. 继续 `Phase 8` 主线，优先再找一个收益明确的 generator 覆盖缺口或 dispatch / invoker 反射收敛点继续推进
2. 若继续文档主线，优先再扫 `docs/zh-CN/api-reference` 与教程入口页，补齐仍过时的 CQRS API / 命名空间表述
3. 若后续再出现新的 PR review 或 review thread 变化，再重新执行 `$gframework-pr-review` 作为独立验证步骤
