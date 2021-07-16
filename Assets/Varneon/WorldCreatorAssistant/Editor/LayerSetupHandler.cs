#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using UnityEditor;
using UnityEngine;
using System;

namespace Varneon.WorldCreatorAssistant
{
    public static class LayerSetupHandler
    {
        private static readonly string LogPrefix = "[<color=#009999>WorldCreatorAssistant</color>]:";

        [InitializeOnLoadMethod]
        public static void SetupLayers()
        {
            PackageManager packageManager = new PackageManager();
            WCAData wcaData = UtilityMethods.LoadWCAData();

            if (wcaData == null) { return; }
            
            if(!wcaData.AreVRCLayersSetup)
            {
                try
                {
                    UpdateLayers.SetupEditorLayers();
                    UpdateLayers.SetupCollisionLayerMatrix();
                    wcaData.AreVRCLayersSetup = true;
                    Debug.Log($"{LogPrefix} VRC Layers have been set up!");
                }
                catch(Exception e)
                {
                    Debug.LogError(e);

                    return;
                }
            }

            AssetDatabase.DeleteAsset("Assets/Varneon/WorldCreatorAssistant/Editor/LayerSetupHandler.cs");
        }
    }
}
#endif
