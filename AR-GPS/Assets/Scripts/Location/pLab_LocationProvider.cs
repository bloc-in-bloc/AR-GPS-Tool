/******************************************************************************
* File         : pLab_LocationProvider.cs
* Lisence      : BSD 3-Clause License
* Copyright    : Lapland University of Applied Sciences
* Authors      : Arto Söderström
* BSD 3-Clause License
*
* Copyright (c) 2019, Lapland University of Applied Sciences
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
* 
* 1. Redistributions of source code must retain the above copyright notice, this
*  list of conditions and the following disclaimer.
*
* 2. Redistributions in binary form must reproduce the above copyright notice,
*  this list of conditions and the following disclaimer in the documentation
*  and/or other materials provided with the distribution.
*
* 3. Neither the name of the copyright holder nor the names of its
*  contributors may be used to endorse or promote products derived from
*  this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
* AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
* FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
* DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
* SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
* CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
* OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
* OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BlocInBloc;
using BlocInBloc.NativeBluetooth;
using BlocInBloc.Trame;
using UnityEngine;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// Location updated event arguments. 
/// </summary>
public class pLab_LocationUpdatedEventArgs : EventArgs
{
    public pLab_LatLon location;
    public float altitude;
    public float horizontalAccuracy;
    public float verticalAccuracy;
    public double timestamp;
}

public class pLab_LocationProvider : MonoBehaviour
{
    #region Variables
    public NativeBluetooth nativeBluetooth;
    public NativeLocation nativeLocation;

    /// <summary>
    /// Using higher value like 500 usually does not require to turn GPS chip on and thus saves battery power. 
    /// Values like 5-10 could be used for getting best accuracy.
    /// </summary>
    [SerializeField]
    [Range(0.2f, 10)]
    private float desiredAccuracyInMeters = 1f;

    /// <summary>
    /// The minimum distance (measured in meters) a device must move laterally before Input.location property is updated. 
    /// Higher values like 500 imply less overhead.
    /// </summary>
    [SerializeField]
    [Range(0.2f, 30)]
    private float updateDistanceInMeters = 1f;

    Coroutine pollRoutine;

    
    private double lastLocationTimestamp;

    private WaitForSeconds wait;

    private pLab_LatLon location;

    private pLab_LatLon latestAccurateLocation;

    private LocationInfo latestLocationInfo;
    private bool _isConnected = false;
    private int _positionQuality;
    private double _latitudeError;
    private double _longitudeError;

#region Debug Variables
    [Header("Debug")]
    [SerializeField]
    private bool useFakeData = false;

    [SerializeField]
    private pLab_LatLon fakeCoordinates;

    #endregion

    #endregion

    #region Properties

    public pLab_LatLon Location { get => location; set => location = value; }

    public double LastLocationTimestamp { get { return lastLocationTimestamp; } }

    public LocationInfo LatestLocationInfo { get { return latestLocationInfo; } }
    public int PositionQuality { get { return _positionQuality; } }
    public double LatitudeError { get { return _latitudeError; } }
    public double LongitudeError { get { return _longitudeError; } }


#region Debug Properties

    public bool UseFakeData {
        get { return useFakeData; }
        set
        {
            bool previousVal = useFakeData;
            useFakeData = value;
            if (previousVal != useFakeData) {
                StartPollLocationRoutine();
            }
        }
    }
    
    #endregion Debug Properties

    #endregion

    #region Events

    /// <summary>
    /// Occurs when on location updates.
    /// </summary>
    public event EventHandler<pLab_LocationUpdatedEventArgs> OnLocationUpdated;

    #endregion

    #region Inherited Methods

    // Start is called before the first frame update
    void Start()
    {
        wait = new WaitForSeconds(1f);

        StartPollLocationRoutine();
        
        nativeBluetooth.OnNewTrame.AddListener (OnFrameReceived);
    }

    #endregion

    #region IEnumerators/Coroutines
    
    /// <summary>
    /// Enable location and compass services.
    /// Sends continuous location and heading updates based on 
    /// _desiredAccuracyInMeters and _updateDistanceInMeters.
    /// </summary>
    /// <returns>The location routine.</returns>
    private IEnumerator PollLocationRoutine()
    {
        bool locationIsInitialized = nativeLocation.isInitialized;
        if (!locationIsInitialized) {
            locationIsInitialized = nativeLocation.Init ();
        }
        
        if (_isConnected) {
            yield break;
        }
        NativeBluetooth.InitializeBluetooth ();

        Debug.LogError (NativeBluetooth.GetConnectedBluetoothAccessories ().Length);
        
        NativeBluetooth.BluetoothAccessory[] tmp = NativeBluetooth.GetConnectedBluetoothAccessories ()
                                                                  .Where (ba => ba.name.Contains ("FLX100")).ToArray ();

        if (tmp.Length == 0) {
            Debug.LogError ("Can't find any FLX100 to connect !");
            yield break;
        }

        NativeBluetooth.BluetoothAccessory connectedBluetoothAccessory = tmp[0];
        
        if (!NativeBluetooth.SetupController (connectedBluetoothAccessory.connectionId)) {
            Debug.LogError ($"GNNS: Failed to setup controller with {connectedBluetoothAccessory.connectionId}");
            yield break;
        }

        if (!NativeBluetooth.OpenSession ()) {
            Debug.LogError ($"GNNS: Failed to open session with {connectedBluetoothAccessory.connectionId}");
            yield break;
        }

        _isConnected = true;
    }

    private IEnumerator PollLocationRoutineFake() {
        yield return new WaitForSeconds(1f);

        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

        while(true) {
            
            double timestamp = (System.DateTime.UtcNow - epochStart).TotalMilliseconds;

            location = fakeCoordinates;
            latestLocationInfo = new LocationInfo();
            
            pLab_LocationUpdatedEventArgs locationEventArgs = new pLab_LocationUpdatedEventArgs()
            {
                location = location,
                altitude = 1f,
                horizontalAccuracy = UnityEngine.Random.Range(3.21f, 20f),
                verticalAccuracy = UnityEngine.Random.Range(3.21f, 20f),
                timestamp = timestamp
            };

            if (OnLocationUpdated != null) {
                OnLocationUpdated(this, locationEventArgs);
            }

            yield return new WaitForSeconds(2f);
        }
        
    }

    #endregion

    #region Private Methods

    public void OnFrameReceived (string trame) {
        Match match = Regex.Match (trame, @"^.[^\*]*");
        if (!match.Success) {
            Debug.LogError ("NMEA Trame does not match regex");
            return;
        }
        trame = match.Groups.First ().Value;
        string[] tokens = trame.Split (',');
        if (!tokens.Any ()) {
            return;
        }
        if (tokens[0].Contains ("GGA")) {
            try {
                GGATrame tmpLLQTrame = new GGATrame (tokens);

                if (tmpLLQTrame.altitude == 0 && tmpLLQTrame.latitude == 0) {
                    return;
                }

                double timestamp = Convert.ToDouble(tmpLLQTrame.utcTime);

                if (timestamp > lastLocationTimestamp) {
                    lastLocationTimestamp = timestamp;

                    location = new pLab_LatLon(tmpLLQTrame.latitude, tmpLLQTrame.longitude);
                
                    latestLocationInfo = new LocationInfo();
                    pLab_LocationUpdatedEventArgs locationEventArgs = new pLab_LocationUpdatedEventArgs()
                    {
                        location = location,
                        altitude = Convert.ToSingle (tmpLLQTrame.altitude),
                        horizontalAccuracy = 0.02f,
                        verticalAccuracy = 0.02f,
                        timestamp = timestamp
                    };

                    if (OnLocationUpdated != null) {
                        OnLocationUpdated(this, locationEventArgs);
                    }

                    latestAccurateLocation = location;
                }
            } catch (InvalidDataException e) {
                Debug.LogError (e);
            }
        } else if (tokens[0].Contains ("GGA")) {
            try {
                GGATrame ggaTrame = new GGATrame (tokens);
                _positionQuality = ggaTrame.positionQuality;
            } catch (InvalidDataException e) {
                Debug.LogError (e);
            }
        } else if (tokens[0].Contains ("GST")) {
            Debug.Log (trame);
            try {
                GSTTrame gstTrame = new GSTTrame (tokens);
                _latitudeError = gstTrame.latitudeError;
                _longitudeError = gstTrame.longitudeError;
            } catch (InvalidDataException e) {
                Debug.LogError (e);
            }
        }
    }

    private void StartPollLocationRoutine() {
        if (pollRoutine != null) {
            StopCoroutine(pollRoutine);
        }

        if (useFakeData) {
            pollRoutine = StartCoroutine(PollLocationRoutineFake());
        } else {
            pollRoutine = StartCoroutine(PollLocationRoutine());
        }
    }

    private void SendUpdatedLocation()
    {
        if (OnLocationUpdated != null)
        {
            pLab_LocationUpdatedEventArgs eventArgs = new pLab_LocationUpdatedEventArgs()
            {
                location = location,
                altitude = latestLocationInfo.altitude,
                horizontalAccuracy = latestLocationInfo.horizontalAccuracy,
                verticalAccuracy = latestLocationInfo.verticalAccuracy,
                timestamp = latestLocationInfo.timestamp
            };
            
            OnLocationUpdated(this, eventArgs);
        }
    }

    #endregion

}
