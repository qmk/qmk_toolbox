import Foundation

@objc
class WindowState: NSObject {
    static let shared = WindowState()

    @objc dynamic var canFlash: Bool = false
    @objc dynamic var canClearEEPROM: Bool = false
    @objc dynamic var canReset: Bool = false

    @objc dynamic var autoFlashEnabled: Bool = false {
        didSet {
            NotificationCenter.default.post(name: NSNotification.Name("AutoFlashEnabledChanged"), object: autoFlashEnabled)
        }
    }

    @objc dynamic var showAllDevices: Bool = UserDefaults.standard.bool(forKey: "ShowAllDevices") {
        didSet {
            UserDefaults.standard.set(showAllDevices, forKey: "ShowAllDevices")
        }
    }
}
