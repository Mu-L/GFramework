# Documentation Full Coverage Governance Trace

## 2026-04-23

### 当前恢复点：RP-022

- 按当前使用反馈继续执行 `documentation-full-coverage-governance` 下的 skill 文档治理。
- 本轮目标定义为“在不扩张语义范围的前提下，收口 docs landing / API 导航页中的裸路径仓库入口”。
- 本轮执行的修复：
  - 将 `docs/zh-CN/getting-started/index.md` 中 `Cqrs`、`Game`、`Godot` 的仓库模块入口改为可点击链接
  - 将 `docs/zh-CN/core/index.md`、`game/index.md`、`source-generators/index.md` 的“对应模块入口”统一改为可点击链接
  - 将 `docs/zh-CN/api-reference/index.md` 中根 README 与模块 README 映射改为可点击链接
  - 将 `docs/zh-CN/abstractions/core-abstractions.md` 与 `game-abstractions.md` 的回跳入口改为可点击链接
  - 同步更新 active tracking / trace，记录新的治理切片与恢复点

### 当前决策（RP-022）

- 继续使用 `origin/main` 作为 `$gframework-batch-boot 75` 的固定基线，并以“分支累计 diff 文件数”作为主状态指标。
- 对文档治理类批次，优先选择“导航可达性 / 渲染一致性”这类不改变产品语义的低风险切片。
- 在 docs 页面里出现仓库内 README 路径时，优先使用可点击的相对链接，而不是裸路径代码片段。
- 当 docs 页需要跳转到 `docs/` 外部的 README 时，使用 GitHub `main` 分支 blob 外链，而不是跨出 `docs/` 根目录的相对路径。

### 当前验证（RP-022）

- 导航热点巡检：
  - `rg -n '`GFramework\\.[^`]+/README\\.md`|`docs/zh-CN/[^`]+\\.md`|仓库根 `README\\.md`' docs/zh-CN -g '*.md'`
  - 结果：命中 landing / API 导航页中的裸路径仓库入口，已按本轮批次收口 7 个页面。
- 构建校验：
  - `bun run build`（工作目录：`docs/`）
  - 结果：通过；将仓库 README 跳转改为 GitHub `main` blob 外链后，不再触发 VitePress dead link；仅保留既有大 chunk warning。

### 归档摘要（RP-021）

- 为 `.agents/skills/gframework-batch-boot/SKILL.md` 与 `.agents/skills/README.md` 补齐数字速记 stop condition 语义。
- 明确 `$gframework-batch-boot 75` / `75 2000` 默认绑定 `origin/main` 累计 diff 口径。
- `docs/` 站点构建通过。

### 归档指针

- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-through-rp-007.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-through-rp-016.md`

### 下一步

1. 提交并推送本地修正后，再次抓取 PR `#272`，确认 Greptile open thread 是否已在新 head commit 上消失。
2. 若继续执行文档治理批处理，优先排查 `core/cqrs.md`、`ecs/arch.md`、`game/scene.md`、`game/ui.md`
   与 source generator 专题页中剩余的裸路径 README 入口，而不是扩成跨模块大波次。
