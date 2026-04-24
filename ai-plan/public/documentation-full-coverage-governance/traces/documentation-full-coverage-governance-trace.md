# Documentation Full Coverage Governance Trace

## 2026-04-24

### 当前恢复点：RP-029

- 以 `origin/main`（`a8447a6`，`2026-04-24T12:53:39+08:00`）为 `$gframework-batch-boot 75` baseline，确认 `RP-028` 提交后的当前分支累计 diff 为 `29` 个文件。
- 选择“README 与落地页 reader-facing 文档入口对齐”作为本批次低风险切片，集中处理模块 README、仓库根 README、`docs/index.md` 与多组中文落地页中的裸路径标签和 code span 文档入口。
- 本批次修改了 `29` 个 README / docs 页面，并补充了 `2` 个 `ai-plan` 入口更新。

### 当前决策（RP-029）

- 当 branch diff 接近 `58 / 75` 时，继续批量推进的前提应变成“每批只有很小的 write set 且收益明确”；否则优先停在当前恢复点，保留 reviewability。
- README 与 landing page 的 reader-facing 入口应优先显示模块名、栏目名或功能名，而不是直接暴露仓库路径。
- `docs/index.md` 作为语言落地页，即使主要依赖脚本跳转，也应保留明确的 `title` / `description` metadata，避免站点入口缺失基础说明。

### 当前验证（RP-029）

- 站点构建：
  - `bun run build`（工作目录：`docs/`）
  - 结果：通过；模块 README、中文落地页 reader-facing 文档入口对齐，以及 `docs/index.md` metadata 调整后站点仍可正常构建，仅保留既有大 chunk warning。
- 当前 stop-condition metric：
  - 本批次 write set 为 `31` 个文件（`29` 个 README / docs 页面 + `2` 个 `ai-plan` 入口）；本批次提交后分支 diff 为 `58 / 75` 个 changed files。

### 归档指针

- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-through-rp-007.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-rp-023-to-rp-025-2026-04-24.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-rp-023-to-rp-025-2026-04-24.md`

### 下一步

1. 若继续执行 `$gframework-batch-boot 75`，优先选择 `5` 到 `10` 个文件以内的小批次，例如剩余零散的 README 路径引用或单页 reader-facing 标签修正。
2. 推送本批次 commit 后，再次执行 `$gframework-pr-review`，确认 PR `#282` 的 unresolved review threads 是否已在新 head commit 上消失。
