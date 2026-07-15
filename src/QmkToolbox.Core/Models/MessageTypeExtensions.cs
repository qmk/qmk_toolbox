namespace QmkToolbox.Core.Models;

public static class MessageTypeExtensions
{
    /// <summary>
    /// True for message types that carry a raw byte stream (tool stdout/stderr, HID console
    /// output) — written to the log verbatim so progress bars and partial lines render live.
    /// False for discrete, line-oriented messages (status, errors, command echo).
    /// </summary>
    public static bool IsRawStream(this MessageType type) =>
        type is MessageType.CommandOutput or MessageType.CommandError or MessageType.HidOutput;
}
