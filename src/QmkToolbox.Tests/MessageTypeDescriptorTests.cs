using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Models;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Exhaustiveness guard for the message-type rendering table: every enum value must have a
/// descriptor row, so a new MessageType fails here at test time instead of throwing at render
/// time. (Per-value raw/line classification is pinned in MessageTypeRoutingTests.)
/// </summary>
public class MessageTypeDescriptorTests
{
    [Fact]
    public void EveryMessageType_HasADescriptorRow()
    {
        foreach (MessageType type in Enum.GetValues<MessageType>())
            Assert.True(MessageTypeDescriptors.All.ContainsKey(type), $"Missing descriptor for MessageType.{type}");
    }
}
