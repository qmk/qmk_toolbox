using System.Threading.Tasks;

namespace QMK_Toolbox.Views;

public interface IWindow
{
    public void ShowMessage(string message);
    public void OnClose();
    public Task OnFileOpen();
}