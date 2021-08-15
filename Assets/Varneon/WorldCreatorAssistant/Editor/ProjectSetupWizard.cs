using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    internal class ProjectSetupWizard : EditorWindow
    {
        const string LogPrefix = "[<color=#33AA77>WCA Project Setup Wizard</color>]:";

        bool resetLightSettings;
        bool validCacheDirectory;
        bool[] uasPackagesToImport, upmPackagesToImport, communityToolsToImport;
        DataStructs.SDKVariant selectedSDK, customSDKVariant;
        string customSDKPath;
        Dictionary.Translations dictionary;
        int page = 0;
        int udonSharpIndex;
        string packageCacheDirectory;
        Vector2 scrollPos;
        WCAData wcaData;
        SetupMode setupMode;
        List<string> customUnitypackagePaths = new List<string>();
        List<PSWPage> pages = new List<PSWPage>();
        int pageCount;
        float lastWindowWidth;
        float progressBarLabelSpaceRemainder;
        float progressBarPointSpacing;
        const float ProgressBarMargin = 64f;

        private struct PSWPage
        {
            internal string Name;
            internal Action Draw;

            internal PSWPage(string name, Action draw)
            {
                Name = name;
                Draw = draw;
            }
        }

        private enum SetupMode
        {
            Basic,
            Advanced
        }

        private void OnEnable()
        {
            wcaData = UtilityMethods.LoadWCAData();

            LoadActiveLanguage();

            if (EditorPrefs.HasKey(EditorPreferenceKeys.PackageCache))
            {
                packageCacheDirectory = EditorPrefs.GetString(EditorPreferenceKeys.PackageCache);
                if (Directory.Exists(packageCacheDirectory))
                {
                    validCacheDirectory = true;
                }
            }

            wcaData.DownloadedUASPackages = new List<DataStructs.AssetStorePackage>(PackageManager.Instance.GetDownloadedUASPackages());

            uasPackagesToImport = new bool[wcaData.DownloadedUASPackages.Count];

            upmPackagesToImport = new bool[wcaData.UPMPackages.Count];

            communityToolsToImport = new bool[wcaData.CommunityTools.Count];

            for(int i = 0; i < wcaData.CommunityTools.Count; i++)
            {
                if(wcaData.CommunityTools[i].Name != "UdonSharp") { continue; }

                udonSharpIndex = i;

                break;
            }

            pageCount = pages.Count;

            CalculateProgressBarLayoutVariables();
        }

        private void OnGUI()
        {
            //Detect window width changes
            if(position.width != lastWindowWidth) { CalculateProgressBarLayoutVariables(); }

            #region Progress Bar
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            for (int i = 0; i < pageCount; i += 2)
            {
                if(i == 0 || i == pageCount - 1)
                {
                    if (i == pageCount - 1) { GUILayout.Space(progressBarLabelSpaceRemainder); }
                    GUILayout.Label(pages[i].Name, page == i ? GUIStyles.CenteredBoldLabel : GUIStyles.CenteredLabel, GUILayout.Width(ProgressBarMargin * 2f));
                    if(i < pageCount - 1) { GUILayout.Space(progressBarLabelSpaceRemainder); }
                    else if(pageCount == 3 && i == 0) { GUILayout.FlexibleSpace(); }
                }
                else
                {
                    GUILayout.Label(pages[i].Name, page == i ? GUIStyles.CenteredBoldLabel : GUIStyles.CenteredLabel, GUILayout.Width(progressBarPointSpacing * 2f));
                    if (i == pageCount - 2) { GUILayout.Space(ProgressBarMargin); }
                }
                
            }
            GUILayout.EndHorizontal();

#if UNITY_2019
            EditorGUI.ProgressBar(new Rect(ProgressBarMargin, 31, position.width - ProgressBarMargin * 2, 10), page / (pageCount - 1f), "");
#else
            EditorGUI.ProgressBar(new Rect(ProgressBarMargin, 30, position.width - ProgressBarMargin * 2, 8), page / (pageCount - 1f), "");
#endif
            GUILayout.BeginHorizontal();
            GUILayout.Space(58);

            for (int i = 0; i < pageCount; i++)
            {
                GUI.color = i <= page ? new Color(0.5f, 0.75f, 1f) : new Color(0.8f, 0.8f, 0.8f);
#if UNITY_2019
                GUI.Box(new Rect(ProgressBarMargin + progressBarPointSpacing * i - 8, 28, 0, 0), string.Empty, EditorStyles.radioButton);
#else
                GUI.Box(new Rect(ProgressBarMargin + progressBarPointSpacing * i - 8, 25, 0, 0), string.Empty, EditorStyles.radioButton);
#endif
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(21);

            GUILayout.BeginHorizontal();
            GUILayout.Space(ProgressBarMargin);
            for (int i = 1; i < pageCount; i += 2)
            {
                if (i == pageCount - 1)
                {
                    GUILayout.Space(progressBarLabelSpaceRemainder);
                    GUILayout.Label(pages[i].Name, page == i ? GUIStyles.CenteredBoldLabel : GUIStyles.CenteredLabel, GUILayout.Width(ProgressBarMargin * 2f));
                }
                else
                {
                    GUILayout.Label(pages[i].Name, page == i ? GUIStyles.CenteredBoldLabel : GUIStyles.CenteredLabel, GUILayout.Width(progressBarPointSpacing * 2f));
                    if (i == pageCount - 2) { GUILayout.Space(ProgressBarMargin); }
                }

            }
            GUILayout.EndHorizontal();
#endregion

            GUILayout.Space(10);

            EditorGUI.DrawRect(new Rect(0, position.size.y - 32, position.width, 32), EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.65f, 0.65f, 0.65f));

            pages[page].Draw.Invoke();

            GUILayout.Space(7);
        }

        private void InitializePages()
        {
            pages.Clear();
            pages.Add(new PSWPage(dictionary.CHOOSE_VRCSDK, DrawSDKPage));
            pages.Add(new PSWPage(dictionary.UNITYPACKAGE_IMPORTER, DrawUnitypackageImporter));
            pages.Add(new PSWPage(dictionary.SETUP_OPTIONS, DrawSetupOptionsPage));
            pages.Add(new PSWPage(dictionary.PACKAGE_IMPORTER, DrawUPMImporterPage));
            pages.Add(new PSWPage(dictionary.GITHUB_IMPORTER, DrawGitHubImporterPage));
            pages.Add(new PSWPage(dictionary.ASSET_IMPORTER, DrawUASImporterPage));
        }

        private void CalculateProgressBarLayoutVariables()
        {
            lastWindowWidth = position.width;

            progressBarLabelSpaceRemainder = (position.width - ProgressBarMargin * 2f) / (pageCount - 1) - ProgressBarMargin;

            progressBarPointSpacing = (position.width - ProgressBarMargin * 2) * (1f / (pageCount - 1));
        }

#region Pages
        private void DrawSDKPage()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label(dictionary.PLEASE_CHOOSE_VRCSDK, EditorStyles.boldLabel);

#region SDK2
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(position.width / 2f - 10f));

            GUILayout.Label("VRCSDK2", GUIStyles.CenteredHeaderLabel);

            if (selectedSDK == DataStructs.SDKVariant.SDK2)
            {
                GUI.color = Color.green;
#if UNITY_2019
                GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.MinHeight(33));
#else
                GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.MinHeight(29));
#endif
                GUILayout.Label(dictionary.SELECTED, GUIStyles.CenteredLabel, GUILayout.MinHeight(22));
                GUILayout.EndHorizontal();
                GUI.color = Color.white;
            }
            else if (GUILayout.Button(dictionary.SELECT, GUIStyles.FlatStandardButton, GUILayout.MinHeight(30)))
            {
                ResetSDKInfo();
                selectedSDK = DataStructs.SDKVariant.SDK2;

                for (int i = 0; i < communityToolsToImport.Length; i++)
                {
                    if (!wcaData.CommunityTools[i].SDK3Only) { continue; }
                    
                    communityToolsToImport[i] = false;
                }
            }

            GUILayout.Space(10);

            GUILayout.Label(dictionary.SDK2_DESCRIPTION, EditorStyles.wordWrappedLabel);

            GUILayout.Space(10);

            GUI.color = Color.yellow;

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUILayout.Label(new GUIContent(dictionary.DEPRECATED, dictionary.SDK2_DEPRECATION_TEXT), GUIStyles.CenteredLabel);

            GUILayout.EndHorizontal();

            GUI.color = Color.white;

            GUILayout.EndVertical();
#endregion

#region SDK3-Worlds
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("VRCSDK3-Worlds", GUIStyles.CenteredHeaderLabel);

            if (selectedSDK == DataStructs.SDKVariant.SDK3Worlds)
            {
                GUI.color = Color.green;
#if UNITY_2019
                GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.MinHeight(33));
#else
                GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.MinHeight(29));
#endif
                GUILayout.Label(dictionary.SELECTED, GUIStyles.CenteredLabel, GUILayout.MinHeight(22));
                GUILayout.EndHorizontal();
                GUI.color = Color.white;
            }
            else if (GUILayout.Button(dictionary.SELECT, GUIStyles.FlatStandardButton, GUILayout.MinHeight(30)))
            {
                ResetSDKInfo();
                selectedSDK = DataStructs.SDKVariant.SDK3Worlds;
            }

            GUILayout.Space(10);

            GUILayout.Label(dictionary.SDK3_DESCRIPTION, EditorStyles.wordWrappedLabel);

            GUILayout.Space(10);

            /*
            EditorGUI.BeginDisabledGroup(selectedSDK != DataStructs.SDKVariant.SDK3Worlds);
            importAV3 = GUILayout.Toggle(importAV3, new GUIContent("Import Avatar 3.0", "AV3 contains useful components for extending the functionality of seats.\n\nIf you are planning to add more advanced seats in your worlds, it's recommended to install AV3 as well."));
            EditorGUI.EndDisabledGroup();
            */

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
#endregion

            GUILayout.Space(20);

#region Links
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent(dictionary.VRC_DOCUMENTATION, "https://docs.vrchat.com/docs/choosing-your-sdk"), GUIStyles.FlatStandardButton, GUILayout.Width(position.width / 2f - 10f)))
            {
                Application.OpenURL("https://docs.vrchat.com/docs/choosing-your-sdk");
            }
            else if (GUILayout.Button(new GUIContent(dictionary.VRC_DOWNLOAD_PAGE, "https://vrchat.com/home/download"), GUIStyles.FlatStandardButton))
            {
                Application.OpenURL("https://vrchat.com/home/download");
            }

            GUILayout.EndHorizontal();
#endregion

            GUILayout.EndVertical();

            if(setupMode == SetupMode.Advanced)
            {
                GUILayout.Label("- OR -", GUIStyles.CenteredHeaderLabel);

                DrawCustomSDKField();
            }

            GUIElements.DrawHintPanel(dictionary.PSW_PAGE_HINT_CHOOSE_VRCSDK);

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            GUIElements.LanguageSelection(LoadActiveLanguage, false);

            DrawSetupModeSelection();

            DrawNavigationFooter(NextPage, nextDisabled: selectedSDK == DataStructs.SDKVariant.None && customSDKVariant == DataStructs.SDKVariant.None);

            GUILayout.EndHorizontal();
        }

        private void DrawSetupModeSelection()
        {
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(dictionary.SETUP_MODE);

                EditorGUI.BeginChangeCheck();

                setupMode = (SetupMode)EditorGUILayout.EnumPopup(setupMode, GUILayout.Width(100));

                if (EditorGUI.EndChangeCheck())
                {
                    if(setupMode == SetupMode.Basic) { ResetSDKInfo(); }
                }
            }
        }

        private void DrawCustomSDKField()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(dictionary.IMPORT_OPEN_BETA_SDK_MANUALLY, EditorStyles.boldLabel);

                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label(customSDKPath, EditorStyles.wordWrappedLabel);
                    if (GUIElements.BrowseButton(dictionary.BROWSE))
                    {
                        string newPath = EditorUtility.OpenFilePanelWithFilters(dictionary.SELECT_OPEN_BETA_SDK, "", new string[] { "Unitypackage", "unitypackage" });
                        if (!string.IsNullOrEmpty(newPath) && customSDKPath != newPath)
                        {
                            ResetSDKInfo();
                            customSDKPath = newPath;
                            customSDKVariant = UtilityMethods.GetVRCSDKVariantFromFileName(newPath);
                        }
                    }
                }

                switch (customSDKVariant)
                {
                    case DataStructs.SDKVariant.SDK3Worlds:
                        GUIElements.DrawSuccessPanel($"{dictionary.DETECTED_SDK_VARIANT}: {customSDKVariant}");
                        break;
                    case DataStructs.SDKVariant.SDK2:
                        GUIElements.DrawSuccessPanel($"{dictionary.DETECTED_SDK_VARIANT}: {customSDKVariant}");
                        break;
                    default:
                        GUIElements.DrawWarningBox(dictionary.SDK_VARIANT_NOT_DETECTED);
                        break;
                }
            }
        }

        private void DrawUnitypackageImporter()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                for (int i = 0; i < customUnitypackagePaths.Count; i++)
                {
                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        GUILayout.Label(customUnitypackagePaths[i], GUIStyles.WrappedText);

                        if (GUILayout.Button(dictionary.REMOVE, GUIStyles.FlatStandardButton))
                        {
                            customUnitypackagePaths.RemoveAt(i);
                        }
                    }
                }

                EditorGUILayout.EndScrollView();

                if (GUILayout.Button(dictionary.ADD_NEW_CUSTOM_UNITYPACKAGE, GUIStyles.FlatStandardButton))
                {
                    string packagePath = EditorUtility.OpenFilePanelWithFilters(dictionary.ADD_NEW_CUSTOM_UNITYPACKAGE, "", new string[] { "Unitypackage", "unitypackage" });
                    if (!string.IsNullOrEmpty(packagePath))
                    {
                        if (customUnitypackagePaths.Contains(packagePath)) { EditorUtility.DisplayDialog(dictionary.PACKAGE_HAS_ALREADY_BEEN_ADDED, dictionary.PACKAGE_WITH_SAME_PATH_ALREADY_ADDED, dictionary.OK); }
                        else
                        {
                            customUnitypackagePaths.Add(packagePath);
                        }
                    }
                }
            }

            GUIElements.DrawHintPanel(dictionary.PSW_PAGE_HINT_IMPORT_CUSTOM_UNITYPACKAGES);

            DrawNavigationFooter(NextPage, PreviousPage);
        }

        private void DrawSetupOptionsPage()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(dictionary.PROJECT_SETUP_OPTIONS, EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.EndDisabledGroup();
            resetLightSettings = GUILayout.Toggle(resetLightSettings, new GUIContent(dictionary.RESET_UNUSED_LIGHTING_SETTINGS, Regex.Unescape(dictionary.RESET_UNUSED_LIGHTING_SETTINGS_DESC)));
            EditorGUI.BeginDisabledGroup(selectedSDK == DataStructs.SDKVariant.SDK2);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();

            GUIElements.DrawHintPanel(dictionary.PSW_PAGE_HINT_SETUP_OPTIONS);

            DrawNavigationFooter(NextPage, PreviousPage);
        }

        private void DrawUPMImporterPage()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.Label(dictionary.IMPORT_UPM_PACKAGES, EditorStyles.boldLabel);
            if (upmPackagesToImport != null)
            {
                for (int i = 0; i < wcaData.UPMPackages.Count; i++)
                {
                    upmPackagesToImport[i] = GUILayout.Toggle(upmPackagesToImport[i], wcaData.UPMPackages[i].Name);
                }
            }
            else
            {
                GUIElements.DrawWarningBox(dictionary.COULD_NOT_FETCH_PACKAGE_LIST);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUIElements.DrawHintPanel(dictionary.PSW_PAGE_HINT_UPM_IMPORTER);

            DrawNavigationFooter(NextPage, PreviousPage);
        }

        private void DrawGitHubImporterPage()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.Label(dictionary.IMPORT_COMMUNITY_TOOLS, EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(dictionary.SPECIFY_PACKAGE_CACHE_DIRECTORY_DESC, EditorStyles.wordWrappedLabel);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
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
                        EditorPrefs.SetString(EditorPreferenceKeys.PackageCache, packageCacheDirectory);
                        validCacheDirectory = true;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (validCacheDirectory)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                for (int i = 0; i < wcaData.CommunityTools.Count; i++)
                {
                    DataStructs.Repository repo = wcaData.CommunityTools[i];

                    EditorGUI.BeginDisabledGroup((repo.SDK3Only && selectedSDK != DataStructs.SDKVariant.SDK3Worlds && customSDKVariant != DataStructs.SDKVariant.SDK3Worlds) || (repo.RequiresUdonSharp && !communityToolsToImport[udonSharpIndex]));
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    communityToolsToImport[i] = GUILayout.Toggle(communityToolsToImport[i] && (repo.RequiresUdonSharp ? communityToolsToImport[udonSharpIndex] : true), $"{repo.Name} {(repo.RequiresUdonSharp ? "[Prerequisites: UdonSharp]" : string.Empty)}");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(wcaData.CommunityTools[i].Author);
                    GUILayout.EndHorizontal();
                    GUILayout.Label(wcaData.CommunityTools[i].Description, EditorStyles.wordWrappedLabel);
                    GUILayout.EndVertical();
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUILayout.EndScrollView();
                
                if(selectedSDK != DataStructs.SDKVariant.SDK3Worlds && customSDKVariant != DataStructs.SDKVariant.SDK3Worlds)
                {
                    GUI.color = Color.yellow;
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUI.color = Color.white;
                    GUILayout.Label(dictionary.SOME_COMMUNITY_TOOLS_UNAVAILABLE_SDK, EditorStyles.wordWrappedLabel);
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUI.color = Color.yellow;
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(dictionary.DEFINE_VALID_CACHE_DIRECTORY);
                EditorGUILayout.EndHorizontal();
                GUI.color = Color.white;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUIElements.DrawHintPanel(dictionary.PSW_PAGE_HINT_GITHUB_IMPORTER);

            DrawNavigationFooter(CheckForGitHubApiRequestLimitAndNextPage, PreviousPage);
        }

        private void CheckForGitHubApiRequestLimitAndNextPage()
        {
            DataStructs.GitHubApiStatus gitHubApiStatus = PackageManager.Instance.GetGitHubApiRateLimit();
            Debug.Log($"{LogPrefix}[<color=#999999>GitHub API</color>]:{gitHubApiStatus.RequestsRemaining}/{gitHubApiStatus.RequestLimit} {dictionary.USES_LEFT} | {dictionary.RESETS}: {gitHubApiStatus.ResetDateTime.ToLocalTime():MMMM dd, yyyy | h:mm:ss tt}");
            if (gitHubApiStatus.RequestsRemaining < communityToolsToImport.Count(c => c))
            {
                EditorUtility.DisplayDialog(dictionary.GITHUB_API_RATE_WARNING, $"{Regex.Unescape(dictionary.GITHUB_NOT_ENOUGH_REQUESTS)}:\n{gitHubApiStatus.ResetDateTime.ToLocalTime():MMMM dd, yyyy | h:mm: ss tt}\n\n{dictionary.PSW_REDUCE_REPOSITORIES_OR_WAIT}", "OK");
            }
            else
            {
                NextPage();
            }
        }

        private void DrawUASImporterPage()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.Label(dictionary.IMPORT_ASSET_STORE_PACKAGES, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if(uasPackagesToImport != null && uasPackagesToImport.Length > 0)
            {
                for (int i = 0; i < wcaData.DownloadedUASPackages.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    uasPackagesToImport[i] = GUILayout.Toggle(
                        uasPackagesToImport[i],
                        wcaData.DownloadedUASPackages[i].Name,
                        GUILayout.Width(position.width / 2)
                        );
                    GUILayout.Label(wcaData.DownloadedUASPackages[i].Author, GUIStyles.LeftGreyLabel, GUILayout.Height(20));
                    GUILayout.Label($"{UtilityMethods.ParseFileSize(wcaData.DownloadedUASPackages[i].Size)}", GUIStyles.VersionLabel);
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUIElements.DrawWarningBox(dictionary.NO_ASSET_STORE_PACKAGES_AVAILABLE);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUIElements.DrawHintPanel(dictionary.PSW_PAGE_HINT_UAS_IMPORTER);

            DrawNavigationFooter(StartSetup, PreviousPage, dictionary.CONTINUE);
        }

        private void DrawNavigationFooter(Action next, Action prev = null, string nextText = "", bool nextDisabled = false)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (prev != null && GUILayout.Button(dictionary.PREVIOUS, GUIStyles.FlatStandardButton, new GUILayoutOption[] { GUILayout.Width(position.width / 4), GUILayout.Height(22) }))
            {
                prev();
            }
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(nextDisabled);
            if (GUILayout.Button(string.IsNullOrEmpty(nextText) ? dictionary.NEXT : nextText, GUIStyles.FlatStandardButton, new GUILayoutOption[] { GUILayout.Width(position.width / 4), GUILayout.Height(22) }))
            {
                next();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        private void PreviousPage()
        {
            page--;
        }

        private void NextPage()
        {
            page++;
        }
#endregion

        private void LoadActiveLanguage()
        {
            dictionary = DictionaryLoader.ActiveDictionary;

            InitializePages();
        }

        private void StartSetup()
        {
            string upmPackageList = string.Empty;

            for (int i = 0; i < upmPackagesToImport.Length; i++)
            {
                if (!upmPackagesToImport[i]) { continue; }

                upmPackageList += $"\t{wcaData.UPMPackages[i].Name}\n";
            }

            string assetList = string.Empty;

            for (int i = 0; i < uasPackagesToImport.Length; i++)
            {
                if (!uasPackagesToImport[i]) { continue; }

                assetList += $"\t{wcaData.DownloadedUASPackages[i].Name}\n";
            }

            string repositoryList = string.Empty;

            for (int i = 0; i < communityToolsToImport.Length; i++)
            {
                if (!communityToolsToImport[i]) { continue; }

                repositoryList += $"\t{wcaData.CommunityTools[i].Name}\n";
            }

            string summary = String.Format(dictionary.YOU_ARE_ABOUT_TO_DOWNLOAD + ":\n\n{0}\n\t{1}\n\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}\n{8}\n\t{9}\n\n{10}",
                    "VRCSDK:",
                    selectedSDK == DataStructs.SDKVariant.None ? customSDKPath : selectedSDK.ToString(),
                    "Unity Package Manager:",
                    upmPackageList,
                    "GitHub:",
                    repositoryList,
                    $"{dictionary.UNITY_ASSET_STORE}:",
                    assetList,
                    $"{dictionary.PACKAGE_CACHE_DIRECTORY}:",
                    validCacheDirectory ? packageCacheDirectory : string.Empty,
                    dictionary.NOTE_VRC_LAYERS_AND_LIGHTMAP_GENERATION
                );

            if (EditorUtility.DisplayDialog(
                "Project Setup Wizard - Download & Import",
                summary,
                dictionary.CONTINUE,
                dictionary.CANCEL))
            {
                DownloadAndImport();
            }
        }

        private void DownloadAndImport()
        {
            for (int i = 0; i < wcaData.UPMPackages.Count; i++)
            {
                if (upmPackagesToImport[i])
                {
                    PackageManager.Instance.AddUPMPackage(wcaData.UPMPackages[i].Name);
                }
            }

            for (int i = 0; i < wcaData.DownloadedUASPackages.Count; i++)
            {
                if (uasPackagesToImport[i])
                {
                    PackageManager.Instance.ImportPackage(wcaData.DownloadedUASPackages[i].Path);
                }
            }

            if (selectedSDK != DataStructs.SDKVariant.None) { PackageManager.Instance.DownloadSDK(selectedSDK); }
            else { PackageManager.Instance.ImportPackage(customSDKPath); }

            for (int i = 0; i < communityToolsToImport.Length; i++)
            {
                if (!communityToolsToImport[i])
                {
                    ClearRepositoryData(wcaData.CommunityTools[i].Author, wcaData.CommunityTools[i].Name);

                    continue;
                }

                DownloadAndImportRepository(wcaData.CommunityTools[i].Author, wcaData.CommunityTools[i].Name);
            }

            foreach(string path in customUnitypackagePaths)
            {
                PackageManager.Instance.ImportPackage(path);
            }

            if (resetLightSettings) { ResetLightingSettings(); }

            DisableAutoLightmapGeneration();

            UtilityMethods.SaveAsset(wcaData);

            OpenWorldCreatorAssistant();
        }

        private void DownloadAndImportRepository(string author, string name)
        {
            DataStructs.ImportResponse response = PackageManager.Instance.DownloadAndImportLatestRepository(packageCacheDirectory, author, name);

            if (!response.Succeeded) { Debug.LogError($"{LogPrefix} GitHub repository import failed! ({author}/{name})"); }

            string version = response.Version;

            for(int i = 0; i < wcaData.CommunityTools.Count; i++)
            {
                DataStructs.Repository repo = wcaData.CommunityTools[i];

                if (repo.Name != name || repo.Author != author) { continue; }

                repo.UpdateAvailable = false;
                repo.LatestCached = true;
                repo.CurrentVersion = version;
                repo.DownloadedVersion = version;
                repo.ImportedVersion = version;
                repo.LastRefreshed = DateTime.UtcNow.ToFileTime();

                wcaData.CommunityTools[i] = repo;

                return;
            }
        }

        private void ClearRepositoryData(string author, string name)
        {
            for (int i = 0; i < wcaData.CommunityTools.Count; i++)
            {
                DataStructs.Repository repo = wcaData.CommunityTools[i];

                if (repo.Name != name || repo.Author != author) { continue; }

                repo.Imported = false;
                repo.Downloaded = false;
                repo.UpdateAvailable = false;
                repo.LatestCached = false;
                repo.CurrentVersion = "0.0.0";
                repo.DownloadedVersion = "0.0.0";
                repo.ImportedVersion = "0.0.0";
                repo.LastRefreshed = 0;

                wcaData.CommunityTools[i] = repo;

                return;
            }
        }

        private void DisableAutoLightmapGeneration()
        {
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;

            Debug.Log($"{LogPrefix} Automatic lightmap generation has been disabled!");
        }

        private void ResetLightingSettings()
        {
            RenderSettings.fogColor = new Color();
            RenderSettings.fogDensity = 0f;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 0f;
            RenderSettings.fogEndDistance = 1f;
            RenderSettings.haloStrength = 0f;
            RenderSettings.flareFadeSpeed = 0f;
            RenderSettings.flareStrength = 0f;

            Debug.Log($"{LogPrefix} Lighting settings have been reset!");
        }

        private void ResetSDKInfo()
        {
            customSDKPath = string.Empty;
            customSDKVariant = DataStructs.SDKVariant.None;
            selectedSDK = DataStructs.SDKVariant.None;
        }

        private void OpenWorldCreatorAssistant()
        {
            EditorWindow window = CreateInstance<WorldCreatorAssistant>();
            window.titleContent.image = UnityEngine.Resources.Load<Texture>($"Icons/World_{(EditorGUIUtility.isProSkin ? "W" : "B")}");
            window.titleContent.text = "World Creator Assistant";
            window.minSize = new Vector2(512f, 512f);
            window.Show();
            this.Close();
        }
    }
}
