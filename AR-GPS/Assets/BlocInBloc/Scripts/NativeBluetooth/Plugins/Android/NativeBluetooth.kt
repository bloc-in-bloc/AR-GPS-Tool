package com.blocinbloc.bluetooth

import android.app.Activity
import android.content.Context
import android.annotation.SuppressLint
import android.content.Intent
import android.net.Uri
import android.os.Handler
import android.os.Looper
import android.util.Log
import androidx.core.content.ContextCompat
import androidx.core.content.ContextCompat.startActivity
import com.unity3d.player.UnityPlayer
import org.json.JSONObject
import androidx.core.content.ContextCompat.startActivity

class NativeBluetooth {
    private var macAddress: String? = null

    lateinit var bluetoothSerialManager: BluetoothSerialManager

    private var context : Context? = null

    fun OpenZenoConnect(){
        var launchIntent: Intent? =
            context?.packageManager?.getLaunchIntentForPackage("com.leica.zenoconnect")

        //if app is not installed on the phone
        if (launchIntent == null){
            launchIntent = Intent(Intent.ACTION_VIEW, Uri.parse("market://details?id=com.leica.zenoconnect"))
        }
        launchIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
        startActivity(context!!, launchIntent , null)
    }
    
    
    fun SetContextAndActivity(context: Context, activity: Activity) {
        this.context = context
        Log.d("BIB", "SetContextAndActivity")
        bluetoothSerialManager = BluetoothSerialManager(context, activity)
        bluetoothSerialManager.listener = object : BluetoothSerialManager.OnMessageListener {
            override fun onReceiveSocketMessage(message: String) {
                executeOnUi {
                    SendTrameToUnity(message)
                }
            }
        }
    }

    @SuppressLint("MissingPermission")
    fun GetDevices(): String {
        Log.d("BIB", "GetDevices")
        return bluetoothSerialManager.getPairedDevices().map {
            JSONObject().apply {
                put("name", it.name)
                put("connectionId", it.address)
            }
        }.toString()
    }

    fun Setup(macAddress: String): Boolean {
        Log.d("BIB", "Setup")
        this.macAddress = macAddress
        return true
    }

    fun OpenSession(): Boolean {
        Log.d("BIB", "OpenSession")
        macAddress?.let {
            executeOnBackground {
                bluetoothSerialManager.connectToDevice(it)
            }
        }
        return true
    }

    fun SendTrameToUnity(data: String) {
        Log.d("BIB", "Receive message: $data")
        UnityPlayer.UnitySendMessage("NativeBluetooth", "OnTrameReceived", data)
    }

    fun CloseSession() {
        Log.d("BIB", "CloseSession")
        bluetoothSerialManager.disconnect()
    }
}
