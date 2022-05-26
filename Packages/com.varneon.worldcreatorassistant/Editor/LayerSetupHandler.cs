
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace Varneon.WorldCreatorAssistant
{
    public static class LayerSetupHandler
    {
        private const string LogPrefix = "[<color=#009999>WorldCreatorAssistant</color>]:";

        private static readonly object[] EmptyArgs = new object[0];

        [InitializeOnLoadMethod]
        public static void SetupLayers()
        {
            WCAData wcaData = UtilityMethods.LoadWCAData(false);

            if(wcaData.AreLayersSetup && wcaData.IsCollisionLayerMatrixSetup) { return; }

            Type updateLayersType = Type.GetType("UpdateLayers, VRCCore-Editor");

            if (updateLayersType == null)
            {
                return;
            }

            try
            {
                MethodInfo areLayersSetupInfo = updateLayersType.GetMethod("AreLayersSetup", BindingFlags.Static | BindingFlags.Public);
                MethodInfo isCollisionLayerMatrixSetupInfo = updateLayersType.GetMethod("IsCollisionLayerMatrixSetup", BindingFlags.Static | BindingFlags.Public);
                MethodInfo setupEditorLayersInfo = updateLayersType.GetMethod("SetupEditorLayers", BindingFlags.Static | BindingFlags.Public);
                MethodInfo setupCollisionLayerMatrixInfo = updateLayersType.GetMethod("SetupCollisionLayerMatrix", BindingFlags.Static | BindingFlags.Public);

                if (!(wcaData.AreLayersSetup = (bool)areLayersSetupInfo.Invoke(null, EmptyArgs)))
                {
                    setupEditorLayersInfo.Invoke(null, EmptyArgs);

                    wcaData.AreLayersSetup = true;

                    Debug.Log($"{LogPrefix} VRC Layers have been set up!");
                }
                if (!(wcaData.IsCollisionLayerMatrixSetup = (bool)isCollisionLayerMatrixSetupInfo.Invoke(null, EmptyArgs)))
                {
                    setupCollisionLayerMatrixInfo.Invoke(null, EmptyArgs);

                    wcaData.IsCollisionLayerMatrixSetup = true;

                    Debug.Log($"{LogPrefix} VRC Collision Matrix has been set up!");
                }

                UtilityMethods.SaveAsset(wcaData);
            }
            catch (Exception e)
            {
                Debug.LogError($"{LogPrefix} {e}");

                return;
            }
        }
    }
}
