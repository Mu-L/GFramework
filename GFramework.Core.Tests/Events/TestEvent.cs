namespace GFramework.Core.Tests.Events;

public sealed class TestEvent
{
    public int ReceivedValue { get; init; }
}

public sealed class EmptyEvent;