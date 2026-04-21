# Documentation Governance And Refresh 追踪

## 2026-04-19

### 阶段：local-plan 迁移收口（RP-001）

- 复核当前工作树后确认：worktree 根目录仅剩一个 legacy `local-plan/`，其内容属于文档治理与重写主题的
  durable recovery state，不应继续作为独立根目录入口存在
- 按 `ai-plan` 治理规则建立 `ai-plan/public/documentation-governance-and-refresh/` 主题目录，并补齐：
  - `todos/`
  - `traces/`
  - `archive/todos/`
  - `archive/traces/`
- 将原 `local-plan` 中的详细 tracking / trace 迁入主题内历史归档，并为 active 入口只保留当前恢复点、
  活跃事实、风险与下一步
- 在 `ai-plan/public/README.md` 中建立
  `docs/sdk-update-documentation` -> `documentation-governance-and-refresh` 的 worktree 映射
- 同步更新 `ai-plan-governance` 的 tracking / trace，记录本次迁移已验证当前工作树不再依赖 worktree-root
  `local-plan/`

### Archive Context

- 历史跟踪归档：
  - `ai-plan/public/documentation-governance-and-refresh/archive/todos/documentation-governance-and-refresh-history-through-2026-04-18.md`
- 历史 trace 归档：
  - `ai-plan/public/documentation-governance-and-refresh/archive/traces/documentation-governance-and-refresh-history-through-2026-04-18.md`

### 下一步（RP-001）

1. 后续继续该主题时，只从 `ai-plan/public/documentation-governance-and-refresh/` 进入，不再恢复 `local-plan/`
2. 若 active 入口再次积累多轮已完成且已验证阶段，继续按同一模式迁入该主题自己的 `archive/`

## 2026-04-21

### 阶段：栏目 landing page 收口（RP-002）

- 依据 `ai-plan/public/README.md` 的 worktree 映射恢复 `documentation-governance-and-refresh` 主题，并确认该分支下一步应优先处理 `docs/zh-CN/core/*`、`game/*` 与 `source-generators/*`
- 复核 `docs/zh-CN/core/index.md`、`docs/zh-CN/game/index.md`、`docs/zh-CN/source-generators/index.md` 后确认：这三页仍保留旧版“大而全教程”结构，与当前模块 README、包拆分关系和推荐接入路径明显漂移
- 对照 `GFramework.Core/README.md`、`GFramework.Game/README.md`、`GFramework.Core.SourceGenerators/README.md`、
  `GFramework.Game.SourceGenerators/README.md`、`GFramework.Cqrs.SourceGenerators/README.md` 与
  `GFramework.Godot.SourceGenerators/README.md`，重写三个栏目 landing page，使其回到“模块定位、包关系、最小接入路径、继续阅读”的可信入口形态
- 首次执行 `cd docs && bun run build` 时发现 VitePress 会把跳到 `docs/` 目录外的相对链接判定为 dead link，因此将 landing page 末尾的模块 README 入口改为纯文本路径提示而非站内链接
- 第二次执行 `cd docs && bun run build` 通过，说明当前 landing page 重写没有破坏站点构建

### 当前结论

- 当前默认导航入口已显著收敛，但专题页仍需逐页按源码与测试继续核对
- 后续优先级应从 `core` 专题页开始，再向 `game` 与 `source-generators` 扩展

### 下一步（RP-002）

1. 审核 `docs/zh-CN/core/architecture.md`、`context.md`、`lifecycle.md`、`command.md`、`query.md`、`cqrs.md`
2. 记录每页的失真点、真实 API 名称与应保留的最小示例
3. 完成一轮专题页重写后再次执行 `cd docs && bun run build`

### 补充：2026-04-21 内容引用迁移

- 按当前文档治理主题，继续清理活跃规范与面向读者的内容入口中的旧参考仓库命名
- `AGENTS.md` 已把“secondary evidence source”从特定项目名收口为 `ai-libs/` 下的已验证只读参考实现
- `GFramework.Game/README.md`、`GFramework.Game.Abstractions/README.md` 与
  `docs/zh-CN/game/index.md` 已同步改为 `ai-libs/` 参考表述，并去掉特定参考项目名称与项目内类型名线索
- `documentation-governance-and-refresh` active tracking 已同步把风险缓解中的参考来源更新为
  `ai-libs/` 下已验证参考实现
- 下一次专题页重写时，继续沿用同一表述，不再把特定参考项目名写入新的活跃文档入口

### 补充：2026-04-21 Core 专题页收口（RP-003）

- 复核 `docs/zh-CN/core/architecture.md`、`context.md`、`lifecycle.md`、`command.md`、`query.md` 与 `cqrs.md`
  后确认：这些页面仍大量保留旧 API 叙述，例如 `Init()`、属性式 `CommandBus` / `QueryBus`、旧 `Input`
  赋值式命令/查询示例，以及已移除的 `RegisterMediatorBehavior`
- 对照 `Architecture`、`ArchitectureContext`、`IArchitectureContext`、`ContextAwareBase`、旧
  `AbstractCommand` / `AbstractQuery` 基类和 `GFramework.Cqrs/README.md` 后，重写上述六个页面
- 新版专题页将结构统一为“当前角色、真实公开入口、最小示例、兼容边界、迁移方向”，避免继续复刻旧版大而全教程
- `core/context.md` 已明确把 `GameContext` 收束为兼容回退路径，而不是新代码的推荐接法
- `core/command.md` 与 `core/query.md` 已明确旧体系仍可用，但新功能应优先走 `GFramework.Cqrs`
- `core/cqrs.md` 已与当前 runtime / generator / handler 注册语义对齐，并明确 `RegisterCqrsPipelineBehavior<TBehavior>()`
  是公开入口
- 执行 `cd docs && bun run build` 通过，说明本轮 `core` 专题页重写没有破坏文档站构建

### 下一步（RP-003）

### 补充：2026-04-21 PR review 跟进收口（RP-004）

- 通过 `gframework-pr-review` 复查当前分支 PR 时发现：脚本把同一 head commit 上空 body 的 `APPROVED`
  review 误当成“最新 review body”，导致 `Nitpick comments` 未被结构化提取
- 对照 GitHub API 的 review 列表后，确认真正包含 `Nitpick comments (2)` 的是更早 3 秒提交的
  `COMMENTED` review；因此调整脚本为“保持最新 review 元数据输出不变，但解析时优先选择同一提交上的最新非空
  CodeRabbit review body”
- 根据重新提取的 Nitpick 内容，补齐 `docs/zh-CN/core/index.md` 里 `Godot` 与 `Source Generators`
  栏目的可点击链接
- 顺手修正 active trace 中重复的 `### 下一步` 标题，消除 `MD024/no-duplicate-heading` 告警，避免后续 PR
  review 再次把文档治理入口本身标成噪音

### 验证（RP-004）

- `python3 .codex/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --format json`
- `cd docs && bun run build`

### 下一步（RP-004）

1. 继续处理 `docs/zh-CN/core/events.md`、`property.md`、`state-management.md`、`coroutine.md`、`logging.md`
2. 若 active trace 继续累计多个已完成恢复点，按 `archive/traces/` 粒度归档旧阶段细节
3. 保持 PR review 跟进时优先验证最新未解决线程、非空 CodeRabbit review body 与 MegaLinter 明确告警

### 阶段：Core 剩余高风险专题页核对（RP-005）

- 依据 `documentation-governance-and-refresh` active tracking 的恢复点，继续核对
  `docs/zh-CN/core/events.md`、`property.md`、`state-management.md`、`coroutine.md`、`logging.md`
- 对照 `GFramework.Core/Events/*`、`Property/*`、`Logging/*`、`StateManagement/*`、`Coroutine/*` 以及对应测试后确认：
  - `events.md`、`property.md` 与 `logging.md` 仍带有旧版“大而全 API 列表”写法，与当前公开入口和推荐边界不匹配
  - `state-management.md` 与 `coroutine.md` 已和当前 runtime / 测试语义基本对齐，本轮无需为了统一文风做额外重写
- 重写 `events.md`，使其回到“上下文入口、`EventBus` / `EnhancedEventBus`、优先级传播、局部事件对象、与 Store / CQRS
  的边界”的当前结构
- 重写 `property.md`，使其回到“字段级响应式值、何时继续使用 `BindableProperty<T>`、何时切到 `Store<TState>`”的当前结构，
  并补充 `BindableProperty<T>.Comparer` 按闭合泛型共享的兼容注意点
- 重写 `logging.md`，使其回到“`LoggerFactoryResolver` 默认行为、`ArchitectureConfiguration` 日志 provider 配置、
  `IStructuredLogger` / `LogContext`、provider 替换边界”的当前结构
- 执行 `cd docs && bun run build` 通过，说明本轮 `core` 专题页收口没有破坏文档站构建

### 当前结论（RP-005）

- 本轮计划中的 `core` 剩余高风险页面已完成核对；`state-management` 与 `coroutine` 经复核后可继续保留
- `core` 栏目下一步不再需要围绕这五页反复停留，后续重心应转到 `docs/zh-CN/game/*` 与 `docs/zh-CN/source-generators/*`

### 下一步（RP-005）

1. 继续核对 `docs/zh-CN/game/*`，优先处理仍引用旧安装方式、旧状态系统或旧 UI / Scene 接法的页面
2. 再推进 `docs/zh-CN/source-generators/*`，重点核对生成器 wiring、包关系与最小接入示例
3. 若 active trace 继续累计多个已完成恢复点，按 `archive/traces/` 粒度归档旧阶段细节

### 阶段：Game Scene / UI 专题页收口（RP-006）

- 依据 `documentation-governance-and-refresh` active tracking 的下一步，优先复核 `docs/zh-CN/game/scene.md` 与
  `docs/zh-CN/game/ui.md`
- 对照 `GFramework.Game.Abstractions/Scene/*`、`GFramework.Game.Abstractions/UI/*`、`GFramework.Game/Scene/SceneRouterBase.cs`、
  `GFramework.Game/UI/UiRouterBase.cs`、`GFramework.Game/README.md` 与 `ai-libs/CoreGrid` 参考接法后确认：
  - `scene.md` 仍把场景系统写成框架自带完整注册/装配的一体化方案，没有突出 `ISceneFactory`、`ISceneRoot` 和项目侧
    router 派生类的责任边界
  - `ui.md` 仍按旧教程式结构展开，没有清楚区分 `Page` 栈与 `Overlay/Modal/Toast/Topmost` 层级 UI，也缺少当前
    `UiInteractionProfile`、`TryDispatchUiAction(...)` 与 World 输入阻断语义
- 重写 `scene.md`，使其回到“当前公开入口、场景栈语义、最小接入路径、守卫/过渡处理器扩展点、与旧写法的边界”的结构
- 重写 `ui.md`，使其回到“页面栈与层级 UI 分流、输入仲裁、暂停/阻断语义、最小接入路径、扩展点”的结构
- 新版两页都明确了：factory、root、引擎节点与注册表仍由项目或适配层提供，框架当前提供的是 router 基类与通用编排

### 验证（RP-006）

- `cd docs && bun run build`

### 下一步（RP-006）

1. 继续核对 `docs/zh-CN/source-generators/*`，优先处理仍引用旧初始化方式、旧聚合包名或过时 generator wiring 的页面
2. 重点复核 `priority-generator.md`、`context-aware-generator.md` 与 Godot 相关生成器页面，确认示例仍与当前 runtime /
   generator 入口一致
3. 若 `source-generators` 出现多页连续收口结果，再按恢复点粒度整理 active trace，避免默认入口继续膨胀
