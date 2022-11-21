import Foundation

class LUFAHIDDevice: BootloaderDevice {
    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        if revisionBCD == 0x0936 {
            name = "QMK HID"
            type = .qmkHid
        } else {
            name = "LUFA HID"
            type = .lufaHid
        }
        //resettable = true
    }

    override func flash(_ mcu: String, file: String) {
        runProcess("hid_bootloader_cli", args: ["-mmcu=\(mcu)", file, "-v"])
    }

    // hid_bootloader_cli 210130 lacks -b flag
    // Next LUFA release should have it thanks to abcminiuser/lufa#173
    //override func reset(_ mcu: String) {
    //    runProcess("hid_bootloader_cli", args: ["-mmcu=\(mcu)", "-bv"])
    //}
}
