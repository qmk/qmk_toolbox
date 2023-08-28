import Cocoa

@main
class AppDelegate: NSObject, NSApplicationDelegate {
    @objc dynamic var windowState = WindowState.shared

    func application(_ sender: NSApplication, openFile filename: String) -> Bool {
        NotificationCenter.default.post(name: NSNotification.Name("OpenedFile"), object: URL(fileURLWithPath: filename))
        return true
    }

    func application(_ application: NSApplication, open urls: [URL]) {
        NotificationCenter.default.post(name: NSNotification.Name("OpenedFile"), object: urls[0])
    }

    func applicationSupportsSecureRestorableState(_ app: NSApplication) -> Bool {
        return true
    }

    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        return true
    }
}
