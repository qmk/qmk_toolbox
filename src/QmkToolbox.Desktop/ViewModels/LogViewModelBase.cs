using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using QmkToolbox.Desktop.Models;

namespace QmkToolbox.Desktop.ViewModels;

public abstract partial class LogViewModelBase : ObservableObject
{
    // Fires a single Reset notification after bulk-removing front entries,
    // avoiding the O(excess × n) cost of individual RemoveAt(0) calls.
    private sealed class LogEntryCollection : ObservableCollection<LogEntry>
    {
        public void TrimToMax(int maxEntries)
        {
            int excess = Count - maxEntries;
            if (excess <= 0)
                return;
            CheckReentrancy();
            // List<T>.RemoveRange is a single O(n) memmove, not excess × O(n) shifts.
            ((List<LogEntry>)Items).RemoveRange(0, excess);
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public ObservableCollection<LogEntry> LogEntries { get; } = new LogEntryCollection();

    private const int MaxLogEntries = 10_000;

    protected void TrimLogEntries() => ((LogEntryCollection)LogEntries).TrimToMax(MaxLogEntries);
}
