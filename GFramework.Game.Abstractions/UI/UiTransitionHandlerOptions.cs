namespace GFramework.Game.Abstractions.UI;

/// <summary>
///     UI切换处理器执行选项
/// </summary>
public record UiTransitionHandlerOptions(int TimeoutMs = 0, bool ContinueOnError = true);