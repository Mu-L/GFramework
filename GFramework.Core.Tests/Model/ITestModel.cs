using GFramework.Core.Abstractions.Model;

namespace GFramework.Core.Tests.Model;

public interface ITestModel : IModel
{
    int GetCurrentXp { get; }
}