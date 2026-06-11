using System.Diagnostics;
using System.Runtime.InteropServices;
using QmkToolbox.Core.Services;

namespace QmkToolbox.Desktop.Services;

/// <summary>
/// Linux-only udev installer using qmk_id and 50-qmk.rules (bundled resources).
/// Invokes pkexec to copy files into system directories and reload udev rules.
/// All methods are no-ops on non-Linux platforms.
/// </summary>
public static class LinuxUdevInstaller
{
    private const string QmkIdFilename = "qmk_id";
    private const string RulesFilename = "50-qmk.rules";

    public static async Task InstallAsync(IFlashToolProvider toolProvider, Action<string> logOutput, Action<string> logError)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return;

        string resourceFolder = toolProvider.GetResourceFolder();
        string qmkIdSrc = Path.Combine(resourceFolder, QmkIdFilename);
        string rulesSrc = Path.Combine(resourceFolder, RulesFilename);

        if (!File.Exists(qmkIdSrc) || !File.Exists(rulesSrc))
        {
            logError("udev resources not found. Please clear and re-extract resources via Tools → Clear Resources.");
            return;
        }

        // Embed paths in single-quoted shell strings to prevent metacharacter expansion
        // (e.g. '$', '`', '"' in a path would break double-quoted embedding).
        // Single-quote escaping: replace each ' with '\'' (end quote, literal, reopen).
        static string shellSingleQuote(string p) => "'" + p.Replace("'", "'\\''") + "'";

        string script = $"""
            #!/bin/sh
            # Remove existing QMK udev rules and helpers from all standard locations
            for dir in /etc/udev/rules.d /run/udev/rules.d /usr/lib/udev/rules.d /usr/local/lib/udev/rules.d /lib/udev/rules.d; do
                for f in "$dir"/*-qmk.rules; do
                    [ -e "$f" ] && echo "Removing existing $f" && rm -f "$f"
                done
            done
            for dir in /usr/lib/udev /usr/local/lib/udev /lib/udev; do
                [ -e "$dir/qmk_id" ] && echo "Removing existing $dir/qmk_id" && rm -f "$dir/qmk_id"
            done

            echo "Installing /usr/lib/udev/qmk_id..." &&
            install -m 0755 {shellSingleQuote(qmkIdSrc)} /usr/lib/udev/qmk_id &&
            echo "Installing /etc/udev/rules.d/50-qmk.rules..." &&
            install -m 0644 {shellSingleQuote(rulesSrc)} /etc/udev/rules.d/50-qmk.rules &&
            echo "Reloading udev rules..." &&
            udevadm control --reload-rules &&
            udevadm trigger &&
            echo "Done."
            """;

        // Create an owner-only temp directory so no other local user can race-replace
        // the script between writing it and pkexec executing it.
        DirectoryInfo tmpDir = Directory.CreateTempSubdirectory("qmk_udev_");
        File.SetUnixFileMode(tmpDir.FullName, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        string scriptPath = Path.Combine(tmpDir.FullName, "install.sh");
        try
        {
            File.WriteAllText(scriptPath, script);
            File.SetUnixFileMode(scriptPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);

            var startInfo = new ProcessStartInfo("pkexec") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            startInfo.ArgumentList.Add("/bin/sh");
            startInfo.ArgumentList.Add(scriptPath);
            using var process = new Process { StartInfo = startInfo };

            try
            {
                if (!process.Start())
                {
                    logError("Could not start pkexec.");
                    return;
                }

                Task stdoutTask = ReadLinesAsync(process.StandardOutput, logOutput);
                Task stderrTask = ReadLinesAsync(process.StandardError, logError);

                await Task.WhenAll(stdoutTask, stderrTask);
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                    logError($"udev installation failed with exit code {process.ExitCode}.");
            }
            catch (Exception ex)
            {
                logError($"udev installation failed: {ex.Message}");
            }
        }
        finally
        {
            try
            {
                tmpDir.Delete(recursive: true);
            }
            catch { }
        }
    }

    private static async Task ReadLinesAsync(StreamReader reader, Action<string> onLine)
    {
        string? line;
        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
            onLine(line);
    }
}
