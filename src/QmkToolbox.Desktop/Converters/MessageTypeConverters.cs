using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.Converters;

internal static class LogBrushes
{
    public static readonly IBrush Bootloader = new SolidColorBrush(Color.Parse("#FFFF00"));
    public static readonly IBrush Silver = new SolidColorBrush(Color.Parse("#C0C0C0"));
    public static readonly IBrush Error = new SolidColorBrush(Color.Parse("#F08080"));
    public static readonly IBrush Hid = new SolidColorBrush(Color.Parse("#87CEEB"));
    public static readonly IBrush HidOutput = new SolidColorBrush(Color.Parse("#F0FFFF"));
    public static readonly IBrush UdevOutput = new SolidColorBrush(Color.Parse("#90EE90"));
}

public class MessageTypeToForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is MessageType type ? GetForeground(type) : Brushes.White;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    internal static IBrush GetForeground(MessageType type) => type switch
    {
        MessageType.Bootloader => LogBrushes.Bootloader,
        MessageType.Command => Brushes.White,
        MessageType.CommandError => LogBrushes.Silver,
        MessageType.CommandOutput => LogBrushes.Silver,
        MessageType.Error => LogBrushes.Error,
        MessageType.Hid => LogBrushes.Hid,
        MessageType.HidOutput => LogBrushes.HidOutput,
        MessageType.Info => LogBrushes.Silver,
        MessageType.Usb => Brushes.White,
        MessageType.UdevOutput => LogBrushes.UdevOutput,
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
        => value is MessageType type ? GetPrefixForeground(type) : Brushes.White;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    internal static IBrush GetPrefixForeground(MessageType type) => type switch
    {
        MessageType.Bootloader => Brushes.Transparent,
        MessageType.Command => Brushes.White,
        MessageType.CommandError => LogBrushes.Error,
        MessageType.CommandOutput => Brushes.White,
        MessageType.Error => Brushes.Transparent,
        MessageType.Hid => Brushes.Transparent,
        MessageType.HidOutput => LogBrushes.Hid,
        MessageType.Info => Brushes.White,
        MessageType.Usb => Brushes.Transparent,
        MessageType.UdevOutput => Brushes.Transparent,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}
