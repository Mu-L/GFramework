# Documentation Full Coverage Governance Trace

## 2026-04-23

### 当前恢复点：RP-018

- 使用 `$gframework-pr-review` 重新复核当前分支 PR `#272`。
- GitHub latest-head review 当前暴露 1 条新的 Greptile open thread：
  `GFramework.Godot.SourceGenerators/README.md:135` 把示例命名空间写成了不存在的
  `GFramework.Godot.Attribute`。
- 本地核对源码与现有文档后，确认 `[GetNode]` / `[BindNodeSignal]` 应来自
  `GFramework.Godot.SourceGenerators.Abstractions`，该评论成立。
- 本轮执行的修复：
  - 将 `GFramework.Godot.SourceGenerators/README.md` 的示例 using 改为
    `GFramework.Godot.SourceGenerators.Abstractions`
  - 同步更新 active tracking / trace，记录该 PR review follow-up 与新的恢复点

### 当前决策（RP-018）

- PR review 结果以 GitHub latest-head open threads 为准；即便 active tracking 曾记录“无 open thread”，也必须按新抓取结果回写。
- 对 `GFramework.Godot.SourceGenerators/README.md` 这类模块 README，最小代码样例的命名空间必须与源码中的
  `Abstractions` 包保持一致，不能沿用历史别名或猜测命名。
- 当前本地修复完成后，下一次 GitHub 侧复核需要基于新提交/新 head commit，而不是旧的 PR review 快照。

### 当前验证（RP-018）

- PR review 抓取：
  - `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/current-pr-review.json`
  - 结果：通过；PR `#272` 处于 `OPEN`，latest head commit 存在 1 条 Greptile open thread，定位到
    `GFramework.Godot.SourceGenerators/README.md:135` 的错误命名空间引用。
- README 校验：
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh GFramework.Godot.SourceGenerators/README.md`
  - 结果：通过。
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-code-blocks.sh GFramework.Godot.SourceGenerators/README.md`
  - 结果：通过。
- 构建校验：
  - `bun run build`（工作目录：`docs/`）
  - 结果：通过；仅保留既有 VitePress 大 chunk warning，无构建失败。

### 归档摘要（RP-017）

- active recovery artifact 只保留当前恢复点、当前事实、风险、验证结果与下一步；旧阶段细节统一转移到 archive。
- `Game` persistence docs surface 继续以 `data.md`、`storage.md`、`serialization.md`、`setting.md` 作为最小巡检集合。
- `GFramework.Godot.SourceGenerators/README.md` 的生命周期接法说明应直接复用与 tutorial 一致的最小样例，避免 README 与教程再次分叉。

### 归档指针

- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-through-rp-007.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-through-rp-016.md`

### 下一步

1. 提交并推送本地修正后，再次抓取 PR `#272`，确认 Greptile open thread 是否已在新 head commit 上消失。
2. 如果 PR `#272` 的 `Title check` 仍需要处理，到 GitHub 上把标题改成更具体的文档治理描述。
