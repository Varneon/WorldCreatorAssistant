#if UNITY_EDITOR
namespace Varneon.WorldCreatorAssistant.Pages
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UI;
    #if VRC_SDK_VRCSDK3
    using VRC.Udon;
    #endif

    public class Utilities
    {
        static readonly Type[] uiTypes = new Type[] { typeof(Button), typeof(Slider), typeof(InputField), typeof(Scrollbar), typeof(Toggle), typeof(Dropdown) };

        static readonly string logPrefix = "<color=#009999>[World Creator Assistant]</color>:";

        enum uiTypesEnum
        {
            Button,
            Slider,
            InputField,
            Scrollbar,
            Toggle,
            Dropdown
        }

        enum applyType
        {
            Selected,
            SelectedAndChildren,
            Scene
        }

        public void Draw()
        {
#region UI Navigation
            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 25, rect.width, 1), (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : Color.grey));
            GUILayout.Label("Disable UI navigation", EditorStyles.boldLabel);
            GUILayout.Space(4);
            GUILayout.Label("If you have UI elements in your scene, setting the UI navigation to None can help if the player accidentally ends up taking control of a UI in the world when pressing keys on keyboard", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("https://docs.unity3d.com/2018.4/Documentation/Manual/script-SelectableNavigation.html", DataStructs.ButtonHyperlink))
            {
                Application.OpenURL("https://docs.unity3d.com/2018.4/Documentation/Manual/script-SelectableNavigation.html");
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply to selected", DataStructs.FlatStandardButton))
            {
                setUINavigationNone(applyType.Selected);
            }
            else if (GUILayout.Button("Apply to children", DataStructs.FlatStandardButton))
            {
                setUINavigationNone(applyType.SelectedAndChildren);
            }
            else if (GUILayout.Button("Apply to entire scene", DataStructs.FlatStandardButton))
            {
                setUINavigationNone(applyType.Scene);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
#endregion

#if VRC_SDK_VRCSDK3
#region UdonBehaviour collision ownership transfer
            rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 25, rect.width, 1), (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : Color.grey));
            GUILayout.Label("Disable UdonBehaviour collision ownership transfer", EditorStyles.boldLabel);
            GUILayout.Space(4);
            GUILayout.Label("Every object in a VRChat world is always 'owned' by one of the players and the physics are handled by that player. Having collision ownership transfer enabled may lead into unintended ownership transfers and cause issues regarding physics", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("https://ask.vrchat.com/t/ownership-of-objects/719/3", DataStructs.ButtonHyperlink))
            {
                Application.OpenURL("https://ask.vrchat.com/t/ownership-of-objects/719/3");
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply to selected", DataStructs.FlatStandardButton))
            {
                disableUdonCollisionOwnershipTransfer(applyType.Selected);
            }
            else if (GUILayout.Button("Apply to children", DataStructs.FlatStandardButton))
            {
                disableUdonCollisionOwnershipTransfer(applyType.SelectedAndChildren);
            }
            else if (GUILayout.Button("Apply to entire scene", DataStructs.FlatStandardButton))
            {
                disableUdonCollisionOwnershipTransfer(applyType.Scene);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
#endregion
#endif
        }

        private void setUINavigationNone(applyType type)
        {
            int componentCount = 0;

            foreach (Type t in uiTypes)
            {
                List<Component> components = new List<Component>();
                Navigation nav;
                uiTypesEnum typeEnum = (uiTypesEnum)Enum.Parse(typeof(uiTypesEnum), t.Name);
                switch (type)
                {
                    case applyType.Selected:
                        foreach (GameObject go in Selection.gameObjects)
                        {
                            components.AddRange(go.GetComponents(t));
                        }
                        break;
                    case applyType.SelectedAndChildren:
                        foreach (GameObject go in Selection.gameObjects)
                        {
                            components.AddRange(go.GetComponentsInChildren(t));
                        }
                        break;
                    case applyType.Scene:
                        components.AddRange((Component[])UnityEngine.Object.FindObjectsOfType(t));
                        break;
                }

                foreach (Component c in components)
                {
                    switch (typeEnum)
                    {
                        case uiTypesEnum.Button:
                            nav = ((Button)c).navigation;
                            nav.mode = Navigation.Mode.None;
                            ((Button)c).navigation = nav;
                            break;
                        case uiTypesEnum.Dropdown:
                            nav = ((Dropdown)c).navigation;
                            nav.mode = Navigation.Mode.None;
                            ((Dropdown)c).navigation = nav;
                            break;
                        case uiTypesEnum.InputField:
                            nav = ((InputField)c).navigation;
                            nav.mode = Navigation.Mode.None;
                            ((InputField)c).navigation = nav;
                            break;
                        case uiTypesEnum.Scrollbar:
                            nav = ((Scrollbar)c).navigation;
                            nav.mode = Navigation.Mode.None;
                            ((Scrollbar)c).navigation = nav;
                            break;
                        case uiTypesEnum.Slider:
                            nav = ((Slider)c).navigation;
                            nav.mode = Navigation.Mode.None;
                            ((Slider)c).navigation = nav;
                            break;
                        case uiTypesEnum.Toggle:
                            nav = ((Toggle)c).navigation;
                            nav.mode = Navigation.Mode.None;
                            ((Toggle)c).navigation = nav;
                            break;
                    }
                }

                componentCount += components.Count;
            }

            Debug.Log($"{logPrefix} Navigation has been disabled on {componentCount} UI components");
        }

#if VRC_SDK_VRCSDK3

        private void disableUdonCollisionOwnershipTransfer(applyType type)
        {
            List<UdonBehaviour> udonBehaviours = new List<UdonBehaviour>();
            //UdonBehaviour[] udonBehaviours = UnityEngine.Object.FindObjectsOfType<UdonBehaviour>();
            switch (type)
            {
                case applyType.Selected:
                    foreach (GameObject go in Selection.gameObjects)
                    {
                        udonBehaviours.AddRange(go.GetComponents<UdonBehaviour>());
                    }
                    break;
                case applyType.SelectedAndChildren:
                    foreach (GameObject go in Selection.gameObjects)
                    {
                        udonBehaviours.AddRange(go.GetComponentsInChildren<UdonBehaviour>());
                    }
                    break;
                case applyType.Scene:
                    udonBehaviours.AddRange(UnityEngine.Object.FindObjectsOfType<UdonBehaviour>());
                    break;
            }

            Undo.RecordObjects(udonBehaviours.ToArray(), "Disable collision ownership transfer");
            foreach (UdonBehaviour u in udonBehaviours)
            {
                u.AllowCollisionOwnershipTransfer = false;
            }

            Debug.Log($"{logPrefix} Disabled ownership collision transfer on {udonBehaviours.Count} UdonBehaviours");
        }

#endif

    }
}
#endif