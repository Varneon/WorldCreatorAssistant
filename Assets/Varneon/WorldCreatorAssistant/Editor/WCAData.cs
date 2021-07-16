using System.Collections.Generic;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    public class WCAData : ScriptableObject
    {
        public bool AreVRCLayersSetup;
        public string InstalledVRCSDKVersion, LatestVRCSDKVersion;
        public bool IsVRCSDKUpdateAvailable;
        public List<DataStructs.Repository> CommunityTools = new List<DataStructs.Repository>();
        public List<DataStructs.AssetStorePackage> DownloadedUASPackages = new List<DataStructs.AssetStorePackage>();
        public List<DataStructs.UPMPackage> UPMPackages = new List<DataStructs.UPMPackage>();
        public List<DataStructs.Repository> PrefabRepositories = new List<DataStructs.Repository>();
    }
}
