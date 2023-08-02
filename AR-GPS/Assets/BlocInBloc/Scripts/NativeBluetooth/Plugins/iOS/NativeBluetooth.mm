//
//  NativeBluetoothBridge.m
//  NativeBluetooth
//
//  Created by Kévin ETOURNEAU on 24/02/2022.
//

#import "NativeBluetooth.h"
#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>
#include "UnityFramework/UnityFramework-Swift.h"
#include "UnityInterface.h"

NativeBluetooth* nativeBluetooth;

extern "C" {
    void Initialize() {
        nativeBluetooth = [[NativeBluetooth alloc] init];
        [[BluetoothPlugin shared] Initialize];
    }

    char* GetDevices() {
        if (nativeBluetooth != nil) {
            return [nativeBluetooth getDevices];
        } else {
            NSLog(@"Error: nativeBluetooth not initialized");
        }
        return nil;
    }

    bool SetupController(char* connectionId) {
        if (nativeBluetooth != nil) {
            return [nativeBluetooth setupController:(connectionId)];
        } else {
            NSLog(@"Error: nativeBluetooth not initialized");
        }
        return false;
    }

    bool OpenSession() {
        if (nativeBluetooth != nil) {
            return [nativeBluetooth openSession];
        } else {
            NSLog(@"Error: nativeBluetooth not initialized");
        }
        return  false;
    }
   
    void CloseSession() {
        if (nativeBluetooth != nil) {
            [nativeBluetooth closeSession];
        } else {
            NSLog(@"Error: nativeBluetooth not initialized");
        }
    }

    void OpenZenoConnect() {
        NSURL* zenoConnectURL = [NSURL URLWithString:@"ZenoConnect://"];
        if ([[UIApplication sharedApplication] canOpenURL:(zenoConnectURL)]) {
            [[UIApplication sharedApplication] openURL:zenoConnectURL options:@{} completionHandler:nil];
        } else {
            [[UIApplication sharedApplication] openURL:[NSURL URLWithString:@"https://apps.apple.com/fr/app/zeno-connect/id1310344749"] options:@{} completionHandler:nil];
        }
    }
}

@implementation NativeBluetooth : NSObject

- (instancetype) init {
    self = [super init];
    return self;
}

+ (void) sendMessageToUnity:(NSString *)methodName message:(NSString *)msg {
    UnitySendMessage("NativeBluetooth", methodName.UTF8String, msg.UTF8String);
}

+ (char *) convertNSStringToCString:(NSString *)nsString {
    if (nsString == NULL) return NULL;

    const char *nsStringUtf8 = [nsString UTF8String];
    //create a null terminated C string on the heap so that our string's memory isn't wiped out right after method's return
    char *cString = (char *)malloc(strlen(nsStringUtf8) + 1);
    strcpy(cString, nsStringUtf8);

    return cString;
}

- (char*) getDevices {
    return [NativeBluetooth convertNSStringToCString: [[BluetoothPlugin shared] GetConnectedAccessories]];
}

- (bool) setupController:(char*)connectionId {
    return [[BluetoothPlugin shared] SetupControllerWithConnectionId:[NSString stringWithUTF8String:connectionId].intValue];
}

- (bool) openSession {
    return [[BluetoothPlugin shared] OpenSession];
}

- (void) closeSession {
    [[BluetoothPlugin shared] CloseSession];
}

@end
