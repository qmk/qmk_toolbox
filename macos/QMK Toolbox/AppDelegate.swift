import Cocoa

@main
class AppDelegate: NSObject, NSApplicationDelegate, HIDConsoleListenerDelegate, USBListenerDelegate {
    @IBOutlet var window: QMKWindow!
    @IBOutlet var filepathBox: NSComboBox!
    @IBOutlet var mcuBox: MicrocontrollerSelector!
    @IBOutlet var logTextView: LogTextView!
    @IBOutlet var clearMenuItem: NSMenuItem!
    @IBOutlet var consoleListBox: NSComboBox!

    private var keyTesterWindowController: NSWindowController!

    @objc private dynamic var autoFlashEnabled: Bool = false {
        didSet {
            if autoFlashEnabled {
                logTextView.logInfo("Auto-Flash enabled")
                disableUI()
            } else {
                logTextView.logInfo("Auto-Flash disabled")
                enableUI()
            }
        }
    }
    @objc private dynamic var canFlash: Bool = false
    @objc private dynamic var canReset: Bool = false
    @objc private dynamic var canClearEEPROM: Bool = false
    @objc private dynamic var showAllDevices: Bool = false {
        didSet {
            UserDefaults.standard.set(showAllDevices, forKey: "ShowAllDevices")
        }
    }

    // MARK: App Delegate

    func application(_ sender: NSApplication, openFile filename: String) -> Bool {
        setFilePath(URL(fileURLWithPath: filename))
        return true
    }

    func applicationWillFinishLaunching(_ notification: Notification) {
        NSAppleEventManager.shared().setEventHandler(self, andSelector: #selector(handleGetUrlEvent(_:with:)), forEventClass: AEEventClass(kInternetEventClass), andEventID: AEEventID(kAEGetURL))
    }

    @objc func handleGetUrlEvent(_ event: NSAppleEventDescriptor, with reply: NSAppleEventDescriptor) {
        setFilePath(URL(string: (event.paramDescriptor(forKeyword: keyDirectObject)?.stringValue)!)!)
    }

    func applicationDidFinishLaunching(_ notification: Notification) {
        logTextView.menu?.addItem(NSMenuItem.separator())
        logTextView.menu?.addItem(clearMenuItem)

        loadRecentDocuments()
        showAllDevices = UserDefaults.standard.bool(forKey: "ShowAllDevices")

        let version = Bundle.main.infoDictionary!["CFBundleShortVersionString"] as! String
        logTextView.logInfo("QMK Toolbox \(version) (https://qmk.fm/toolbox)")
        logTextView.logInfo("Supported bootloaders:")
        logTextView.logInfo(" - ARM DFU (APM32, Kiibohd, STM32, STM32duino) and RISC-V DFU (GD32V) via dfu-util (http://dfu-util.sourceforge.net/)")
        logTextView.logInfo(" - Atmel/LUFA/QMK DFU via dfu-programmer (http://dfu-programmer.github.io/)")
        logTextView.logInfo(" - Atmel SAM-BA (Massdrop) via Massdrop Loader (https://github.com/massdrop/mdloader)")
        logTextView.logInfo(" - BootloadHID (Atmel, PS2AVRGB) via bootloadHID (https://www.obdev.at/products/vusb/bootloadhid.html)")
        logTextView.logInfo(" - Caterina (Arduino, Pro Micro) via avrdude (http://nongnu.org/avrdude/)")
        logTextView.logInfo(" - HalfKay (Teensy, Ergodox EZ) via Teensy Loader (https://pjrc.com/teensy/loader_cli.html)")
        logTextView.logInfo(" - LUFA/QMK HID via hid_bootloader_cli (https://github.com/abcminiuser/lufa)")
        logTextView.logInfo(" - WB32 DFU via wb32-dfu-updater_cli (https://github.com/WestberryTech/wb32-dfu-updater)")
        logTextView.logInfo(" - LUFA Mass Storage")
        logTextView.logInfo("Supported ISP flashers:")
        logTextView.logInfo(" - AVRISP (Arduino ISP)")
        logTextView.logInfo(" - USBasp (AVR ISP)")
        logTextView.logInfo(" - USBTiny (AVR Pocket)")

        usbListener = USBListener()
        usbListener.delegate = self
        usbListener.start()

        consoleListener = HIDConsoleListener()
        consoleListener.delegate = self
        consoleListener.start()
    }

    func loadRecentDocuments() {
        NSDocumentController.shared.recentDocumentURLs.forEach { url in
            filepathBox.addItem(withObjectValue: url.path)
        }

        if filepathBox.numberOfItems > 0 {
            filepathBox.selectItem(at: 0)
        }
    }

    @IBAction
    func clearRecentDocuments(_ sender: Any) {
        NSDocumentController.shared.clearRecentDocuments(sender)
        filepathBox.removeAllItems()
        filepathBox.stringValue = ""
    }

    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        true
    }

    func applicationWillTerminate(_ notification: Notification) {
        usbListener.stop()
        consoleListener.stop()
    }

    // MARK: HID Console

    private var consoleListener: HIDConsoleListener!
    private var lastReportedDevice: HIDConsoleDevice?

    public func consoleDeviceDidConnect(_ device: HIDConsoleDevice) {
        lastReportedDevice = device
        updateConsoleList()
        logTextView.logHID("HID console connected: \(device)")
    }

    public func consoleDeviceDidDisconnect(_ device: HIDConsoleDevice) {
        lastReportedDevice = nil
        updateConsoleList()
        logTextView.logHID("HID console disconnected: \(device)")
    }

    public func consoleDevice(_ device: HIDConsoleDevice, didReceiveReport report: String) {
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

    // MARK: USB Devices & Bootloaders

    private var usbListener: USBListener!

    public func bootloaderDeviceDidConnect(_ device: BootloaderDevice) {
        logTextView.logBootloader("\(device.name) device connected: \(device)")

        if autoFlashEnabled {
            flashAll()
        } else {
            enableUI()
        }
    }

    public func bootloaderDeviceDidDisconnect(_ device: BootloaderDevice) {
        logTextView.logBootloader("\(device.name) device disconnected: \(device)")

        if !autoFlashEnabled {
            enableUI()
        }
    }

    public func bootloaderDevice(_ device: BootloaderDevice, didReceiveCommandOutput data: String, type: MessageType) {
        DispatchQueue.main.sync {
            logTextView.log(data, type: type)
        }
    }

    public func usbDeviceDidConnect(_ device: USBDevice) {
        if showAllDevices {
            logTextView.logUSB("USB device connected: \(device)")
        }
    }

    public func usbDeviceDidDisconnect(_ device: USBDevice) {
        if showAllDevices {
            logTextView.logUSB("USB device disconnected: \(device)")
        }
    }

    // MARK: UI Interaction

    func flashAll() {
        let file = filepathBox.stringValue

        guard file.count > 0 else {
            logTextView.logError("Please select a file")
            return
        }

        guard mcuBox.indexOfSelectedItem > 0 else {
            logTextView.logError("Please select a microcontroller")
            return
        }

        let mcu = mcuBox.keyForSelectedItem()

        if !autoFlashEnabled {
            disableUI()
        }

        logTextView.logBootloader("Attempting to flash, please don't remove device")

        for b in findBootloaders() {
            b.flash(mcu, file: file)
        }

        logTextView.logBootloader("Flash complete")

        if !autoFlashEnabled {
            enableUI()
        }
    }

    func resetAll() {
        guard mcuBox.indexOfSelectedItem > 0 else {
            logTextView.logError("Please select a microcontroller")
            return
        }

        let mcu = mcuBox.keyForSelectedItem()

        if !autoFlashEnabled {
            disableUI()
        }

        for b in findBootloaders() {
            if b.resettable {
                b.reset(mcu)
            }
        }

        if !autoFlashEnabled {
            enableUI()
        }
    }

    func clearEEPROMAll() {
        guard mcuBox.indexOfSelectedItem > 0 else {
             logTextView.logError("Please select a microcontroller")
             return
         }

        let mcu = mcuBox.keyForSelectedItem()

        if !autoFlashEnabled {
            disableUI()
        }

        logTextView.logBootloader("Attempting to clear EEPROM, please don't remove device")

        for b in findBootloaders() {
            if b.eepromFlashable {
                b.flashEEPROM(mcu, file: "reset.eep")
            }
        }

        logTextView.logBootloader("EEPROM clear complete")

        if !autoFlashEnabled {
            enableUI()
        }
    }

    func setHandednessAll(left: Bool) {
        guard mcuBox.indexOfSelectedItem > 0 else {
            logTextView.logError("Please select a microcontroller")
            return
        }

        let mcu = mcuBox.keyForSelectedItem()

        if !autoFlashEnabled {
            disableUI()
        }

        logTextView.logBootloader("Attempting to set handedness, please don't remove device")

        for b in findBootloaders() {
            if b.eepromFlashable {
                b.flashEEPROM(mcu, file: left ? "left.eep" : "right.eep")
            }
        }

        logTextView.logBootloader("EEPROM write complete")

        if !autoFlashEnabled {
            enableUI()
        }
    }

    @IBAction
    func flashButtonClick(_ sender: Any) {
        flashAll()
    }

    @IBAction
    func resetButtonClick(_ sender: Any) {
        resetAll()
    }

    @IBAction
    func clearEEPROMButtonClick(_ sender: Any) {
        clearEEPROMAll()
    }

    @IBAction
    func setHandednessButtonClick(_ sender: NSView) {
        setHandednessAll(left: sender.tag == 0)
    }

    func findBootloaders() -> [BootloaderDevice] {
        usbListener.devices.filter { d in
            d is BootloaderDevice
        } as! [BootloaderDevice]
    }

    @IBAction
    func openButtonClick(_ sender: Any) {
        let openPanel = NSOpenPanel()
        openPanel.canChooseDirectories = false
        openPanel.allowsMultipleSelection = false
        openPanel.message = "Select firmware to load"
        openPanel.allowedFileTypes = ["bin", "hex"]
        openPanel.begin { response in
            if response == .OK {
                self.setFilePath(openPanel.urls[0])
            }
        }
    }

    @IBAction
    func updateFilePath(_ sender: Any) {
        if let filePath = URL(string: filepathBox.stringValue) {
            setFilePath(filePath)
        }
    }

    func setFilePath(_ path: URL) {
        guard ["hex", "bin"].contains(path.pathExtension.lowercased()) else { return }

        if path.scheme == "qmk" {
            let unwrappedPath = path.absoluteString.dropFirst(path.absoluteString.hasPrefix("qmk://") ? 6 : 4)
            let unwrappedUrl = NSURL(string: String(unwrappedPath))!
            downloadFile(unwrappedUrl)
        } else {
            loadLocalFile(path.path)
        }
    }

    func loadLocalFile(_ path: String) {
        if filepathBox.indexOfItem(withObjectValue: path) == NSNotFound {
            filepathBox.addItem(withObjectValue: path)
        }
        filepathBox.selectItem(withObjectValue: path)
        NSDocumentController.shared.noteNewRecentDocumentURL(URL(fileURLWithPath: path))
    }

    func downloadFile(_ url: NSURL) {
        do {
            let downloadsUrl = try FileManager.default.url(for: .downloadsDirectory, in: .userDomainMask, appropriateFor: nil, create: false)
            let destFileUrl = downloadsUrl.appendingPathComponent(url.lastPathComponent!)
            logTextView.logInfo("Downloading the file: \(url.absoluteString!)")
            let data = try Data(contentsOf: url as URL)
            try data.write(to: destFileUrl, options: .atomic)
            logTextView.logInfo("File saved to: \(destFileUrl.path)")
            loadLocalFile(destFileUrl.path)
        } catch let error {
            logTextView.logError("Could not download file: \(error.localizedDescription)")
        }
    }

    func enableUI() {
        let bootloaders = findBootloaders()
        canFlash = bootloaders.count > 0
        canReset = bootloaders.contains { $0.resettable }
        canClearEEPROM = bootloaders.contains { $0.eepromFlashable }
    }

    func disableUI() {
        canFlash = false
        canReset = false
        canClearEEPROM = false
    }

    @IBAction
    func keyTesterButtonClick(_ sender: Any) {
        if keyTesterWindowController == nil {
            keyTesterWindowController = NSWindowController(windowNibName: "KeyTesterWindow")
        }
        keyTesterWindowController.showWindow(self)
    }

    @IBAction
    func clearButtonClick(_ sender: Any) {
        logTextView.string = ""
    }
}
