// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

namespace GFramework.Game.Abstractions.Input;

/// <summary>
///     定义面向 UI 语义动作的输入分发入口。
/// </summary>
public interface IUiInputDispatcher
{
    /// <summary>
    ///     尝试把逻辑动作分发到当前 UI 路由。
    /// </summary>
    /// <param name="actionName">逻辑动作名称。</param>
    /// <returns>如果该动作被映射为 UI 动作并成功分发，则返回 <see langword="true" />。</returns>
    bool TryDispatch(string actionName);
}
