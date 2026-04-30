using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs;

/// <summary>
///     定义由源码生成器或手写注册器提供的 request invoker 元数据契约。
/// </summary>
/// <remarks>
///     该 seam 允许运行时在首次创建 request dispatch binding 时，
///     直接复用编译期已知的请求/响应类型映射，而不是总是通过反射闭合泛型方法生成调用委托。
///     当当前程序集没有提供匹配项时，dispatcher 仍会回退到既有的反射绑定创建路径。
/// </remarks>
public interface ICqrsRequestInvokerProvider
{
    /// <summary>
    ///     尝试为指定请求/响应类型对提供运行时元数据。
    /// </summary>
    /// <param name="requestType">请求运行时类型。</param>
    /// <param name="responseType">响应运行时类型。</param>
    /// <param name="descriptor">命中时返回的 request invoker 元数据。</param>
    /// <returns>若当前 provider 可处理该请求/响应类型对则返回 <see langword="true" />；否则返回 <see langword="false" />。</returns>
    bool TryGetDescriptor(
        Type requestType,
        Type responseType,
        out CqrsRequestInvokerDescriptor? descriptor);
}
