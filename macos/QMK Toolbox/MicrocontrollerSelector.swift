import Foundation

@objc
public class MicrocontrollerSelector: NSComboBox, NSComboBoxDelegate, NSComboBoxDataSource {
    var keys: [String] = []
    var values: [String] = []

    public override init(frame frameRect: NSRect) {
        super.init(frame: frameRect)
        customInit()
    }

    public required init?(coder: NSCoder) {
        super.init(coder: coder)
        customInit()
    }

    func customInit() {
        let path = Bundle.main.path(forResource: "mcu-list", ofType: "txt")!
        do {
            let fileContents = try String(contentsOfFile: path, encoding: .utf8)

            for microcontroller in fileContents.components(separatedBy: "\n") {
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

    @objc
    public func keyForSelectedItem() -> String {
        keys[indexOfSelectedItem]
    }

    public func comboBox(_ comboBox: NSComboBox, objectValueForItemAt index: Int) -> Any? {
        values[index]
    }

    public func numberOfItems(in comboBox: NSComboBox) -> Int {
        values.count
    }

    public func comboBox(_ comboBox: NSComboBox, indexOfItemWithStringValue string: String) -> Int {
        values.firstIndex(of: string) ?? -1
    }

    public func comboBoxSelectionDidChange(_ notification: Notification) {
        UserDefaults.standard.set(keyForSelectedItem(), forKey: "Microcontroller")
    }
}