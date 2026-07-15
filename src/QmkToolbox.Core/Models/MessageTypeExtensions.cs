namespace QmkToolbox.Core.Models;

public static class MessageTypeExtensions
{
    /// <summary>
    /// True for message types that carry a raw byte stream (tool stdout/stderr, HID console
    /// output) — written to the log verbatim so progress bars and partial lines render live.
    /// False for discrete, line-oriented messages (status, errors, command echo).
    /// </summary>
    public static bool IsRawStream(this MessageType type) => type switch
    {
        MessageType.CommandOutput => true,
        MessageType.CommandError => true,
        MessageType.HidOutput => true,
        MessageType.Bootloader => throw new NotImplementedException(),
        MessageType.Command => throw new NotImplementedException(),
        MessageType.Debug => throw new NotImplementedException(),
        MessageType.Error => throw new NotImplementedException(),
        MessageType.Hid => throw new NotImplementedException(),
        MessageType.Info => throw new NotImplementedException(),
        MessageType.Usb => throw new NotImplementedException(),
        MessageType.UdevOutput => throw new NotImplementedException(),
        _ => false,
    };
}
