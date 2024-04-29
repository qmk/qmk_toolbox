import Cocoa

class HIDConsoleViewController: NSViewController, HIDListenerDelegate {
    @IBOutlet var consoleListBox: NSComboBox!

    @IBOutlet var logTextView: LogTextView!

    override func viewDidLoad() {
        hidListener = HIDListener()
        hidListener.delegate = self
        hidListener.start()
    }

    override func viewWillDisappear() {
        hidListener.stop()
    }

    @IBAction
    func clearButtonClick(_ sender: Any) {
        logTextView.string = ""
    }

    // MARK: HID Console Devices

    var hidListener: HIDListener!
    var lastReportedDevice: HIDConsoleDevice?
    func hidDeviceDidConnect(_ device: HIDDevice) {
        if device is HIDConsoleDevice {
            lastReportedDevice = (device as! HIDConsoleDevice)
            updateConsoleList()
            logTextView.logHID("HID console connected: \(device)")
        } else {
            logTextView.logHID("Raw HID device connected: \(device)")
        }
    }

    func hidDeviceDidDisconnect(_ device: HIDDevice) {
        if device is HIDConsoleDevice {
            lastReportedDevice = nil
            updateConsoleList()
            logTextView.logHID("HID console disconnected: \(device)")
        } else {
            logTextView.logHID("Raw HID device disconnected: \(device)")
        }
    }

    func consoleDevice(_ device: HIDConsoleDevice, didReceiveReport report: String) {
        let selectedDevice = consoleListBox.indexOfSelectedItem
        let consoleDevices = hidListener.devices.filter { $0 is HIDConsoleDevice }
        if selectedDevice == 0 || consoleDevices[selectedDevice - 1] == device {
            if lastReportedDevice != device {
                logTextView.logHID("\(device.manufacturer ?? "") \(device.product ?? "")")
                lastReportedDevice = device
            }
            logTextView.logHIDOutput(report)
        }
    }

    func updateConsoleList() {
        let selectedItem = consoleListBox.indexOfSelectedItem >= 0 ? consoleListBox.indexOfSelectedItem : 0
        consoleListBox.deselectItem(at: selectedItem)
        consoleListBox.removeAllItems()

        hidListener.devices.filter { $0 is HIDConsoleDevice }.forEach { device in
            consoleListBox.addItem(withObjectValue: device.description)
        }

        if consoleListBox.numberOfItems > 0 {
            consoleListBox.insertItem(withObjectValue: "(All connected devices)", at: 0)
            consoleListBox.selectItem(at: consoleListBox.numberOfItems > selectedItem ? selectedItem : 0)
        }
    }
}
