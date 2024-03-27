using System.Runtime.InteropServices;

namespace QMK_Toolbox.Helpers;

public static class LinuxPermissions
{
    [DllImport("libc", SetLastError = true)]
    private static extern int chmod(string pathname, int mode);

    // user permissions
    const int S_IRUSR = 0x100;
    const int S_IWUSR = 0x80;
    const int S_IXUSR = 0x40;

    // group permission
    const int S_IRGRP = 0x20;
    const int S_IWGRP = 0x10;
    const int S_IXGRP = 0x8;

    // other permissions
    const int S_IROTH = 0x4;
    const int S_IWOTH = 0x2;
    
    public static void MakeExecutable(string path)
    {
        const int _0755 =
            S_IRUSR | S_IXUSR | S_IWUSR
            | S_IRGRP | S_IXGRP | S_IROTH | S_IWOTH;
        chmod(path, _0755);
    }
}