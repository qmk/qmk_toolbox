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
using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Converters;
using QmkToolbox.Desktop.Models;

namespace QmkToolbox.Desktop.Views.Controls;

public partial class LogPanel : UserControl
{
    public static readonly StyledProperty<TerminalBuffer?> BufferProperty =
        AvaloniaProperty.Register<LogPanel, TerminalBuffer?>(nameof(Buffer));

    public static readonly StyledProperty<ICommand?> CopyCommandProperty =
        AvaloniaProperty.Register<LogPanel, ICommand?>(nameof(CopyCommand));

    public static readonly StyledProperty<ICommand?> ClearCommandProperty =
        AvaloniaProperty.Register<LogPanel, ICommand?>(nameof(ClearCommand));

    public TerminalBuffer? Buffer
    {
        get => GetValue(BufferProperty);
        set => SetValue(BufferProperty, value);
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
        ActualThemeVariantChanged += (_, _) => RenderBuffer();
        LogText.PointerMoved += OnLogTextPointerMoved;
        LogText.PointerExited += OnLogTextPointerExited;
        LogText.PointerPressInterceptor = OnLogTextPointerPress;
        LogText.LayoutUpdated += (_, _) => _urlRectCache = null;
    }

    private bool IsDark => ActualThemeVariant == ThemeVariant.Dark;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BufferProperty)
        {
            if (change.OldValue is TerminalBuffer oldBuffer)
                oldBuffer.Changed -= OnBufferChanged;
            if (change.NewValue is TerminalBuffer newBuffer)
                newBuffer.Changed += OnBufferChanged;

            RenderBuffer();
            ScheduleScrollToEnd();
        }
    }

    private bool _renderPending;

    // Coalesce a burst of buffer writes into a single re-render + scroll per UI tick.
    private void OnBufferChanged()
    {
        if (_renderPending)
            return;
        _renderPending = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                _renderPending = false;
                RenderBuffer();
                // Don't yank the view to the bottom while the user is selecting text.
                if (LogText.SelectionStart == LogText.SelectionEnd)
                    LogScroller.ScrollToEnd();
            },
            DispatcherPriority.Background);
    }

    private readonly List<UrlRange> _urlRanges = [];
    private List<(UrlRange Range, Rect[] Rects)>? _urlRectCache;
    private Run? _hoveredRun;

    private static readonly Regex UrlRegex = new(@"https?://[^\s\)\]}>""']+", RegexOptions.Compiled);

    private record struct UrlRange(int Start, int End, string Url, Run UrlRun);

    private int _renderedTextLength;

    private void RenderBuffer()
    {
        TerminalBuffer? buffer = Buffer;
        if (buffer == null)
            return;

        // Rebuilding the inlines drops the current text selection. Remember it so it can be
        // restored when the content only grew (the common append case), where every offset
        // before the selection is unchanged.
        int selStart = LogText.SelectionStart;
        int selEnd = LogText.SelectionEnd;
        bool hadSelection = selStart != selEnd;
        int prevLength = _renderedTextLength;

        LogText.Inlines?.Clear();
        _urlRanges.Clear();
        _urlRectCache = null;
        _hoveredRun = null;

        IBrush linkForeground = IsDark ? LogBrushes.DarkLink : LogBrushes.LightLink;
        bool isDark = IsDark;
        int textPos = 0;

        // Render completed lines
        foreach (TerminalLine line in buffer.Lines)
        {
            AppendLineInlines(line, linkForeground, isDark, ref textPos);
            LogText.Inlines?.Add(new LineBreak());
            textPos++;
        }

        // Render current line if it has content
        if (buffer.CurrentLine.Segments.Count > 0)
            AppendLineInlines(buffer.CurrentLine, linkForeground, isDark, ref textPos);

        _renderedTextLength = textPos;

        // Only restore when the buffer grew; a shrink (clear/trim) invalidates the offsets.
        if (hadSelection && textPos >= prevLength)
        {
            LogText.SelectionStart = Math.Min(selStart, textPos);
            LogText.SelectionEnd = Math.Min(selEnd, textPos);
        }
    }

    private void AppendLineInlines(TerminalLine line, IBrush linkForeground, bool isDark, ref int textPos)
    {
        InlineCollection? inlines = LogText.Inlines;
        if (inlines == null)
            return;

        // The line's prefix (e.g. "> ", "* ") is keyed off its first segment's type.
        if (line.Segments.Count > 0)
        {
            MessageType lineType = line.Segments[0].Type;
            string prefix = MessageTypeStyles.GetPrefix(lineType);
            if (prefix.Length > 0)
            {
                inlines.Add(new Run(prefix)
                {
                    Foreground = MessageTypeStyles.GetPrefixForeground(lineType, isDark),
                });
                textPos += prefix.Length;
            }
        }

        foreach (TerminalSegment seg in line.Segments)
        {
            // Resolve brush at render time based on segment type + theme
            IBrush foreground = MessageTypeStyles.GetForeground(seg.Type, isDark);

            // Check for URLs within the segment text and split into runs
            int lastIndex = 0;
            string text = seg.Text;

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
                _urlRanges.Add(new UrlRange(textPos, textPos + url.Length, url, urlRun));
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

    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

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
        Dispatcher.UIThread.Post(
            LogScroller.ScrollToEnd,
            DispatcherPriority.Background);
    }
}
