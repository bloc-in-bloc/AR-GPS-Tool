package com.blocinbloc.bluetooth

import android.Manifest
import android.annotation.SuppressLint
import android.app.Activity
import android.bluetooth.BluetoothAdapter
import android.bluetooth.BluetoothDevice
import android.bluetooth.BluetoothManager
import android.bluetooth.BluetoothSocket
import android.content.Context
import android.content.IntentFilter
import android.content.pm.PackageManager
import android.util.Log
import androidx.core.app.ActivityCompat
import java.io.BufferedReader
import java.io.IOException
import java.io.InputStreamReader
import java.nio.charset.StandardCharsets
import java.util.*
import android.content.Intent
import android.content.BroadcastReceiver
import com.unity3d.player.UnityPlayer

class BluetoothSerialManager(private val context: Context, private val activity: Activity) {

    companion object {
        private val TAG: String = BluetoothSerialManager::class.java.simpleName
        private val SERIAL_PORT_PROFILE_UUID: UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB")
    }

    private val mReceiver: BroadcastReceiver = object : BroadcastReceiver() {
        override fun onReceive(context: Context, intent: Intent) {
            val action = intent.action
            if (action == BluetoothAdapter.ACTION_STATE_CHANGED) {
                val state = intent.getIntExtra(
                    BluetoothAdapter.EXTRA_STATE,
                    BluetoothAdapter.ERROR
                )
                sendBluetoothStateToUnity(state.toString())

            }
        }
    }
    
    private val bluetoothManager: BluetoothManager by lazy { context.getSystemService(Context.BLUETOOTH_SERVICE) as BluetoothManager }
    private val bluetoothAdapter by lazy { bluetoothManager.adapter }

    private var pairedDevices: Set<BluetoothDevice> = emptySet()

    private lateinit var socket: BluetoothSocket
    private var isSocketConnected: Boolean = false

    var listener: OnMessageListener? = null

    fun sendBluetoothStateToUnity(message: String){
        executeOnUi {
            UnityPlayer.UnitySendMessage("NativeBluetooth", "BluetoothStateChanged", message)
        }
    }

    init {
        // Register for broadcasts on BluetoothAdapter state change
        val filter :IntentFilter  = IntentFilter(BluetoothAdapter.ACTION_STATE_CHANGED);
        context.registerReceiver(mReceiver, filter);
        sendBluetoothStateToUnity(bluetoothAdapter.state.toString())
    }

    @SuppressLint("NewApi")
    fun getPairedDevices(): Set<BluetoothDevice> {
        if (ActivityCompat.checkSelfPermission(context, Manifest.permission.BLUETOOTH_CONNECT) != PackageManager.PERMISSION_GRANTED) {
            ActivityCompat.requestPermissions(activity, arrayOf(Manifest.permission.BLUETOOTH_CONNECT), PackageManager.PERMISSION_GRANTED)
        }

        pairedDevices = bluetoothAdapter.bondedDevices

        return pairedDevices
    }

    fun connectToDevice(macAddress: String) {
        if (ActivityCompat.checkSelfPermission(context, Manifest.permission.BLUETOOTH_CONNECT) != PackageManager.PERMISSION_GRANTED) {
            ActivityCompat.requestPermissions(activity, arrayOf(Manifest.permission.BLUETOOTH_CONNECT), PackageManager.PERMISSION_GRANTED)
        }

        val device = bluetoothAdapter.getRemoteDevice(macAddress)
        socket = device.createInsecureRfcommSocketToServiceRecord(SERIAL_PORT_PROFILE_UUID)
        try {
            socket.connect()
            isSocketConnected = true
            while (isSocketConnected && socket.isConnected) {
                val reader = BufferedReader(InputStreamReader(socket.inputStream, StandardCharsets.UTF_8))
                for(line in reader.lines()) {
                    listener?.onReceiveSocketMessage(line)
                }
            }
            socket.close()
        } catch (e: IOException) { }

        executeOnUi {
            UnityPlayer.UnitySendMessage("NativeBluetooth", "OnAccessoryDisconnect", "")
        }
    }

    fun disconnect() {
        isSocketConnected = false
    }

    interface OnMessageListener {
        fun onReceiveSocketMessage(message: String)
    }
}
