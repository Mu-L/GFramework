namespace GFramework.Game.Config;

/// <summary>
///     表示一个节点上声明的 <c>const</c> 约束。
///     该模型同时保留稳定比较键与原始 JSON 文本，分别供运行时匹配和诊断输出复用。
/// </summary>
internal sealed class YamlConfigConstantValue
{
    /// <summary>
    ///     初始化常量约束模型。
    /// </summary>
    /// <param name="comparableValue">用于与 YAML 节点比较的稳定键。</param>
    /// <param name="displayValue">用于诊断输出的原始常量文本。</param>
    public YamlConfigConstantValue(string comparableValue, string displayValue)
    {
        ArgumentNullException.ThrowIfNull(comparableValue);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayValue);

        ComparableValue = comparableValue;
        DisplayValue = displayValue;
    }

    /// <summary>
    ///     获取用于运行时比较的稳定键。
    /// </summary>
    public string ComparableValue { get; }

    /// <summary>
    ///     获取用于诊断输出的原始 JSON 常量文本。
    /// </summary>
    public string DisplayValue { get; }
}
