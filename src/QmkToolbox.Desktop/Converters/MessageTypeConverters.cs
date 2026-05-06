using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.Converters;

internal static class LogBrushes
{
    // Dark palette
    public static readonly IBrush DarkBootloader = new SolidColorBrush(Color.Parse("#FFFF00"));
    public static readonly IBrush DarkSilver = new SolidColorBrush(Color.Parse("#C0C0C0"));
    public static readonly IBrush DarkError = new SolidColorBrush(Color.Parse("#F08080"));
    public static readonly IBrush DarkHid = new SolidColorBrush(Color.Parse("#87CEEB"));
    public static readonly IBrush DarkHidOutput = new SolidColorBrush(Color.Parse("#F0FFFF"));
    public static readonly IBrush DarkUdevOutput = new SolidColorBrush(Color.Parse("#90EE90"));
    public static readonly IBrush DarkForeground = Brushes.White;

    // Light palette — same hues, darkened for contrast on a light background
    public static readonly IBrush LightBootloader = new SolidColorBrush(Color.Parse("#7A7000"));
    public static readonly IBrush LightSilver = new SolidColorBrush(Color.Parse("#555555"));
    public static readonly IBrush LightError = new SolidColorBrush(Color.Parse("#B00020"));
    public static readonly IBrush LightHid = new SolidColorBrush(Color.Parse("#0066AA"));
    public static readonly IBrush LightHidOutput = new SolidColorBrush(Color.Parse("#006070"));
    public static readonly IBrush LightUdevOutput = new SolidColorBrush(Color.Parse("#1A7A1A"));
    public static readonly IBrush LightForeground = new SolidColorBrush(Color.Parse("#1A1A1A"));
}

public class MessageTypeToForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MessageType type ? GetForeground(type, isDark: true) : Brushes.White;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    internal static IBrush GetForeground(MessageType type, bool isDark) => isDark
        ? type switch
        {
            MessageType.Bootloader => LogBrushes.DarkBootloader,
            MessageType.Command => LogBrushes.DarkForeground,
            MessageType.CommandError => LogBrushes.DarkSilver,
            MessageType.CommandOutput => LogBrushes.DarkSilver,
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
            MessageType.Error => LogBrushes.LightError,
            MessageType.Hid => LogBrushes.LightHid,
            MessageType.HidOutput => LogBrushes.LightHidOutput,
            MessageType.Info => LogBrushes.LightSilver,
            MessageType.Usb => LogBrushes.LightForeground,
            MessageType.UdevOutput => LogBrushes.LightUdevOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
}

public class MessageTypeToPrefixConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MessageType type ? GetPrefix(type) : "";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    internal static string GetPrefix(MessageType type) => type switch
    {
        MessageType.Bootloader => "",
        MessageType.Command => "> ",
        MessageType.CommandError => "> ",
        MessageType.CommandOutput => "> ",
        MessageType.Error => "",
        MessageType.Hid => "",
        MessageType.HidOutput => "> ",
        MessageType.Info => "* ",
        MessageType.Usb => "",
        MessageType.UdevOutput => "",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}

public class MessageTypeToPrefixForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MessageType type ? GetPrefixForeground(type, isDark: true) : Brushes.White;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    internal static IBrush GetPrefixForeground(MessageType type, bool isDark) => isDark
        ? type switch
        {
            MessageType.Bootloader => Brushes.Transparent,
            MessageType.Command => LogBrushes.DarkForeground,
            MessageType.CommandError => LogBrushes.DarkError,
            MessageType.CommandOutput => LogBrushes.DarkForeground,
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
            MessageType.Error => Brushes.Transparent,
            MessageType.Hid => Brushes.Transparent,
            MessageType.HidOutput => LogBrushes.LightHid,
            MessageType.Info => LogBrushes.LightForeground,
            MessageType.Usb => Brushes.Transparent,
            MessageType.UdevOutput => Brushes.Transparent,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
}
