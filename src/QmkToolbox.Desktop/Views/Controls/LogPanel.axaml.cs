using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
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
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LogEntriesProperty)
        {
            if (change.OldValue is ObservableCollection<LogEntry> oldCollection)
                oldCollection.CollectionChanged -= OnLogEntriesChanged;

            LogText.Inlines?.Clear();
            _lastEntryInlineStart = 0;

            if (change.NewValue is ObservableCollection<LogEntry> newCollection)
            {
                foreach (LogEntry entry in newCollection)
                    AppendInlines(entry);
                newCollection.CollectionChanged += OnLogEntriesChanged;
            }
        }
    }

    // Index into LogText.Inlines where the last entry's Runs begin (after any LineBreak separator).
    private int _lastEntryInlineStart;
    private bool _scrollPending;

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
                LogText.Inlines?.Clear();
                _lastEntryInlineStart = 0;
                if (LogEntries != null)
                {
                    foreach (LogEntry entry in LogEntries)
                        AppendInlines(entry);
                }

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

    private void AppendInlines(LogEntry entry)
    {
        InlineCollection? inlines = LogText.Inlines;
        if (inlines == null)
            return;

        if (inlines.Count > 0)
            inlines.Add(new LineBreak());

        _lastEntryInlineStart = inlines.Count;
        AddEntryRuns(inlines, entry);
    }

    private void ReplaceLastInlines(LogEntry entry)
    {
        InlineCollection? inlines = LogText.Inlines;
        if (inlines == null)
            return;

        while (inlines.Count > _lastEntryInlineStart)
            inlines.RemoveAt(inlines.Count - 1);

        AddEntryRuns(inlines, entry);
    }

    private static void AddEntryRuns(InlineCollection inlines, LogEntry entry)
    {
        string prefix = MessageTypeToPrefixConverter.GetPrefix(entry.Type);
        if (prefix.Length > 0)
            inlines.Add(new Run(prefix) { Foreground = MessageTypeToPrefixForegroundConverter.GetPrefixForeground(entry.Type) });
        inlines.Add(new Run(entry.Text) { Foreground = MessageTypeToForegroundConverter.GetForeground(entry.Type) });
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
