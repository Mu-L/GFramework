# Documentation Full Coverage Governance 跟踪

## 目标

持续治理 `GFramework` 的 README、`docs/zh-CN`、站点导航、XML 文档和 API 参考链路，避免阶段性刷新完成后再次回漂。

- 用源码、测试、`*.csproj` 和必要的 `ai-libs/` 证据校正文档
- 以模块族为单位闭环 README、landing page、专题页、教程入口和 API 参考链路
- 把 XML 文档缺口与 reader-facing 采用路径持续纳入同一主题治理

## 当前恢复点

- 恢复点编号：`DOCUMENTATION-FULL-COVERAGE-GOV-RP-053`
- 当前阶段：`Phase 5 - Governance Maintenance`
- 当前焦点：
  - 处理 PR `#308` latest-head review 中仍成立的 3 条 `CodeRabbit` open threads：继续瘦身 active `ai-plan` 文档，并为 `Schema 配置生成器` 专题补集中式迁移与兼容性说明
- 当前事实：
  - `2026-05-01` 重新抓取 `$gframework-pr-review` 后确认：PR `#308` 处于 `OPEN`，latest reviewed commit 为 `097e97bcd66c89c79b4dafc30e24e8b650c7db63`
  - 当前 latest-head review 只剩 `CodeRabbit` `3` 条 open threads，分别指向本 tracking、active trace 和 `docs/zh-CN/source-generators/schema-config-generator.md`
  - GitHub Test Reporter 汇总为 `2222 passed / 0 failed`
  - `Title check` 仍为 `Inconclusive`，属于 PR 元数据问题，不是仓库文件内可直接修复的阻塞项
  - 本地已完成 review 指向文件的收口，但在变更尚未提交推送前，重新抓取的 PR review 仍会继续显示旧的 latest reviewed commit 与同一批 open threads
- 当前风险：
  - active tracking / trace 若继续保留阶段性细节与逐项验证，会再次偏离“快速恢复入口”的用途
  - `Schema 配置生成器` 页已经说明边界，但迁移步骤、兼容边界和回退方式仍分散在多个段落中

## 当前状态摘要

- `Core`、`Ecs.Arch`、`Cqrs`、`Game`、`Godot` 五个模块族当前都已有 README / landing / topic / API 参考层级的已验证入口。
- `source-generators` 栏目已经补出 `Schema 配置生成器` 专题页，并把 `Game.SourceGenerators` 接回 landing、API 入口与侧栏。
- `Cqrs.SourceGenerators` 的 fallback 精度、`GF_Cqrs_001` 诊断边界与共享支撑层阅读路线，当前已经回收进现有专题页与入口页，而不是继续扩成新的维护者导向页面。
- `RP-049` 到 `RP-052` 的阶段细节、逐命令验证和批次决策已迁入归档；active 文档只保留当前恢复事实、风险、验证结果与下一步。

## 归档指针

- 详细验证历史（`RP-001` 到 `RP-007`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-through-rp-007.md`
- 阶段状态归档（`RP-001` 到 `RP-016`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-through-rp-016.md`
- 阶段状态归档（`RP-023` 到 `RP-025`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-rp-023-to-rp-025-2026-04-24.md`
- 阶段状态归档（`RP-049` 到 `RP-052`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-rp-049-to-rp-052-2026-05-01.md`
- 时间线归档（`RP-001` 到 `RP-016`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-through-rp-016.md`
- 时间线归档（`RP-023` 到 `RP-025`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-rp-023-to-rp-025-2026-04-24.md`
- 时间线归档（`RP-041` 到 `RP-048`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-rp-041-to-rp-048-2026-04-28.md`
- 时间线归档（`RP-049` 到 `RP-052`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-rp-049-to-rp-052-2026-05-01.md`
- 验证历史归档（`RP-041` 到 `RP-048`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-rp-041-to-rp-048-2026-04-28.md`
- 验证历史归档（`RP-049` 到 `RP-052`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-rp-049-to-rp-052-2026-05-01.md`

## 最新验证

- `2026-05-01` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/source-generators/schema-config-generator.md`
  - 结果：通过；新增“迁移与兼容性”小节后，页面的 frontmatter、链接与代码块校验仍然通过。
- `2026-05-01` `bun run build`（工作目录：`docs/`）
  - 结果：通过；active `ai-plan` 瘦身与 schema 专题页更新后站点仍可构建，仅保留既有大 chunk warning。
- `2026-05-01` `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/current-pr-review.json`
  - 结果：通过；PR `#308` 处于 `OPEN`，latest-head review 当前只剩 `3` 条 `CodeRabbit` open threads，测试汇总为 `2222 passed / 0 failed`，`Greptile` / `Gemini Code Assist` 当前无 open thread。
- `2026-05-01` `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/current-pr-review-after-fix.json`
  - 结果：通过；remote latest reviewed commit 仍是 `097e97bcd66c89c79b4dafc30e24e8b650c7db63`，因此在本地改动尚未提交推送前，PR 页面仍显示同一批 `3` 条 open threads。
- `2026-04-30` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/source-generators/schema-config-generator.md`
  - 结果：通过；`Schema 配置生成器` 专题页的 frontmatter、链接与代码块校验通过。
- `2026-04-30` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/source-generators/index.md`
  - 结果：通过；source-generators landing 的链接与结构校验通过。
- `2026-04-30` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/source-generators/cqrs-handler-registry-generator.md`
  - 结果：通过；`Cqrs` generator 专题页在补足 fallback 精度说明后校验通过。
- `2026-04-30` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/core/cqrs.md`
  - 结果：通过；`CQRS` 运行时页在补足 generated registry 协作说明后校验通过。
- `2026-04-30` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/api-reference/index.md`
  - 结果：通过；API 入口页在补充 source-generator 阅读路线后校验通过。
- `2026-04-30` `bun run build`（工作目录：`docs/`）
  - 结果：通过；新增 source-generators 专题页与侧栏入口后站点仍可构建，仅保留既有大 chunk warning。

## 下一步

1. 完成本轮 3 个 latest-head review follow-up 后，重新运行最小文档验证，并重新抓取 `$gframework-pr-review`，确认 remote open threads 是否清空。
2. 若 review 线程清空，再继续按 `$gframework-batch-boot 50` 挑选“已有 package README、但站内专题仍不足”的 coverage 切片，而不是继续扩写共享支撑层。
3. 若 `Title check` 仍保留，则单独修改 GitHub PR 标题，不把它和仓库文件内容混为同一类修复项。
