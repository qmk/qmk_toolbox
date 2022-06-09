import Foundation
import IOKit.storage

class LUFAMSDevice: BootloaderDevice {
    private var mountPoint: String?

    override init(usbDevice: USBDevice) {
        super.init(usbDevice: usbDevice)
        name = "LUFA MS"
        type = .lufaMs
        while mountPoint == nil {
            mountPoint = findMountPoint()
        }
    }

    override func flash(_ mcu: String, file: String) {
        guard file.lowercased().hasSuffix(".bin") else {
            print(message: "Only firmware files in .bin format can be flashed with this bootloader!", type: .error)
            return
        }

        let destFile = "\(mountPoint!)/FLASH.BIN"

        do {
            let fileManager = FileManager.default

            print(message: "Deleting \(destFile)...", type: .command)
            try fileManager.removeItem(atPath: destFile)

            print(message: "Copying \(file) to \(destFile)...", type: .command)
            try fileManager.copyItem(atPath: file, toPath: destFile)

            print(message: "Done, please eject drive now.", type: .command)
        } catch let error {
            print(message: "IO Error: \(error.localizedDescription)", type: .error)
        }
    }

    func findMountPoint() -> String? {
        let massStorageMatcher = IOServiceMatching(kIOMediaClass) as NSMutableDictionary
        massStorageMatcher[kIOMediaRemovableKey] = true

        var massStorageIterator: io_iterator_t = 0
        guard IOServiceGetMatchingServices(kIOMasterPortDefault, massStorageMatcher as CFDictionary, &massStorageIterator) == KERN_SUCCESS else { return nil }

        repeat {
            let media = IOIteratorNext(massStorageIterator)
            guard media != 0 else { break }

            let parentVendorID = usbDevice.vendorID(service: media)
            let parentProductID = usbDevice.productID(service: media)

            if parentVendorID == vendorID && parentProductID == productID {
                if let session = DASessionCreate(kCFAllocatorDefault) {
                    if let disk = DADiskCreateFromIOMedia(kCFAllocatorDefault, session, media) {
                        if let desc = DADiskCopyDescription(disk) as? [String: CFTypeRef] {
                            if let mountPoint = desc[kDADiskDescriptionVolumePathKey as String] as? URL {
                                return mountPoint.path
                            }
                        }
                    }
                }
            }
        } while true

        return nil
    }

    override var description: String {
        "\(super.description) [\(mountPoint!)]"
    }
}
