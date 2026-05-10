// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Game.Abstractions.Input;
using GFramework.Game.Abstractions.UI;

namespace GFramework.Game.Input;

/// <summary>
///     提供逻辑动作到 UI 路由语义分发的默认桥接。
/// </summary>
public sealed class UiInputDispatcher : IUiInputDispatcher
{
    private readonly IUiInputActionMap _actionMap;
    private readonly IUiRouter _router;

    /// <summary>
    ///     初始化 UI 输入分发器。
    /// </summary>
    /// <param name="actionMap">动作映射表。</param>
    /// <param name="router">目标 UI 路由器。</param>
    public UiInputDispatcher(IUiInputActionMap actionMap, IUiRouter router)
    {
        _actionMap = actionMap ?? throw new ArgumentNullException(nameof(actionMap));
        _router = router ?? throw new ArgumentNullException(nameof(router));
    }

    /// <inheritdoc />
    public bool TryDispatch(string actionName)
    {
        if (!_actionMap.TryMap(actionName, out var action))
        {
            return false;
        }

        return _router.TryDispatchUiAction(action);
    }
}
