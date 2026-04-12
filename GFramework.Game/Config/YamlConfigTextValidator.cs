namespace GFramework.Game.Config;

/// <summary>
///     提供面向宿主的 YAML 文本校验入口，使保存前校验可以复用运行时同一套 schema 规则。
/// </summary>
public static class YamlConfigTextValidator
{
    /// <summary>
    ///     使用指定 schema 文件同步校验 YAML 文本。
    /// </summary>
    /// <param name="tableName">所属配置表名称。</param>
    /// <param name="schemaPath">Schema 文件绝对路径。</param>
    /// <param name="yamlPath">YAML 文件路径，仅用于诊断信息。</param>
    /// <param name="yamlText">待校验的 YAML 文本。</param>
    public static void Validate(
        string tableName,
        string schemaPath,
        string yamlPath,
        string yamlText)
    {
        var schema = YamlConfigSchemaValidator.Load(tableName, schemaPath);
        YamlConfigSchemaValidator.Validate(tableName, schema, yamlPath, yamlText);
    }

    /// <summary>
    ///     使用指定 schema 文件异步校验 YAML 文本。
    /// </summary>
    /// <param name="tableName">所属配置表名称。</param>
    /// <param name="schemaPath">Schema 文件绝对路径。</param>
    /// <param name="yamlPath">YAML 文件路径，仅用于诊断信息。</param>
    /// <param name="yamlText">待校验的 YAML 文本。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public static async Task ValidateAsync(
        string tableName,
        string schemaPath,
        string yamlPath,
        string yamlText,
        CancellationToken cancellationToken = default)
    {
        var schema = await YamlConfigSchemaValidator.LoadAsync(tableName, schemaPath, cancellationToken)
            .ConfigureAwait(false);
        YamlConfigSchemaValidator.Validate(tableName, schema, yamlPath, yamlText);
    }
}
