#import <Foundation/Foundation.h>

@protocol USBDevice <NSObject>

@property io_service_t service;

@property NSString *manufacturerString;

@property NSString *productString;

@property ushort vendorID;

@property ushort productID;

@property ushort revisionBCD;

@end

@interface USBDevice : NSObject<USBDevice>

- (id)initWithService:(io_service_t)service;

- (ushort)vendorIDForService:(io_service_t)service;

- (ushort)productIDForService:(io_service_t)service;

-(NSString *)findSerialPort;

@end
