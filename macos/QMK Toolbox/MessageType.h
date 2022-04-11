typedef enum MessageType: NSUInteger {
    MessageType_Bootloader,
    MessageType_Command,
    MessageType_CommandError,
    MessageType_CommandOutput,
    MessageType_Error,
    MessageType_HID,
    MessageType_HIDOutput,
    MessageType_Info,
    MessageType_USB
} MessageType;
