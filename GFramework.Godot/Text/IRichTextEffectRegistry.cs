namespace GFramework.Godot.Text;

/// <summary>
///     富文本效果注册表，负责把配置中的效果键解析为可安装的 <see cref="RichTextEffect" /> 实例。
/// </summary>
public interface IRichTextEffectRegistry
{
    /// <summary>
    ///     根据指定配置创建完整的效果实例集合。
    /// </summary>
    /// <param name="profile">效果组合配置。</param>
    /// <param name="animatedEffectsEnabled">当前是否允许字符级动态效果生效。</param>
    /// <returns>可直接写入 <see cref="RichTextLabel.CustomEffects" /> 的效果实例集合。</returns>
    IReadOnlyList<RichTextEffect> CreateEffects(RichTextProfile profile, bool animatedEffectsEnabled);

    /// <summary>
    ///     根据单个效果键创建对应效果实例。
    /// </summary>
    /// <param name="key">效果键。</param>
    /// <param name="animatedEffectsEnabled">当前是否允许字符级动态效果生效。</param>
    /// <returns>解析成功时返回效果实例；否则返回 <see langword="null" />。</returns>
    RichTextEffect? CreateEffect(string key, bool animatedEffectsEnabled);
}
