using System.Text;
using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.Models;

public readonly record struct TerminalSegment(string Text, MessageType Type);

public class TerminalLine
{
    public List<TerminalSegment> Segments { get; } = [];
}

/// <summary>
/// A simple terminal-style buffer that handles \r (carriage return) and \n (newline) semantics.
/// 
/// - Regular characters: inserted/overwritten at the current column position within the active line.
///   The line extends as needed when writing past its length.
/// - '\r' (carriage return): resets column to 0 within the current line without adding a new line.
/// - '\n' (newline): flushes the current line to the completed buffer, creates a fresh line at col 0.
/// 
/// The active (current) line is always included in iteration via Lines property.
/// </summary>
public class TerminalBuffer
{
    private readonly List<TerminalLine> _lines = [];

    /// <summary> Raised whenever the buffer's contents change (write, clear, or trim). </summary>
    public event Action? Changed;

    /// <summary> Completed lines. </summary>
    public IReadOnlyList<TerminalLine> Lines => _lines;

    /// <summary> The currently active (in-progress) line being written to. </summary>
    public TerminalLine CurrentLine { get; private set; } = new();

    /// <summary> Cursor column within the current line. </summary>
    public int Col { get; private set; }

    public void Clear()
    {
        _lines.Clear();
        CurrentLine = new TerminalLine();
        Col = 0;
        Changed?.Invoke();
    }

    public void Write(string text, MessageType type)
    {
        if (text.Length == 0)
            return;

        foreach (char c in text)
        {
            switch (c)
            {
                case '\r':
                    Col = 0;
                    break;

                case '\n':
                    _lines.Add(CurrentLine);
                    CurrentLine = new TerminalLine();
                    Col = 0;
                    break;

                default:
                    InsertCharAt(c, type);
                    Col++;
                    break;
            }
        }

        Changed?.Invoke();
    }

    private void InsertCharAt(char ch, MessageType type)
    {
        List<TerminalSegment> segments = CurrentLine.Segments;
        int offset = 0;

        for (int i = 0; i < segments.Count; i++)
        {
            TerminalSegment seg = segments[i];
            if (offset + seg.Text.Length > Col)
            {
                // Overwrite the character at Col within an existing segment, keeping that
                // segment's colour (in-place overwrites — e.g. progress bars — reuse the
                // original type, so splitting the run for a colour change isn't worth it).
                var sb = new StringBuilder(seg.Text);
                sb[Col - offset] = ch;
                segments[i] = seg with { Text = sb.ToString() };
                return;
            }
            offset += seg.Text.Length;
        }

        // At or past the end: extend the trailing same-type segment so a line stays a few
        // contiguous runs (not one run per character), which keeps URL detection — and the
        // rendered inline count — sane.
        int last = segments.Count - 1;
        if (last >= 0 && segments[last].Type == type)
            segments[last] = segments[last] with { Text = segments[last].Text + ch };
        else
            segments.Add(new TerminalSegment(ch.ToString(), type));
    }

    /// <summary> Trim the oldest lines to stay within maxLines. </summary>
    public void TrimToMax(int maxLines)
    {
        int total = _lines.Count + 1; // completed + current line
        if (total <= maxLines)
            return;
        int excess = total - maxLines;
        if (excess > _lines.Count)
            excess = _lines.Count;
        _lines.RemoveRange(0, excess);
        Changed?.Invoke();
    }

    /// <summary> Returns the plain text content of all lines (for clipboard export). </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        bool first = true;
        foreach (TerminalLine line in AllLines())
        {
            if (!first)
                sb.AppendLine();
            foreach (TerminalSegment seg in line.Segments)
                sb.Append(seg.Text);
            first = false;
        }
        return sb.ToString();
    }

    private IEnumerable<TerminalLine> AllLines()
    {
        foreach (TerminalLine line in _lines)
            yield return line;
        if (CurrentLine.Segments.Count > 0)
            yield return CurrentLine;
    }
}
