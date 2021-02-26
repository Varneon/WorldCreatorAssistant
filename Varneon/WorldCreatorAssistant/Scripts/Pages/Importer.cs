#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Varneon.WorldCreatorAssistant
{
    public class Importer
    {
        List<bool> communityToolsExpanded; //TODO abstract!
        bool sdkExpanded, vfabsExpanded, toolsExpanded;//, personalExpanded; // Coming soon
        public Texture iconCheckmark, iconNotification, iconDownload, iconGitHub, iconImport;
        bool udonSharpImported, sdkCleanInstall;
        string packageCacheDirectory;
        PackageManager packageManager = new PackageManager();
        DataStructs.GitHubApiRateLimit gitHubApiRateLimit;
        DateTime gitHubApiRateLimitReset;
        WCAData wcaData;

        private static string gitHubApi(string author, string name)
        {
            return $"https://api.github.com/repos/{author}/{name}/releases/latest";
        }

        private static string gitHubURL(string author, string name)
        {
            return $"https://github.com/{author}/{name}";
        }

        #region Static

        private static readonly string logPrefix = "<color=#990099>[WorldCreatorAssistant]</color>:";

        #endregion

        public void Draw()
        {
            #region VRCSDK
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            if (GUILayout.Button("VRCSDK", DataStructs.BlockHeaderButton))
            {
                sdkExpanded ^= true;
            }
            if (sdkExpanded)
            {
                sdkCleanInstall = GUILayout.Toggle(sdkCleanInstall, new GUIContent("Clean install", "Delete all existing VRCSDK files before importing"));

                /*
                if (GUILayout.Button("Check VRCSDK version", DataStructs.FlatStandardButton))
                {
                    string path = $"Assets/Varneon/WorldCreatorAssistant/Scripts/config.json";
                    if (File.Exists(path))
                    {
                        StreamReader reader = new StreamReader(path);
                        DataStructs.VRCApiConfig config = JsonUtility.FromJson<DataStructs.VRCApiConfig>(reader.ReadToEnd().Replace("sdk3-worlds", "sdk3_worlds").Replace("sdk3-avatars", "sdk3_avatars"));
                        Debug.Log($"SDK2: {config.downloadUrls.sdk2}");
                        Debug.Log($"SDK3-Worlds: {config.downloadUrls.sdk3_worlds}");
                        Debug.Log($"SDK3-Avatars: {config.downloadUrls.sdk3_avatars}");
                    }
                }
                */
                GUILayout.BeginHorizontal();

#if VRC_SDK_VRCSDK2
                if (GUILayout.Button("Update VRCSDK2", DataStructs.FlatStandardButton))
                {
                    if (EditorUtility.DisplayDialog("Update VRCSDK?", "Do you want to update VRChat SDK?", "Yes", "Cancel"))
                    {
                        packageManager.DownloadSDK(DataStructs.SDKVariant.SDK2, sdkCleanInstall);
                    }
                }
#elif VRC_SDK_VRCSDK3
                if(GUILayout.Button("Update VRCSDK3", DataStructs.FlatStandardButton))
                {
                    if (EditorUtility.DisplayDialog("Update VRCSDK?", "Do you want to update VRChat SDK?", "Yes", "Cancel"))
                    {
                        packageManager.DownloadSDK(DataStructs.SDKVariant.SDK3Worlds, sdkCleanInstall);
                    }
                }
                else if(GUILayout.Button("Download / Update AV3", DataStructs.FlatStandardButton))
                {
                    if (EditorUtility.DisplayDialog("Download / Update AV3?", "Do you want to download / update VRChat Avatar 3.0 SDK?", "Yes", "Cancel"))
                    {
                        packageManager.DownloadSDK(DataStructs.SDKVariant.SDK3Avatars, sdkCleanInstall);
                    }
                }
#endif
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
#endregion

#region Varneon's Prefabs

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            if (GUILayout.Button("Varneon's Udon Prefabs", DataStructs.BlockHeaderButton))
            {
                vfabsExpanded ^= true;
            }
            if (vfabsExpanded)
            {
#if VRC_SDK_VRCSDK2
                GUILayout.Label("Unfortunately my prefabs are not compatible with VRCSDK2");
#elif VRC_SDK_VRCSDK3
                GUILayout.Label("Coming soon");
#endif
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
#endregion

#region Community Tools
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Recommended Community Tools", DataStructs.BlockHeaderButton))
            {
                toolsExpanded ^= true;
            }
            if (toolsExpanded && GUILayout.Button(new GUIContent("Check for updates"), DataStructs.FlatStandardButton, GUILayout.Width(120)))
            {
                if (EditorUtility.DisplayDialog("Check for updates?", "Do you want to check for updates for the GitHub repositories?", "Yes", "Cancel"))
                {
                    gitHubApiRateLimit = packageManager.GetGitHubApiRateLimit();
                    gitHubApiRateLimitReset = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(gitHubApiRateLimit.resources.core.reset);
                    Debug.Log($"{logPrefix}<color=#999999>[GitHub API]</color>:{gitHubApiRateLimit.resources.core.remaining}/{gitHubApiRateLimit.resources.core.limit} Uses left | Resets: {gitHubApiRateLimitReset.ToLocalTime().ToString("MMMM dd, yyyy | h:mm:ss tt")}");
                    if (gitHubApiRateLimit.resources.core.remaining < wcaData.CommunityTools.Count)
                    {
                        EditorUtility.DisplayDialog("GitHub API rate limit warning!", $"You don't have enough GitHub API requests left\n\nRestrictions will be reset:\n{gitHubApiRateLimitReset.ToLocalTime().ToString("MMMM dd, yyyy | h:mm: ss tt")}", "OK");
                    }
                    else
                    {
                        getLatestCommunityToolVersions();
                    }
                    checkCommunityTools();
                }
            }
            GUILayout.EndHorizontal();
            if (toolsExpanded)
            {
                for (int i = 0; i < wcaData.CommunityTools.Count; i++)
                {
                    drawRepositoryBlock(wcaData.CommunityTools[i], i);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
#endregion
        }

        private void writeCommunityToolRepositoryInfo()
        {
            if (File.Exists("Assets/Varneon/WorldCreatorAssistant/Scripts/WCAData.asset"))
            {
                AssetDatabase.SaveAssets();
            }
            else
            {
                ScriptableObject data = ScriptableObject.CreateInstance<WCAData>();
                AssetDatabase.CreateAsset(data, "Assets/Varneon/WorldCreatorAssistant/Scripts/WCAData.asset");
                AssetDatabase.SaveAssets();
            }
        }

        private void drawRepositoryBlock(DataStructs.Repository repository, int index = 0)
        {
            string url = gitHubURL(repository.Author, repository.Name);
            Rect rect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button($"{repository.Name} | {repository.Author}", DataStructs.BlockHeaderButton))
            {
                communityToolsExpanded[index] ^= true;
            }
#if VRC_SDK_VRCSDK2
            if (repository.SDK3Only)
            {
                GUILayout.Label("VRCSDK3 Only", DataStructs.VersionLabel, GUILayout.MaxWidth(120));
            }
#elif VRC_SDK_VRCSDK3
            if (repository.RequiresUdonSharp && !udonSharpImported)
            {
                GUILayout.Label("Requires UdonSharp", DataStructs.VersionLabel, GUILayout.MaxWidth(120));
            }
#endif
            else
            {
                string importedLabel = repository.Imported ? $"Imported: {repository.ImportedVersion}" : (repository.Downloaded ? $"Downloaded: {repository.DownloadedVersion}" : string.Empty);
                GUILayout.Label(importedLabel, DataStructs.VersionLabel, GUILayout.MinHeight(20));
                if (repository.Imported && repository.UpdateAvailable)
                {
                    GUI.color = Color.green;
                    GUILayout.Box(new GUIContent("Update Available"), DataStructs.UpdateLabel, new GUILayoutOption[] { GUILayout.MaxWidth(105), GUILayout.MinHeight(20) });
                    GUI.color = Color.white;
                }
            }

            GUILayout.EndHorizontal();
            if (communityToolsExpanded[index])
            {
                GUILayout.Space(5);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + 30, rect.width, 1), (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : Color.grey));

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(repository.Description);
                if (GUILayout.Button(new GUIContent(iconGitHub, "GitHub"), GUIStyle.none, GUILayout.MaxWidth(40)))
                {
                    Application.OpenURL(url);
                }
                if(repository.RequiresUdonSharp ? udonSharpImported : true)
                {
#if VRC_SDK_VRCSDK2
                    if(!repository.SDK3Only)
#elif VRC_SDK_VRCSDK3
                    if(true)
#endif
                    {
                        if (repository.Imported && !repository.UpdateAvailable)
                        {
                            GUILayout.Box(new GUIContent(iconCheckmark, "Up to date"), GUIStyle.none, GUILayout.MaxWidth(40));
                        }
                        else if (repository.UpdateDownloaded && GUILayout.Button(new GUIContent(iconImport, "Import"), GUIStyle.none, GUILayout.MaxWidth(40)))
                        {
                            packageManager.ImportPackage(getLatestCachedCommunityTool(repository.Name));
                        }
                        else if (!repository.UpdateDownloaded && GUILayout.Button(new GUIContent(iconDownload, "Download"), GUIStyle.none, GUILayout.MaxWidth(40)))
                        {
                            repository.ImportedVersion = packageManager.DownloadRepositoryLatest(packageCacheDirectory, repository.Author, repository.Name);
                            repository.DownloadedVersion = repository.ImportedVersion;
                            repository.Downloaded = true;
                            repository.Imported = true;
                            repository.LastRefreshed = DateTime.UtcNow.ToFileTime();

                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                if(repository.LastRefreshed != 0)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUILayout.Label($"Last check: {DateTime.FromFileTime(repository.LastRefreshed).ToLocalTime().ToString("MMMM dd, yyyy | h:mm:ss tt")}");
                    EditorGUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        public Importer(string packageCacheDir)
        {
            packageCacheDirectory = packageCacheDir;

            wcaData = AssetDatabase.LoadAssetAtPath<WCAData>("Assets/Varneon/WorldCreatorAssistant/Scripts/WCAData.asset");

            //communityToolsExpanded = new List<bool>(wcaData.CommunityTools.Count);
            communityToolsExpanded = new List<bool>() { false, false, false, false };

            udonSharpImported = checkUdonSharp();

            checkCommunityTools();
        }

        private void getLatestCommunityToolVersions()
        {
            for (int i = 0; i < wcaData.CommunityTools.Count; i++)
            {
                DataStructs.Repository repo = wcaData.CommunityTools[i];
#if VRC_SDK_VRCSDK2
                if (repo.SDK3Only) { continue; }
#endif
                Version version = packageManager.GetLatestReleaseVersion(repo.Author, repo.Name);
                repo.UpdateDownloaded = new Version(repo.DownloadedVersion) >= version;
                repo.UpdateAvailable = (repo.UpdateDownloaded ? new Version(repo.DownloadedVersion) : version) > new Version(repo.ImportedVersion);
                repo.CurrentVersion = version.ToString();
                repo.LastRefreshed = DateTime.UtcNow.ToFileTime();
                wcaData.CommunityTools[i] = repo;
            }
            writeCommunityToolRepositoryInfo();
        }

        private void checkCommunityTools()
        {
            if(wcaData != null)
            {
                for (int i = 0; i < wcaData.CommunityTools.Count; i++)
                {
                    DataStructs.Repository repo = wcaData.CommunityTools[i];
                    string path = $"Assets/{repo.Name}/version.txt";
                    if (File.Exists(path))
                    {
                        StreamReader reader = new StreamReader(path);
                        repo.ImportedVersion = parseVersionText(reader.ReadToEnd()).ToString();
                        repo.Imported = true;
                    }
                    else
                    {
                        repo.ImportedVersion = "0.0.0";
                        repo.Imported = false;
                    }

                    string dir = $"{packageCacheDirectory}/{repo.Name}";
                    if (Directory.Exists(dir))
                    {
                        string[] files = Directory.GetFiles(dir);
                        if (files.Length > 0)
                        {
                            Version latestVersion = new Version();
                            foreach (string v in files)
                            {
                                Version version = new Version(Regex.Replace(v, "[^0-9.]", "").TrimStart('.').TrimEnd('.'));
                                if (version > latestVersion)
                                {
                                    latestVersion = version;
                                }
                            }
                            repo.DownloadedVersion = latestVersion.ToString();
                            repo.Downloaded = true;
                            if (repo.CurrentVersion == "0.0.0")
                            {
                                repo.CurrentVersion = repo.DownloadedVersion;
                                repo.UpdateDownloaded = true;
                            }
                        }
                        else
                        {
                            repo.Downloaded = false;
                        }
                    }

                    if (repo.Imported)
                    {
                        Version importedVersion = new Version(repo.ImportedVersion);
                        Version downloadedVersion = new Version(repo.DownloadedVersion);
                        Version currentVersion = new Version(repo.CurrentVersion);
                        if (downloadedVersion > importedVersion || currentVersion > importedVersion)
                        {
                            repo.UpdateAvailable = true;
                            repo.UpdateDownloaded = downloadedVersion >= currentVersion;
                        }
                        else
                        {
                            repo.UpdateAvailable = false;
                            repo.UpdateDownloaded = false;
                        }
                    }
                    wcaData.CommunityTools[i] = repo;
                }
            }
        }

        private Version parseVersionText(string version)
        {
            return new Version(Regex.Replace(version, "[^0-9.]", "").TrimStart('.').TrimEnd('.'));
        }

        private string getLatestCachedCommunityTool(string name)
        {
            string path = $"{packageCacheDirectory}/{name}";
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                int latestFile = 0;
                Version latestVersion = new Version();
                for(int i = 0; i < files.Length; i++)
                {
                    Version v = parseVersionText(files[i]);
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

        private bool checkUdonSharp()
        {
            if (File.Exists("Assets/UdonSharp/version.txt"))
            {
                StreamReader reader = new StreamReader("Assets/UdonSharp/version.txt");
                reader.Close();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
#endif