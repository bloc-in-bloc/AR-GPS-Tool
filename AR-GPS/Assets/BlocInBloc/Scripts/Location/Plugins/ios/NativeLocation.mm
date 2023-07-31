//
//  NativeLocation.mm
//
//  Created by BlocInBloc on 20/01/2020.
//  Copyright © 2020 BlocInBloc. All rights reserved.
//

#import "NativeLocation.h"
#import <UIKit/UIKit.h>
#import "UnityInterface.h"

NativeLocation *nativeLocation;

extern "C" {
bool InitLocationManager()
{
    nativeLocation = [[NativeLocation alloc] init];
    return [nativeLocation initLocationAndHeading];
}

char* AuthorizationStatus()
{
    if (nativeLocation != nil) {
        NSString *status = [nativeLocation authorizationStatus];
        return [NativeLocation convertNSStringToCString:status];
    } else {
        NSLog(@"Error: nativeLocation not initialized");
        return nil;
    }
}

void StartUpdate()
{
    if (nativeLocation != nil) {
        [nativeLocation startUpdate];
    } else {
        NSLog(@"Error: nativeLocation not initialized");
    }
}

void StopUpdate()
{
    if (nativeLocation != nil) {
        [nativeLocation stopUpdate];
    } else {
        NSLog(@"Error: nativeLocation not initialized");
    }
}

char * GetErrorMsg()
{
    if (nativeLocation == nil || nativeLocation->errorMsg == nil) {
        return NULL;
    }
    return [NativeLocation convertNSStringToCString:nativeLocation->errorMsg];
}

//MARK: Heading
bool GetHeadingAvailable()
{
    return nativeLocation != nil && nativeLocation->headingAvailable && nativeLocation->lastHeading != nil;
}

float GetMagneticHeading()
{
    if (nativeLocation == nil || nativeLocation->lastHeading == nil) {
        return -1;
    }
    return nativeLocation->lastHeading.magneticHeading;
}

float GetTrueHeading()
{
    if (nativeLocation == nil || nativeLocation->lastHeading == nil) {
        return -1;
    }
    return nativeLocation->lastHeading.trueHeading;
}

float GetHeadingAccuracy()
{
    if (nativeLocation == nil || nativeLocation->lastHeading == nil) {
        return -1;
    }
    return nativeLocation->lastHeading.headingAccuracy;
}

double GetHeadingTimestamp()
{
    if (nativeLocation == nil || nativeLocation->lastHeading == nil) {
        return -1;
    }
    return nativeLocation->lastHeading.timestamp.timeIntervalSince1970;
}

float GetMagneticField()
{
    return sqrt(nativeLocation->lastHeading.x * nativeLocation->lastHeading.x + nativeLocation->lastHeading.y * nativeLocation->lastHeading.y + nativeLocation->lastHeading.z * nativeLocation->lastHeading.z);
}

//MARK: Location

bool GetLocationAvailable()
{
    return nativeLocation != nil && nativeLocation->locationAvailable && nativeLocation->lastLocation != nil;
}

double GetLocationLatitude()
{
    if (nativeLocation == nil || nativeLocation->lastLocation == nil) {
        return 0;
    }
    return nativeLocation->lastLocation.coordinate.latitude;
}

double GetLocationLongitude()
{
    if (nativeLocation == nil || nativeLocation->lastLocation == nil) {
        return -1;
    }
    return nativeLocation->lastLocation.coordinate.longitude;
}

double GetLocationAltitude()
{
    if (nativeLocation == nil || nativeLocation->lastLocation == nil) {
        return -1;
    }
    return nativeLocation->lastLocation.altitude;
}

float GetLocationHorizontalAccuracy()
{
    if (nativeLocation == nil || nativeLocation->lastLocation == nil) {
        return -1;
    }
    return nativeLocation->lastLocation.horizontalAccuracy;
}

float GetLocationVerticalAccuracy()
{
    if (nativeLocation == nil || nativeLocation->lastLocation == nil) {
        return -1;
    }
    return nativeLocation->lastLocation.verticalAccuracy;
}

double GetLocationTimestamp()
{
    if (nativeLocation == nil || nativeLocation->lastLocation == nil) {
        return -1;
    }
    return nativeLocation->lastLocation.timestamp.timeIntervalSince1970;
}

// Values : NotInitialized, Initializing, Starting, Failed, Unauthorized, Authorized, Running, Stopped
char * GetLocationStatus()
{
    NSString *status = nativeLocation->lastLocationManagerStatus;
    if (nativeLocation == nil || nativeLocation->lastLocationManagerStatus == nil) {
        status = @"NotInitialized";
    }
    return [NativeLocation convertNSStringToCString:status];
}

float GetCourseAccuracy()
{
    if (nativeLocation == nil || nativeLocation->lastLocation == nil) {
        return -1;
    }
    return nativeLocation->lastLocation.courseAccuracy;
}

float GetCourse()
{
    if (nativeLocation == nil || nativeLocation->lastLocation == nil) {
        return -1;
    }
    return nativeLocation->lastLocation.course;
}
}

@implementation NativeLocation : NSObject

//@ObservedObject var orientationInfo = OrientationInfo()
//var orientationListener: AnyCancellable? = nil

CLLocationManager *_locationManager;

- (instancetype)init
{
    self = [super init];
    if (self) {
        lastHeading = nil;
        lastLocation = nil;
        errorMsg = nil;
        headingAvailable = false;
        locationAvailable = false;
        lastLocationManagerStatus = @"NotInitialized";
    }
    return self;
}

- (bool)initLocationAndHeading {
    if (![CLLocationManager locationServicesEnabled] || ![CLLocationManager headingAvailable]) {
        errorMsg = [NSString stringWithFormat:@"Error: Location service is %@, Heading service is %@",([CLLocationManager locationServicesEnabled] ? @"available" : @"unavailable"), ([CLLocationManager headingAvailable] ? @"available" : @"unavailable")];
        NSLog(errorMsg);
        return false;
    }
    _locationManager = [[CLLocationManager alloc] init];

    [self setHeadingOrientationWithInterfaceOrientation:UIApplication.sharedApplication.statusBarOrientation];
    [self listenDeviceOrientation];
    _locationManager.delegate = self;
    _locationManager.desiredAccuracy = kCLLocationAccuracyBestForNavigation;

    lastLocationManagerStatus = @"Initialized";

    return true;
}

- (NSString*)authorizationStatus {
    if (@available(iOS 14.0, *)) {
        return [self authorizationStatusToString: _locationManager.authorizationStatus];
    } else {
        return [self authorizationStatusToString: [CLLocationManager authorizationStatus]];
    }
}

- (bool)requestAuthorizationNeeded {
    if (@available(iOS 14.0, *)) {
        return _locationManager.authorizationStatus == kCLAuthorizationStatusNotDetermined;
    } else {
        return [CLLocationManager authorizationStatus] == kCLAuthorizationStatusNotDetermined;
    }
}

- (void)startUpdate {
    lastLocationManagerStatus = @"Starting";
    if ([self requestAuthorizationNeeded]) {
        [_locationManager requestWhenInUseAuthorization];
    }

    [_locationManager startUpdatingHeading];
    [_locationManager startUpdatingLocation];
}

- (void)stopUpdate {
    [_locationManager stopUpdatingHeading];
    [_locationManager stopUpdatingLocation];
    lastHeading = nil;
    lastLocation = nil;
    headingAvailable = false;
    locationAvailable = false;
    lastLocationManagerStatus = @"Stopped";
}

+ (char *)convertNSStringToCString:(NSString *)nsString {
    if (nsString == NULL) return NULL;

    const char *nsStringUtf8 = [nsString UTF8String];
    //create a null terminated C string on the heap so that our string's memory isn't wiped out right after method's return
    char *cString = (char *)malloc(strlen(nsStringUtf8) + 1);
    strcpy(cString, nsStringUtf8);

    return cString;
}

//MARK: CLLocationManagerDelegate implementations

// Allow system to show calibration overlay if needed
- (BOOL)locationManagerShouldDisplayHeadingCalibration:(CLLocationManager *)manager {
    return true;
}

- (void)locationManager:(CLLocationManager *)manager didUpdateHeading:(CLHeading *)newHeading {
    headingAvailable = newHeading.headingAccuracy > 0 ? true : false;
    lastHeading = newHeading;
}

- (void)locationManager:(CLLocationManager *)manager didUpdateLocations:(NSArray<CLLocation *> *)locations {
    locationAvailable = true;
    lastLocation = locations.lastObject;
    lastLocationManagerStatus = @"Running";
}

- (void)locationManager:(CLLocationManager *)manager didFailWithError:(NSError *)error {
    errorMsg = error.localizedDescription;
    headingAvailable = false;
    locationAvailable = false;
    lastLocationManagerStatus = @"Failed";
}

- (void)locationManagerDidPauseLocationUpdates:(CLLocationManager *)manager {
    lastLocationManagerStatus = @"Stopped";
}

- (void)locationManagerDidResumeLocationUpdates:(CLLocationManager *)manager {
    lastLocationManagerStatus = @"Running";
}

- (void)locationManager:(CLLocationManager *)manager didChangeAuthorizationStatus:(CLAuthorizationStatus)status {
    UnitySendMessage("NativeLocation", "OnLocationAuthorizationStatusChange", [[self authorizationStatusToString: status] UTF8String]);
    // Not determined to prevent error message at first call
    if (status != kCLAuthorizationStatusNotDetermined && status != kCLAuthorizationStatusAuthorizedWhenInUse && status != kCLAuthorizationStatusAuthorizedAlways) {
        errorMsg = @"You should accept location access";
        headingAvailable = false;
        locationAvailable = false;
        lastLocationManagerStatus = @"Unauthorized";
    } else {
        errorMsg = nil;
        lastLocationManagerStatus = @"Authorized";
    }
}

//MARK: UIDeviceOrientation listener

- (void)listenDeviceOrientation {
    [[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];

    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(orientationChanged:) name:UIDeviceOrientationDidChangeNotification object:[UIDevice currentDevice]];
}

- (void)orientationChanged:(NSNotification *)note {
    [self setHeadingOrientationWithDeviceOrientation:UIDevice.currentDevice.orientation];
}

- (void)setHeadingOrientationWithInterfaceOrientation:(UIInterfaceOrientation)interfaceOrientation {
    switch (interfaceOrientation) {
        case UIInterfaceOrientationPortrait:
            _locationManager.headingOrientation = CLDeviceOrientation::CLDeviceOrientationPortrait;
            break;
        case UIInterfaceOrientationPortraitUpsideDown:
            _locationManager.headingOrientation = CLDeviceOrientation::CLDeviceOrientationPortraitUpsideDown;
            break;
        case UIInterfaceOrientationLandscapeLeft:
            _locationManager.headingOrientation = CLDeviceOrientation::CLDeviceOrientationLandscapeLeft;
            break;
        case UIInterfaceOrientationLandscapeRight:
            _locationManager.headingOrientation = CLDeviceOrientation::CLDeviceOrientationLandscapeRight;
            break;
        case UIInterfaceOrientationUnknown:
            _locationManager.headingOrientation = CLDeviceOrientation::CLDeviceOrientationUnknown;
            break;
        default:
            break;
    }
}

- (void)setHeadingOrientationWithDeviceOrientation:(UIDeviceOrientation)deviceOrientation {
    switch (deviceOrientation) {
        case UIDeviceOrientationPortrait:
            _locationManager.headingOrientation = CLDeviceOrientation::CLDeviceOrientationPortrait;
            break;
        case UIDeviceOrientationPortraitUpsideDown:
            _locationManager.headingOrientation = CLDeviceOrientation::CLDeviceOrientationPortraitUpsideDown;
            break;
        case UIDeviceOrientationLandscapeLeft:
            _locationManager.headingOrientation = CLDeviceOrientation::CLDeviceOrientationLandscapeLeft;
            break;
        case UIDeviceOrientationLandscapeRight:
            _locationManager.headingOrientation = CLDeviceOrientation::CLDeviceOrientationLandscapeRight;
            break;
        case UIDeviceOrientationUnknown:
            _locationManager.headingOrientation = CLDeviceOrientation::CLDeviceOrientationUnknown;
            break;
        default:
            break;
    }
}

- (NSString*)authorizationStatusToString:(CLAuthorizationStatus)authorizationStatus{
    NSString *result = nil;
    switch(authorizationStatus) {
        case kCLAuthorizationStatusRestricted:
            result = @"kCLAuthorizationStatusRestricted";
            break;
        case kCLAuthorizationStatusDenied:
            result = @"kCLAuthorizationStatusDenied";
            break;
        case kCLAuthorizationStatusAuthorizedAlways:
            result = @"kCLAuthorizationStatusAuthorizedAlways";
            break;
        case kCLAuthorizationStatusAuthorizedWhenInUse:
            result = @"kCLAuthorizationStatusAuthorizedWhenInUse";
            break;
        default:
            result =@"kCLAuthorizationStatusNotDetermined";
            break;
    }
    return result;
}

@end
