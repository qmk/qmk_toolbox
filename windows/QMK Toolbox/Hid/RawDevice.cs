using HidLibrary;

namespace QMK_Toolbox.Hid
{
    public class RawDevice : BaseHidDevice
    {
        public RawDevice(IHidDevice device) : base(device)
        {
        }
    }
}
