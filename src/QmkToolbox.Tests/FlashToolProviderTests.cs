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

    // Realistic names from the qmk_flashutils / qmk_udev archives.
    [Theory]
    [InlineData("avrdude", true)]                  // tool binary, no extension
    [InlineData("dfu-programmer", true)]
    [InlineData("libhidapi-hidraw.so.0", true)]    // versioned shared library
    [InlineData("libusb-1.0.so", true)]
    [InlineData("libhidapi.dylib", true)]
    [InlineData("post-install.sh", true)]
    [InlineData("avrdude.conf", false)]
    [InlineData("reset.eep", false)]
    [InlineData("50-qmk.rules", false)]
    [InlineData("mcu-list.txt", false)]
    [InlineData("flashutils_release_linuxX64", false)] // extension-less, but a manifest
    [InlineData("qmk_udev_release_linuxX64", false)]
    public void IsExecutable_ClassifiesByFileType(string fileName, bool expected) =>
        Assert.Equal(expected, FlashToolProvider.IsExecutable($"/resources/{fileName}"));
}
