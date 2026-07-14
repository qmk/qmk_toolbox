using System.Text;
using NSubstitute;
using QmkToolbox.Core.Models;
using QmkToolbox.Core.Services;
using Xunit;

namespace QmkToolbox.Tests;

/// <summary>
/// Verifies that <see cref="FlashService.RunToolAsync"/> forwards tool output as a raw
/// stream with embedded '\r'/'\n' intact.
/// </summary>
public class RunToolAsyncTests
{
    [Fact]
    public async Task RunToolAsync_ForwardsOutputRaw_WithCrAndLfIntact()
    {
        IFlashToolProvider provider = Substitute.For<IFlashToolProvider>();
        provider.GetToolPath(Arg.Any<string>()).Returns("/usr/bin/printf");
        provider.GetResourceFolder().Returns(Path.GetTempPath());

        const string payload = "\rErase 0%\rErase 100%\nDone\n";
        var received = new StringBuilder();
        int exitCode = await FlashService.RunToolAsync("printf", ["%s", payload], provider,
            (data, type) =>
            {
                if (type == MessageType.CommandOutput)
                    received.Append(data);
            });

        Assert.Equal(0, exitCode);
        Assert.Equal(payload, received.ToString());
    }
}
