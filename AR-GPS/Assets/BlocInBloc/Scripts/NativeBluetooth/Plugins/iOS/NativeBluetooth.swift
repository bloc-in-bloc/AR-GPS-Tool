//
//  NativeBluetooth.swift
//  NativeBluetooth
//
//  Created by Kévin ETOURNEAU on 24/02/2022.
//

import Foundation
import ExternalAccessory
import CoreBluetooth
    
@objc public class BluetoothPlugin : NSObject , CBCentralManagerDelegate{
    struct EAAccessoryCodable: Codable {
        var name: String
        var manufacturer: String
        var connectionId: Int
    }
    
    @objc public static let shared = BluetoothPlugin()
    
    var accessoryList: [EAAccessory] = []
    var bluetoothManager :CBCentralManager!

    @objc public func Initialize() {
        bluetoothManager = CBCentralManager.init(delegate: self, queue: nil)
    }
    
    @objc public func GetConnectedAccessories() -> String {
        accessoryList = EAAccessoryManager.shared().connectedAccessories
        
        var unityAccessoryList:[EAAccessoryCodable] = []
        for accessory in accessoryList {
            unityAccessoryList.append(EAAccessoryCodable(name: accessory.name, manufacturer: accessory.manufacturer, connectionId: accessory.connectionID))
        }
        
        let result = String(data:(try? JSONEncoder().encode(unityAccessoryList)) ?? Data(), encoding: .utf8) ?? "[]"
        // print(result);
        return result;
    }
    
    @objc public func SetupController(connectionId:Int) -> Bool {
        let accessory = accessoryList.first(where: {$0.connectionID == Int(connectionId)});
        if (accessory == nil) {
            return false;
        }
        print("SetupController");
        SessionController.sharedController.setupController(forAccessory: accessory!);
        return true;
    }
    
    @objc public func OpenSession() -> Bool {
        print("OpenSession");
        return SessionController.sharedController.openSession();
    }
    
    @objc public func CloseSession() {
        print("CloseSession");
        SessionController.sharedController.closeSession();
    }
        
    public func centralManagerDidUpdateState(_ central: CBCentralManager) {
        //print(central.state.rawValue);
        NativeBluetooth.sendMessage(toUnity: "BluetoothStateChanged", message: String(central.state.rawValue));
    }
}
