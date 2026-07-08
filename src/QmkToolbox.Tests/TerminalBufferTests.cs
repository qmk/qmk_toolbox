using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Models;
using Xunit;

namespace QmkToolbox.Tests;

public class TerminalBufferTests
{
    private static string Render(TerminalBuffer buffer) => buffer.ToString();

    [Fact]
    public void Newline_CommitsLine()
    {
        var buffer = new TerminalBuffer();
        buffer.Write("hello\n", MessageType.CommandOutput);
        Assert.Equal("hello", Render(buffer));
    }

    [Fact]
    public void MultipleWrites_WithoutTerminator_StayOnSameLine()
    {
        var buffer = new TerminalBuffer();
        buffer.Write("foo", MessageType.CommandOutput);
        buffer.Write("bar", MessageType.CommandOutput);
        Assert.Equal("foobar", Render(buffer));
    }

    [Fact]
    public void SameTypeText_CoalescesIntoOneSegment()
    {
        // A whole line of same-type text must be one contiguous segment, otherwise
        // per-segment URL detection (and rendering) never sees the full text.
        var buffer = new TerminalBuffer();
        buffer.Write("visit http://qmk.fm now\n", MessageType.Info);
        Assert.Single(buffer.Lines);
        Assert.Single(buffer.Lines[0].Segments);
        Assert.Equal("visit http://qmk.fm now", buffer.Lines[0].Segments[0].Text);
    }

    [Fact]
    public void DifferentTypes_StaySeparateSegments()
    {
        var buffer = new TerminalBuffer();
        buffer.Write("out", MessageType.CommandOutput);
        buffer.Write("err", MessageType.Error);
        Assert.Equal(2, buffer.CurrentLine.Segments.Count);
        Assert.Equal("out", buffer.CurrentLine.Segments[0].Text);
        Assert.Equal("err", buffer.CurrentLine.Segments[1].Text);
    }

    [Fact]
    public void EmbeddedNewlines_SplitIntoLines()
    {
        var buffer = new TerminalBuffer();
        buffer.Write("a\nb\nc\n", MessageType.CommandOutput);
        Assert.Equal("a\nb\nc", Render(buffer).ReplaceLineEndings("\n"));
    }

    [Fact]
    public void CarriageReturn_OverwritesFromColumnZero()
    {
        // The canonical case: "aaa\rbb\n" renders as "bba".
        var buffer = new TerminalBuffer();
        buffer.Write("aaa\rbb\n", MessageType.CommandOutput);
        Assert.Equal("bba", Render(buffer));
    }

    [Fact]
    public void CarriageReturn_ShorterOverwrite_LeavesTrailingChars()
    {
        var buffer = new TerminalBuffer();
        buffer.Write("12345\rAB\n", MessageType.CommandOutput);
        Assert.Equal("AB345", Render(buffer));
    }

    [Fact]
    public void CarriageReturn_LongerOverwrite_Extends()
    {
        var buffer = new TerminalBuffer();
        buffer.Write("ab\rWXYZ\n", MessageType.CommandOutput);
        Assert.Equal("WXYZ", Render(buffer));
    }

    [Fact]
    public void ProgressSequence_OverwritesInPlace_UntilCommitted()
    {
        // Mimics a progress bar: repeated overwrites of the current line, then a final line.
        var buffer = new TerminalBuffer();
        buffer.Write("10%\r", MessageType.CommandOutput);
        buffer.Write("55%\r", MessageType.CommandOutput);
        buffer.Write("100%\n", MessageType.CommandOutput);
        Assert.Equal("100%", Render(buffer));
    }

    [Fact]
    public void Clear_EmptiesBuffer()
    {
        var buffer = new TerminalBuffer();
        buffer.Write("something\n", MessageType.CommandOutput);
        buffer.Clear();
        Assert.Equal(string.Empty, Render(buffer));
        Assert.Empty(buffer.Lines);
    }

    [Fact]
    public void TrimToMax_DropsOldestLines()
    {
        var buffer = new TerminalBuffer();
        for (int i = 0; i < 10; i++)
            buffer.Write($"line{i}\n", MessageType.CommandOutput);
        buffer.TrimToMax(3);
        // 10 committed lines + 1 empty current line = 11; trim to 3 keeps the last 2 committed
        // lines plus the current line.
        Assert.True(buffer.Lines.Count <= 3);
        Assert.Equal("line9", string.Concat(buffer.Lines[^1].Segments.ConvertAll(s => s.Text)));
    }

    [Fact]
    public void Changed_RaisedOnWriteAndClear()
    {
        var buffer = new TerminalBuffer();
        int count = 0;
        buffer.Changed += () => count++;
        buffer.Write("x\n", MessageType.CommandOutput);
        buffer.Clear();
        Assert.Equal(2, count);
    }

    [Fact]
    public void EmptyWrite_DoesNothing()
    {
        var buffer = new TerminalBuffer();
        int count = 0;
        buffer.Changed += () => count++;
        buffer.Write(string.Empty, MessageType.CommandOutput);
        Assert.Equal(0, count);
        Assert.Equal(string.Empty, Render(buffer));
    }
}
