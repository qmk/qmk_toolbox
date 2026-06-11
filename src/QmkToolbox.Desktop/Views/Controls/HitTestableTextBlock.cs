using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.TextFormatting;

namespace QmkToolbox.Desktop.Views.Controls;

internal class HitTestableTextBlock : SelectableTextBlock
{
    public new TextLayout? TextLayout => base.TextLayout;

    // When set, called on every left-button press. Return true to consume the press
    // (prevents text selection from starting); false to fall through to normal selection.
    public Func<PointerPressedEventArgs, bool>? PointerPressInterceptor { get; set; }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (PointerPressInterceptor?.Invoke(e) == true)
        {
            e.Handled = true;
            return;
        }
        base.OnPointerPressed(e);
    }
}
