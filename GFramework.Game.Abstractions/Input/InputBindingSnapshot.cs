// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

namespace GFramework.Game.Abstractions.Input;

/// <summary>
///     描述一组动作绑定的可持久化快照。
/// </summary>
public sealed class InputBindingSnapshot
{
    /// <summary>
    ///     初始化一个输入绑定快照。
    /// </summary>
    /// <param name="actions">动作绑定集合。</param>
    public InputBindingSnapshot(IReadOnlyList<InputActionBinding> actions)
    {
        Actions = actions ?? Array.Empty<InputActionBinding>();
    }

    /// <summary>
    ///     获取动作绑定集合。
    /// </summary>
    public IReadOnlyList<InputActionBinding> Actions { get; }
}
