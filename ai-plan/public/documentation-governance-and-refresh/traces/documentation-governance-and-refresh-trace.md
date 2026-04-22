# Documentation Governance And Refresh Trace

## 2026-04-22

### 当前恢复点：RP-009

- 本轮从 PR #268 的最新 review 数据恢复，未发现失败检查；CTRF 报告显示 2139 个测试全部通过
- 最新未解决 review 线程要求：
  - 精简 active trace，避免默认恢复入口继续膨胀
  - 为 `docs/zh-CN/game/scene.md` 补充项目目录与文件约定
  - 为 `docs/zh-CN/game/ui.md` 补充项目目录与文件约定
- 已闭环 RP-001 到 RP-008 的执行细节已归档到
  `ai-plan/public/documentation-governance-and-refresh/archive/traces/documentation-governance-and-refresh-rp-001-through-rp-008.md`
- 本轮还修复 `.agents/skills/gframework-doc-refresh/SKILL.md` 的 YAML frontmatter，使包含冒号的长描述不再破坏 skill 加载

### 当前决策

- active trace 只保留当前恢复点、关键事实、验证和下一步；完成阶段继续进入 `archive/traces/`
- `scene.md` 与 `ui.md` 的集成说明必须同时覆盖目录布局、文件命名、接口实现关系、最小 wiring 和兼容说明
- `gframework-pr-review` 抓取结果以最新未解决 head review thread 为准；旧 summary 或已过期评论只作为参考

### 验证

- `python3 -B .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --branch docs/sdk-update-documentation --format json --json-output /tmp/current-pr-review.json`
- `cd docs && bun run build`

### 下一步

1. 继续使用 `gframework-doc-refresh` 对 `Godot.SourceGenerators` 做真实模块扫描
2. 优先刷新 `godot-project-generator.md`、`get-node-generator.md` 与 `bind-node-signal-generator.md`
3. 若发现 `module-map.json` 在 Godot 场景下缺少别名或 docs 映射，先回补共享映射，再更新具体页面
