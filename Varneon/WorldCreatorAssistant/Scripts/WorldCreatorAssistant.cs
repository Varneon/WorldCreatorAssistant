#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.U2D;
using System.Linq;

namespace Varneon.WorldCreatorAssistant
{
    public class WorldCreatorAssistant : EditorWindow
    {
        #region Serialized
        List<DataStructs.Repository> repositories;
        string packageCacheDirectory;
        public DataStructs.Skills skills;
        #endregion

        Pages.Utilities utilities;
        Importer importer;
        Resources resources;

        WCAData wcaData;

        bool showPackageCacheDirectory, showDangerZoneSettings;

        SerializedObject so;

        #region Page Variables
        private int pageNum = 0;
        private Page page = Page.Main;
        private Vector2 scrollPos;
        private static string[] pages;
        #endregion

        #region Static

        private static readonly string logPrefix = "<color=#009999>[WorldCreatorAssistant]</color>:";

        #endregion

        #region Enums
        public enum Page
        {
            Main,
            Utilities,
            Tutorials,
            Importer,
            Resources,
            Settings
        }

#endregion

        private void OnEnable()
        {
            if (EditorPrefs.HasKey("Varneon/WCA/PackageCacheDirectory"))
            {
                packageCacheDirectory = EditorPrefs.GetString("Varneon/WCA/PackageCacheDirectory");
            }

            wcaData = AssetDatabase.LoadAssetAtPath<WCAData>("Assets/Varneon/WorldCreatorAssistant/Scripts/WCAData.asset");

            pages = System.Enum.GetNames(typeof(Page));
            utilities = new Pages.Utilities();
            importer = new Importer(packageCacheDirectory);
            resources = new Resources();

            #region Icon Generation
            /*
            importer.iconCheckmark = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Checkmark.png");
            importer.iconDownload = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Download.png");
            importer.iconNotification = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Notification.png");
            importer.iconGitHub = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_GitHub.png");
            importer.iconImport = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Import.png");
            resources.iconWeb = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Web.png");
            resources.iconCopy = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Copy.png");
            */
            /*
            importer.iconCheckmark = wcaData.Icons[DataStructs.IconType.Checkmark];
            importer.iconDownload = wcaData.Icons[DataStructs.IconType.Download];
            importer.iconNotification = wcaData.Icons[DataStructs.IconType.Notification];
            importer.iconGitHub = wcaData.Icons[DataStructs.IconType.GitHub];
            importer.iconImport = wcaData.Icons[DataStructs.IconType.Import];
            resources.iconWeb = wcaData.Icons[DataStructs.IconType.Web];
            resources.iconCopy = wcaData.Icons[DataStructs.IconType.Copy];
            */
            /*
            wcaData.Icons = new Dictionary<DataStructs.IconType, Texture>();
            wcaData.Icons.Add(DataStructs.IconType.Checkmark, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Checkmark.png"));
            wcaData.Icons.Add(DataStructs.IconType.Copy, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Copy.png"));
            wcaData.Icons.Add(DataStructs.IconType.Download, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Download.png"));
            wcaData.Icons.Add(DataStructs.IconType.GitHub, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_GitHub.png"));
            wcaData.Icons.Add(DataStructs.IconType.Import, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Import.png"));
            wcaData.Icons.Add(DataStructs.IconType.Notification, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Notification.png"));
            wcaData.Icons.Add(DataStructs.IconType.Web, AssetDatabase.LoadAssetAtPath<Texture>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icon_Web.png"));
            AssetDatabase.SaveAssets();
            */
            #endregion

            string iconVariant = EditorGUIUtility.isProSkin ? "W" : "B";

            SpriteAtlas iconAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icons.spriteatlas");
            importer.iconCheckmark = iconAtlas.GetSprite("Checkmark").texture;
            importer.iconDownload = iconAtlas.GetSprite($"Download_{iconVariant}").texture;
            importer.iconNotification = iconAtlas.GetSprite("Notification").texture;
            importer.iconGitHub = iconAtlas.GetSprite($"GitHub_{iconVariant}").texture;
            importer.iconImport = iconAtlas.GetSprite($"Import_{iconVariant}").texture;
            resources.iconWeb = iconAtlas.GetSprite($"Web_{iconVariant}").texture;
            resources.iconCopy = iconAtlas.GetSprite($"Copy_{iconVariant}").texture;
        }

        private void OnGUI()
        {
            if (EditorApplication.isPlaying) return;

            GUI.color = new Color(0.65f, 0.65f, 0.65f);
            //pageNum = GUILayout.Toolbar(pageNum, pages, EditorStyles.toolbar, GUILayout.MinHeight(30));
            pageNum = GUILayout.Toolbar(pageNum, pages, DataStructs.HeaderPageSelection);
            GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            page = (Page)pageNum;
            switch (page)
            {
                case Page.Main:
                    drawMainPage();
                    //TODO Is this page needed at all?
                    break;
                case Page.Utilities:
                    utilities.Draw();
                    break;
                case Page.Tutorials:
                    GUILayout.Label("Coming Soon", DataStructs.CenteredHeaderLabel);
                    //TODO Create tutorial framework
                    break;
                case Page.Importer:
                    importer.Draw();
                    break;
                case Page.Resources:
                    resources.Draw();
                    break;
                case Page.Settings:
                    //settings.Draw();
                    drawSettingsPage();
                    break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

            EditorGUI.DrawRect(new Rect(0, Screen.height - 45, Screen.width, 22), (EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.65f, 0.65f, 0.65f)));
            EditorGUI.DrawRect(new Rect(0, Screen.height - 48, Screen.width, 3), (EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.5f, 0.5f, 0.5f)));
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.Label("World Creator Assistant by: Varneon", EditorStyles.largeLabel);
            GUILayout.Label("[Closed Alpha]", new GUIStyle(EditorStyles.largeLabel) { alignment = TextAnchor.MiddleRight });
            GUILayout.EndHorizontal();
        }

        private void drawMainPage()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Thank you for using World Creator Assistant!", DataStructs.CenteredHeaderLabel);
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("This editor extension is still heavily work in progress and should only be used for testing purposes.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            GUILayout.Label("Current features:", EditorStyles.boldLabel);
            GUILayout.Label("-Project Setup Wizard\n-Utilities\n-VRCSDK and GitHub importer\n-Resources and FAQ");
            GUILayout.Space(10);
            GUILayout.Label("Features in development:", EditorStyles.boldLabel);
            GUILayout.Label("-Automatic GitHub repository release check\n-Built-in tutorials\n-Custom GitHub repository importer");
        }

        private void drawSettingsPage()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical();
            showPackageCacheDirectory = EditorGUILayout.Foldout(showPackageCacheDirectory, "Package Cache Directory", true);
            if (showPackageCacheDirectory)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(packageCacheDirectory);
                if (GUILayout.Button("Browse", DataStructs.NonPaddedButton, new GUILayoutOption[] { GUILayout.MaxWidth(100), GUILayout.MaxHeight(16) }))
                {
                    string newPath = EditorUtility.OpenFolderPanel("Select Package Cache Directory", "", "");
                    if (!string.IsNullOrEmpty(newPath) && packageCacheDirectory != newPath)
                    {
                        packageCacheDirectory = newPath;
                        EditorPrefs.SetString("Varneon/WCA/PackageCacheDirectory", packageCacheDirectory);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            so = new SerializedObject(this);
            so.Update();
            SerializedProperty property = so.FindProperty("skills");
            EditorGUILayout.PropertyField(property, true);
            so.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();

            GUI.color = Color.red;
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.color = Color.white;
            GUILayout.BeginVertical();
            showDangerZoneSettings = EditorGUILayout.Foldout(showDangerZoneSettings, "Danger Zone", true);
            if (showDangerZoneSettings)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear WCA registry keys", DataStructs.FlatStandardButton))
                {
                    if (EditorUtility.DisplayDialog("Clear WCA registry keys?", "Are you sure you want to clear WCA registry keys?", "Yes", "Cancel"))
                    {
                        if (EditorPrefs.HasKey("Varneon/WCA/PackageCacheDirectory"))
                        {
                            EditorPrefs.DeleteKey("Varneon/WCA/PackageCacheDirectory");
                        }
                    }
                }
                else if (GUILayout.Button("Clear VRC script define keywords", DataStructs.FlatStandardButton))
                {
                    if (EditorUtility.DisplayDialog("Clear VRC script define keywords?", "Are you sure you want to clear VRC script define keywords?\nOnly do this if you have deleted all VRCSDK files and are planning to do a clean install", "Yes", "Cancel"))
                    {
                        List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';').ToList();

                        if (defines.Contains("UDON")) { defines.Remove("UDON"); }
                        if (defines.Contains("VRC_SDK_VRCSDK3")) { defines.Remove("VRC_SDK_VRCSDK3"); }
                        if (defines.Contains("VRC_SDK_VRCSDK2")) { defines.Remove("VRC_SDK_VRCSDK2"); }

                        Debug.Log($"{logPrefix} Removed VRC script define symbols!");

                        PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", defines));
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
#endif