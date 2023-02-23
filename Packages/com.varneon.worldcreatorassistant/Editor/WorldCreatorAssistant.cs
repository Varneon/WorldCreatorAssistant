﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    internal class WorldCreatorAssistant : EditorWindow
    {
        private const string LogPrefix = "[<color=#009999>WorldCreatorAssistant</color>]:";

        private bool showPackageCacheDirectory, showDangerZoneSettings;
        private Dictionary.Translations dictionary;
        private Importer importer;
        private Resources resources;
        private string packageCacheDirectory;
        private DataStructs.UpdateCheckStatus wcaUpdateStatus = DataStructs.UpdateCheckStatus.Unchecked;
        private WCAFileUtility.FileValidityReport wcaFileValidityStatus;
        private bool wcaCleanInstall;
        private bool isWCAEmbeddedPackage;
        private static readonly GUILayoutOption[] settingsBlockButtonLayoutOptions = new GUILayoutOption[] { GUILayout.ExpandWidth(false), GUILayout.Height(15) };

        #region Page Variables
        private int pageNum = 0;
        private Page page = Page.Main;
        private Vector2 scrollPos;
        private string[] pages = System.Enum.GetNames(typeof(Page));
        #endregion

        private enum Page
        {
            Main,
            Tutorials,
            Importer,
            Resources,
            Settings
        }

        private void OnEnable()
        {
            isWCAEmbeddedPackage = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(AssetDatabase.GUIDToAssetPath("079b4a06e6b33ce458a06fea527f6a81")).source.Equals(UnityEditor.PackageManager.PackageSource.Embedded);

            if (EditorPrefs.HasKey(EditorPreferenceKeys.PackageCache))
            {
                packageCacheDirectory = EditorPrefs.GetString(EditorPreferenceKeys.PackageCache);
            }

            importer = new Importer(packageCacheDirectory);

            resources = new Resources();

            LoadActiveLanguage();
        }

        private void OnDestroy()
        {
            AssetDatabase.SaveAssets();
            UnityEngine.Resources.UnloadUnusedAssets();
        }

        private void OnGUI()
        {
            GUI.color = new Color(0.65f, 0.65f, 0.65f);
            pageNum = GUILayout.Toolbar(pageNum, pages, GUIResources.HeaderPageSelection, GUILayout.Width(position.width));
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
                    if (GUILayout.Button("https://vcc.docs.vrchat.com/guides/getting-started", EditorStyles.linkLabel))
                    {
                        Application.OpenURL("https://vcc.docs.vrchat.com/guides/getting-started");
                    }
                    if (GUILayout.Button("https://docs.vrchat.com/docs/creating-your-first-world", EditorStyles.linkLabel))
                    {
                        Application.OpenURL("https://docs.vrchat.com/docs/creating-your-first-world");
                    }
                    if (GUILayout.Button("https://docs.vrchat.com/docs/getting-started-with-udon", EditorStyles.linkLabel))
                    {
                        Application.OpenURL("https://docs.vrchat.com/docs/getting-started-with-udon");
                    }
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

            EditorGUI.DrawRect(new Rect(0, position.height - 26, position.width, 26), EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.65f, 0.65f, 0.65f));
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

            if (DictionaryLoader.ActiveLanguageIndex > 0)
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
            GUI.color = Color.red;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(dictionary.WCA_IS_DEPRECATED, GUIResources.CenteredHeaderLabel);
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
            GUILayout.Label(dictionary.PLEASE_USE_VRCHAT_CREATOR_COMPANION, EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            if(GUILayout.Button("https://vcc.docs.vrchat.com/", EditorStyles.linkLabel))
            {
                Application.OpenURL("https://vcc.docs.vrchat.com/");
            }
            GUILayout.Space(30);
            GUILayout.Label(Regex.Unescape(dictionary.WCA_THANK_YOU_FOR_USING), EditorStyles.wordWrappedLabel);
            GUILayout.Space(10);
            if (GUILayout.Button("https://github.com/Varneon", EditorStyles.linkLabel))
            {
                Application.OpenURL("https://github.com/Varneon");
            }
            GUILayout.Space(30);
            GUILayout.Label("- Varneon", EditorStyles.wordWrappedLabel);
        }

        private void DrawSettingsPage()
        {
            if (isWCAEmbeddedPackage)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.BeginHorizontal();
                GUILayout.Label(dictionary.CHECK_FOR_UPDATES);
                switch (wcaUpdateStatus)
                {
                    case DataStructs.UpdateCheckStatus.Unchecked:
                        if (GUILayout.Button(dictionary.CHECK_FOR_UPDATES, GUIResources.FlatStandardButton, settingsBlockButtonLayoutOptions)) { CheckForWCAUpdates(); }
                        break;
                    case DataStructs.UpdateCheckStatus.UpdateAvailable:
                        wcaCleanInstall = GUILayout.Toggle(wcaCleanInstall, dictionary.CLEAN_INSTALL, GUILayout.ExpandWidth(false));
                        if (GUILayout.Button(dictionary.UPDATE, GUIResources.FlatStandardButton, settingsBlockButtonLayoutOptions)) { UpdateWCA(); }
                        break;
                    case DataStructs.UpdateCheckStatus.VersionFileMissing:
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            GUILayout.Button(dictionary.VERSION_UNAVAILABLE, GUIResources.FlatStandardButton, settingsBlockButtonLayoutOptions);
                        }
                        break;
                    case DataStructs.UpdateCheckStatus.UpToDate:
                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            GUILayout.Button(dictionary.UP_TO_DATE, GUIResources.FlatStandardButton, settingsBlockButtonLayoutOptions);
                        }
                        break;
                    case DataStructs.UpdateCheckStatus.CouldNotFetchRelease:
                        if (GUILayout.Button(dictionary.CHECK_FOR_UPDATES, GUIResources.FlatStandardButton, settingsBlockButtonLayoutOptions)) { CheckForWCAUpdates(); }
                        break;
                }
                GUILayout.EndHorizontal();
                if (wcaUpdateStatus == DataStructs.UpdateCheckStatus.UpdateAvailable && !wcaFileValidityStatus.Verified)
                {
                    GUIElements.DrawWarningBox($"{dictionary.SOME_WCA_FILES_INVALID_DIRECTORIES}\n\n{wcaFileValidityStatus.InvalidDirectoryCount} {dictionary.N_FILES_IN_INVALID_DIRECTORIES}\n{wcaFileValidityStatus.InvalidGUIDCount} {dictionary.N_FILES_HAVE_INVALID_GUID}\n\n{dictionary.WCA_MAY_MALFUNCTION_AUTOMATIC_IMPORT}");
                }
                GUILayout.EndVertical();
            }

            GUIElements.LanguageSelection(LoadActiveLanguage);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            showPackageCacheDirectory = EditorGUILayout.Foldout(showPackageCacheDirectory, dictionary.PACKAGE_CACHE_DIRECTORY, true);
            if (showPackageCacheDirectory)
            {
                GUIElements.DrawHintPanel(dictionary.SPECIFY_PACKAGE_CACHE_DIRECTORY_DESC);

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(packageCacheDirectory, EditorStyles.wordWrappedLabel);
                if (GUIElements.BrowseButton(dictionary.BROWSE))
                {
                    string newPath = EditorUtility.OpenFolderPanel(dictionary.SELECT_PACKAGE_CACHE_DIRECTORY, Path.GetFullPath(Path.Combine(Application.dataPath, @"..\..\")), "");
                    if (!string.IsNullOrEmpty(newPath) && packageCacheDirectory != newPath)
                    {
                        if (Path.GetFullPath(newPath).StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath, @"..\")).TrimEnd(Path.DirectorySeparatorChar)))
                        {
                            EditorUtility.DisplayDialog(dictionary.INVALID_PACKAGE_CACHE_DIRECTORY, dictionary.PACKAGE_CACHE_CANT_BE_INSIDE_PROJECT, dictionary.OK);
                        }
                        else
                        {
                            packageCacheDirectory = newPath;
                            importer.UpdatePackageCacheDirectory(newPath);
                            EditorPrefs.SetString(EditorPreferenceKeys.PackageCache, packageCacheDirectory);
                        }
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
                if (GUILayout.Button(dictionary.CLEAR_WCA_REGISTRY_KEYS, GUIResources.FlatStandardButton))
                {
                    if (EditorUtility.DisplayDialog($"{dictionary.CLEAR_WCA_REGISTRY_KEYS}?", dictionary.ARE_YOU_SURE_CLEAR_WCA_REGISTRY_KEYS, dictionary.YES, dictionary.CANCEL))
                    {
                        UtilityMethods.DeleteRegistryKey(EditorPreferenceKeys.LastVRCAPIRequest);
                        UtilityMethods.DeleteRegistryKey(EditorPreferenceKeys.PackageCache);
                        UtilityMethods.DeleteRegistryKey(EditorPreferenceKeys.Language);
                    }
                }
                else if (GUILayout.Button(dictionary.CLEAR_VRC_SCRIPT_DEFINE_KEYWORDS, GUIResources.FlatStandardButton))
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

        private void CheckForWCAUpdates()
        {
            if (EditorUtility.DisplayDialog(dictionary.CHECK_FOR_UPDATES, dictionary.DO_YOU_WANT_CHECK_UPDATES_WCA, dictionary.YES, dictionary.CANCEL))
            {
                wcaFileValidityStatus = WCAFileUtility.CheckValidityOfWCAFiles();
                wcaUpdateStatus = UtilityMethods.CheckForWCAUpdates();
                switch (wcaUpdateStatus)
                {
                    case DataStructs.UpdateCheckStatus.OutOfRequests:
                        EditorUtility.DisplayDialog(dictionary.GITHUB_API_RATE_WARNING, $"{dictionary.GITHUB_NOT_ENOUGH_REQUESTS}", dictionary.OK);
                        break;
                    case DataStructs.UpdateCheckStatus.UpdateAvailable:
                        EditorUtility.DisplayDialog(dictionary.UPDATE_AVAILABLE, dictionary.WCA_NEW_VERSION_AVAILABLE, dictionary.OK);
                        break;
                    case DataStructs.UpdateCheckStatus.VersionFileMissing:
                        EditorUtility.DisplayDialog(dictionary.VERSION_UNAVAILABLE, $"{dictionary.VERSION_FILE_MISSING}", dictionary.OK);
                        break;
                    case DataStructs.UpdateCheckStatus.UpToDate:
                        EditorUtility.DisplayDialog(dictionary.UP_TO_DATE, $"{dictionary.WCA_IS_UP_TO_DATE}", dictionary.OK);
                        break;
                    case DataStructs.UpdateCheckStatus.CouldNotFetchRelease:
                        EditorUtility.DisplayDialog(dictionary.GITHUB_API_NOT_RESPONDING, $"{Regex.Unescape(dictionary.GITHUB_COULD_NOT_FETCH_RELEASE)}", dictionary.OK);
                        break;
                }
            }
        }

        private void UpdateWCA()
        {
            if (wcaCleanInstall)
            {
                string importedWCAPath = PackageManager.Instance.DownloadLatestRepository(packageCacheDirectory, "Varneon", "WorldCreatorAssistant");

                if (wcaCleanInstall) { WCAFileUtility.DeleteAllWCAFiles(importedWCAPath, this); }
            }
            else
            {
                DataStructs.ImportResponse response = PackageManager.Instance.DownloadAndImportLatestRepository(packageCacheDirectory, "Varneon", "WorldCreatorAssistant");

                if (response.Succeeded) { WCAFileUtility.DeleteWCAData(); Close(); }
            }
        }
    }
}
