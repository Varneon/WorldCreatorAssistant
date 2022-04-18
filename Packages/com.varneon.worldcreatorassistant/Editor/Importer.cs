using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    internal class Importer
    {
        const string LogPrefix = "[<color=#990099>WorldCreatorAssistant</color>]:";

        bool cleanInstall;
        bool sdkExpanded, prefabsExpanded, toolsExpanded;
        DataStructs.SDKVariant installedSDKVariant = DataStructs.SDKVariant.None;
        internal Texture iconCheckmark, iconDownload, iconGitHub, iconImport;
        readonly bool udonSharpImported;
        readonly List<bool> communityToolsExpanded, prefabRepositoriesExpanded;
        string packageCacheDirectory;
        bool isPackageCacheValid;
        readonly WCAData wcaData;
        Dictionary.Translations dictionary;

        internal void Draw()
        {
            if (!isPackageCacheValid) 
            {
                GUILayout.Label(dictionary.PLEASE_DEFINE_VALID_CACHE_DIRECTORY, GUIStyles.CenteredHeaderLabel);

                return; 
            }

            sdkExpanded = GUIElements.Foldout(sdkExpanded, "VRCSDK", DrawVRCSDKDrawer, dictionary.CHECK_FOR_UPDATES, CheckForVRCSDKUpdates);

            toolsExpanded = GUIElements.Foldout(toolsExpanded, dictionary.RECOMMENDED_COMMUNITY_TOOLS, DrawToolRepositoryList, dictionary.CHECK_FOR_UPDATES, CheckForCommunityToolUpdates);

            prefabsExpanded = GUIElements.Foldout(prefabsExpanded, dictionary.PREFAB_REPOSITORIES, DrawPrefabRepositoryList, dictionary.CHECK_FOR_UPDATES, CheckForPrefabRepositoryUpdates);

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            cleanInstall = GUILayout.Toggle(cleanInstall, new GUIContent(dictionary.CLEAN_INSTALL, dictionary.CLEAN_INSTALL_INFO));

            if (cleanInstall)
            {
                GUI.color = Color.red;

                GUILayout.Label(new GUIContent(dictionary.WARNING, dictionary.CLEAN_INSTALL_WARNING), EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));

                GUI.color = Color.white;
            }

            GUILayout.EndHorizontal();
        }

        #region Foldout drawers

        private void DrawVRCSDKDrawer()
        {
            GUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(!wcaData.IsVRCSDKUpdateAvailable);
#if VRC_SDK_VRCSDK2
            if (GUILayout.Button($"{dictionary.UPDATE} VRCSDK2", GUIStyles.FlatStandardButton))
            {
                if (EditorUtility.DisplayDialog($"{dictionary.UPDATE} VRCSDK?", dictionary.DO_YOU_WANT_TO_UPDATE_VRCSDK, dictionary.YES, dictionary.CANCEL))
                {
                    PackageManager.Instance.DownloadSDK(DataStructs.SDKVariant.SDK2, cleanInstall);
                }
            }
#elif VRC_SDK_VRCSDK3
            if (GUILayout.Button($"{dictionary.UPDATE} VRCSDK3", GUIStyles.FlatStandardButton))
            {
                if (EditorUtility.DisplayDialog($"{dictionary.UPDATE} VRCSDK?", dictionary.DO_YOU_WANT_TO_UPDATE_VRCSDK, dictionary.YES, dictionary.CANCEL))
                {
                    PackageManager.Instance.DownloadSDK(DataStructs.SDKVariant.SDK3Worlds, cleanInstall);
                }
            }
            /*
            else if (GUILayout.Button("Download / Update AV3", GUIStyles.FlatStandardButton))
            {
                if (EditorUtility.DisplayDialog("Download / Update AV3?", "Do you want to download / update VRChat Avatar 3.0 SDK?", "Yes", "Cancel"))
                {
                    PackageManager.Instance.DownloadSDK(DataStructs.SDKVariant.SDK3Avatars, cleanInstall);
                }
            }
            */
#endif
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label($"{dictionary.INSTALLED_VERSION}: {wcaData.InstalledVRCSDKVersion}");

            GUILayout.Label($"{dictionary.LATEST_VERSION}: {(string.IsNullOrEmpty(wcaData.LatestVRCSDKVersion) ? dictionary.CHECK_FOR_UPDATES_GET_LATEST_VERSION : wcaData.LatestVRCSDKVersion)}");

            GUIElements.DrawWarningBox(dictionary.CLEAN_INSTALL_VRCSDK_NOTICE);

            GUILayout.EndVertical();
        }

        private void DrawPrefabRepositoryList()
        {
            for(int i = 0; i < wcaData.PrefabRepositories.Count; i++)
            {
                DrawRepositoryBlock(wcaData.PrefabRepositories[i], prefabRepositoriesExpanded, i);
            }
        }

        private void DrawToolRepositoryList()
        {
            for (int i = 0; i < wcaData.CommunityTools.Count; i++)
            {
                DrawRepositoryBlock(wcaData.CommunityTools[i], communityToolsExpanded, i);
            }
        }

        #endregion

        private void CheckForCommunityToolUpdates()
        {
            CheckForRepositoryUpdates(wcaData.CommunityTools);
        }

        private void CheckForPrefabRepositoryUpdates()
        {
            CheckForRepositoryUpdates(wcaData.PrefabRepositories);
        }

        private void CheckForRepositoryUpdates(List<DataStructs.Repository> repositories)
        {
            if (EditorUtility.DisplayDialog(dictionary.CHECK_FOR_UPDATES, dictionary.DO_YOU_WANT_CHECK_UPDATES_GITHUB, dictionary.YES, dictionary.CANCEL))
            {
                DataStructs.GitHubApiStatus gitHubApiStatus = PackageManager.Instance.GetGitHubApiRateLimit();
                Debug.Log($"{LogPrefix}[<color=#999999>GitHub API</color>]:{gitHubApiStatus.RequestsRemaining}/{gitHubApiStatus.RequestLimit} {dictionary.USES_LEFT} | {dictionary.RESETS}: {gitHubApiStatus.ResetDateTime.ToLocalTime():MMMM dd, yyyy | h:mm:ss tt}");
                if (gitHubApiStatus.RequestsRemaining < repositories.Count)
                {
                    EditorUtility.DisplayDialog(dictionary.GITHUB_API_RATE_WARNING, $"{Regex.Unescape(dictionary.GITHUB_NOT_ENOUGH_REQUESTS)}:\n{gitHubApiStatus.ResetDateTime.ToLocalTime():MMMM dd, yyyy | h:mm: ss tt}", "OK");
                }
                else
                {
                    GetLatestRepositoryVersions(repositories);
                }

                CheckRepositories(repositories);
            }
        }

        private void CheckForVRCSDKUpdates()
        {
            if (EditorPrefs.HasKey(EditorPreferenceKeys.LastVRCAPIRequest))
            {
                int lastRequest = EditorPrefs.GetInt(EditorPreferenceKeys.LastVRCAPIRequest);

                int timeSinceLastRequest = UtilityMethods.GetElapsedTimeFromUnix(lastRequest);

                Debug.Log($"{LogPrefix} Last VRCSDK API request: {lastRequest} ({timeSinceLastRequest} seconds ago)");

                if(timeSinceLastRequest > 60)
                {
                    DisplayVRCSDKCheckUpdateDialog();
                }
                else
                {
                    EditorUtility.DisplayDialog(dictionary.WARNING, dictionary.PLEASE_WAIT_VRCAPI, dictionary.OK);
                }
            }
            else
            {
                EditorPrefs.SetInt(EditorPreferenceKeys.LastVRCAPIRequest, UtilityMethods.GetUnixTime());

                DisplayVRCSDKCheckUpdateDialog();
            }
        }

        private void DisplayVRCSDKCheckUpdateDialog() 
        {
            if (EditorUtility.DisplayDialog(dictionary.CHECK_FOR_UPDATES, dictionary.DO_YOU_WANT_CHECK_UPDATES_VRCSDK, dictionary.YES, dictionary.CANCEL))
            {
                Debug.Log($"{LogPrefix} Checking for VRCSDK updates...");

                string response = PackageManager.Instance.GetVRCSDKConfig();
                
                if(response == null)
                {
                    Debug.LogError($"{LogPrefix} Couldn't access VRC API config!");

                    return;
                }

                DataStructs.VRCApiConfig config = JsonUtility.FromJson<DataStructs.VRCApiConfig>(response.Replace("sdk3-worlds", "sdk3_worlds").Replace("sdk3-avatars", "sdk3_avatars"));
                
                Debug.Log($"SDK2: {config.downloadUrls.sdk2}");
                Debug.Log($"SDK3-Worlds: {config.downloadUrls.sdk3_worlds}");
                Debug.Log($"SDK3-Avatars: {config.downloadUrls.sdk3_avatars}");

                EditorPrefs.SetInt(EditorPreferenceKeys.LastVRCAPIRequest, UtilityMethods.GetUnixTime());

                switch (installedSDKVariant)
                {
                    case DataStructs.SDKVariant.SDK3Worlds:
                        wcaData.LatestVRCSDKVersion = UtilityMethods.ParseVersionText(config.downloadUrls.sdk3_worlds).ToString();
                        break;
                    case DataStructs.SDKVariant.SDK2:
                        wcaData.LatestVRCSDKVersion = UtilityMethods.ParseVersionText(config.downloadUrls.sdk2).ToString();
                        break;
                    case DataStructs.SDKVariant.None:
                        wcaData.LatestVRCSDKVersion = dictionary.UNAVAILABLE;
                        break;
                }

                wcaData.IsVRCSDKUpdateAvailable = new Version(wcaData.LatestVRCSDKVersion) > new Version(wcaData.InstalledVRCSDKVersion);
            }
        }

        private void DrawRepositoryBlock(DataStructs.Repository repository, List<bool> expandedList, int index = 0)
        {
            string url = UtilityMethods.GitHubPageURL(repository.Author, repository.Name);
            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            if(GUILayout.Button($"{repository.Name} | {repository.Author}", GUIStyles.BlockHeaderButton))
            {
                expandedList[index] ^= true;
            }

#if VRC_SDK_VRCSDK2
            if (repository.SDK3Only)
            {
                GUILayout.Label("VRCSDK3 Only", GUIStyles.VersionLabel, GUILayout.MaxWidth(120));
            }
#elif VRC_SDK_VRCSDK3
            if (repository.RequiresUdonSharp && !udonSharpImported)
            {
                GUILayout.Label("Requires UdonSharp", GUIStyles.VersionLabel, GUILayout.MaxWidth(120));
            }
#endif

            else
            {
                GUILayout.FlexibleSpace();
                if (repository.Imported && repository.UpdateAvailable)
                {
                    GUI.color = Color.green;
                    GUILayout.Box(new GUIContent($"{dictionary.UPDATE_AVAILABLE} | {(repository.LatestCached ? repository.DownloadedVersion : repository.CurrentVersion)}"), GUIStyles.UpdateLabel);
                    GUI.color = Color.white;
                }

                //Don't worry about this, I'll fix it at some point
                string statusLabel = repository.Imported ? $"{dictionary.IMPORTED}: {(repository.ImportedVersion == "0.0.0" ? $"{dictionary.VERSION_UNAVAILABLE} | {dictionary.DOWNLOADED}: {repository.DownloadedVersion}" : repository.ImportedVersion)}" : (repository.Downloaded ? $"{dictionary.DOWNLOADED}: {repository.DownloadedVersion}" : string.Empty);
                GUILayout.Label(statusLabel, GUIStyles.VersionLabel, GUILayout.MinHeight(20));
            }

            GUILayout.EndHorizontal();
            if (expandedList[index])
            {
                GUILayout.Space(5);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + 30, rect.width, 1), (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : Color.grey));

                GUILayout.BeginHorizontal();
                GUILayout.Label(repository.Description, GUIStyles.WrappedText);
                if (GUILayout.Button(new GUIContent(iconGitHub, "GitHub"), GUIStyle.none, GUILayout.MaxWidth(40)))
                {
                    Application.OpenURL(url);
                }
                if(!repository.RequiresUdonSharp || udonSharpImported)
                {
#if VRC_SDK_VRCSDK2
                    if(!repository.SDK3Only)
#elif VRC_SDK_VRCSDK3
                    if(true)
#endif
                    {
                        if (!cleanInstall && repository.Imported && !repository.UpdateAvailable && !repository.VersionFileMissing)
                        {
                            GUILayout.Box(new GUIContent(iconCheckmark, dictionary.UP_TO_DATE), GUIStyle.none, GUILayout.MaxWidth(40));
                        }
                        else if (repository.LatestCached && GUILayout.Button(new GUIContent(iconImport, dictionary.IMPORT), GUIStyle.none, GUILayout.MaxWidth(40)))
                        {
                            DataStructs.ImportResponse response = ImportRepository(repository, true);
                            if (response.Succeeded) { repository.ImportedVersion = response.Version; }
                        }
                        else if (!repository.LatestCached && GUILayout.Button(new GUIContent(iconDownload, dictionary.DOWNLOAD), GUIStyle.none, GUILayout.MaxWidth(40)))
                        {
                            DataStructs.GitHubApiStatus gitHubApiStatus = PackageManager.Instance.GetGitHubApiRateLimit();
                            Debug.Log($"{LogPrefix}[<color=#999999>GitHub API</color>]:{gitHubApiStatus.RequestsRemaining}/{gitHubApiStatus.RequestLimit} {dictionary.USES_LEFT} | {dictionary.RESETS}: {gitHubApiStatus.ResetDateTime.ToLocalTime():MMMM dd, yyyy | h:mm:ss tt}");
                            if (gitHubApiStatus.RequestsRemaining < 1)
                            {
                                EditorUtility.DisplayDialog(dictionary.GITHUB_API_RATE_WARNING, $"{Regex.Unescape(dictionary.GITHUB_NOT_ENOUGH_REQUESTS)}:\n{gitHubApiStatus.ResetDateTime.ToLocalTime():MMMM dd, yyyy | h:mm: ss tt}", "OK");
                            }
                            else
                            {
                                DataStructs.ImportResponse response = ImportRepository(repository, false);
                                if (response.Succeeded)
                                {
                                    repository.ImportedVersion = response.Version;
                                    repository.DownloadedVersion = repository.ImportedVersion;
                                    repository.Downloaded = true;
                                    repository.Imported = true;
                                    repository.LastRefreshed = DateTime.UtcNow.ToFileTime();
                                }
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
                if(repository.LastRefreshed != 0)
                {
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUILayout.Label($"{dictionary.LAST_CHECK}: {DateTime.FromFileTime(repository.LastRefreshed).ToLocalTime():MMMM dd, yyyy | h:mm:ss tt}");
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        private DataStructs.ImportResponse ImportRepository(DataStructs.Repository repository, bool fromCache)
        {
            if (!cleanInstall || (cleanInstall && ClearFolders(repository.Directories.ToArray())))
            {
                if (fromCache) 
                {
                    return PackageManager.Instance.ImportPackage(GetLatestCachedCommunityTool(repository.Name), returnVersion: true); 
                }

                return PackageManager.Instance.DownloadAndImportLatestRepository(packageCacheDirectory, repository.Author, repository.Name);
            }

            return new DataStructs.ImportResponse();
        }

        private bool ClearFolders(string[] directories)
        {
            string promptDescription = dictionary.WCA_WILL_TRY_TO_CLEAR_DIRECTORIES;

            foreach(string directory in directories)
            {
                promptDescription += $"\n{directory}";
            }

            if(!EditorUtility.DisplayDialog(dictionary.WCA_FOLDER_CLEANUP, promptDescription, dictionary.OK, dictionary.CANCEL)){ return false; };

            UtilityMethods.ClearDirectories(directories);

            return true;
        }

        internal Importer(string packageCacheDir)
        {
            UpdatePackageCacheDirectory(packageCacheDir);

            wcaData = UtilityMethods.LoadWCAData();

            LoadActiveDictionary();

            communityToolsExpanded = new List<bool>();

            for(int i = 0; i < wcaData.CommunityTools.Count; i++)
            {
                communityToolsExpanded.Add(false);
            }

            prefabRepositoriesExpanded = new List<bool>();

            for (int i = 0; i < wcaData.PrefabRepositories.Count; i++)
            {
                prefabRepositoriesExpanded.Add(false);
            }

            udonSharpImported = IsUdonSharpImported();

            CheckRepositories(wcaData.CommunityTools);

            CheckRepositories(wcaData.PrefabRepositories);

            wcaData.InstalledVRCSDKVersion = UtilityMethods.GetVersionFromDirectory("VRCSDK").ToString();

            GetInstalledSDKVariant();
        }

        internal void UpdatePackageCacheDirectory(string path)
        {
            packageCacheDirectory = path;

            isPackageCacheValid = Directory.Exists(packageCacheDirectory);
        }

        internal void LoadActiveDictionary()
        {
            dictionary = DictionaryLoader.ActiveDictionary;
        }

        private void GetInstalledSDKVariant()
        {
#if VRC_SDK_VRCSDK2
            installedSDKVariant = DataStructs.SDKVariant.SDK2;
#elif VRC_SDK_VRCSDK3
            installedSDKVariant = DataStructs.SDKVariant.SDK3Worlds;
#else
            installedSDKVariant = DataStructs.SDKVariant.None;
#endif
        }

        private void GetLatestRepositoryVersions(List<DataStructs.Repository> repositories)
        {
            for (int i = 0; i < repositories.Count; i++)
            {
                DataStructs.Repository repo = repositories[i];
#if VRC_SDK_VRCSDK2
                if (repo.SDK3Only) { continue; }
#endif
                Version version = PackageManager.Instance.GetLatestReleaseVersion(repo.Author, repo.Name);
                repo.CurrentVersion = version.ToString();
                repo.LastRefreshed = DateTime.UtcNow.ToFileTime();
                repositories[i] = repo;
            }

            UtilityMethods.SaveAsset(wcaData);
        }

        private void CheckRepositories(List<DataStructs.Repository> repositories)
        {
            for(int i = 0; i < repositories.Count; i++)
            {
                DataStructs.Repository repo = repositories[i];

                repo.UpdateRepositoryStatus(packageCacheDirectory);

                repositories[i] = repo;
            }

            UtilityMethods.SaveAsset(wcaData);
        }

        private string GetLatestCachedCommunityTool(string name)
        {
            string path = $"{packageCacheDirectory}/{name}";
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                int latestFile = 0;
                Version latestVersion = new Version();
                for(int i = 0; i < files.Length; i++)
                {
                    Version v = UtilityMethods.ParseVersionText(Path.GetFileName(files[i]));
                    if(v > latestVersion)
                    {
                        latestVersion = v;
                        latestFile = i;
                    }
                }
                return files[latestFile];
            }
            return string.Empty;
        }

        private bool IsUdonSharpImported()
        {
            foreach(DataStructs.Repository repo in wcaData.CommunityTools)
            {
                if(repo.Name != "UdonSharp") { continue; }

                return repo.GetRepositoryStatus(packageCacheDirectory).Imported;
            }

            return false;
        }
    }
}
