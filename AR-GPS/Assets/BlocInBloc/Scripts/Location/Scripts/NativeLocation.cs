#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace BlocInBloc {
    public class NativeLocation : MonoBehaviour {
        public enum LocationStatus { NotInitialized, Initializing, Starting, Failed, Authorized, Unauthorized, Running, Stopped }
        
        public enum LocationAuthorizationStatus { Undefined = 0, Denied, Authorized }
        
        public UnityEvent<LocationAuthorizationStatus> onLocationAuthorizationStatusChange = new UnityEvent<LocationAuthorizationStatus> ();

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport ("__Internal")]
        private static extern bool InitLocationManager ();
        [DllImport ("__Internal")]
        private static extern string AuthorizationStatus ();
        [DllImport ("__Internal")]
        private static extern void StartUpdate ();
        [DllImport ("__Internal")]
        private static extern void StopUpdate ();

        #region Heading
        [DllImport ("__Internal")]
        private static extern bool GetHeadingAvailable ();
        [DllImport ("__Internal")]
        private static extern float GetMagneticHeading ();
        [DllImport ("__Internal")]
        private static extern float GetTrueHeading ();
        [DllImport ("__Internal")]
        private static extern float GetHeadingAccuracy ();
        [DllImport ("__Internal")]
        private static extern double GetHeadingTimestamp ();
        [DllImport ("__Internal")]
        private static extern float GetMagneticField ();
        #endregion

        #region Location
        [DllImport ("__Internal")]
        private static extern bool GetLocationAvailable ();
        [DllImport ("__Internal")]
        private static extern double GetLocationLatitude ();
        [DllImport ("__Internal")]
        private static extern double GetLocationLongitude ();
        [DllImport ("__Internal")]
        private static extern double GetLocationAltitude ();
        [DllImport ("__Internal")]
        private static extern float GetLocationHorizontalAccuracy ();
        [DllImport ("__Internal")]
        private static extern float GetLocationVerticalAccuracy ();
        [DllImport ("__Internal")]
        private static extern float GetCourseAccuracy ();
        [DllImport ("__Internal")]
        private static extern float GetCourse ();
        [DllImport ("__Internal")]
        private static extern double GetLocationTimestamp ();
        // Values : NotInitialized, Initializing, Starting, Failed, Unauthorized, Running, Stopped, Authorized
        [DllImport ("__Internal")]
        private static extern string GetLocationStatus ();
        #endregion
        [DllImport ("__Internal")]
        private static extern string GetErrorMsg ();
#endif

        public bool isInitialized { get; private set; }
        public bool isStarted { get; private set; }

#if UNITY_ANDROID
        private LocationAuthorizationStatus _authorizationStatusEnum = LocationAuthorizationStatus.Undefined;
#endif

        private void Awake () {
            Init ();
        }
        
#region Heading
        public float unityMainCameraHeading {
            get {
                Vector3 direction = Camera.main.transform.forward;
                if (Vector3.Angle (Vector3.up, direction) > 90) {
                    direction = Camera.main.transform.up;
                }
                Vector3 headingDirection = Vector3.ProjectOnPlane (direction, Vector3.up);
                return Vector3.SignedAngle (Vector3.forward, headingDirection, Vector3.up);
            }
        }

        public bool headingAvailable {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetHeadingAvailable ();
#elif UNITY_EDITOR
                return true;
#else
                return Input.compass.enabled;
#endif
            }
        }

        public float trueHeading {
            get {
#if UNITY_EDITOR
                return unityMainCameraHeading;
#elif UNITY_IOS && !UNITY_EDITOR
                return GetTrueHeading ();
#else
                return Input.compass.trueHeading;
#endif
            }
        }

        public float magneticHeading {
            get {
#if UNITY_EDITOR
                return unityMainCameraHeading;
#elif UNITY_IOS && !UNITY_EDITOR
                return GetMagneticHeading ();
#else
                return Input.compass.magneticHeading;
#endif
            }
        }

        public float headingAccuracy {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetHeadingAccuracy ();
#else
                return Input.compass.headingAccuracy;
#endif
            }
        }

        public float magneticField {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetMagneticField ();
#else
                return Input.compass.rawVector.magnitude;
#endif
            }
        }

        public double headingTimestamp {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetHeadingTimestamp ();
#else
                return Input.compass.timestamp;
#endif
            }
        }
#endregion

#region Location
        public string authorizationStatus {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return AuthorizationStatus ();
#else
                return "";
#endif
            }
        }

        public LocationAuthorizationStatus authorizationStatusEnum {
            get {
#if UNITY_EDITOR
                return LocationAuthorizationStatus.Authorized;
#elif UNITY_IOS && !UNITY_EDITOR
                return ParseAuthorizationStatus (AuthorizationStatus ());
#elif UNITY_ANDROID && !UNITY_EDITOR
                return _authorizationStatusEnum;
#else
                return LocationAuthorizationStatus.Undefined;
#endif
            }
        }
            
        public bool locationAuthorizationNeeded {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return authorizationStatusEnum != LocationAuthorizationStatus.Authorized;
#elif UNITY_ANDROID
                return !Permission.HasUserAuthorizedPermission (Permission.FineLocation);
#else
                return false;
#endif
            }
        }

        public bool locationAvailable {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetLocationAvailable ();
#elif UNITY_EDITOR
                return true;
#else
                return !Input.location.lastData.Equals (default (LocationInfo));
#endif
            }
        }

        public double locationLatitude {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetLocationLatitude ();
#else
                return Input.location.lastData.latitude;
#endif
            }
        }

        public double locationLongitude {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetLocationLongitude ();
#else
                return Input.location.lastData.longitude;
#endif
            }
        }

        public double locationAltitude {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetLocationAltitude ();
#else
                return Input.location.lastData.altitude;
#endif
            }
        }

        public float locationHorizontalAccuracy {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetLocationHorizontalAccuracy ();
#else
                return Input.location.lastData.horizontalAccuracy;
#endif
            }
        }
        
        public float locationVerticalAccuracy {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetLocationVerticalAccuracy ();
#else
                return Input.location.lastData.verticalAccuracy;
#endif
            }
        }

        public double locationTimestamp {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetLocationTimestamp ();
#else
                return Input.location.lastData.timestamp;
#endif
            }
        }

        public float locationCourseAccuracy {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetCourseAccuracy ();
#else
                return 0f;
#endif
            }
        }

        public float locationCourse {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetCourse ();
#else
                return 0f;
#endif
            }
        }

        public LocationStatus locationStatus {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return (LocationStatus) Enum.Parse (typeof (LocationStatus), GetLocationStatus ());
#elif UNITY_EDITOR
                return LocationStatus.Running;
#else
                return ParseStatus (Input.location.status);
#endif
            }
        }
#endregion

        public string errorMsg {
            get {
#if UNITY_IOS && !UNITY_EDITOR
                return GetErrorMsg ();
#else
                return null;
#endif
            }
        }

        public bool Init () {
#if UNITY_IOS && !UNITY_EDITOR
            isInitialized = InitLocationManager ();
#elif UNITY_ANDROID || UNITY_EDITOR
            isInitialized = true;
#else
            isInitialized = false;
#endif
            return isInitialized;
        }

#if UNITY_ANDROID
        public async void StartLocation (bool gps = true, bool compass = true, bool gyro = true) {
#else
        public void StartLocation (bool gps = true, bool compass = true, bool gyro = true) {
#endif
            isStarted = true;
#if UNITY_IOS && !UNITY_EDITOR
            StartUpdate ();
#else
#if UNITY_ANDROID
            //TODO Android has a "always ask" state that is not represented in Unity, so when user set "always ask" it doesn't work as we expect "always"
            if (locationAuthorizationNeeded) {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += (permissionName) => {
                    _authorizationStatusEnum = LocationAuthorizationStatus.Denied;
                };
                callbacks.PermissionGranted += (permissionName) => {
                    _authorizationStatusEnum = LocationAuthorizationStatus.Authorized;
                };
                callbacks.PermissionDeniedAndDontAskAgain += (permissionName) => {
                    _authorizationStatusEnum = LocationAuthorizationStatus.Denied;
                };
                Permission.RequestUserPermission (Permission.FineLocation, callbacks);
                
                await UniTask.WaitUntil (() => _authorizationStatusEnum == LocationAuthorizationStatus.Undefined);
            }
#endif
            // start the location service
            if (gps) Input.location.Start ();
            // enable the compass
            if (compass) Input.compass.enabled = true;
            // enable the gyro
            if (gyro) Input.gyro.enabled = true;
#endif
        }

        public void Stop () {
            isStarted = false;
#if UNITY_IOS && !UNITY_EDITOR
            StopUpdate ();
#else
            // stop the location service
            Input.location.Stop ();
            // disable the compass
            Input.compass.enabled = false;
            // disable the gyro
            Input.gyro.enabled = false;
#endif
        }

        public void OnLocationAuthorizationStatusChange (string authorization) {
#if UNITY_IOS
            onLocationAuthorizationStatusChange.Invoke (ParseAuthorizationStatus (authorization));
#endif
        }

        LocationAuthorizationStatus ParseAuthorizationStatus (string authorization) {
            LocationAuthorizationStatus authorizationStatus = LocationAuthorizationStatus.Undefined;
#if UNITY_EDITOR
            authorizationStatus = LocationAuthorizationStatus.Authorized;
#elif UNITY_IOS
            if (authorization == "kCLAuthorizationStatusRestricted" || authorization == "kCLAuthorizationStatusDenied") {
                authorizationStatus = LocationAuthorizationStatus.Denied;
            } else if (authorization == "kCLAuthorizationStatusAuthorizedAlways" || authorization == "kCLAuthorizationStatusAuthorizedWhenInUse") {
                authorizationStatus = LocationAuthorizationStatus.Authorized;
            } else {
                authorizationStatus = LocationAuthorizationStatus.Undefined;
            }
#endif
            return authorizationStatus;
        }
        
        LocationStatus ParseStatus (LocationServiceStatus locationServiceStatus) {
            LocationStatus locationStatus = LocationStatus.Stopped;
            switch (locationServiceStatus) {
                case LocationServiceStatus.Running:
                    locationStatus = LocationStatus.Running;
                    break;
                case LocationServiceStatus.Failed:
                    locationStatus = LocationStatus.Failed;
                    break;
                case LocationServiceStatus.Initializing:
                    locationStatus = LocationStatus.Initializing;
                    break;
                case LocationServiceStatus.Stopped:
                    locationStatus = LocationStatus.Stopped;
                    break;
            }
            return locationStatus;
        }
    }
}