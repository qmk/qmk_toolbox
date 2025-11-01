import Cocoa
import UniformTypeIdentifiers

class MainViewController: NSViewController, USBListenerDelegate {
    @IBOutlet var filepathBox: NSComboBox!
    @IBOutlet var mcuBox: MicrocontrollerSelector!
    @IBOutlet var logTextView: LogTextView!
    @IBOutlet var clearMenuItem: NSMenuItem!

    @objc dynamic var windowState = WindowState.shared

    // MARK: Window Events

    override func viewDidLoad() {
        super.viewDidLoad()
        loadRecentDocuments()

        logTextView.menu?.addItem(NSMenuItem.separator())
        logTextView.menu?.addItem(clearMenuItem)

        NotificationCenter.default.addObserver(forName: NSNotification.Name("AutoFlashEnabledChanged"), object: nil, queue: .main, using: autoFlashEnabledChanged(_:))
        NotificationCenter.default.addObserver(forName: NSNotification.Name("OpenedFile"), object: nil, queue: .main, using: openedFile(_:))

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
    }

    override func viewWillDisappear() {
        usbListener.stop()
    }

    func loadRecentDocuments() {
        NSDocumentController.shared.recentDocumentURLs.forEach { url in
            filepathBox.addItem(withObjectValue: url.path)
        }

        if filepathBox.numberOfItems > 0 {
            filepathBox.selectItem(at: 0)
        }
    }

    func clearRecentDocuments(_ sender: Any) {
        NSDocumentController.shared.clearRecentDocuments(sender)
        filepathBox.removeAllItems()
        filepathBox.stringValue = ""
    }

    func autoFlashEnabledChanged(_ notification: Notification) {
        guard let enabled = notification.object as? Bool else { return }
        logTextView.logInfo("Auto-Flash \(enabled ? "enabled" : "disabled")")
    }

    func openedFile(_ notification: Notification) {
        guard let file = notification.object as? URL else { return }
        setFilePath(file)
    }

    // MARK: USB Devices & Bootloaders

    var usbListener: USBListener!

    func bootloaderDeviceDidConnect(_ device: BootloaderDevice) {
        logTextView.logBootloader("\(device.name) device connected: \(device)")

        if windowState.autoFlashEnabled {
            flashAll()
        } else {
            enableUI()
        }
    }

    func bootloaderDeviceDidDisconnect(_ device: BootloaderDevice) {
        logTextView.logBootloader("\(device.name) device disconnected: \(device)")

        if !windowState.autoFlashEnabled {
            enableUI()
        }
    }

    func bootloaderDevice(_ device: BootloaderDevice, didReceiveCommandOutput data: String, type: MessageType) {
        logTextView.log(data, type: type)
    }

    func usbDeviceDidConnect(_ device: USBDevice) {
        if windowState.showAllDevices {
            logTextView.logUSB("USB device connected: \(device)")
        }
    }

    func usbDeviceDidDisconnect(_ device: USBDevice) {
        if windowState.showAllDevices {
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

        guard FileManager.default.fileExists(atPath: file) else {
            logTextView.logError("File does not exist")
            return
        }

        guard mcuBox.indexOfSelectedItem > 0 else {
            logTextView.logError("Please select a microcontroller")
            return
        }

        let mcu = mcuBox.keyForSelectedItem()

        if !windowState.autoFlashEnabled {
            disableUI()
        }

        logTextView.logBootloader("Attempting to flash, please don't remove device")

        for b in findBootloaders() {
            b.flash(mcu, file: file)
        }

        logTextView.logBootloader("Flash complete")

        if !windowState.autoFlashEnabled {
            enableUI()
        }
    }

    func resetAll() {
        guard mcuBox.indexOfSelectedItem > 0 else {
            logTextView.logError("Please select a microcontroller")
            return
        }

        let mcu = mcuBox.keyForSelectedItem()

        if !windowState.autoFlashEnabled {
            disableUI()
        }

        for b in findBootloaders() {
            if b.resettable {
                b.reset(mcu)
            }
        }

        if !windowState.autoFlashEnabled {
            enableUI()
        }
    }

    func clearEEPROMAll() {
        guard mcuBox.indexOfSelectedItem > 0 else {
             logTextView.logError("Please select a microcontroller")
             return
         }

        let mcu = mcuBox.keyForSelectedItem()

        if !windowState.autoFlashEnabled {
            disableUI()
        }

        logTextView.logBootloader("Attempting to clear EEPROM, please don't remove device")

        for b in findBootloaders() {
            if b.eepromFlashable {
                b.flashEEPROM(mcu, file: "reset.eep")
            }
        }

        logTextView.logBootloader("EEPROM clear complete")

        if !windowState.autoFlashEnabled {
            enableUI()
        }
    }

    func setHandednessAll(left: Bool) {
        guard mcuBox.indexOfSelectedItem > 0 else {
            logTextView.logError("Please select a microcontroller")
            return
        }

        let mcu = mcuBox.keyForSelectedItem()

        if !windowState.autoFlashEnabled {
            disableUI()
        }

        logTextView.logBootloader("Attempting to set handedness, please don't remove device")

        for b in findBootloaders() {
            if b.eepromFlashable {
                b.flashEEPROM(mcu, file: left ? "reset_left.eep" : "reset_right.eep")
            }
        }

        logTextView.logBootloader("EEPROM write complete")

        if !windowState.autoFlashEnabled {
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

    func enableUI() {
        let bootloaders = findBootloaders()
        windowState.canFlash = bootloaders.count > 0
        windowState.canReset = bootloaders.contains { $0.resettable }
        windowState.canClearEEPROM = bootloaders.contains { $0.eepromFlashable }
    }

    func disableUI() {
        windowState.canFlash = false
        windowState.canReset = false
        windowState.canClearEEPROM = false
    }

    @IBAction
    func openButtonClick(_ sender: Any) {
        guard let window = self.view.window else { return }

        let openPanel = NSOpenPanel()
        openPanel.canChooseDirectories = false
        openPanel.allowsMultipleSelection = false
        openPanel.message = "Select firmware to load"
        openPanel.allowedContentTypes = [
            UTType(filenameExtension: "bin")!,
            UTType(filenameExtension: "hex")!
        ]
        openPanel.beginSheetModal(for: window) { response in
            guard response == .OK, let file = openPanel.url else { return }
            self.setFilePath(file)
        }
    }

    @IBAction
    func updateFilePath(_ sender: Any) {
        if let filePath = URL(string: filepathBox.stringValue) {
            setFilePath(filePath)
        }
    }

    @IBAction
    func clearButtonClick(_ sender: Any) {
        logTextView.string = ""
    }

    func unwrapQmkUrl(_ url: URL) -> URL {
        let unwrappedPath = url.absoluteString.dropFirst(url.absoluteString.hasPrefix("qmk://") ? 6 : 4)
        return URL(string: String(unwrappedPath))!
    }

    func setFilePath(_ path: URL) {
        let url = path.scheme == "qmk" ? unwrapQmkUrl(path) : path
        guard ["hex", "bin"].contains(url.pathExtension.lowercased()) else { return }

        if path.scheme == "qmk" {
            downloadFile(url)
        } else {
            loadLocalFile(url.path)
        }
    }

    func loadLocalFile(_ path: String) {
        if filepathBox.indexOfItem(withObjectValue: path) == NSNotFound {
            filepathBox.addItem(withObjectValue: path)
        }
        filepathBox.selectItem(withObjectValue: path)
        NSDocumentController.shared.noteNewRecentDocumentURL(URL(fileURLWithPath: path))
    }

    func downloadFile(_ url: URL) {
        logTextView.logInfo("Downloading the file: \(url.absoluteString)")

        var request = URLRequest(url: url)
        request.setValue("QMK Toolbox", forHTTPHeaderField: "User-Agent")
        let downloadTask = URLSession.shared.downloadTask(with: request) { urlOrNil, responseOrNil, errorOrNil in
            guard let fileUrl = urlOrNil else { return }
            do {
                let downloadsUrl = try FileManager.default.url(for: .downloadsDirectory, in: .userDomainMask, appropriateFor: nil, create: false)
                let destFileUrl = downloadsUrl.appendingPathComponent(url.lastPathComponent)
                try FileManager.default.moveItem(at: fileUrl, to: destFileUrl)

                DispatchQueue.main.sync {
                    self.logTextView.logInfo("File saved to: \(destFileUrl.path)")
                    self.loadLocalFile(destFileUrl.path)
                }
            } catch {
                DispatchQueue.main.sync {
                    self.logTextView.logError("Could not download file: \(error.localizedDescription)")
                }
            }
        }
        downloadTask.resume()
    }
}
