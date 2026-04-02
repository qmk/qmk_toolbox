using QmkToolbox.Desktop.Services;
using Xunit;

namespace QmkToolbox.Tests;

// xUnit v2 has no built-in conditional skip — a custom FactAttribute subclass
// sets Skip when the condition is false, making skipped tests visible in the runner
// rather than silently passing.
public class FactOnLinuxAttribute : FactAttribute
{
    public FactOnLinuxAttribute()
    {
        if (!OperatingSystem.IsLinux())
            Skip = "Linux-only test";
    }
}

public class FlashToolProviderTests
{
    private static FlashToolProvider Provider() => new();

    [Fact]
    public void GetResourceFolder_IsRooted() => Assert.True(Path.IsPathRooted(Provider().GetResourceFolder()));

    [Fact]
    public void GetResourceFolder_EndsWithQmkToolboxResources()
    {
        string folder = Provider().GetResourceFolder();
        Assert.EndsWith(Path.Combine("QMK", "Toolbox", "Resources"), folder);
    }

    [Fact]
    public void GetToolPath_ReturnsPathWithinResourceFolder()
    {
        FlashToolProvider provider = Provider();
        Assert.StartsWith(provider.GetResourceFolder(), provider.GetToolPath("avrdude"));
    }

    [Fact]
    public void GetToolPath_ContainsToolName() => Assert.Contains("avrdude", Provider().GetToolPath("avrdude"));

    [FactOnLinux]
    public void GetToolPath_NoExeSuffixOnLinux() =>
        Assert.DoesNotContain(".exe", Provider().GetToolPath("avrdude"));
}
