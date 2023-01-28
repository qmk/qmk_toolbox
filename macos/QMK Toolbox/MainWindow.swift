import Cocoa

class MainWindow: NSWindow, NSDraggingDestination {
    override func awakeFromNib() {
        self.registerForDraggedTypes([.fileURL])
    }

    func draggingEntered(_ sender: NSDraggingInfo) -> NSDragOperation {
        let pasteboard = sender.draggingPasteboard
        if pasteboard.pasteboardItems?.count == 1 {
            if ["bin", "hex"].contains(NSURL(from: pasteboard)?.pathExtension) {
                return .copy
            }
        }
        return []
    }

    func performDragOperation(_ sender: NSDraggingInfo) -> Bool {
        guard let file = NSURL(from: sender.draggingPasteboard) as? URL else { return false }
        NotificationCenter.default.post(name: NSNotification.Name("OpenedFile"), object: file)
        return true
    }
}
