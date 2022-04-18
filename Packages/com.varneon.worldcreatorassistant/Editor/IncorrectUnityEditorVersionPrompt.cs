using UnityEngine;
using UnityEditor;

namespace Varneon.WorldCreatorAssistant
{
    internal class IncorrectUnityEditorVersionPrompt : EditorWindow
    {
        private void OnGUI()
        {
#if UNITY_2018_4
            GUI.color = Color.yellow;
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUI.color = Color.white;
                GUILayout.Label("VRChat Has Upgraded To Unity 2019.4 LTS!", GUIStyles.CenteredHeaderLabel);
            }
#else
            GUI.color = Color.red;
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUI.color = Color.white;
                GUILayout.Label("Incorrect Unity Editor Version", GUIStyles.CenteredHeaderLabel);
            }
#endif

            GUILayout.Space(10);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
#if UNITY_2018_4
                GUILayout.Label("VRChat has upgraded to 2019.4 LTS and this Unity Editor Version is not supported anymore.\n\nPlease follow the instructions below on how to migrate from 2018 LTS to 2019 LTS.", GUIStyles.WrappedText);
                GUILayout.Space(10);
                if (GUILayout.Button("VRChat Docs - Migrating from 2018 LTS to 2019 LTS", GUIStyles.FlatStandardButton)) { Application.OpenURL("https://docs.vrchat.com/docs/migrating-from-2018-lts-to-2019-lts"); }
#else
                GUILayout.Label("This Unity editor version cannot be used for creating content for VRChat.\n\nPlease follow the instructions below to use the correct version of Unity.", GUIStyles.WrappedText);
                GUILayout.Space(10);
                if (GUILayout.Button("VRChat Docs - Currently Supported Unity Version", GUIStyles.FlatStandardButton)) { Application.OpenURL("https://docs.vrchat.com/docs/current-unity-version"); }
#endif
            }


            GUILayout.FlexibleSpace();

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button("VRChat Docs", GUIStyles.FlatStandardButton)) { Application.OpenURL("https://docs.vrchat.com"); }
                else if (GUILayout.Button("VRChat Discord", GUIStyles.FlatStandardButton)) { Application.OpenURL(" https://discord.gg/vrchat"); }
            }
        }
    }
}
