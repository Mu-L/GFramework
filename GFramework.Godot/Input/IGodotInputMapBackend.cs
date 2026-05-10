// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Game.Abstractions.Input;

namespace GFramework.Godot.Input;

/// <summary>
///     定义 `GodotInputBindingStore` 依赖的最小 `InputMap` 后端能力。
/// </summary>
internal interface IGodotInputMapBackend
{
    /// <summary>
    ///     获取当前 `InputMap` 中的动作名。
    /// </summary>
    /// <returns>动作名列表。</returns>
    IReadOnlyList<string> GetActionNames();

    /// <summary>
    ///     获取指定动作的框架绑定描述集合。
    /// </summary>
    /// <param name="actionName">动作名称。</param>
    /// <returns>框架绑定描述集合。</returns>
    IReadOnlyList<InputBindingDescriptor> GetBindings(string actionName);

    /// <summary>
    ///     用给定绑定集合替换动作当前绑定。
    /// </summary>
    /// <param name="actionName">动作名称。</param>
    /// <param name="bindings">新的绑定集合。</param>
    void SetBindings(string actionName, IReadOnlyList<InputBindingDescriptor> bindings);

    /// <summary>
    ///     将指定动作恢复为项目默认绑定。
    /// </summary>
    /// <param name="actionName">动作名称。</param>
    void ResetAction(string actionName);

    /// <summary>
    ///     将所有动作恢复为项目默认绑定。
    /// </summary>
    void ResetAll();
}
