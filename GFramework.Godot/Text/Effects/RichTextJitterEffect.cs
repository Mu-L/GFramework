namespace GFramework.Godot.Text.Effects;

/// <summary>
///     为文本提供抖动效果。
/// </summary>
[GlobalClass]
[Tool]
public partial class RichTextJitterEffect : RichTextEffectBase
{
    private readonly bool _animatedEffectsEnabled;
    private readonly FastNoiseLite _noise;

    /// <summary>
    ///     初始化抖动效果。
    /// </summary>
    /// <param name="animatedEffectsEnabled">是否允许动态效果实际生效。</param>
    public RichTextJitterEffect(bool animatedEffectsEnabled = true)
    {
        _animatedEffectsEnabled = animatedEffectsEnabled;
        _noise = new FastNoiseLite
        {
            NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
            FractalOctaves = 8,
            FractalGain = 0.8f
        };
    }

    /// <summary>
    ///     获取标签名。
    /// </summary>
    protected override string TagName => "jitter";

    /// <summary>
    ///     应用抖动位移。
    /// </summary>
    /// <param name="charFx">当前字符上下文。</param>
    /// <returns>始终返回 <see langword="true" />。</returns>
    public override bool _ProcessCustomFX(CharFXTransform charFx)
    {
        if (!_animatedEffectsEnabled)
        {
            return true;
        }

        var amplitude = GetFloat(charFx, "amplitude", 3.0f);
        var speed = GetFloat(charFx, "speed", 600.0f);

        _noise.Seed = (charFx.RelativeIndex + 1) * 131;
        var x = _noise.GetNoise1D((float)charFx.ElapsedTime * speed);
        _noise.Seed = (charFx.RelativeIndex + 1) * 737;
        var y = _noise.GetNoise1D((float)charFx.ElapsedTime * speed);

        charFx.Offset += new Vector2(x, y) * amplitude;
        ApplyVisibility(charFx);
        return true;
    }
}
