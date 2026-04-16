namespace GFramework.Cqrs;

/// <summary>
///     标记程序集中的 CQRS 生成注册器仍需要运行时补充反射扫描。
/// </summary>
/// <remarks>
///     该特性通常由源码生成器自动添加到消费端程序集。
///     当生成器只能安全生成部分 handler 映射时，运行时会先执行生成注册器，再补一次带去重的反射扫描，
///     以覆盖那些生成代码无法直接引用的 handler 类型。
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class CqrsReflectionFallbackAttribute : Attribute
{
}
