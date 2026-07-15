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

        InlineCollection? inlines = LogText.Inlines;
        inlines?.Clear();
        _urlRanges.Clear();
        _urlRectCache = null;
        _hoveredRun = null;

        bool isDark = IsDark;
        IBrush linkForeground = isDark ? LogBrushes.DarkLink : LogBrushes.LightLink;

        // The projection owns all prefix/URL/offset logic; this loop is a pure run -> inline map.
        IReadOnlyList<TerminalRun> runs = TerminalProjection.ToRuns(buffer);
        if (inlines != null)
        {
            foreach (TerminalRun run in runs)
            {
                switch (run.Kind)
                {
                    case TerminalRunKind.LineBreak:
                        inlines.Add(new LineBreak());
                        break;

                    case TerminalRunKind.Prefix:
                        inlines.Add(new Run(run.Text)
                        {
                            Foreground = MessageTypeStyles.GetPrefixForeground(run.Type, isDark),
                        });
                        break;

                    case TerminalRunKind.Url:
                        var urlRun = new Run(run.Text)
                        {
                            Foreground = linkForeground,
                            TextDecorations = TextDecorations.Underline,
                        };
                        inlines.Add(urlRun);
                        _urlRanges.Add(new UrlRange(run.Start, run.Start + run.Text.Length, run.Url!, urlRun));
                        break;

                    case TerminalRunKind.Text:
                    default:
                        inlines.Add(new Run(run.Text)
                        {
                            Foreground = MessageTypeStyles.GetForeground(run.Type, isDark),
                        });
                        break;
                }
            }
        }

        _renderedTextLength = runs.TotalLength();

        // Only restore when the buffer grew; a shrink (clear/trim) invalidates the offsets.
        if (hadSelection && _renderedTextLength >= prevLength)
        {
            LogText.SelectionStart = Math.Min(selStart, _renderedTextLength);
            LogText.SelectionEnd = Math.Min(selEnd, _renderedTextLength);
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
