using System.Text;
using QmkToolbox.Core.Services;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Tests for <see cref="FlashService.ReadLinesAsync"/> covering LF, CRLF, bare-CR,
/// trailing partial lines, and buffer-boundary splits.
/// </summary>
public class ReadLinesAsyncTests
{
    // Collects (text, overwrite) pairs emitted by ReadLinesAsync.
    private static async Task<List<(string Text, bool Overwrite)>> ReadAll(string input, Encoding? encoding = null)
    {
        byte[] bytes = (encoding ?? Encoding.UTF8).GetBytes(input);
        using var ms = new MemoryStream(bytes);
        using var reader = new StreamReader(ms, encoding ?? Encoding.UTF8);
        var results = new List<(string, bool)>();
        await FlashService.ReadLinesAsync(reader, CancellationToken.None, (line, overwrite) => results.Add((line, overwrite)));
        return results;
    }

    // ── Line endings ───────────────────────────────────────────────────────────

    [Fact]
    public async Task LF_EmitsLinesWithOverwriteFalse()
    {
        List<(string Text, bool Overwrite)> lines = await ReadAll("hello\nworld\n");

        Assert.Equal([("hello", false), ("world", false)], lines);
    }

    [Fact]
    public async Task CRLF_EmitsLinesWithOverwriteFalse()
    {
        List<(string Text, bool Overwrite)> lines = await ReadAll("hello\r\nworld\r\n");

        Assert.Equal([("hello", false), ("world", false)], lines);
    }

    [Fact]
    public async Task BareCR_EmitsLinesWithOverwriteTrue()
    {
        List<(string Text, bool Overwrite)> lines = await ReadAll("hello\rworld\r");

        Assert.Equal([("hello", true), ("world", true)], lines);
    }

    [Fact]
    public async Task MixedLineEndings_AllHandledCorrectly()
    {
        // Simulate avrdude-style output: progress on bare-CR, then final LF line.
        List<(string Text, bool Overwrite)> lines = await ReadAll("...\r......\rDone!\n");

        Assert.Equal([("...", true), ("......", true), ("Done!", false)], lines);
    }

    // ── CR followed by LF = CRLF, not two separate lines ──────────────────────

    [Fact]
    public async Task CRFollowedByLF_CountsAsSingleCRLF()
    {
        List<(string Text, bool Overwrite)> lines = await ReadAll("a\r\nb");

        // "a" as LF line, "b" as trailing partial — not "a" then "" then "b"
        Assert.Equal(2, lines.Count);
        Assert.Equal(("a", false), lines[0]);
        Assert.Equal(("b", false), lines[1]);
    }

    // ── Trailing partial lines (no terminator at EOF) ──────────────────────────

    [Fact]
    public async Task TrailingPartialLine_EmittedWithOverwriteFalse()
    {
        List<(string Text, bool Overwrite)> lines = await ReadAll("hello\nworld"); // "world" has no terminator

        Assert.Equal([("hello", false), ("world", false)], lines);
    }

    [Fact]
    public async Task TrailingPartialAfterBareCR_EmittedWithOverwriteTrue()
    {
        // Pending CR at EOF: the buffered content is emitted as overwrite=true.
        List<(string Text, bool Overwrite)> lines = await ReadAll("progress\rpartial\r");

        // "progress" is overwrite (bare CR), "partial" is overwrite (trailing CR with no following char)
        Assert.Equal([("progress", true), ("partial", true)], lines);
    }

    [Fact]
    public async Task EmptyInput_EmitsNothing()
    {
        List<(string Text, bool Overwrite)> lines = await ReadAll("");

        Assert.Empty(lines);
    }

    [Fact]
    public async Task OnlyNewline_EmitsOneEmptyLine()
    {
        List<(string Text, bool Overwrite)> lines = await ReadAll("\n");

        Assert.Equal([("", false)], lines);
    }

    // ── Buffer-boundary CR split ───────────────────────────────────────────────

    [Fact]
    public async Task CRAtBufferBoundary_HandledCorrectly()
    {
        // Force a CR to land at the very end of a 4096-char read buffer.
        // Pad with 4095 'x' chars so the CR is char 4096, then LF follows in the next read.
        string input = new string('x', 4095) + "\r\nfinal\n";
        List<(string Text, bool Overwrite)> lines = await ReadAll(input);

        Assert.Equal(2, lines.Count);
        Assert.Equal(new string('x', 4095), lines[0].Text);
        Assert.False(lines[0].Overwrite); // CRLF — not overwrite
        Assert.Equal(("final", false), lines[1]);
    }

    [Fact]
    public async Task BareCRAtBufferBoundary_EmittedAsOverwrite()
    {
        // CR at position 4096 followed by a non-LF char in the next read.
        // The pending CR is resolved when 'n' arrives: emits the buffered x's as overwrite=true,
        // then 'n' is processed normally as part of the next line.
        string input = new string('x', 4095) + "\rnext\n";
        List<(string Text, bool Overwrite)> lines = await ReadAll(input);

        Assert.Equal(2, lines.Count);
        Assert.Equal(new string('x', 4095), lines[0].Text);
        Assert.True(lines[0].Overwrite); // bare CR
        Assert.Equal(("next", false), lines[1]);
    }
}
