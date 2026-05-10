// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

namespace GFramework.Game.Abstractions.Input;

/// <summary>
///     定义逻辑动作绑定的查询、修改与快照导入导出契约。
/// </summary>
/// <remarks>
///     该接口承担框架输入系统的持久化与重绑定边界。
///     宿主层可以把自己的原生输入系统适配到这里，上层业务则只依赖动作名和绑定描述，不直接接触宿主输入事件。
/// </remarks>
public interface IInputBindingStore
{
    /// <summary>
    ///     获取指定动作的当前绑定。
    /// </summary>
    /// <param name="actionName">动作名称。</param>
    /// <returns>动作绑定快照。</returns>
    InputActionBinding GetBindings(string actionName);

    /// <summary>
    ///     获取所有动作的当前绑定快照。
    /// </summary>
    /// <returns>全量输入绑定快照。</returns>
    InputBindingSnapshot ExportSnapshot();

    /// <summary>
    ///     使用给定快照替换当前绑定。
    /// </summary>
    /// <param name="snapshot">要导入的快照。</param>
    void ImportSnapshot(InputBindingSnapshot snapshot);

    /// <summary>
    ///     把指定绑定设置为动作的主绑定。
    /// </summary>
    /// <param name="actionName">动作名称。</param>
    /// <param name="binding">新绑定。</param>
    /// <param name="swapIfTaken">是否在冲突时交换已占用绑定。</param>
    void SetPrimaryBinding(string actionName, InputBindingDescriptor binding, bool swapIfTaken = true);

    /// <summary>
    ///     将指定动作恢复为默认绑定。
    /// </summary>
    /// <param name="actionName">动作名称。</param>
    void ResetAction(string actionName);

    /// <summary>
    ///     将所有动作恢复为默认绑定。
    /// </summary>
    void ResetAll();
}
