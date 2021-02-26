#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEngine.U2D;
using UnityEditor.SceneManagement;

namespace Varneon.WorldCreatorAssistant
{
    public class ProjectSetupWizard : EditorWindow
    {
        const string sdk2deprecationText = "VRCSDK2 is considered deprecated for creation. It will not receive most/any of the new features available in VRCSDK3-Worlds or VRCSDK3-Avatars. It will still receive important security and maintenance patches.\n\nIf you are creating new content, we strongly recommend using VRCSDK3 instead.";
        PackageManager packageManager = new PackageManager();
        bool importUdonSharp, importCyanEmu, importVRWorldToolkit, importUdonToolkit;
        DataStructs.SDKVariant selectedSDK;
        bool importAV3, resetLightSettings;
        string packageCacheDirectory;
        bool validCacheDirectory;
        WCAData wcaData;
        int page = 0;
        bool[] uasPackagesToImport, upmPackagesToImport;
        Vector2 scrollPos;
        bool isVRCSDKImported;

        #region Static

        private static readonly string logPrefix = "<color=#55FF99>[WCA Project Setup Wizard]</color>:";

        private static readonly string[] hintTexts = new string[]
        {
            "VRChat SDK provides all the essential components and tools for creating a VRChat world and uploading it.",
            "Utilizing these setup options saves you valuable time during the project setup as you only have to tick one box instead of setting up everything manually and they will be ready after the setup is done.",
            "Here you can import packages from the PackageManager",
            "Community has created numerous useful tools to help with VRChat world creation. Downloading and importing them during the setup can save a lot of time and doesn't require any knowledge about GitHub or downloading repositories.",
            "In this page you can import any downloaded packages from Unity Asset Store."
        };

        #endregion

        enum Pages
        {
            SelectSDK,
            SetupParameters,
            GitHubImporter,
            AssetStoreImporter
        }

        private void OnEnable()
        {
            wcaData = AssetDatabase.LoadAssetAtPath<WCAData>("Assets/Varneon/WorldCreatorAssistant/Scripts/WCAData.asset");

            if (EditorPrefs.HasKey("Varneon/WCA/PackageCacheDirectory"))
            {
                packageCacheDirectory = EditorPrefs.GetString("Varneon/WCA/PackageCacheDirectory");
                if (Directory.Exists(packageCacheDirectory))
                {
                    validCacheDirectory = true;
                }
            }

            wcaData.DownloadedUASPackages = new List<DataStructs.AssetStorePackage>(packageManager.GetDownloadedUASPackages());
            uasPackagesToImport = new bool[wcaData.DownloadedUASPackages.Count];

            upmPackagesToImport = new bool[wcaData.UPMPackages.Count];
        }

        private void OnGUI()
        {
            #region Progress Bar Top Labels
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Choose VRCSDK", page == 0 ? DataStructs.CenteredBoldLabel : DataStructs.CenteredLabel, GUILayout.Width(128));
            GUILayout.Label("Package Importer", page == 2 ? DataStructs.CenteredBoldLabel : DataStructs.CenteredLabel);
            GUILayout.Label("Asset Importer", page == 4 ? DataStructs.CenteredBoldLabel : DataStructs.CenteredLabel, GUILayout.Width(128));
            GUILayout.EndHorizontal();
            #endregion

            #region Progress Bar
            EditorGUI.ProgressBar(new Rect(64, 30, Screen.width - 128, 8), page / 4f, "");

            GUILayout.BeginHorizontal();
            //GUILayout.Space(Screen.width / 8 - 6);
            GUILayout.Space(58);
            for (int i = 0; i < 5; i++)
            {
                GUI.color = i <= page ? new Color(0.5f, 0.75f, 1f) : new Color(0.8f, 0.8f, 0.8f);
                GUILayout.Label(new GUIContent(), EditorStyles.radioButton);
                GUILayout.Space((Screen.width - 128) * 0.25f - 20);
                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();
            #endregion

            #region Progress Bar Bottom Labels
            GUILayout.BeginHorizontal();
            GUILayout.Space(64);
            GUILayout.Label("Setup Options", page == 1 ? DataStructs.CenteredBoldLabel : DataStructs.CenteredLabel, GUILayout.Width((Screen.width - 128) / 2));
            GUILayout.Label("GitHub Importer", page == 3 ? DataStructs.CenteredBoldLabel : DataStructs.CenteredLabel, GUILayout.Width((Screen.width - 128) / 2));
            GUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(10);

            EditorGUI.DrawRect(new Rect(0, Screen.height - 54, Screen.width, 31), (EditorGUIUtility.isProSkin ? new Color(0.16f, 0.16f, 0.16f) : new Color(0.65f, 0.65f, 0.65f)));

            switch (page)
            {
                case 0:
                    drawSDKPage();
                    break;
                case 1:
                    drawSetupOptionsPage();
                    break;
                case 2:
                    drawPackageImporterPage();
                    break;
                case 3:
                    drawGitHubImporterPage();
                    break;
                case 4:
                    drawAssetStoreImporterPage();
                    break;
            }

            GUILayout.Space(7);
        }

        private void downloadAndImport()
        {
            for (int i = 0; i < wcaData.UPMPackages.Count; i++)
            {
                if (upmPackagesToImport[i])
                {
                    packageManager.AddUPMPackage(wcaData.UPMPackages[i].Name);
                }
            }
            for (int i = 0; i < wcaData.DownloadedUASPackages.Count; i++)
            {
                if (uasPackagesToImport[i])
                {
                    packageManager.ImportPackage(wcaData.DownloadedUASPackages[i].Path);
                }
            }
            packageManager.DownloadSDK(selectedSDK);
            if(selectedSDK == DataStructs.SDKVariant.SDK3Worlds && importAV3) { packageManager.DownloadSDK(DataStructs.SDKVariant.SDK3Avatars); }
            if (importVRWorldToolkit) { downloadAndImportRepository("oneVR", "VRWorldToolkit"); } else { clearRepositoryData("oneVR", "VRWorldToolkit"); }
            if (importCyanEmu) { downloadAndImportRepository("CyanLaser", "CyanEmu"); } else { clearRepositoryData("CyanLaser", "CyanEmu"); }
            if (importUdonSharp) { downloadAndImportRepository("MerlinVR", "UdonSharp"); } else { clearRepositoryData("MerlinVR", "UdonSharp"); }
            if (importUdonToolkit) { downloadAndImportRepository("orels1", "UdonToolkit"); } else { clearRepositoryData("orels1", "UdonToolkit"); }
            if (resetLightSettings) { resetLightingSettings(); }
            disableAutoLightmapGeneration(); // Will disable always, can't think of situation where anyone would want to leave it enabled
            AssetDatabase.SaveAssets();
            openWorldCreatorAssistant();
        }

        private void drawInfoPanel()
        {
            GUI.color = new Color(0.5f, 0.75f, 1f);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(hintTexts[page], DataStructs.WrappedText);
            GUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private void drawSDKPage()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Please choose the VRCSDK you want to use", EditorStyles.boldLabel);

            #region SDK2
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(Screen.width / 2f - 10f));

            GUILayout.Label("VRCSDK2", DataStructs.CenteredHeaderLabel);

            if (selectedSDK == DataStructs.SDKVariant.SDK2)
            {
                GUI.color = Color.green;
                GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.MinHeight(29));
                GUILayout.Label("Selected", DataStructs.CenteredLabel, GUILayout.MinHeight(22));
                GUILayout.EndHorizontal();
                GUI.color = Color.white;
            }
            else if (GUILayout.Button("Select", DataStructs.FlatStandardButton, GUILayout.MinHeight(30)))
            {
                selectedSDK = DataStructs.SDKVariant.SDK2;
                importUdonSharp = false;
                importUdonToolkit = false;
            }

            GUILayout.Space(10);

            GUILayout.Label("Older SDK based on Triggers and Actions.", EditorStyles.wordWrappedLabel);

            GUILayout.Space(10);

            GUI.color = Color.yellow;

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUILayout.Label(new GUIContent("Deprecated", sdk2deprecationText), DataStructs.CenteredLabel);

            GUILayout.EndHorizontal();

            GUI.color = Color.white;

            GUILayout.EndVertical();
            #endregion

            #region SDK3-Worlds
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("VRCSDK3-Worlds", DataStructs.CenteredHeaderLabel);

            if (selectedSDK == DataStructs.SDKVariant.SDK3Worlds)
            {
                GUI.color = Color.green;
                GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.MinHeight(29));
                GUILayout.Label("Selected", DataStructs.CenteredLabel, GUILayout.MinHeight(22));
                GUILayout.EndHorizontal();
                GUI.color = Color.white;
            }
            else if (GUILayout.Button("Select", DataStructs.FlatStandardButton, GUILayout.MinHeight(30)))
            {
                selectedSDK = DataStructs.SDKVariant.SDK3Worlds;
            }

            GUILayout.Space(10);

            GUILayout.Label("Brand new SDK with VRChat's own visual programming language, Udon.", EditorStyles.wordWrappedLabel);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(selectedSDK != DataStructs.SDKVariant.SDK3Worlds);
            importAV3 = GUILayout.Toggle(importAV3, new GUIContent("Import Avatar 3.0", "AV3 contains useful components for extending the functionality of seats.\n\nIf you are planning to add more advanced seats in your worlds, it's recommended to install AV3 as well."));
            EditorGUI.EndDisabledGroup();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(20);

            #region Links
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("VRChat Documentation", "https://docs.vrchat.com/docs/choosing-your-sdk"), DataStructs.FlatStandardButton, GUILayout.Width(Screen.width / 2f - 10f)))
            {
                Application.OpenURL("https://docs.vrchat.com/docs/choosing-your-sdk");
            }
            else if (GUILayout.Button(new GUIContent("VRChat Download Page", "https://vrchat.com/home/download"), DataStructs.FlatStandardButton))
            {
                Application.OpenURL("https://vrchat.com/home/download");
            }

            GUILayout.EndHorizontal();
            #endregion

            GUILayout.EndVertical();

            drawInfoPanel();

            drawNavigationFooter(nextPage, nextDisabled: selectedSDK == DataStructs.SDKVariant.None);
        }

        private void drawSetupOptionsPage()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Project Setup Options", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.EndDisabledGroup();
            resetLightSettings = GUILayout.Toggle(resetLightSettings, new GUIContent("Reset unused lighting settings", "Reset the lighting setting parameters that are unused by default\n\nOptional"));
            EditorGUI.BeginDisabledGroup(selectedSDK == DataStructs.SDKVariant.SDK2);
            EditorGUI.EndDisabledGroup();
            if(selectedSDK == DataStructs.SDKVariant.SDK2)
            {
                drawWarningBox("For now the template scene is only available in VRCSDK3!");
            }
            GUILayout.EndVertical();

            drawInfoPanel();

            drawNavigationFooter(nextPage, previousPage);
        }

        private void drawPackageImporterPage()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Label("Import downloaded Unity Package Manager packages", EditorStyles.boldLabel);
            for(int i = 0; i < wcaData.UPMPackages.Count; i++)
            {
                upmPackagesToImport[i] = GUILayout.Toggle(upmPackagesToImport[i], wcaData.UPMPackages[i].Name);
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            drawInfoPanel();

            drawWarningBox("Unity Package Manager importer is disabled temporarily due to issues during the project setup process");

            drawNavigationFooter(nextPage, previousPage);
        }

        private void drawGitHubImporterPage()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.Label("Import recommended community tools", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Define directory for storing downloaded GitHub repositories to reduce unnecessary downloads", EditorStyles.wordWrappedLabel);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(packageCacheDirectory);
            if (GUILayout.Button("Browse", DataStructs.FlatStandardButton, new GUILayoutOption[] { GUILayout.MaxWidth(100), GUILayout.MaxHeight(15) }))
            {
                string newPath = EditorUtility.OpenFolderPanel("Select Package Cache Directory", "", "");
                if (!string.IsNullOrEmpty(newPath) && packageCacheDirectory != newPath)
                {
                    packageCacheDirectory = newPath;
                    EditorPrefs.SetString("Varneon/WCA/PackageCacheDirectory", packageCacheDirectory);
                    validCacheDirectory = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (validCacheDirectory)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                importVRWorldToolkit = GUILayout.Toggle(importVRWorldToolkit, "VRWorldToolkit");
                importCyanEmu = GUILayout.Toggle(importCyanEmu, "CyanEmu");
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.BeginDisabledGroup(selectedSDK != DataStructs.SDKVariant.SDK3Worlds);
                importUdonSharp = GUILayout.Toggle(importUdonSharp, "UdonSharp");
                EditorGUI.BeginDisabledGroup(!importUdonSharp);
                importUdonToolkit = GUILayout.Toggle(importUdonToolkit, importUdonSharp ? "UdonToolkit" : "UdonToolkit (Prerequisites: UdonSharp)");
                if (importUdonToolkit && !importUdonSharp) { importUdonToolkit = false; }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
                EditorGUI.EndDisabledGroup();
                if (selectedSDK == DataStructs.SDKVariant.SDK2)
                {
                    drawWarningBox("Anything Udon related is only available in VRCSDK3!");
                }
            }
            else
            {
                GUI.color = Color.yellow;
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label("Please define valid directory before adding repositories");
                EditorGUILayout.EndHorizontal();
                GUI.color = Color.white;
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            drawInfoPanel();

            drawNavigationFooter(nextPage, previousPage);
        }

        private void drawAssetStoreImporterPage()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.Label("Import downloaded Unity Asset Store packages", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < wcaData.DownloadedUASPackages.Count; i++)
            {
                GUILayout.BeginHorizontal();
                uasPackagesToImport[i] = GUILayout.Toggle(
                    uasPackagesToImport[i],
                    wcaData.DownloadedUASPackages[i].Name,
                    GUILayout.Width(Screen.width / 2)
                    );
                GUILayout.Label(wcaData.DownloadedUASPackages[i].Author, DataStructs.LeftGreyLabel, GUILayout.Height(20));
                GUILayout.Label($"{DataStructs.ParseFileSize(wcaData.DownloadedUASPackages[i].Size)}", DataStructs.VersionLabel);
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            drawInfoPanel();

            drawNavigationFooter(startSetup, previousPage, "Continue");
        }

        private void downloadAndImportRepository(string author, string name)
        {
            string version = packageManager.DownloadRepositoryLatest(packageCacheDirectory, author, name);
            for(int i = 0; i < wcaData.CommunityTools.Count; i++)
            {
                DataStructs.Repository repo = wcaData.CommunityTools[i];
                if(repo.Name == name && repo.Author == author)
                {
                    repo.UpdateAvailable = false;
                    repo.UpdateDownloaded = true;
                    repo.CurrentVersion = version;
                    repo.DownloadedVersion = version;
                    repo.ImportedVersion = version;
                    repo.LastRefreshed = DateTime.UtcNow.ToFileTime();
                    wcaData.CommunityTools[i] = repo;
                    return;
                }
            }
        }

        private void clearRepositoryData(string author, string name)
        {
            for (int i = 0; i < wcaData.CommunityTools.Count; i++)
            {
                DataStructs.Repository repo = wcaData.CommunityTools[i];
                if (repo.Name == name && repo.Author == author)
                {
                    repo.Imported = false;
                    repo.Downloaded = false;
                    repo.UpdateAvailable = false;
                    repo.UpdateDownloaded = false;
                    repo.CurrentVersion = "0.0.0";
                    repo.DownloadedVersion = "0.0.0";
                    repo.ImportedVersion = "0.0.0";
                    repo.LastRefreshed = 0;
                    wcaData.CommunityTools[i] = repo;
                    return;
                }
            }
        }

        private void openWorldCreatorAssistant()
        {
            EditorWindow window = CreateInstance<WorldCreatorAssistant>();
            window.titleContent.image = AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icons.spriteatlas").GetSprite($"World_{(EditorGUIUtility.isProSkin ? "W" : "B")}").texture;
            window.titleContent.text = "World Creator Assistant";
            window.minSize = new Vector2(512f, 512f);
            window.Show();
            this.Close();
        }

        private void disableAutoLightmapGeneration()
        {
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
            Debug.Log($"{logPrefix} Automatic lightmap generation has been disabled!");
        }

        private void resetLightingSettings()
        {
            RenderSettings.fogColor = new Color();
            RenderSettings.fogDensity = 0f;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 0f;
            RenderSettings.fogEndDistance = 1f;
            RenderSettings.haloStrength = 0f;
            RenderSettings.flareFadeSpeed = 0f;
            RenderSettings.flareStrength = 0f;
            Debug.Log($"{logPrefix} Lighting settings have been reset!");
        }

        private void startSetup()
        {
            if (EditorUtility.DisplayDialog(
                "Project Setup Wizard - Download & Import",
                String.Format("You are about to download following packages and import them into the project:\n\nVRC{0}{1}{2}{3}{4}{5}{6}{7}",
                    selectedSDK,
                    (selectedSDK == DataStructs.SDKVariant.SDK3Worlds && importAV3 ? $"\nVRC{DataStructs.SDKVariant.SDK3Avatars}" : string.Empty),
                    importVRWorldToolkit ? "\nVRWorldToolkit" : string.Empty,
                    importCyanEmu ? "\nCyanEmu" : string.Empty,
                    importUdonSharp ? "\nUdonSharp" : string.Empty,
                    importUdonToolkit ? "\nUdonToolkit" : string.Empty,
                    validCacheDirectory ? $"\n\nPackage cache directory:\n{packageCacheDirectory}\n" : string.Empty,
                    "\nNOTE: VRC layers will be set up and automatic lightmap generation will be disabled automatically"
                ),
                "Continue", 
                "Cancel"))
            {
                //AssetDatabase.importPackageCompleted += vrcsdkImported;
                downloadAndImport();
            }
        }

        private void previousPage()
        {
            page--;
        }

        private void nextPage()
        {
            page++;
        }

        private void drawNavigationFooter(Action next, Action prev = null, string nextText = "Next", bool nextDisabled = false)
        {
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (prev != null && GUILayout.Button("Previous", DataStructs.FlatStandardButton, new GUILayoutOption[] { GUILayout.Width(Screen.width / 4), GUILayout.Height(22) }))
            {
                prev();
            }
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(nextDisabled);
            if (GUILayout.Button(nextText, DataStructs.FlatStandardButton, new GUILayoutOption[] { GUILayout.Width(Screen.width / 4), GUILayout.Height(22) }))
            {
                next();
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();
        }

        private void drawWarningBox(string text)
        {
            GUI.color = Color.yellow;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(text, DataStructs.WrappedText);
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }
    }
}
#endif