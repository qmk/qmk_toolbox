using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using Avalonia.Threading;
using QmkToolbox.Desktop.Converters;
using QmkToolbox.Desktop.Models;

namespace QmkToolbox.Desktop.Views.Controls;

public partial class LogPanel : UserControl
{
    public static readonly StyledProperty<ObservableCollection<LogEntry>?> LogEntriesProperty =
        AvaloniaProperty.Register<LogPanel, ObservableCollection<LogEntry>?>(nameof(LogEntries));

    public static readonly StyledProperty<ICommand?> CopyCommandProperty =
        AvaloniaProperty.Register<LogPanel, ICommand?>(nameof(CopyCommand));

    public static readonly StyledProperty<ICommand?> ClearCommandProperty =
        AvaloniaProperty.Register<LogPanel, ICommand?>(nameof(ClearCommand));

    public ObservableCollection<LogEntry>? LogEntries
    {
        get => GetValue(LogEntriesProperty);
        set => SetValue(LogEntriesProperty, value);
    }

    public ICommand? CopyCommand
    {
        get => GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }

    public ICommand? ClearCommand
    {
        get => GetValue(ClearCommandProperty);
        set => SetValue(ClearCommandProperty, value);
    }

    public LogPanel()
    {
        InitializeComponent();
        ActualThemeVariantChanged += (_, _) => RebuildAllInlines();
        LogText.PointerMoved += OnLogTextPointerMoved;
        LogText.PointerExited += OnLogTextPointerExited;
        LogText.PointerPressInterceptor = OnLogTextPointerPress;
        LogText.LayoutUpdated += (_, _) => _urlRectCache = null;
    }

    private bool IsDark => ActualThemeVariant == ThemeVariant.Dark;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LogEntriesProperty)
        {
            if (change.OldValue is ObservableCollection<LogEntry> oldCollection)
                oldCollection.CollectionChanged -= OnLogEntriesChanged;

            RebuildAllInlines();

            if (change.NewValue is ObservableCollection<LogEntry> newCollection)
                newCollection.CollectionChanged += OnLogEntriesChanged;
        }
    }

    private int _lastEntryInlineStart;
    private bool _scrollPending;

    private int _currentTextLength;
    private int _lastEntryTextStart;
    private readonly List<UrlRange> _urlRanges = [];
    private List<(UrlRange Range, Rect[] Rects)>? _urlRectCache;
    private Run? _hoveredRun;

    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    private void OnLogEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                {
                    foreach (LogEntry entry in e.NewItems)
                        AppendInlines(entry);
                }

                ScheduleScrollToEnd();
                break;

            case NotifyCollectionChangedAction.Replace:
                if (e.NewItems is [LogEntry replacement])
                    ReplaceLastInlines(replacement);
                ScheduleScrollToEnd();
                break;

            case NotifyCollectionChangedAction.Reset:
                RebuildAllInlines();
                ScheduleScrollToEnd();
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Remove:
                break;
            default:
                break;
        }
    }

    private void RebuildAllInlines()
    {
        LogText.Inlines?.Clear();
        _lastEntryInlineStart = 0;
        _currentTextLength = 0;
        _lastEntryTextStart = 0;
        _urlRanges.Clear();
        _urlRectCache = null;
        _hoveredRun = null;
        if (LogEntries != null)
        {
            foreach (LogEntry entry in LogEntries)
                AppendInlines(entry);
        }
    }

    private void AppendInlines(LogEntry entry)
    {
        InlineCollection? inlines = LogText.Inlines;
        if (inlines == null)
            return;

        if (inlines.Count > 0)
        {
            inlines.Add(new LineBreak());
            _currentTextLength++;
        }

        _lastEntryInlineStart = inlines.Count;
        _lastEntryTextStart = _currentTextLength;
        AddEntryRuns(inlines, entry, IsDark, _urlRanges, ref _currentTextLength);
    }

    private void ReplaceLastInlines(LogEntry entry)
    {
        InlineCollection? inlines = LogText.Inlines;
        if (inlines == null)
            return;

        while (inlines.Count > _lastEntryInlineStart)
            inlines.RemoveAt(inlines.Count - 1);

        _urlRanges.RemoveAll(r => r.Start >= _lastEntryTextStart);
        _currentTextLength = _lastEntryTextStart;
        _urlRectCache = null;
        _hoveredRun = null;

        AddEntryRuns(inlines, entry, IsDark, _urlRanges, ref _currentTextLength);
    }

    private record struct UrlRange(int Start, int End, string Url, Run UrlRun);

    private static readonly Regex UrlRegex = new(@"https?://[^\s\)\]}>""']+", RegexOptions.Compiled);

    private static void AddEntryRuns(InlineCollection inlines, LogEntry entry, bool isDark,
        List<UrlRange> urlRanges, ref int textPos)
    {
        string prefix = MessageTypeToPrefixConverter.GetPrefix(entry.Type);
        if (prefix.Length > 0)
        {
            inlines.Add(new Run(prefix) { Foreground = MessageTypeToPrefixForegroundConverter.GetPrefixForeground(entry.Type, isDark) });
            textPos += prefix.Length;
        }

        IBrush foreground = MessageTypeToForegroundConverter.GetForeground(entry.Type, isDark);
        IBrush linkForeground = isDark ? LogBrushes.DarkLink : LogBrushes.LightLink;
        string text = entry.Text;
        int lastIndex = 0;

        foreach (Match match in UrlRegex.Matches(text))
        {
            if (match.Index > lastIndex)
            {
                string segment = text[lastIndex..match.Index];
                inlines.Add(new Run(segment) { Foreground = foreground });
                textPos += segment.Length;
            }

            string url = match.Value;
            var urlRun = new Run(url)
            {
                Foreground = linkForeground,
                TextDecorations = TextDecorations.Underline,
            };
            inlines.Add(urlRun);
            urlRanges.Add(new UrlRange(textPos, textPos + url.Length, url, urlRun));
            textPos += url.Length;

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < text.Length)
        {
            string remaining = text[lastIndex..];
            inlines.Add(new Run(remaining) { Foreground = foreground });
            textPos += remaining.Length;
        }
    }

    private List<(UrlRange Range, Rect[] Rects)> GetUrlRectCache()
    {
        if (_urlRectCache != null)
            return _urlRectCache;

        TextLayout? layout = LogText.TextLayout;
        _urlRectCache = layout == null
            ? []
            : _urlRanges.Select(r => (r, layout.HitTestTextRange(r.Start, r.End - r.Start).ToArray())).ToList();
        return _urlRectCache;
    }

    private UrlRange? GetUrlRangeAtPoint(Point point)
    {
        foreach ((UrlRange range, Rect[] rects) in GetUrlRectCache())
        {
            foreach (Rect rect in rects)
            {
                if (rect.Contains(point))
                    return range;
            }
        }

        return null;
    }

    private void OnLogTextPointerMoved(object? sender, PointerEventArgs e)
    {
        UrlRange? hovered = GetUrlRangeAtPoint(e.GetPosition(LogText));
        Run? newRun = hovered?.UrlRun;
        if (newRun == _hoveredRun)
            return;

        _hoveredRun?.Foreground = IsDark ? LogBrushes.DarkLink : LogBrushes.LightLink;
        _hoveredRun = newRun;
        _hoveredRun?.Foreground = IsDark ? LogBrushes.DarkLinkHover : LogBrushes.LightLinkHover;

        LogText.Cursor = _hoveredRun != null ? HandCursor : null;
    }

    private void OnLogTextPointerExited(object? sender, PointerEventArgs e)
    {
        _hoveredRun?.Foreground = IsDark ? LogBrushes.DarkLink : LogBrushes.LightLink;
        _hoveredRun = null;
        LogText.Cursor = null;
    }

    private bool OnLogTextPointerPress(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            return false;
        UrlRange? range = GetUrlRangeAtPoint(e.GetPosition(LogText));
        if (!range.HasValue)
            return false;
        _ = TopLevel.GetTopLevel(this)?.Launcher?.LaunchUriAsync(new Uri(range.Value.Url));
        return true;
    }

    private void ScheduleScrollToEnd()
    {
        if (_scrollPending)
            return;
        _scrollPending = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                _scrollPending = false;
                LogScroller.ScrollToEnd();
            },
            DispatcherPriority.Background);
    }
}
