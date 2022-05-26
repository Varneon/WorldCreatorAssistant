using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Varneon.WorldCreatorAssistant
{
    public class WCAData : ScriptableObject
    {
        public string InstalledVRCSDKVersion, LatestVRCSDKVersion;
        public bool IsVRCSDKUpdateAvailable;
        public List<DataStructs.Repository> CommunityTools = new List<DataStructs.Repository>();
        public List<DataStructs.AssetStorePackage> DownloadedUASPackages = new List<DataStructs.AssetStorePackage>();
        public List<DataStructs.UPMPackage> UPMPackages = new List<DataStructs.UPMPackage>();
        public List<DataStructs.Repository> PrefabRepositories = new List<DataStructs.Repository>();

        /// <summary>
        /// Load WCAData from the project
        /// </summary>
        /// <param name="createNewIfNotFound">Should a new WCAData asset be created if none exists in the project</param>
        /// <returns></returns>
        internal static WCAData Load(bool createNewIfNotFound = true)
        {
            WCAData[] wcaDataAssets = UnityEngine.Resources.FindObjectsOfTypeAll<WCAData>();

            if((wcaDataAssets == null || wcaDataAssets.Length == 0) && createNewIfNotFound)
            {
                string path = EditorUtility.SaveFilePanel("Create New WCAData Asset", "Assets", "WCAData", "asset");

                if (!string.IsNullOrEmpty(path))
                {
                    WCAData newWCAData = CreateInstance<WCAData>();

                    AssetDatabase.CreateAsset(newWCAData, $"Assets{path.Replace(Application.dataPath, string.Empty)}");

                    AssetDatabase.Refresh();

                    return newWCAData;
                }

                return null;
            }

            return wcaDataAssets[0];
        }
    }
}
