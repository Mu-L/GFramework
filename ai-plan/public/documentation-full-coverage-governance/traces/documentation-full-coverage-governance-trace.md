# Documentation Full Coverage Governance Trace

## 2026-04-23

### 当前恢复点：RP-020

- 按 `$gframework-batch-boot` 继续执行 `documentation-full-coverage-governance`。
- 将这轮批处理目标定义为“清理 README / `docs/zh-CN` / 模块 README 中仍会在 inline code 里被字面渲染的 HTML 泛型实体”。
- 基线选择为 `origin/main` `aa879d2`（`2026-04-23T17:51:41+08:00`）；当前分支 `docs/sdk-update-documentation`
  与该基线零差异，适合继续做小批次文档治理。
- 以 `rg -n '`[^`]*&lt;[^`]*`|`[^`]*&gt;[^`]*`' README.md GFramework.* docs/zh-CN -g '*.md'` 扫描后，确认剩余热点只在
  `docs/zh-CN/core/functional.md` 与 `docs/zh-CN/tutorials/functional-programming.md`，共 8 处。
- 本轮执行的修复：
  - 将 `docs/zh-CN/core/functional.md` 中的 `Option&lt;T&gt;`、`Result&lt;T&gt;`、`Nullable&lt;T&gt;` 改为真实泛型写法
  - 将 `docs/zh-CN/tutorials/functional-programming.md` 中的 `Option&lt;T&gt;`、`Result&lt;T&gt;` 改为真实泛型写法
  - 同步更新 active tracking / trace，记录 batch objective、基线和新的恢复点

### 当前决策（RP-020）

- 对 Markdown inline code 中的 C# 泛型示例，必须直接写真实的 `<T>` 语法，不能在反引号内部再写
  `&lt;` / `&gt;`，否则 VitePress 会把 entity 当作字面量展示。
- 当一个渲染热点模式可以用本地正则直接衡量时，优先把该模式收敛为一个小批次并一次性清空命中列表，而不是只修单页。
- 对文档治理批处理，主 stop condition 采用“热点列表耗尽”，次级 stop condition 采用“相对基线的分支 diff 不接近大批次阈值”。

### 当前验证（RP-020）

- 同类模式巡检：
  - `rg -n '`[^`]*&lt;[^`]*`|`[^`]*&gt;[^`]*`' README.md GFramework.* docs/zh-CN -g '*.md'`
  - 结果：命中 `docs/zh-CN/core/functional.md` 与 `docs/zh-CN/tutorials/functional-programming.md` 共 8 处，已全部修正。
- 构建校验：
  - `bun run build`（工作目录：`docs/`）
  - 结果：通过；仅保留既有 VitePress 大 chunk warning，无构建失败。

### 归档摘要（RP-019）

- 使用 `$gframework-pr-review` 重新抓取 PR `#272` 后，定位到 `docs/zh-CN/godot/setting.md` 的 inline code HTML entity 渲染问题。
- 顺手扫描当前 PR 已改动的相邻 Godot 文档，又在 `docs/zh-CN/godot/storage.md` 发现同型问题，并已一起修正。
- `docs/` 站点构建通过。

### 归档指针

- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-through-rp-007.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-through-rp-016.md`

### 下一步

1. 提交并推送本地修正后，再次抓取 PR `#272`，确认 Greptile open thread 是否已在新 head commit 上消失。
2. 若继续执行文档治理批处理，优先排查下一类低风险渲染 / 链接热点，而不是扩成跨模块大波次。
