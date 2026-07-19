using System.Text.RegularExpressions;
using QmkToolbox.Core.Models;

namespace QmkToolbox.Desktop.Models;

/// <summary>
/// Projects a <see cref="TerminalBuffer"/> into a flat list of <see cref="TerminalRun"/> ready
/// for a view to render. This is the single place that resolves line prefixes, splits URLs out
/// of segments, and assigns absolute text offsets — so the recurring selection / URL / offset
/// logic is pure and unit-testable, and the view is a thin run-to-inline adapter.
/// </summary>
public static class TerminalProjection
{
    // Same pattern the log has always used: an http(s) URL up to the first whitespace or
    // closing bracket/quote.
    private static readonly Regex UrlRegex = new(@"https?://[^\s\)\]}>""']+", RegexOptions.Compiled);

    /// <summary>
    /// Flattens the buffer's completed lines and in-progress line into runs. Each completed line
    /// is followed by a <see cref="TerminalRunKind.LineBreak"/> run (one offset); the current line
    /// is emitted without a trailing break, matching the buffer's cursor semantics.
    /// </summary>
    public static IReadOnlyList<TerminalRun> ToRuns(TerminalBuffer buffer)
    {
        var runs = new List<TerminalRun>();
        int pos = 0;

        foreach (TerminalLine line in buffer.Lines)
        {
            AppendLine(line, runs, ref pos);
            runs.Add(new TerminalRun("\n", LineType(line), TerminalRunKind.LineBreak, pos, null));
            pos += 1;
        }

        if (buffer.CurrentLine.Segments.Count > 0)
            AppendLine(buffer.CurrentLine, runs, ref pos);

        return runs;
    }

    private static MessageType LineType(TerminalLine line) =>
        line.Segments.Count > 0 ? line.Segments[0].Type : default;

    private static void AppendLine(TerminalLine line, List<TerminalRun> runs, ref int pos)
    {
        if (line.Segments.Count == 0)
            return;

        // The line's prefix is keyed off its first segment's type.
        MessageType lineType = line.Segments[0].Type;
        string prefix = MessageTypeDescriptors.For(lineType).Prefix;
        if (prefix.Length > 0)
        {
            runs.Add(new TerminalRun(prefix, lineType, TerminalRunKind.Prefix, pos, null));
            pos += prefix.Length;
        }

        foreach (TerminalSegment seg in line.Segments)
        {
            string text = seg.Text;
            int lastIndex = 0;

            foreach (Match match in UrlRegex.Matches(text))
            {
                if (match.Index > lastIndex)
                {
                    string chunk = text[lastIndex..match.Index];
                    runs.Add(new TerminalRun(chunk, seg.Type, TerminalRunKind.Text, pos, null));
                    pos += chunk.Length;
                }

                string url = match.Value;
                runs.Add(new TerminalRun(url, seg.Type, TerminalRunKind.Url, pos, url));
                pos += url.Length;

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < text.Length)
            {
                string remaining = text[lastIndex..];
                runs.Add(new TerminalRun(remaining, seg.Type, TerminalRunKind.Text, pos, null));
                pos += remaining.Length;
            }
        }
    }
}
