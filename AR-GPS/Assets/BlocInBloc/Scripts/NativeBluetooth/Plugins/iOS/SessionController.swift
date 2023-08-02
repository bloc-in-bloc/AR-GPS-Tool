import ExternalAccessory

class SessionController: NSObject, EAAccessoryDelegate, StreamDelegate {
    static let sharedController = SessionController()
    var _accessory: EAAccessory?
    var _session: EASession?
    var _protocolString: String?
    var isObservingForegroundNotification : Bool = false
    
    
    @objc func applicationWillEnterForeground() {
        if let _ = _session {
            closeSession()
            _ = openSession()
        }
    }
    
    // MARK: Controller Setup
    
    func setupController(forAccessory accessory: EAAccessory) {
        _accessory = accessory
        _protocolString = accessory.protocolStrings[0]
        
        if( !isObservingForegroundNotification) {
            NotificationCenter.default.addObserver(self, selector: #selector(applicationWillEnterForeground), name: UIApplication.willEnterForegroundNotification, object: nil)
            isObservingForegroundNotification = true
        }
    }
    
    // MARK: Opening & Closing Sessions
    
    func openSession() -> Bool {
        _accessory?.delegate = self
        _session = EASession(accessory: _accessory!, forProtocol: _protocolString!)

        if _session != nil {
            _session?.inputStream?.delegate = self
            _session?.inputStream?.schedule(in: RunLoop.current, forMode: RunLoop.Mode.default)
            _session?.inputStream?.open()
        } else {
            print("Failed to create session")
        }
        
        return _session != nil
    }
    
    func closeSession() {
        _session?.inputStream?.close()
        _session?.inputStream?.remove(from: RunLoop.current, forMode: RunLoop.Mode.default)
        _session?.inputStream?.delegate = nil
        
        _session?.outputStream?.close()
        _session?.outputStream?.remove(from: RunLoop.current, forMode: RunLoop.Mode.default)
        _session?.outputStream?.delegate = nil
        
        _session = nil
    }
    
    func readData(_ stream: InputStream, bufferSize : Int) {
        // set up a buffer, into which you can read the incoming bytes
        var buffer = [UInt8](repeating: 0, count: bufferSize)
        
        //  loop for as long as the input stream has bytes to be read
        while stream.hasBytesAvailable {
            // read bytes from the stream and put them into the buffer you pass in
            let bytesRead = stream.read(&buffer, maxLength: bufferSize)
            
            // error occured or not
            if bytesRead < 0 {
                let e = stream.streamError
                print(e?.localizedDescription ?? "Error occured")
                break
            }
            
            let tmpString = NSString(bytes: buffer, length: bytesRead, encoding: String.Encoding.utf8.rawValue) as String?
            
            if let data = tmpString {
                let lines = data.split(whereSeparator: \.isNewline)
                
                // notify interested parties
                for line in lines {
                    NativeBluetooth.sendMessage(toUnity: "OnTrameReceived", message: String(line));
                }
            }
        }
    }
    
    // MARK: - EAAcessoryDelegate
    
    @objc public func accessoryDidDisconnect(_ accessory: EAAccessory) {
        // Accessory diconnected from iOS, updating accordingly
        NativeBluetooth.sendMessage(toUnity: "OnAccessoryDisconnect", message: "");
    }
    
    // MARK: - NSStreamDelegateEventExtensions
    
    @objc public func stream(_ aStream: Stream, handle eventCode: Stream.Event) {
        switch eventCode {
        case Stream.Event.openCompleted:
            print("openCompleted")
            break
        case Stream.Event.hasBytesAvailable:
            readData(aStream as! InputStream, bufferSize: 1024)
            break
        case Stream.Event.hasSpaceAvailable:
            break
        case Stream.Event.errorOccurred:
            break
        case Stream.Event.endEncountered:
            print("EndEncounteted")
            break
            
        default:
            break
        }
    }
}
