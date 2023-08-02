#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

using UnityEngine.Events;

namespace BlocInBloc.NativeBluetooth {
#if UNITY_IOS || UNITY_EDITOR
    public enum BluetoothState {
        //see https://developer.apple.com/documentation/corebluetooth/cbmanagerstate
        ON = 5, OFF = 4, RESETTING = 1, UNAUTHORIZED = 3, UNKNOWN = 0, UNSUPPORTED = 2
    }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    public enum BluetoothState {
        //see https://developer.android.com/reference/android/bluetooth/BluetoothAdapter
        ON = 12, OFF = 10, CONNECTED = 2, CONNECTING = 1, DISCONNECTED = 0, DISCONNECTING = 3, TURNING_OFF = 13, TURNING_ON = 11, UNKNOWN = 90
    }
#endif
    
    public class NativeBluetooth : MonoBehaviour {
#if UNITY_EDITOR
        public string[] mockedFrames = new[] {"$GPLLQ,113616.00,041006,1351169.857,M,62440353.754,M,3,12,0.010,55.597,M*12"};
        private int _mockedFrameIndex = 0;
#endif

        public UnityEvent<string> OnNewTrame;
        public UnityEvent<BluetoothState> OnNewBluetoothState;
        public UnityEvent bluetoothDeviceDisconnected;

        public BluetoothState currentState = BluetoothState.UNKNOWN;
        private static bool isInitialized = false;
        private static NativeBluetooth Instance;

        [Serializable]
        public class BluetoothAccessory {
            public string connectionId;
            public string manufacturer;
            public string name;
        }

        void Awake () {
            Instance = this;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject _bluetooth = null;
#endif

        public static void InitializeBluetooth () {
            if (!isInitialized) {
                Initialize ();
            }
        }
        
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void Initialize();
#elif UNITY_ANDROID && !UNITY_EDITOR
        private static void Initialize() {
            if (_bluetooth != null) {
                return;
            }
            _bluetooth = new AndroidJavaObject ("com.blocinbloc.bluetooth.NativeBluetooth");
            using (var unityClass = new AndroidJavaClass ("com.unity3d.player.UnityPlayer")) {
                using (AndroidJavaObject currentActivity = unityClass.GetStatic<AndroidJavaObject> ("currentActivity")) {
                    using (AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext")) {
                        _bluetooth.Call ("SetContextAndActivity", context, currentActivity);
                    }
                }
            }
        }
#else
        private static void Initialize () {
            Debug.LogWarning ("Not implemented");
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GetDevices();
#elif UNITY_ANDROID && !UNITY_EDITOR
        public static string GetDevices() {
            if (_bluetooth == null) {
                Initialize ();
            }
            return _bluetooth.Call<string>("GetDevices");
        }
#else
        private static string GetDevices () {
            Debug.LogWarning ("Not implemented");
            return "[{\"name\":\"Leica GG04 plus\",\"connectionId\":44585484}]";
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern bool SetupController(string connectionId);
#elif UNITY_ANDROID && !UNITY_EDITOR
        public static bool SetupController(string macAddress) {
            if (_bluetooth == null) {
                Initialize ();
            }
            return _bluetooth.Call<bool>("Setup", macAddress);
        }
#else
        public static bool SetupController (string connectionId) {
            Debug.LogWarning ("Not implemented");
            return Application.isEditor;
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern bool OpenSession();
#elif UNITY_ANDROID && !UNITY_EDITOR
        public static bool OpenSession() {
            if (_bluetooth == null) {
                Initialize ();
            }
            return _bluetooth.Call<bool>("OpenSession");
        }
#else
        public static bool OpenSession () {
            Debug.LogWarning ("Not implemented");
            Instance.InvokeRepeating ("FrameReceivedMock", 1f, 1f);
            return Application.isEditor;
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void OpenZenoConnect();
#elif UNITY_ANDROID && !UNITY_EDITOR
        public static void OpenZenoConnect() {
            if (_bluetooth == null) {
                Initialize ();
            }
            _bluetooth.Call("OpenZenoConnect");
        }
#else
        public static void OpenZenoConnect () {
            Debug.LogWarning ("Not implemented");
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void CloseSession();
#elif UNITY_ANDROID && !UNITY_EDITOR
        public static void CloseSession() {
            if (_bluetooth == null) {
                Initialize ();
            }
            _bluetooth.Call("CloseSession");
        }
#else
        public static void CloseSession () {
            Debug.LogWarning ("Not implemented");
            if(Instance != null) Instance.CancelInvoke ();
        }
#endif

#if UNITY_EDITOR
        public void FrameReceivedMock () {
            if (!mockedFrames.Any ()) {
                return;
            }
            OnTrameReceived (mockedFrames[_mockedFrameIndex]);
            _mockedFrameIndex++;
            if (_mockedFrameIndex >= mockedFrames.Length) {
                _mockedFrameIndex = 0;
            }
        }
#endif

        public static BluetoothAccessory[] GetConnectedBluetoothAccessories () {
            return JsonConvert.DeserializeObject<BluetoothAccessory[]> (GetDevices ());
        }

        public void OnTrameReceived (string trame) {
            OnNewTrame.Invoke (trame);
        }

        public void BluetoothStateChanged (string enumValue) {
            int state = 0;
            if (Int32.TryParse (enumValue, out state)) {
                currentState = (BluetoothState) state;
                OnNewBluetoothState.Invoke (currentState);
            } else {
                Debug.LogError ("Could not parse enum value");
            }
        }

        public void OnAccessoryDisconnect (string message) {
            bluetoothDeviceDisconnected.Invoke ();
        }

#if UNITY_EDITOR
        [ContextMenu ("ForceBluetoothOn")]
        public void ForceBluetoothOn () {
            BluetoothStateChanged ("5");
        }
        
        [ContextMenu ("ForceBluetoothOff")]
        public void ForceBluetoothOff () {
            BluetoothStateChanged ("4");
        }
        
        [ContextMenu ("ForceAccessoryDisconnect")]
        public void ForceAccessoryDisconnect () {
            OnAccessoryDisconnect ("");
        }
#endif
    }
}