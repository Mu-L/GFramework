using Array = Godot.Collections.Array;

namespace GFramework.Godot.Text;

/// <summary>
///     负责把配置、开关和注册表装配为宿主标签的实际效果集合。
///     该控制器是组合式扩展的装配中心，使 <see cref="GfRichTextLabel" /> 保持轻量。
/// </summary>
internal sealed class RichTextEffectsController
{
    private readonly Func<bool> _animatedEffectsEnabledAccessor;
    private readonly Func<bool> _frameworkEffectsEnabledAccessor;
    private readonly RichTextLabel _host;
    private readonly Func<RichTextProfile?> _profileAccessor;
    private readonly IRichTextEffectRegistry _registry;

    /// <summary>
    ///     初始化控制器实例。
    /// </summary>
    /// <param name="host">目标富文本标签。</param>
    /// <param name="registry">效果注册表。</param>
    /// <param name="profileAccessor">当前配置访问器。</param>
    /// <param name="frameworkEffectsEnabledAccessor">框架效果总开关访问器。</param>
    /// <param name="animatedEffectsEnabledAccessor">字符动画开关访问器。</param>
    public RichTextEffectsController(
        RichTextLabel host,
        IRichTextEffectRegistry registry,
        Func<RichTextProfile?> profileAccessor,
        Func<bool> frameworkEffectsEnabledAccessor,
        Func<bool> animatedEffectsEnabledAccessor)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _profileAccessor = profileAccessor ?? throw new ArgumentNullException(nameof(profileAccessor));
        _frameworkEffectsEnabledAccessor = frameworkEffectsEnabledAccessor
                                           ?? throw new ArgumentNullException(nameof(frameworkEffectsEnabledAccessor));
        _animatedEffectsEnabledAccessor = animatedEffectsEnabledAccessor
                                          ?? throw new ArgumentNullException(nameof(animatedEffectsEnabledAccessor));
    }

    /// <summary>
    ///     初始化并立即刷新宿主标签的效果集合。
    /// </summary>
    public void Initialize()
    {
        RefreshEffects();
    }

    /// <summary>
    ///     根据当前配置和开关重建宿主标签上的 <see cref="RichTextLabel.CustomEffects" />。
    /// </summary>
    public void RefreshEffects()
    {
        if (!_frameworkEffectsEnabledAccessor())
        {
            _host.CustomEffects = new Array();
            return;
        }

        var profile = _profileAccessor() ?? RichTextProfile.CreateBuiltInDefault();
        var effects = _registry.CreateEffects(profile, _animatedEffectsEnabledAccessor());
        var customEffects = new Array();
        foreach (var effect in effects)
        {
            customEffects.Add(effect);
        }

        _host.CustomEffects = customEffects;
    }
}
