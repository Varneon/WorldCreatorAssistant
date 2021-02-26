#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    public class WCAData : ScriptableObject
    {
        public bool AreVRCLayersSetup;
        public List<DataStructs.Repository> CommunityTools = new List<DataStructs.Repository>();
        public List<DataStructs.Resource> Resources = new List<DataStructs.Resource>();
        public List<DataStructs.FAQTopic> Questions = new List<DataStructs.FAQTopic>();
        public List<DataStructs.AssetStorePackage> DownloadedUASPackages = new List<DataStructs.AssetStorePackage>();
        public List<DataStructs.UPMPackage> UPMPackages = new List<DataStructs.UPMPackage>();
    }
}
#endif