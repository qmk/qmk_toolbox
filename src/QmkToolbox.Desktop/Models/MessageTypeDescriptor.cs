using Avalonia.Media;
using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.Models;

/// <summary>
/// Everything the log needs to know to render one <see cref="MessageType"/>: its stream
/// discipline, leading prefix, and per-theme colours. Colours are plain <see cref="Color"/>
/// structs — no brushes — so the pure terminal layers can read prefixes without touching
/// the view; <c>MessageTypeStyles</c> derives cached brushes from this table.
/// </summary>
/// <param name="IsRawStream">
/// True for types carrying a raw byte stream (tool stdout/stderr, HID console output) —
/// written to the log verbatim so progress bars and partial lines render live. False for
/// discrete, line-oriented messages (status, errors, command echo).
/// </param>
/// <param name="Prefix">The leading marker rendered before a line (e.g. "&gt; ", "* ").</param>
public sealed record MessageTypeDescriptor(
    bool IsRawStream,
    string Prefix,
    Color DarkForeground,
    Color LightForeground,
    Color? DarkPrefixForeground = null,   // null = transparent (prefix reserves its column silently)
    Color? LightPrefixForeground = null);

/// <summary>
/// The single place a message type's rendering is defined. Adding a <see cref="MessageType"/>
/// value means adding a row here — the exhaustiveness test fails otherwise.
/// </summary>
public static class MessageTypeDescriptors
{
    // Shared palette (light = same hue darkened for contrast on a light background).
    private static readonly Color DarkText = Colors.White;
    private static readonly Color LightText = Color.Parse("#1A1A1A");
    private static readonly Color DarkMuted = Color.Parse("#C0C0C0");
    private static readonly Color LightMuted = Color.Parse("#222222");
    private static readonly Color DarkError = Color.Parse("#F08080");
    private static readonly Color LightError = Color.Parse("#B00020");
    private static readonly Color DarkHid = Color.Parse("#87CEEB");
    private static readonly Color LightHid = Color.Parse("#0066AA");

    public static readonly IReadOnlyDictionary<MessageType, MessageTypeDescriptor> All =
        new Dictionary<MessageType, MessageTypeDescriptor>
        {
            [MessageType.Bootloader] = new(false, "", Color.Parse("#FFFF00"), Color.Parse("#7A7000")),
            [MessageType.Command] = new(false, "> ", DarkText, LightText, DarkText, LightText),
            [MessageType.CommandError] = new(true, "> ", DarkMuted, LightMuted, DarkError, LightError),
            [MessageType.CommandOutput] = new(true, "> ", DarkMuted, LightMuted, DarkText, LightText),
            [MessageType.Debug] = new(false, "", Color.Parse("#7898B8"), Color.Parse("#486888")),
            [MessageType.Error] = new(false, "", DarkError, LightError),
            [MessageType.Hid] = new(false, "", DarkHid, LightHid),
            [MessageType.HidOutput] = new(true, "> ", Color.Parse("#F0FFFF"), Color.Parse("#006070"), DarkHid, LightHid),
            [MessageType.Info] = new(false, "* ", DarkMuted, LightMuted, DarkText, LightText),
            [MessageType.Usb] = new(false, "", DarkText, LightText),
            [MessageType.UdevOutput] = new(false, "", Color.Parse("#90EE90"), Color.Parse("#1A7A1A")),
        };

    public static MessageTypeDescriptor For(MessageType type) => All[type];
}

public static class MessageTypeExtensions
{
    /// <inheritdoc cref="MessageTypeDescriptor" path="/param[@name='IsRawStream']"/>
    public static bool IsRawStream(this MessageType type) => MessageTypeDescriptors.For(type).IsRawStream;
}
