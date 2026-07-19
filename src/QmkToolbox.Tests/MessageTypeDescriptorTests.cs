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

    [Fact]
    public void EveryDescriptor_HasANonNullPrefix()
    {
        foreach (MessageTypeDescriptor descriptor in MessageTypeDescriptors.All.Values)
            Assert.NotNull(descriptor.Prefix);
    }

    [Fact]
    public void RawStreamTypes_AreExactlyTheStreamingOnes()
    {
        MessageType[] raw = [.. MessageTypeDescriptors.All
            .Where(kv => kv.Value.IsRawStream)
            .Select(kv => kv.Key)
            .Order()];

        Assert.Equal(
            [MessageType.CommandError, MessageType.CommandOutput, MessageType.HidOutput],
            raw);
    }
}
