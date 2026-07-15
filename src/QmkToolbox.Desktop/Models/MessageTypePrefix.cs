using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.Models;

/// <summary>
/// The leading marker rendered before a line of a given <see cref="MessageType"/>
/// (e.g. "&gt; " for command echo/output, "* " for info). Pure — no Avalonia — so the
/// terminal projection can emit prefix runs without touching the view.
/// </summary>
public static class MessageTypePrefix
{
    public static string GetPrefix(MessageType type) => type switch
    {
        MessageType.Bootloader => "",
        MessageType.Command => "> ",
        MessageType.CommandError => "> ",
        MessageType.CommandOutput => "> ",
        MessageType.Debug => "",
        MessageType.Error => "",
        MessageType.Hid => "",
        MessageType.HidOutput => "> ",
        MessageType.Info => "* ",
        MessageType.Usb => "",
        MessageType.UdevOutput => "",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}
