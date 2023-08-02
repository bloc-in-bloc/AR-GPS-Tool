#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;

namespace BlocInBloc.NativeBluetooth {
    public static class PostProcessBuild {
        [PostProcessBuild]
        public static void OnPostBuild (BuildTarget buildTarget, string pathToBuiltProject) {
            PBXProject pbxProject = new PBXProject ();
            string projectPath = PBXProject.GetPBXProjectPath (pathToBuiltProject);
            pbxProject.ReadFromFile (projectPath);

            string unityFrameworkGuid = pbxProject.GetUnityFrameworkTargetGuid ();
            // Modulemap
            pbxProject.AddBuildProperty (unityFrameworkGuid, "DEFINES_MODULE", "YES");
            string moduleFile = $"{pathToBuiltProject}/UnityFramework/UnityFramework.modulemap";
            if (!File.Exists (moduleFile)) {
                FileUtil.CopyFileOrDirectory ("Assets/Plugins/iOS/UnityFramework.modulemap", moduleFile);
                pbxProject.AddFile (moduleFile, "UnityFramework/UnityFramework.modulemap");
                pbxProject.AddBuildProperty (unityFrameworkGuid, "MODULEMAP_FILE", "$(SRCROOT)/UnityFramework/UnityFramework.modulemap");
            }
            
            // Headers
            string unityInterfaceGuid = pbxProject.FindFileGuidByProjectPath ("Libraries/BlocInBloc/Scripts/NativeBluetooth/Plugins/iOS/NativeBluetooth.h");
            pbxProject.AddPublicHeaderToBuild (unityFrameworkGuid, unityInterfaceGuid);

            string stringPbxProject = pbxProject.WriteToString ();
            File.WriteAllText (projectPath, stringPbxProject);

            // Needed to detect leika gnss device
            AddPlistKeys (pathToBuiltProject, "UISupportedExternalAccessoryProtocols", new[] {"com.leica-geosystems.zeno.gcmd", "com.leica-geosystems.zeno.gnss"});
            // Needed to check if zeno connect is installed
            AddPlistKeys (pathToBuiltProject, "LSApplicationQueriesSchemes", new[] {"ZenoConnect"});
            AddPlistKey (pathToBuiltProject, "NSBluetoothAlwaysUsageDescription", "Our app uses bluetooth to find and connect to gnss antennas");
        }

        private static void AddPlistKeys (string pathToBuiltProject, string buildKey, string[] values) {
            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument ();
            plist.ReadFromString (File.ReadAllText (plistPath));

            // Get root
            PlistElementDict rootDict = plist.root;

            // Change value of CFBundleVersion in Xcode plist
            PlistElementArray keyArray = rootDict.CreateArray (buildKey);
            foreach (string value in values) {
                keyArray.AddString (value);
            }

            // Write to file
            File.WriteAllText (plistPath, plist.WriteToString ());
        }
        
        private static void AddPlistKey (string pathToBuiltProject, string buildKey, string value) {
            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument ();
            plist.ReadFromString (File.ReadAllText (plistPath));

            // Get root
            PlistElementDict rootDict = plist.root;
            
            rootDict.SetString (buildKey, value);

            // Write to file
            File.WriteAllText (plistPath, plist.WriteToString ());
        }
    }
}
#endif