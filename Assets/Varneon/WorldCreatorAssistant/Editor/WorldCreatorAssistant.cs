using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Varneon.WorldCreatorAssistant
{
    public class WorldCreatorAssistant : EditorWindow
    {
        const string LogPrefix = "[<color=#009999>WorldCreatorAssistant</color>]:";

        bool showPackageCacheDirectory, showDangerZoneSettings;
        Dictionary.Translations dictionary;
        Importer importer;
        Resources resources;
        string packageCacheDirectory;

        #region Page Variables
        int pageNum = 0;
        Page page = Page.Main;
        Vector2 scrollPos;
        static string[] pages;
        #endregion

        public enum Page
        {
            Main,
            Tutorials,
            Importer,
            Resources,
            Settings
        }

        private void OnEnable()
        {
            if (EditorPrefs.HasKey("Varneon/WCA/PackageCacheDirectory"))
            {
                packageCacheDirectory = EditorPrefs.GetString("Varneon/WCA/PackageCacheDirectory");
            }

            importer = new Importer(packageCacheDirectory);

            resources = new Resources();

            LoadActiveLanguage();

            string iconVariant = EditorGUIUtility.isProSkin ? "W" : "B";

            SpriteAtlas iconAtlas = UnityEngine.Resources.Load<SpriteAtlas>("Icons/Icons");
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
            GUI.color = new Color(0.65f, 0.65f, 0.65f);
            pageNum = GUILayout.Toolbar(pageNum, pages, GUIStyles.HeaderPageSelection, GUILayout.Width(Screen.width));
            GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            page = (Page)pageNum;
            switch (page)
            {
                case Page.Main:
                    DrawMainPage();
                    break;
                case Page.Tutorials:
                    GUILayout.Label(dictionary.COMING_SOON, GUIStyles.CenteredHeaderLabel);
                    //TODO Create tutorial framework
                    break;
                case Page.Importer:
                    importer.Draw();
                    break;
                case Page.Resources:
                    resources.Draw();
                    break;
                case Page.Settings:
                    DrawSettingsPage();
                    break;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

            EditorGUI.DrawRect(new Rect(0, Screen.height - 45, Screen.width, 22), (EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.65f, 0.65f, 0.65f)));
            EditorGUI.DrawRect(new Rect(0, Screen.height - 48, Screen.width, 3), (EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.5f, 0.5f, 0.5f)));
            GUILayout.BeginHorizontal();
            GUILayout.Label("World Creator Assistant by Varneon", EditorStyles.largeLabel);
            GUILayout.Label("[Open Alpha]", new GUIStyle(EditorStyles.largeLabel) { alignment = TextAnchor.MiddleRight });
            GUILayout.EndHorizontal();
        }

        private void LoadActiveLanguage()
        {
            dictionary = DictionaryLoader.ActiveDictionary;

            importer.LoadActiveDictionary();

            resources.LoadActiveDictionary();

            if (DictionaryLoader.ActiveLanguageIndex == 0)
            {
                pages = System.Enum.GetNames(typeof(Page));
            }
            else
            {
                pages = new string[]
                {
                    dictionary.MAIN,
                    dictionary.TUTORIALS,
                    dictionary.IMPORTER,
                    dictionary.RESOURCES,
                    dictionary.SETTINGS
                };
            }
        }

        private void DrawMainPage()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(dictionary.WCA_THANK_YOU_FOR_USING, GUIStyles.CenteredHeaderLabel);
            EditorGUILayout.EndHorizontal();
            GUILayout.Label(dictionary.WCA_THIS_EDITOR, EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            GUILayout.Label($"{dictionary.CURRENT_FEATURES}:", EditorStyles.boldLabel);
            GUILayout.Label(Regex.Unescape(dictionary.WCA_FEATURES));
            GUILayout.Space(10);
            GUILayout.Label($"{dictionary.FEATURES_IN_DEVELOPMENT}:", EditorStyles.boldLabel);
            GUILayout.Label(Regex.Unescape(dictionary.WCA_FEATURES_WIP));
        }

        private void DrawSettingsPage()
        {
            GUIElements.LanguageSelection(LoadActiveLanguage);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            showPackageCacheDirectory = EditorGUILayout.Foldout(showPackageCacheDirectory, dictionary.PACKAGE_CACHE_DIRECTORY, true);
            if (showPackageCacheDirectory)
            {
                GUIElements.DrawHintPanel(dictionary.SPECIFY_PACKAGE_CACHE_DIRECTORY_DESC);

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(packageCacheDirectory);
                if (GUILayout.Button(dictionary.BROWSE, GUIStyles.NonPaddedButton, new GUILayoutOption[] { GUILayout.MaxWidth(100), GUILayout.MaxHeight(16) }))
                {
                    string newPath = EditorUtility.OpenFolderPanel(dictionary.SELECT_PACKAGE_CACHE_DIRECTORY, "", "");
                    if (!string.IsNullOrEmpty(newPath) && packageCacheDirectory != newPath)
                    {
                        packageCacheDirectory = newPath;
                        importer.UpdatePackageCacheDirectory(newPath);
                        EditorPrefs.SetString("Varneon/WCA/PackageCacheDirectory", packageCacheDirectory);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUI.color = Color.red;
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUI.color = Color.white;
            GUILayout.BeginVertical();
            showDangerZoneSettings = EditorGUILayout.Foldout(showDangerZoneSettings, dictionary.DANGER_ZONE, true);
            if (showDangerZoneSettings)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(dictionary.CLEAR_WCA_REGISTRY_KEYS, GUIStyles.FlatStandardButton))
                {
                    if (EditorUtility.DisplayDialog($"{dictionary.CLEAR_WCA_REGISTRY_KEYS}?", dictionary.ARE_YOU_SURE_CLEAR_WCA_REGISTRY_KEYS, dictionary.YES, dictionary.CANCEL))
                    {
                        UtilityMethods.DeleteRegistryKey("Varneon/WCA/LastVRChatAPIRequest");
                        UtilityMethods.DeleteRegistryKey("Varneon/WCA/PackageCacheDirectory");
                        UtilityMethods.DeleteRegistryKey("Varneon/WCA/Language");
                    }
                }
                else if (GUILayout.Button(dictionary.CLEAR_VRC_SCRIPT_DEFINE_KEYWORDS, GUIStyles.FlatStandardButton))
                {
                    if (EditorUtility.DisplayDialog($"{dictionary.CLEAR_VRC_SCRIPT_DEFINE_KEYWORDS}?", Regex.Unescape(dictionary.ARE_YOU_SURE_CLEAR_VRC_SCRIPT_DEFINE_KEYWORDS), dictionary.YES, dictionary.CANCEL))
                    {
                        List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';').ToList();

                        if (defines.Contains("UDON")) { defines.Remove("UDON"); }
                        if (defines.Contains("VRC_SDK_VRCSDK3")) { defines.Remove("VRC_SDK_VRCSDK3"); }
                        if (defines.Contains("VRC_SDK_VRCSDK2")) { defines.Remove("VRC_SDK_VRCSDK2"); }

                        Debug.Log($"{LogPrefix} Removed VRC script define symbols!");

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
