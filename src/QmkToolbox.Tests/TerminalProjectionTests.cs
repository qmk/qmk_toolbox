using QmkToolbox.Core.Models;
using QmkToolbox.Desktop.Models;
using Xunit;

namespace QmkToolbox.Tests;

public class TerminalProjectionTests
{
    // Bootloader has an empty prefix, so it isolates offset behaviour from prefix noise.
    private const MessageType Plain = MessageType.Bootloader;

    private static TerminalBuffer BufferOf(string text, MessageType type = MessageType.Bootloader)
    {
        var buffer = new TerminalBuffer();
        buffer.Write(text, type);
        return buffer;
    }

    [Fact]
    public void ToRuns_PlainLine_HasPrefixThenText()
    {
        // Info renders a "* " prefix; the prefix occupies the first two offsets.
        IReadOnlyList<TerminalRun> runs = TerminalProjection.ToRuns(BufferOf("hello", MessageType.Info));

        Assert.Collection(runs,
            r => { Assert.Equal(TerminalRunKind.Prefix, r.Kind); Assert.Equal("* ", r.Text); Assert.Equal(0, r.Start); },
            r => { Assert.Equal(TerminalRunKind.Text, r.Kind); Assert.Equal("hello", r.Text); Assert.Equal(2, r.Start); });
        Assert.Equal(7, runs.TotalLength());
    }

    [Fact]
    public void ToRuns_CarriageReturn_OverwritesKeepsOffsets()
    {
        // "aaaaa" then \r home then "bb" overwrites the first two columns -> "bbaaa".
        IReadOnlyList<TerminalRun> runs = TerminalProjection.ToRuns(BufferOf("aaaaa\rbb", Plain));

        Assert.Collection(runs,
            r => { Assert.Equal(TerminalRunKind.Text, r.Kind); Assert.Equal("bbaaa", r.Text); Assert.Equal(0, r.Start); });
        Assert.Equal(5, runs.TotalLength());
    }

    [Fact]
    public void ToRuns_Newline_CountsAsOneOffset()
    {
        IReadOnlyList<TerminalRun> runs = TerminalProjection.ToRuns(BufferOf("ab\ncd", Plain));

        Assert.Collection(runs,
            r => { Assert.Equal(TerminalRunKind.Text, r.Kind); Assert.Equal("ab", r.Text); Assert.Equal(0, r.Start); },
            r => { Assert.Equal(TerminalRunKind.LineBreak, r.Kind); Assert.Equal("\n", r.Text); Assert.Equal(2, r.Start); },
            r => { Assert.Equal(TerminalRunKind.Text, r.Kind); Assert.Equal("cd", r.Text); Assert.Equal(3, r.Start); });
        Assert.Equal(5, runs.TotalLength());
    }

    [Fact]
    public void ToRuns_SingleUrl_SplitsIntoThreeRuns()
    {
        IReadOnlyList<TerminalRun> runs = TerminalProjection.ToRuns(BufferOf("see https://x.com now", Plain));

        Assert.Collection(runs,
            r => { Assert.Equal(TerminalRunKind.Text, r.Kind); Assert.Equal("see ", r.Text); Assert.Equal(0, r.Start); },
            r => { Assert.Equal(TerminalRunKind.Url, r.Kind); Assert.Equal("https://x.com", r.Text); Assert.Equal("https://x.com", r.Url); Assert.Equal(4, r.Start); },
            r => { Assert.Equal(TerminalRunKind.Text, r.Kind); Assert.Equal(" now", r.Text); Assert.Equal(17, r.Start); });
        Assert.Equal(21, runs.TotalLength());
    }

    [Fact]
    public void ToRuns_MultipleUrls_OffsetsContiguous()
    {
        IReadOnlyList<TerminalRun> runs = TerminalProjection.ToRuns(BufferOf("https://a.com https://b.com", Plain));

        Assert.Collection(runs,
            r => { Assert.Equal(TerminalRunKind.Url, r.Kind); Assert.Equal("https://a.com", r.Text); Assert.Equal(0, r.Start); },
            r => { Assert.Equal(TerminalRunKind.Text, r.Kind); Assert.Equal(" ", r.Text); Assert.Equal(13, r.Start); },
            r => { Assert.Equal(TerminalRunKind.Url, r.Kind); Assert.Equal("https://b.com", r.Text); Assert.Equal(14, r.Start); });

        // Every run begins exactly where the previous one ended.
        for (int i = 1; i < runs.Count; i++)
            Assert.Equal(runs[i - 1].Start + runs[i - 1].Text.Length, runs[i].Start);
        Assert.Equal(27, runs.TotalLength());
    }

    [Fact]
    public void ToRuns_UrlAtLineEnd_NoTrailingTextRun()
    {
        IReadOnlyList<TerminalRun> runs = TerminalProjection.ToRuns(BufferOf("go https://a.com", Plain));

        Assert.Collection(runs,
            r => { Assert.Equal(TerminalRunKind.Text, r.Kind); Assert.Equal("go ", r.Text); Assert.Equal(0, r.Start); },
            r => { Assert.Equal(TerminalRunKind.Url, r.Kind); Assert.Equal("https://a.com", r.Text); Assert.Equal(3, r.Start); });
        Assert.Equal(16, runs.TotalLength());
    }

    [Fact]
    public void ToRuns_TotalLength_GrowsOnAppend()
    {
        var buffer = new TerminalBuffer();
        buffer.Write("abc", Plain);
        int before = TerminalProjection.ToRuns(buffer).TotalLength();

        buffer.Write("def", Plain);
        int after = TerminalProjection.ToRuns(buffer).TotalLength();

        Assert.Equal(3, before);
        Assert.Equal(6, after);
        Assert.True(after >= before);
    }

    [Fact]
    public void ToRuns_TotalLength_ShrinksOnClear()
    {
        TerminalBuffer buffer = BufferOf("abc\ndef", Plain);
        int before = TerminalProjection.ToRuns(buffer).TotalLength();

        buffer.Clear();
        int after = TerminalProjection.ToRuns(buffer).TotalLength();

        Assert.True(before > 0);
        Assert.Equal(0, after);
    }
}
