# Documentation Full Coverage Governance Trace

## 2026-04-23

### 当前恢复点：RP-021

- 按当前使用反馈继续执行 `documentation-full-coverage-governance` 下的 skill 文档治理。
- 本轮目标定义为“为 `$gframework-batch-boot` 补齐数字速记 stop condition 语义，并消除分支 diff 阈值的歧义”。
- 本轮执行的修复：
  - 为 `.agents/skills/gframework-batch-boot/SKILL.md` 增加 `Shorthand Stop-Condition Syntax`
  - 明确 `$gframework-batch-boot 75` 默认表示“当前分支全部提交相对远程 `origin/main` 接近 75 个分支 diff 文件时停止”
  - 明确 `$gframework-batch-boot 75 2000` 默认表示“当前分支全部提交相对远程 `origin/main` 接近 75 个文件或 2000 行变更时停止”
  - 明确 `75 | 2000` 只作为可理解的 OR 输入保留，推荐统一归一化为无 `|` 版本
  - 为 `.agents/skills/README.md` 同步补充公开入口示例与速记说明
- 本轮执行的修复：
  - 同步更新 active tracking / trace，记录该 skill 语义收口和新的恢复点

### 当前决策（RP-021）

- 对 `$gframework-batch-boot` 的纯数字速记，默认第一位数字绑定“文件数阈值”，第二位数字绑定“行数阈值”。
- 对 `$gframework-batch-boot` 的纯数字速记，默认比较口径固定为“当前分支全部提交相对远程 `origin/main` 的累计 diff”。
- 多个数字阈值的默认逻辑为 OR，而不是 AND；否则不符合“任一 reviewability 阈值接近上限就停”的批处理目标。
- 为避免 shell 语义干扰，文档与后续回复中应优先使用无 `|` 的规范写法，即 `$gframework-batch-boot 75 2000`。

### 当前验证（RP-021）

- skill 文档巡检：
  - `sed -n '1,260p' .agents/skills/gframework-batch-boot/SKILL.md`
  - `sed -n '1,220p' .agents/skills/README.md`
  - 结果：确认原文缺少数字速记与 OR 语义定义，现已补齐。
- 构建校验：
  - `bun run build`（工作目录：`docs/`）
  - 结果：通过；仅保留既有 VitePress 大 chunk warning，无构建失败。

### 归档摘要（RP-020）

- 对 `README.md`、模块 README 与 `docs/zh-CN` 做 HTML entity 泛型热点清理。
- 修复 `docs/zh-CN/core/functional.md` 与 `docs/zh-CN/tutorials/functional-programming.md` 中剩余的 8 处写法。
- `docs/` 站点构建通过。

### 归档指针

- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-through-rp-007.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-through-rp-016.md`

### 下一步

1. 提交并推送本地修正后，再次抓取 PR `#272`，确认 Greptile open thread 是否已在新 head commit 上消失。
2. 若继续执行文档治理批处理，优先排查下一类低风险渲染 / 链接热点，而不是扩成跨模块大波次。
