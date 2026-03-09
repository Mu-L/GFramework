using GFramework.Core.Abstractions.Command;
using GFramework.Core.Rule;

namespace GFramework.Core.Command;

/// <summary>
///     抽象命令类，实现 ICommand 接口，为具体命令提供基础架构支持
/// </summary>
public abstract class AbstractCommand : ContextAwareBase, ICommand
{
    /// <summary>
    ///     执行命令的入口方法，实现 ICommand 接口的 Execute 方法
    /// </summary>
    void ICommand.Execute()
    {
        OnExecute();
    }

    /// <summary>
    ///     命令执行的抽象方法，由派生类实现具体的命令逻辑
    /// </summary>
    protected abstract void OnExecute();
}