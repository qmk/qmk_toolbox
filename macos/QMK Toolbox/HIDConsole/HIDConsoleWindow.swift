import AppKit

class HIDConsoleWindow: NSPanel, NSWindowDelegate, HIDConsoleListenerDelegate {
    @IBOutlet var consoleListBox: NSComboBox!

    @IBOutlet var logTextView: LogTextView!

    @IBOutlet var clearMenuItem: NSMenuItem!

    override init(contentRect: NSRect, styleMask style: NSWindow.StyleMask, backing backingStoreType: NSWindow.BackingStoreType, defer flag: Bool) {
        super.init(contentRect: contentRect, styleMask: style, backing: backingStoreType, defer: flag)
        self.delegate = self
    }

    func windowDidBecomeKey(_ notification: Notification) {
        logTextView.menu?.addItem(NSMenuItem.separator())
        logTextView.menu?.addItem(clearMenuItem)

        consoleListener = HIDConsoleListener()
        consoleListener.delegate = self
        consoleListener.start()
    }

    func windowWillClose(_ notification: Notification) {
        consoleListener.stop()
    }

    var consoleListener: HIDConsoleListener!
    var lastReportedDevice: HIDConsoleDevice?

    func consoleDeviceDidConnect(_ device: HIDConsoleDevice) {
        lastReportedDevice = device
        updateConsoleList()
        logTextView.logHID("HID console connected: \(device)")
    }

    func consoleDeviceDidDisconnect(_ device: HIDConsoleDevice) {
        lastReportedDevice = nil
        updateConsoleList()
        logTextView.logHID("HID console disconnected: \(device)")
    }

    func consoleDevice(_ device: HIDConsoleDevice, didReceiveReport report: String) {
        let selectedDevice = consoleListBox.indexOfSelectedItem
        if selectedDevice == 0 || consoleListener.devices[selectedDevice - 1] == device {
            if lastReportedDevice != device {
                logTextView.logHID("\(device.manufacturer ?? "") \(device.product ?? "")")
                lastReportedDevice = device
            }
        }
        logTextView.logHIDOutput(report)
    }

    func updateConsoleList() {
        let selectedItem = consoleListBox.indexOfSelectedItem >= 0 ? consoleListBox.indexOfSelectedItem : 0
        consoleListBox.deselectItem(at: selectedItem)
        consoleListBox.removeAllItems()

        consoleListener.devices.forEach { device in
            consoleListBox.addItem(withObjectValue: device.description)
        }

        if consoleListBox.numberOfItems > 0 {
            consoleListBox.insertItem(withObjectValue: "(All connected devices)", at: 0)
            consoleListBox.selectItem(at: consoleListBox.numberOfItems > selectedItem ? selectedItem : 0)
        }
    }

    @IBAction
    func clearButtonClick(_ sender: Any) {
        logTextView.string = ""
    }
}
