# Documentation Full Coverage Governance Trace

## 2026-04-25

### 当前恢复点：RP-033

- 当前批次聚焦 reader-facing 文档口吻治理，目标是清理公开页面中的反问式标题、维护者视角边界说明、产品评审式表述和裸文件名链接标签。
- 以 `origin/main`（`9964962`，`2026-04-24 23:05:53 +0800`）为 `$gframework-batch-boot 75` baseline；当前 committed branch diff 仍为 `0 / 75`，本轮待提交 write set 在写回 active tracking / trace 前为 `36` 个文件。
- 本批次同时把同类约束补进 `AGENTS.md`、`gframework-doc-refresh/SKILL.md` 和 `module-landing` 模板，避免后续刷新再次写回 AI 式公开文案。
- 使用 `$gframework-pr-review` 重新抓取当前 PR `#287` 后，latest head commit `8209d7a29f35d969fca6258b9817da9b33a203a3` 仅剩
  `1` 条 Greptile open thread，定位到 `docs/zh-CN/api-reference/index.md` 的站内入口表格链接标签风格不一致。

### 当前决策（RP-033）

- 公开文档允许说明采用边界、限制和迁移关系，但必须改写成读者采用 guidance，不能继续使用“当前阶段结论”“不建议立即启动”这类维护者记录口吻。
- 公开文档中的链接标签必须直接说明目的地语义，不再使用 `README.md`、`game/index.md`、`../core/cqrs.md` 这类文件名或路径作为可见导航文字。
- 文档治理规则不能只靠本轮改文案；必须同步更新 `AGENTS.md` 与 `gframework-doc-refresh` 的输出约束，让后续批次默认遵守同一套 reader-facing 标准。
- 对 latest-head review 中仍成立的单点问题，优先做最小范围修复，但若同页存在同类 reader-facing 标签缺陷，则允许一并收口，避免 PR 反复产生同类文案评论。

### 当前验证（RP-033）

- 热点扫描：
  - `rg -n '你真正会用到的公开入口|先理解包关系|这个栏目应该回答什么|什么时候看别的页面|当前阶段的结论|当前不单独启动桌面版的原因|## 边界说明|\\[[^\\]]+\\.md\\]\\([^)]*\\.md\\)|GFramework\\.[A-Za-z.]+ README|对应模块目录下的 \`README\\.md\`' docs/zh-CN README.md GFramework.*/README.md`
  - 结果：通过；当前公开文档已无上述目标模式命中。
- PR review 抓取：
  - `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/current-pr-review.json`
  - 结果：通过；PR `#287` 处于 `OPEN`，无 failed checks，tests `2156 passed`，latest head review 仅剩 `1` 条 Greptile open thread。
- 构建验证：
  - `bun run build`（工作目录：`docs/`）
  - 结果：通过；`docs/zh-CN/api-reference/index.md` 的站内入口标签统一为语义化写法后站点仍可构建，仅保留既有大 chunk warning。
- 技能自检：
  - `python3 .agents/skills/gframework-doc-refresh/scripts/scan_module_evidence.py Core`
  - 结果：通过；`Core` 模块扫描链路正常，新增输出约束未破坏 skill 主流程。
- 规则校验：
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN`
  - 结果：通过；本轮触达页面的 frontmatter、链接与代码块校验均通过，脚本仅继续报告仓库中既有页面的“代码块缺少语言标记”警告。

### 归档指针

- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-through-rp-007.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-rp-023-to-rp-025-2026-04-24.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-rp-023-to-rp-025-2026-04-24.md`

### 下一步

1. 提交并推送当前 PR follow-up 后，重新抓取 `$gframework-pr-review`，确认 PR `#287` 的 latest-head review 是否清空 open thread。
2. 若继续下一轮 `$gframework-batch-boot 75`，优先清理验证脚本持续提示的代码块语言标记缺口，或继续扫描剩余公开页中的维护者口吻尾项。
