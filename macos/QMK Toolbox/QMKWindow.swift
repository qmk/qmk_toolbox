import Foundation
import AppKit

class QMKWindow: NSWindow, NSDraggingDestination {
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
        let pasteboard = sender.draggingPasteboard
        guard let file = (NSURL(from: pasteboard) as URL?) else { return false }
        (NSApplication.shared.delegate as! AppDelegate).setFilePath(file)
        return true
    }
}
