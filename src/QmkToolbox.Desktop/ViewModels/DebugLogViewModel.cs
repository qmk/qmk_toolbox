using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.ViewModels;

public partial class DebugLogViewModel : LogViewModelBase
{
    public void Append(string message) =>
        Log($"{DateTime.Now:HH:mm:ss.fff}  {message}\n", MessageType.Debug);
}
