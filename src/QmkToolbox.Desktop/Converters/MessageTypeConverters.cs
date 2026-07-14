using Avalonia.Media;
using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.Converters;

internal static class LogBrushes
{
    // Dark palette
    public static readonly IBrush DarkBootloader = new SolidColorBrush(Color.Parse("#FFFF00"));
    public static readonly IBrush DarkSilver = new SolidColorBrush(Color.Parse("#C0C0C0"));
    public static readonly IBrush DarkDebug = new SolidColorBrush(Color.Parse("#7898B8"));
    public static readonly IBrush DarkError = new SolidColorBrush(Color.Parse("#F08080"));
    public static readonly IBrush DarkHid = new SolidColorBrush(Color.Parse("#87CEEB"));
    public static readonly IBrush DarkHidOutput = new SolidColorBrush(Color.Parse("#F0FFFF"));
    public static readonly IBrush DarkUdevOutput = new SolidColorBrush(Color.Parse("#90EE90"));
    public static readonly IBrush DarkForeground = Brushes.White;

    // Light palette — same hues, darkened for contrast on a light background
    public static readonly IBrush LightBootloader = new SolidColorBrush(Color.Parse("#7A7000"));
    public static readonly IBrush LightSilver = new SolidColorBrush(Color.Parse("#222222"));
    public static readonly IBrush LightDebug = new SolidColorBrush(Color.Parse("#486888"));
    public static readonly IBrush LightError = new SolidColorBrush(Color.Parse("#B00020"));
    public static readonly IBrush LightHid = new SolidColorBrush(Color.Parse("#0066AA"));
    public static readonly IBrush LightHidOutput = new SolidColorBrush(Color.Parse("#006070"));
    public static readonly IBrush LightUdevOutput = new SolidColorBrush(Color.Parse("#1A7A1A"));
    public static readonly IBrush LightForeground = new SolidColorBrush(Color.Parse("#1A1A1A"));

    // Link colours
    public static readonly IBrush DarkLink = new SolidColorBrush(Color.Parse("#60CDFF"));
    public static readonly IBrush DarkLinkHover = new SolidColorBrush(Color.Parse("#99E5FF"));
    public static readonly IBrush LightLink = new SolidColorBrush(Color.Parse("#0067C0"));
    public static readonly IBrush LightLinkHover = new SolidColorBrush(Color.Parse("#004A8F"));
}

internal static class MessageTypeStyles
{
    internal static IBrush GetForeground(MessageType type, bool isDark) => isDark
        ? type switch
        {
            MessageType.Bootloader => LogBrushes.DarkBootloader,
            MessageType.Command => LogBrushes.DarkForeground,
            MessageType.CommandError => LogBrushes.DarkSilver,
            MessageType.CommandOutput => LogBrushes.DarkSilver,
            MessageType.Debug => LogBrushes.DarkDebug,
            MessageType.Error => LogBrushes.DarkError,
            MessageType.Hid => LogBrushes.DarkHid,
            MessageType.HidOutput => LogBrushes.DarkHidOutput,
            MessageType.Info => LogBrushes.DarkSilver,
            MessageType.Usb => LogBrushes.DarkForeground,
            MessageType.UdevOutput => LogBrushes.DarkUdevOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        }
        : type switch
        {
            MessageType.Bootloader => LogBrushes.LightBootloader,
            MessageType.Command => LogBrushes.LightForeground,
            MessageType.CommandError => LogBrushes.LightSilver,
            MessageType.CommandOutput => LogBrushes.LightSilver,
            MessageType.Debug => LogBrushes.LightDebug,
            MessageType.Error => LogBrushes.LightError,
            MessageType.Hid => LogBrushes.LightHid,
            MessageType.HidOutput => LogBrushes.LightHidOutput,
            MessageType.Info => LogBrushes.LightSilver,
            MessageType.Usb => LogBrushes.LightForeground,
            MessageType.UdevOutput => LogBrushes.LightUdevOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

    internal static string GetPrefix(MessageType type) => type switch
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

    internal static IBrush GetPrefixForeground(MessageType type, bool isDark) => isDark
        ? type switch
        {
            MessageType.Bootloader => Brushes.Transparent,
            MessageType.Command => LogBrushes.DarkForeground,
            MessageType.CommandError => LogBrushes.DarkError,
            MessageType.CommandOutput => LogBrushes.DarkForeground,
            MessageType.Debug => Brushes.Transparent,
            MessageType.Error => Brushes.Transparent,
            MessageType.Hid => Brushes.Transparent,
            MessageType.HidOutput => LogBrushes.DarkHid,
            MessageType.Info => LogBrushes.DarkForeground,
            MessageType.Usb => Brushes.Transparent,
            MessageType.UdevOutput => Brushes.Transparent,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        }
        : type switch
        {
            MessageType.Bootloader => Brushes.Transparent,
            MessageType.Command => LogBrushes.LightForeground,
            MessageType.CommandError => LogBrushes.LightError,
            MessageType.CommandOutput => LogBrushes.LightForeground,
            MessageType.Debug => Brushes.Transparent,
            MessageType.Error => Brushes.Transparent,
            MessageType.Hid => Brushes.Transparent,
            MessageType.HidOutput => LogBrushes.LightHid,
            MessageType.Info => LogBrushes.LightForeground,
            MessageType.Usb => Brushes.Transparent,
            MessageType.UdevOutput => Brushes.Transparent,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
}
