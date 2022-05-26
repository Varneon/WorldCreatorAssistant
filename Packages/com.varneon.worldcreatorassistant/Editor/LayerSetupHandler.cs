#if (VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3) && !WCA_LAYERS_SETUP
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Varneon.WorldCreatorAssistant
{
    public static class LayerSetupHandler
    {
        private const string LogPrefix = "[<color=#009999>WorldCreatorAssistant</color>]:";

        [InitializeOnLoadMethod]
        public static void SetupLayers()
        {
            try
            {
                if (!UpdateLayers.AreLayersSetup()) { UpdateLayers.SetupEditorLayers(); }

                if (!UpdateLayers.IsCollisionLayerMatrixSetup()) { UpdateLayers.SetupCollisionLayerMatrix(); }
                    
                Debug.Log($"{LogPrefix} VRC Layers and Collision Matrix have been set up!");

                List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';').ToList();

                if (!defines.Contains("WCA_LAYERS_SETUP")) { defines.Add("WCA_LAYERS_SETUP"); }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", defines));
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
