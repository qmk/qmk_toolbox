using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.ViewModels;

public partial class DebugLogViewModel : LogViewModelBase
{
    // Debug is a line type, so Log ends the line itself — no trailing '\n' needed here.
    public void Append(string message) =>
        Log($"{DateTime.Now:HH:mm:ss.fff}  {message}", MessageType.Debug);
}
