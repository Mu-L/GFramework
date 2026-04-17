using GFramework.Core.Abstractions.Pause;
using GFramework.Game.Abstractions.Enums;

namespace GFramework.Game.Abstractions.UI;

/// <summary>
///     描述一个 UI 页面在输入、World 阻断与暂停上的运行时语义。
/// </summary>
public sealed class UiInteractionProfile
{
    /// <summary>
    ///     获取默认值实例。
    /// </summary>
    public static UiInteractionProfile Default { get; } = new();

    /// <summary>
    ///     声明当前页面要捕获的语义动作集合。
    /// </summary>
    public UiInputActionMask CapturedActions { get; init; } = UiInputActionMask.None;

    /// <summary>
    ///     指示当前页面是否阻断 World 指针输入，例如地图点击或相机拖拽。
    /// </summary>
    public bool BlocksWorldPointerInput { get; init; }

    /// <summary>
    ///     指示当前页面是否阻断 World 语义动作输入，例如 gameplay 快捷键。
    /// </summary>
    public bool BlocksWorldActionInput { get; init; }

    /// <summary>
    ///     指示当前页面的可见性是否应驱动暂停栈。
    /// </summary>
    public UiPauseMode PauseMode { get; init; } = UiPauseMode.None;

    /// <summary>
    ///     当 <see cref="PauseMode" /> 生效时使用的暂停组。
    /// </summary>
    public PauseGroup PauseGroup { get; init; } = PauseGroup.Global;

    /// <summary>
    ///     当场景树暂停时，该页面是否仍需继续处理输入与动画。
    /// </summary>
    public bool ContinueProcessingWhenPaused { get; init; }

    /// <summary>
    ///     页面向暂停栈登记时使用的原因文本。
    /// </summary>
    public string PauseReason { get; init; } = string.Empty;

    /// <summary>
    ///     判断当前配置是否捕获了指定动作。
    /// </summary>
    /// <param name="action">要查询的语义动作。</param>
    /// <returns>如果当前配置捕获该动作则返回 <see langword="true" />。</returns>
    public bool Captures(UiInputAction action)
    {
        return action switch
        {
            UiInputAction.Cancel => CapturedActions.HasFlag(UiInputActionMask.Cancel),
            UiInputAction.Confirm => CapturedActions.HasFlag(UiInputActionMask.Confirm),
            _ => false
        };
    }

    /// <summary>
    ///     为指定层级生成默认交互配置。
    /// </summary>
    /// <param name="layer">UI 层级。</param>
    /// <returns>该层级的默认交互语义。</returns>
    public static UiInteractionProfile CreateDefault(UiLayer layer)
    {
        return layer switch
        {
            UiLayer.Modal => new UiInteractionProfile
            {
                CapturedActions = UiInputActionMask.Cancel,
                BlocksWorldPointerInput = true,
                BlocksWorldActionInput = true
            },
            UiLayer.Topmost => new UiInteractionProfile
            {
                CapturedActions = UiInputActionMask.Cancel,
                BlocksWorldPointerInput = true,
                BlocksWorldActionInput = true
            },
            _ => Default
        };
    }
}
