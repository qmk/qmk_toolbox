using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QmkToolbox.Desktop.Models;

namespace QmkToolbox.Desktop.ViewModels;

public partial class KeyTesterViewModel : ObservableObject
{
    [ObservableProperty] private string _lastKeycode = "";
    [ObservableProperty] private string _lastScanCode = "";

    public ObservableCollection<KeyViewModel> Keys { get; } = new(BuildKeys());

    private readonly Dictionary<PhysicalKey, KeyViewModel> _keyMap;

    public KeyTesterViewModel()
    {
        _keyMap = Keys.ToDictionary(k => k.Key);
    }

    public void OnKeyDown(PhysicalKey key)
    {
        LastKeycode = key.ToString();
        LastScanCode = $"0x{(int)key:X4}";
        if (_keyMap.TryGetValue(key, out KeyViewModel? vm))
            vm.State = KeyState.Pressed;
    }

    public void OnKeyUp(PhysicalKey key)
    {
        if (_keyMap.TryGetValue(key, out KeyViewModel? vm))
            vm.State = KeyState.Tested;
    }

    [RelayCommand]
    private void Reset()
    {
        foreach (KeyViewModel k in Keys)
            k.State = KeyState.Default;
        LastKeycode = "";
        LastScanCode = "";
    }

    private static IEnumerable<KeyViewModel> BuildKeys()
    {
        var keys = new List<KeyViewModel>
        {
            // ── Function row (y=0) ─────────────────────────────────────────────
            new(PhysicalKey.Escape, "Esc", 0, 0, 40),
            new(PhysicalKey.F1, "F1", 66, 0, 40),
            new(PhysicalKey.F2, "F2", 110, 0, 40),
            new(PhysicalKey.F3, "F3", 154, 0, 40),
            new(PhysicalKey.F4, "F4", 198, 0, 40),
            new(PhysicalKey.F5, "F5", 254, 0, 40),
            new(PhysicalKey.F6, "F6", 298, 0, 40),
            new(PhysicalKey.F7, "F7", 342, 0, 40),
            new(PhysicalKey.F8, "F8", 386, 0, 40),
            new(PhysicalKey.F9, "F9", 442, 0, 40),
            new(PhysicalKey.F10, "F10", 486, 0, 40),
            new(PhysicalKey.F11, "F11", 530, 0, 40),
            new(PhysicalKey.F12, "F12", 574, 0, 40),
            new(PhysicalKey.PrintScreen, "PrtSc", 634, 0, 40),
            new(PhysicalKey.ScrollLock, "ScrLk", 678, 0, 40),
            new(PhysicalKey.Pause, "Pause", 722, 0, 40),

            // ── Number row (y=52) ─────────────────────────────────────────────
            new(PhysicalKey.Backquote, "`", 0, 52, 40),
            new(PhysicalKey.Digit1, "1", 44, 52, 40),
            new(PhysicalKey.Digit2, "2", 88, 52, 40),
            new(PhysicalKey.Digit3, "3", 132, 52, 40),
            new(PhysicalKey.Digit4, "4", 176, 52, 40),
            new(PhysicalKey.Digit5, "5", 220, 52, 40),
            new(PhysicalKey.Digit6, "6", 264, 52, 40),
            new(PhysicalKey.Digit7, "7", 308, 52, 40),
            new(PhysicalKey.Digit8, "8", 352, 52, 40),
            new(PhysicalKey.Digit9, "9", 396, 52, 40),
            new(PhysicalKey.Digit0, "0", 440, 52, 40),
            new(PhysicalKey.Minus, "-", 484, 52, 40),
            new(PhysicalKey.Equal, "=", 528, 52, 40),
            new(PhysicalKey.Backspace, "Bksp", 572, 52, 86),
            new(PhysicalKey.Insert, "Ins", 722, 52, 40),
            new(PhysicalKey.Home, "Home", 766, 52, 40),
            new(PhysicalKey.PageUp, "PgUp", 810, 52, 40),
            new(PhysicalKey.NumLock, "NmLk", 856, 52, 40),
            new(PhysicalKey.NumPadDivide, "/", 900, 52, 40),
            new(PhysicalKey.NumPadMultiply, "*", 944, 52, 40),
            new(PhysicalKey.NumPadSubtract, "-", 988, 52, 40),

            // ── QWERTY row (y=96) ─────────────────────────────────────────────
            new(PhysicalKey.Tab, "Tab", 0, 96, 62),
            new(PhysicalKey.Q, "Q", 66, 96, 40),
            new(PhysicalKey.W, "W", 110, 96, 40),
            new(PhysicalKey.E, "E", 154, 96, 40),
            new(PhysicalKey.R, "R", 198, 96, 40),
            new(PhysicalKey.T, "T", 242, 96, 40),
            new(PhysicalKey.Y, "Y", 286, 96, 40),
            new(PhysicalKey.U, "U", 330, 96, 40),
            new(PhysicalKey.I, "I", 374, 96, 40),
            new(PhysicalKey.O, "O", 418, 96, 40),
            new(PhysicalKey.P, "P", 462, 96, 40),
            new(PhysicalKey.BracketLeft, "[", 506, 96, 40),
            new(PhysicalKey.BracketRight, "]", 550, 96, 40),
            new(PhysicalKey.Backslash, "\\", 594, 96, 64),
            new(PhysicalKey.Delete, "Del", 722, 96, 40),
            new(PhysicalKey.End, "End", 766, 96, 40),
            new(PhysicalKey.PageDown, "PgDn", 810, 96, 40),
            new(PhysicalKey.NumPad7, "7", 856, 96, 40),
            new(PhysicalKey.NumPad8, "8", 900, 96, 40),
            new(PhysicalKey.NumPad9, "9", 944, 96, 40),
            new(PhysicalKey.NumPadAdd, "+", 988, 96, 40, 84), // 2-row height

            // ── Home row (y=140) ──────────────────────────────────────────────
            new(PhysicalKey.CapsLock, "Caps", 0, 140, 74),
            new(PhysicalKey.A, "A", 78, 140, 40),
            new(PhysicalKey.S, "S", 122, 140, 40),
            new(PhysicalKey.D, "D", 166, 140, 40),
            new(PhysicalKey.F, "F", 210, 140, 40),
            new(PhysicalKey.G, "G", 254, 140, 40),
            new(PhysicalKey.H, "H", 298, 140, 40),
            new(PhysicalKey.J, "J", 342, 140, 40),
            new(PhysicalKey.K, "K", 386, 140, 40),
            new(PhysicalKey.L, "L", 430, 140, 40),
            new(PhysicalKey.Semicolon, ";", 474, 140, 40),
            new(PhysicalKey.Quote, "'", 518, 140, 40),
            new(PhysicalKey.Enter, "Enter", 562, 140, 96),
            new(PhysicalKey.NumPad4, "4", 856, 140, 40),
            new(PhysicalKey.NumPad5, "5", 900, 140, 40),
            new(PhysicalKey.NumPad6, "6", 944, 140, 40),

            // ── Shift row (y=184) ─────────────────────────────────────────────
            new(PhysicalKey.ShiftLeft, "LShift", 0, 184, 96),
            new(PhysicalKey.Z, "Z", 100, 184, 40),
            new(PhysicalKey.X, "X", 144, 184, 40),
            new(PhysicalKey.C, "C", 188, 184, 40),
            new(PhysicalKey.V, "V", 232, 184, 40),
            new(PhysicalKey.B, "B", 276, 184, 40),
            new(PhysicalKey.N, "N", 320, 184, 40),
            new(PhysicalKey.M, "M", 364, 184, 40),
            new(PhysicalKey.Comma, ",", 408, 184, 40),
            new(PhysicalKey.Period, ".", 452, 184, 40),
            new(PhysicalKey.Slash, "/", 496, 184, 40),
            new(PhysicalKey.ShiftRight, "RShift", 540, 184, 118),
            new(PhysicalKey.ArrowUp, "↑", 766, 184, 40),
            new(PhysicalKey.NumPad1, "1", 856, 184, 40),
            new(PhysicalKey.NumPad2, "2", 900, 184, 40),
            new(PhysicalKey.NumPad3, "3", 944, 184, 40),
            new(PhysicalKey.NumPadEnter, "Ent", 988, 184, 40, 84), // 2-row height

            // ── Modifier row (y=228) ──────────────────────────────────────────
            new(PhysicalKey.ControlLeft, "LCtrl", 0, 228, 62),
            new(PhysicalKey.MetaLeft, "LWin", 66, 228, 62),
            new(PhysicalKey.AltLeft, "LAlt", 132, 228, 62),
            new(PhysicalKey.Space, "Space", 198, 228, 276),
            new(PhysicalKey.AltRight, "RAlt", 478, 228, 62),
            new(PhysicalKey.MetaRight, "RWin", 544, 228, 62),
            new(PhysicalKey.ContextMenu, "Menu", 610, 228, 40),
            new(PhysicalKey.ControlRight, "RCtrl", 654, 228, 62),
            new(PhysicalKey.ArrowLeft, "←", 722, 228, 40),
            new(PhysicalKey.ArrowDown, "↓", 766, 228, 40),
            new(PhysicalKey.ArrowRight, "→", 810, 228, 40),
            new(PhysicalKey.NumPad0, "0", 856, 228, 84),
            new(PhysicalKey.NumPadDecimal, ".", 944, 228, 40)
        };

        return keys;
    }
}
