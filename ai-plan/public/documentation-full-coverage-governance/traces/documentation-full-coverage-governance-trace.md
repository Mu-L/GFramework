# Documentation Full Coverage Governance Trace

## 2026-04-25

### 当前恢复点：RP-034

- 本轮按 `$gframework-batch-boot 50` 执行，baseline 固定为 `origin/main`（`984fb21`，`2026-04-25 11:11:56 +08:00`）；开始时 committed branch diff 为 `5 / 50` 个 changed files。
- 已接受 worker A 的 README 切片结果：5 个模块 README 的 reader-facing 链接标签修正已落在提交 `bd5cdb5`（`docs(readme): 优化链接标签`）。
- 主线程补齐了 `docs/zh-CN/core` 下 7 个热点页面与 `docs/zh-CN/tutorials/basic` 下 7 个教程页面的裸 fenced code block opening 语言标记，按内容分别落为 `csharp` 或 `text`。
- 以当前 write set 估算，本轮文档文件与 active tracking / trace 一并提交后，branch diff 预计为 `21 / 50` 个 changed files，仍有后续小批次空间。

### 当前决策（RP-034）

- README 批次只改 reader-facing 可见标签，不改链接目标；复核结果通过后直接接受 worker A 的独立提交，避免主线程重复改写同一组文件。
- 代码块语言标记批次以 opening fence 为唯一修正点，不重写示例内容；目录树、流程图、控制台输出统一标 `text`，可执行或 API 示例标 `csharp`。
- 教程 `01` 到 `07` 当前未发现额外裸 opening fence 之外的高风险文案问题，因此本轮不扩展到结构性重写，保持在低语义风险范围内。

### 当前验证（RP-034）

- README 链接校验：
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh` 逐个验证 5 个目标 README
  - 结果：通过；目标链接有效。
- README 标签复扫：
  - `rg -n '\\[[^\\]]*(README\\.md|\\.md|\\.md/|/zh-CN/[^\\]]*)\\]\\([^)]*\\)' GFramework.Core/README.md GFramework.Core.SourceGenerators/README.md GFramework.Cqrs.SourceGenerators/README.md GFramework.Ecs.Arch/README.md GFramework.Game.SourceGenerators/README.md`
  - 结果：无命中；本轮目标 README 已无可见路径式 / 文件名式标签残留。
- `Core` 校验：
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/core`
  - 结果：通过；`Core` 栏目 frontmatter、链接与代码块校验通过。
- 教程校验：
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/tutorials/basic`
  - 结果：通过；基础教程栏目 frontmatter、链接与代码块校验通过。
- 站点构建：
  - `bun run build`（工作目录：`docs/`）
  - 结果：通过；仅保留既有大 chunk warning。

### 归档指针

- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-through-rp-007.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-rp-023-to-rp-025-2026-04-24.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-rp-023-to-rp-025-2026-04-24.md`

### 下一步

1. 提交当前批次后，复算 `origin/main...HEAD` 的实际 changed-file 数，确认是否与预计的 `21 / 50` 一致。
2. 若继续下一轮 `$gframework-batch-boot 50`，优先重新抓取 `$gframework-pr-review`，再选择新的低风险 reader-facing 文档切片。
