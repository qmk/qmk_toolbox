using Avalonia.Media;
using Avalonia.Media.Immutable;
using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Models;

namespace QmkToolbox.Desktop.Converters;

internal static class LogBrushes
{
    // Link colours
    public static readonly IBrush DarkLink = new SolidColorBrush(Color.Parse("#60CDFF"));
    public static readonly IBrush DarkLinkHover = new SolidColorBrush(Color.Parse("#99E5FF"));
    public static readonly IBrush LightLink = new SolidColorBrush(Color.Parse("#0067C0"));
    public static readonly IBrush LightLinkHover = new SolidColorBrush(Color.Parse("#004A8F"));
}

/// <summary>
/// Brush lookups derived mechanically from <see cref="MessageTypeDescriptors"/> — no per-type
/// knowledge lives here, so a new <see cref="MessageType"/> only needs its descriptor row.
/// </summary>
internal static class MessageTypeStyles
{
    private static readonly IBrush Transparent = new ImmutableSolidColorBrush(Colors.Transparent);

    private static readonly Dictionary<MessageType, IBrush> DarkForeground = Build(d => d.DarkForeground);
    private static readonly Dictionary<MessageType, IBrush> LightForeground = Build(d => d.LightForeground);
    private static readonly Dictionary<MessageType, IBrush> DarkPrefixForeground = Build(d => d.DarkPrefixForeground);
    private static readonly Dictionary<MessageType, IBrush> LightPrefixForeground = Build(d => d.LightPrefixForeground);

    private static Dictionary<MessageType, IBrush> Build(Func<MessageTypeDescriptor, Color?> colour) =>
        MessageTypeDescriptors.All.ToDictionary(
            kv => kv.Key,
            kv => colour(kv.Value) is { } c ? new ImmutableSolidColorBrush(c) : Transparent);

    internal static IBrush GetForeground(MessageType type, bool isDark) =>
        (isDark ? DarkForeground : LightForeground)[type];

    internal static IBrush GetPrefixForeground(MessageType type, bool isDark) =>
        (isDark ? DarkPrefixForeground : LightPrefixForeground)[type];
}
