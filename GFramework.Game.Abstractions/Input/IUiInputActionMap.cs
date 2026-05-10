// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Game.Abstractions.UI;

namespace GFramework.Game.Abstractions.Input;

/// <summary>
///     定义逻辑动作名到 UI 语义动作的映射规则。
/// </summary>
public interface IUiInputActionMap
{
    /// <summary>
    ///     尝试把逻辑动作映射为 UI 语义动作。
    /// </summary>
    /// <param name="actionName">逻辑动作名称。</param>
    /// <param name="action">映射出的 UI 语义动作。</param>
    /// <returns>如果映射成功则返回 <see langword="true" />。</returns>
    bool TryMap(string actionName, out UiInputAction action);
}
