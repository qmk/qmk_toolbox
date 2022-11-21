import AppKit

class MicrocontrollerSelector: NSComboBox, NSComboBoxDelegate, NSComboBoxDataSource {
    var keys: [String] = []
    var values: [String] = []

    override init(frame frameRect: NSRect) {
        super.init(frame: frameRect)
        customInit()
    }

    required init?(coder: NSCoder) {
        super.init(coder: coder)
        customInit()
    }

    func customInit() {
        let path = Bundle.main.path(forResource: "mcu-list", ofType: "txt")!
        do {
            let fileContents = try String(contentsOfFile: path, encoding: .utf8)

            for microcontroller in fileContents.split(separator: "\n") {
                guard microcontroller.count > 0 else { return }
                let parts = microcontroller.components(separatedBy: ":")
                guard parts.count == 2 else { return }
                keys.append(parts[0])
                values.append(parts[1])
            }
        } catch {}

        dataSource = self
        delegate = self

        selectItem(withKey: UserDefaults.standard.string(forKey: "Microcontroller") ?? "atmega32u4")
    }

    func selectItem(withKey key: String) {
        guard let index = keys.firstIndex(of: key) else { return }
        selectItem(at: index)
    }

    func keyForSelectedItem() -> String {
        keys[indexOfSelectedItem]
    }

    func comboBox(_ comboBox: NSComboBox, objectValueForItemAt index: Int) -> Any? {
        values[index]
    }

    func numberOfItems(in comboBox: NSComboBox) -> Int {
        values.count
    }

    func comboBox(_ comboBox: NSComboBox, indexOfItemWithStringValue string: String) -> Int {
        values.firstIndex(of: string) ?? -1
    }

    func comboBoxSelectionDidChange(_ notification: Notification) {
        UserDefaults.standard.set(keyForSelectedItem(), forKey: "Microcontroller")
    }
}
