#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using UnityEditor;
using UnityEngine;
using System;

namespace Varneon.WorldCreatorAssistant
{
    public static class LayerSetupHandler
    {
        private const string LogPrefix = "[<color=#009999>WorldCreatorAssistant</color>]:";

        private const string GUID = "0c1cedbb001e2684b8659677756a1986";

        [InitializeOnLoadMethod]
        public static void SetupLayers()
        {
            try
            {
                if (!UpdateLayers.AreLayersSetup()) { UpdateLayers.SetupEditorLayers(); }

                if (!UpdateLayers.IsCollisionLayerMatrixSetup()) { UpdateLayers.SetupCollisionLayerMatrix(); }
                    
                Debug.Log($"{LogPrefix} VRC Layers and Collision Matrix have been set up!");

                string path = AssetDatabase.GUIDToAssetPath(GUID);

                if (!path.EndsWith("LayerSetupHandler.cs")) { Debug.LogError($"{LogPrefix} LayerSetupHandler.cs has invalid GUID! Please delete this file manually."); return; }

                AssetDatabase.DeleteAsset(path);
            }
            catch(Exception e)
            {
                Debug.LogError(e);

                return;
            }
        }
    }
}
#endif
