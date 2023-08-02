//
//  NativeCompass.h
//
//  Created by BlocInBloc on 20/01/2020.
//  Copyright © 2020 BlocInBloc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>

@interface NativeBluetooth : NSObject
+ (void) sendMessageToUnity:(NSString *)methodName message:(NSString *)msg;
+ (char*) convertNSStringToCString:(NSString*) nsString;
- (char*) getDevices;
- (bool) setupController:(char*)connectionId;
- (bool) openSession;
- (void) closeSession;
@end
