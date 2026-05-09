# GFramework Skills

公开入口目前包含 `gframework-doc-refresh`、`gframework-batch-boot` 与 `gframework-multi-agent-batch`。

## 公开入口

### `gframework-doc-refresh`

按源码模块驱动文档刷新，而不是按 `guide`、`tutorial`、`api` 等类型拆入口。

适用场景：

- 刷新某个模块的 landing page
- 复核专题页是否与源码、测试、README 一致
- 评估是否需要补 API reference 或教程
- 在 adoption path 不清晰时引入 `ai-libs/` 消费者接法作为补充证据

推荐调用：

```bash
/gframework-doc-refresh <module>
```

示例：

```bash
/gframework-doc-refresh Core
/gframework-doc-refresh Godot.SourceGenerators
/gframework-doc-refresh Cqrs
```

### `gframework-batch-boot`

在 `gframework-boot` 的基础上，自动推进可分批执行的重复性任务，不需要人工一轮轮重新触发。

适用场景：

- analyzer warning reduction
- 大批量测试结构收口
- 分模块文档刷新 wave
- 任何有明确 stop condition 的多批次任务

推荐调用：

```bash
/gframework-batch-boot <task-or-stop-condition>
```

批处理阈值速记：

```bash
/gframework-batch-boot 75
/gframework-batch-boot 75 2000
```

- 单个数字默认表示“当前分支全部提交相对远程 `origin/main` 接近多少个文件变更时停止”
- 两个数字默认表示“当前分支全部提交相对远程 `origin/main` 的 `文件数 OR 变更行数`”，顺序固定为 `<files> <lines>`
- 不推荐写 `/gframework-batch-boot 75 | 2000`，因为 `|` 很像 shell pipe；若用户这样写，也应按 OR 语义理解并在后续说明中归一化成无 `|` 版本

示例：

```bash
/gframework-batch-boot 75
/gframework-batch-boot 75 2000
/gframework-batch-boot continue analyzer warning reduction until branch diff vs origin/main approaches 75 files
/gframework-batch-boot keep refactoring repetitive source-generator tests in bounded batches
```

### `gframework-multi-agent-batch`

当用户希望主 Agent 负责拆分任务、派发互不冲突的 subagent 切片、核对进度、维护 `ai-plan`、验收结果并持续推进时，使用该入口。

适用场景：

- 复杂任务已经明确可以拆成多个互不冲突的写面
- 主 Agent 需要持续 review / integrate，而不是把执行权完全交给单个 worker
- 需要把 delegated scope、验证结果与下一恢复点同步写回 `ai-plan`
- 任务仍要受 branch diff、context budget 与 reviewability 边界约束

推荐调用：

```bash
/gframework-multi-agent-batch <task>
```

示例：

```bash
/gframework-multi-agent-batch continue the current cqrs optimization by delegating non-conflicting benchmark and runtime slices
/gframework-multi-agent-batch coordinate parallel subagents, keep ai-plan updated, and stop when reviewability starts to degrade
```

## 共享资源

- `_shared/DOCUMENTATION_STANDARDS.md`
  - 统一的文档规则、证据顺序与验证要求
- `_shared/module-map.json`
  - 机器可读的模块映射表
- `_shared/module-config.sh`
  - 轻量 shell 辅助函数

## 内部资源

`gframework-doc-refresh/` 下包含：

- `references/`
  - 模块选择、证据顺序、输出策略
- `templates/`
  - landing page、专题页、API reference、教程模板
- `scripts/`
  - 模块扫描与文档验证脚本

旧 `vitepress-*` skills 不再作为并列公开入口保留。
