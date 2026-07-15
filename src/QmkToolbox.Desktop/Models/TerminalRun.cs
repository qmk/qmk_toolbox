using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.Models;

/// <summary>How a <see cref="TerminalRun"/> should be rendered.</summary>
public enum TerminalRunKind
{
    /// <summary>Ordinary body text.</summary>
    Text,

    /// <summary>A line's leading marker (e.g. "&gt; ", "* "), styled with the prefix foreground.</summary>
    Prefix,

    /// <summary>A hyperlink; <see cref="TerminalRun.Url"/> is non-null.</summary>
    Url,

    /// <summary>A line break; occupies one offset position and carries no visible glyphs.</summary>
    LineBreak,
}

/// <summary>
/// A single contiguous piece of rendered terminal output with its absolute offset in the
/// flattened text. The projection computes every offset — including the one consumed by each
/// <see cref="TerminalRunKind.LineBreak"/> — so text-selection and URL hit-testing agree on
/// positions without the view re-deriving them.
/// </summary>
/// <param name="Text">The run's characters. For <see cref="TerminalRunKind.LineBreak"/> this is "\n".</param>
/// <param name="Type">The originating message type (drives colour; ignored for line breaks).</param>
/// <param name="Kind">How to render the run.</param>
/// <param name="Start">Absolute offset of the run's first character in the flattened text.</param>
/// <param name="Url">The link target when <see cref="Kind"/> is <see cref="TerminalRunKind.Url"/>, else null.</param>
public readonly record struct TerminalRun(
    string Text,
    MessageType Type,
    TerminalRunKind Kind,
    int Start,
    string? Url);

public static class TerminalRunExtensions
{
    /// <summary>Total length of the flattened text — the exclusive end offset of the last run.</summary>
    public static int TotalLength(this IReadOnlyList<TerminalRun> runs) =>
        runs.Count == 0 ? 0 : runs[^1].Start + runs[^1].Text.Length;
}
