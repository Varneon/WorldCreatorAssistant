#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using UnityEditor;
using UnityEngine;
using System;

namespace Varneon.WorldCreatorAssistant
{
    public static class LayerSetupHandler
    {
        private static readonly string logPrefix = "<color=#009999>[WorldCreatorAssistant]</color>:";

        [InitializeOnLoadMethod]
        public static void SetupLayers()
        {
            WCAData wcaData = AssetDatabase.LoadAssetAtPath<WCAData>("Assets/Varneon/WorldCreatorAssistant/Scripts/WCAData.asset");
            if (wcaData != null && !wcaData.AreVRCLayersSetup)
            {
                try
                {
                    UpdateLayers.SetupEditorLayers();
                    UpdateLayers.SetupCollisionLayerMatrix();
                    AssetDatabase.LoadAssetAtPath<WCAData>("Assets/Varneon/WorldCreatorAssistant/Scripts/WCAData.asset").AreVRCLayersSetup = true;
                    Debug.Log($"{logPrefix} VRC Layers have been set up!");
                    AssetDatabase.DeleteAsset("Assets/Varneon/WorldCreatorAssistant/Scripts/LayerSetupHandler.cs");
                }
                catch(Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }
}
#endif