---
title: VS Code 配置工具
description: 说明 GFramework AI-First 配置工作流对应的 VS Code 工具入口、工作区约定、常用命令与使用边界。
---

# VS Code 配置工具

`GFramework Config Tool` 是面向 AI-First 配置工作流的 VS Code 扩展。它不是新的运行时模块，而是把
`config/`、`schemas/`、轻量校验、表单预览和批量编辑收敛到编辑器侧的一条辅助工作流。

如果你正在维护 `GFramework.Game` 的 YAML + JSON Schema 配置，这个工具通常比纯手写 YAML 更适合做日常浏览、
校验和批量修改；如果你要做复杂嵌套结构或超出当前支持子集的 schema 设计，仍然应该回到原始 YAML 和 schema 文件。

## 适合什么时候用

- 你已经采用 `config/**/*.yaml` + `schemas/**/*.schema.json`
- 你希望在 VS Code 里快速浏览配置域和对应 schema
- 你需要批量修改同一配置域的顶层标量或标量数组字段
- 你想先走表单预览，再决定是否回到 raw YAML

不适合：

- 项目不使用 `GFramework.Game` 的配置工作流
- 需要完整 JSON Schema 编辑器，而不是当前仓库落地的稳定子集
- 需要在编辑器里处理更深层对象数组嵌套，且不接受回退到 raw YAML

## 工作区约定

默认目录约定是：

```text
GameProject/
├─ config/
│  ├─ monster/
│  │  ├─ slime.yaml
│  │  └─ goblin.yaml
│  └─ item/
│     └─ potion.yaml
└─ schemas/
   ├─ monster.schema.json
   └─ item.schema.json
```

扩展默认会把：

- `config/` 视为配置根目录
- `schemas/` 视为 schema 根目录

如果你的项目用了不同目录，可以通过工作区设置覆盖。

## 扩展当前提供什么

### Explorer 视图

扩展会在 VS Code Explorer 中提供一个独立视图，用来浏览配置域和配置文件。

### 常用命令

当前命令面向这几类操作：

- 刷新配置树
- 打开 raw YAML
- 打开对应 schema
- 打开轻量表单预览
- 对单个配置域做批量编辑
- 运行全量校验

如果你更关心“当前 schema 和 YAML 是否仍一致”，优先使用全量校验；如果你只是定位单个字段或注释，优先使用
Explorer + 表单预览。

## 推荐工作流

### 1. 浏览配置与 schema

先从 Explorer 里进入目标配置文件，再根据需要：

- 打开 raw YAML
- 跳转到对应 schema
- 进入轻量表单预览

### 2. 先校验，再批量改

如果你准备改同一配置域下多份文件，推荐顺序是：

1. 先运行全量校验
2. 再进入配置域批量编辑
3. 批量修改完成后回到 raw YAML 或表单确认结果

### 3. 嵌套结构优先分层处理

当前工具支持：

- 顶层标量字段
- 顶层标量数组
- 嵌套对象字段
- 对象数组

如果你进入更深层对象数组嵌套，当前更稳妥的做法通常是：

1. 用 Explorer 找到目标文件
2. 先看表单预览确认字段结构
3. 再回到 raw YAML 完成最终编辑

## 工作区设置

当前公开设置只有两个：

```json
{
  "gframeworkConfig.configPath": "config",
  "gframeworkConfig.schemasPath": "schemas"
}
```

- `gframeworkConfig.configPath`
  - 配置根目录，默认是 `config`
- `gframeworkConfig.schemasPath`
  - schema 根目录，默认是 `schemas`

## 当前边界

当前扩展重点覆盖的是仓库已经验证过的最小工作流：

- 工作区默认只取第一个 workspace folder
- 校验聚焦仓库当前支持的 schema 子集
- 表单预览支持对象数组，但更深的嵌套对象数组仍可能需要回退到 raw YAML
- 批量编辑当前聚焦顶层标量和顶层标量数组字段

因此，最稳妥的理解方式是：

- 用它加速“浏览、定位、轻量校验、批量维护”
- 不把它当成完整替代 YAML / schema 编辑的唯一入口

## 继续阅读

- [游戏内容配置系统](./config-system.md)
- [Game 模块](./index.md)
- [安装配置](../getting-started/installation.md)
