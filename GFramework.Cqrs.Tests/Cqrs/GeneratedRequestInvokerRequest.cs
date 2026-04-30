using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     用于验证 generated request invoker provider 接线的测试请求。
/// </summary>
internal sealed record GeneratedRequestInvokerRequest(string Value) : IRequest<string>;
