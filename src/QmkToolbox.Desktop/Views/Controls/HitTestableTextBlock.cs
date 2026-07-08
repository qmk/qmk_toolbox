using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.TextFormatting;

namespace QmkToolbox.Desktop.Views.Controls;

internal class HitTestableTextBlock : SelectableTextBlock
{
    // Avalonia style type selectors match the exact runtime type, so the theme's
    // "SelectableTextBlock" style (which supplies SelectionBrush/SelectionForegroundBrush)
    // never reaches this subclass — selection would happen but render invisibly. Point the
    // style key back at the base type so the theme's selection styling applies.
    protected override Type StyleKeyOverride => typeof(SelectableTextBlock);

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
