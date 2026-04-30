namespace GFramework.Cqrs;

/// <summary>
///     描述单个 stream request/response 类型对与其 generated invoker 元数据之间的映射条目。
/// </summary>
/// <param name="RequestType">流式请求运行时类型。</param>
/// <param name="ResponseType">流式响应元素类型。</param>
/// <param name="Descriptor">对应的 generated stream invoker 描述符。</param>
public sealed record CqrsStreamInvokerDescriptorEntry(
    Type RequestType,
    Type ResponseType,
    CqrsStreamInvokerDescriptor Descriptor);
