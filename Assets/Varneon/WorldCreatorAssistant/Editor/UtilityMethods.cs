using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Version = System.Version;

namespace Varneon.WorldCreatorAssistant
{
    internal static class UtilityMethods
    {
        internal static string GetGitHubApiLatestReleaseURL(string author, string name)
        {
            return $"https://api.github.com/repos/{author}/{name}/releases/latest";
        }

        internal static string GitHubPageURL(string owner, string repo)
        {
            return $"https://github.com/{owner}/{repo}";
        }

        internal static Version ParseVersionText(string version)
        {
            string v = Regex.Match(Regex.Match(version, "[0-9.][0-9.][0-9.]*").Value, @"^([^.]*)\.([^.]*)\.([^.]*)").Value.TrimStart('.').TrimEnd('.');

            return new Version(v);
        }

        internal static Version GetVersionFile(string path)
        {
            string fullPath = Path.GetFullPath($"Assets/{path.TrimStart("Assets/".ToCharArray())}/version.txt");

            if (File.Exists(fullPath))
            {
                StreamReader reader = new StreamReader(fullPath);

                Version version = ParseVersionText(Regex.Match(reader.ReadToEnd(), @"^([^.]*)\.([^.]*)\.([^.]*)").Value);

                reader.Close();

                return version;
            }

            return new Version();
        }

        internal static DataStructs.SDKVariant GetVRCSDKVariantFromFileName(string name)
        {
            if(Regex.IsMatch(name, "VRCSDK3-WORLD")) { return DataStructs.SDKVariant.SDK3Worlds; }
            else if(Regex.IsMatch(name, "VRCSDK2")) { return DataStructs.SDKVariant.SDK2; }

            return DataStructs.SDKVariant.None;
        }

        internal static DateTime GetDateTimeFromUnix(int time)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(time);
        }

        internal static int GetElapsedTimeFromUnix(int time)
        {
            return (int)(DateTime.Now - GetDateTimeFromUnix(time)).TotalSeconds;
        }

        internal static int GetUnixTime()
        {
            return (int)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        internal static void DeleteRegistryKey(string key)
        {
            if (EditorPrefs.HasKey(key))
            {
                EditorPrefs.DeleteKey(key);
            }
        }

        internal static void SaveAsset(UnityEngine.Object asset)
        {
            if(asset == null) { return; }

            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();
        }

        internal static DataStructs.RepositoryStatus GetRepositoryStatus(this DataStructs.Repository repository, string packageCacheDirectory)
        {
            DataStructs.RepositoryStatus status = new DataStructs.RepositoryStatus();

            string folder = repository.Directories[0] ?? repository.Name;

            status.ImportedVersion = repository.ImportedVersion;

            status.Imported = Directory.Exists(folder);

            if (status.Imported)
            {
                Version versionFromFile = GetVersionFile(folder);

                if (versionFromFile != new Version())
                {
                    status.ImportedVersion = versionFromFile.ToString();
                }
            }

            string dir = $"{packageCacheDirectory}/{repository.Name}";

            GetRepositoryDownloadStatus(dir, out status.DownloadedVersion, out status.Downloaded);

            Version importedVersion = new Version(status.ImportedVersion);
            Version downloadedVersion = new Version(status.DownloadedVersion);
            Version currentVersion = new Version(repository.CurrentVersion);
            
            if (repository.Imported && (downloadedVersion > importedVersion || currentVersion > importedVersion))
            {
                if (repository.ImportedVersion != "0.0.0")
                { 
                    status.UpdateAvailable = true;
                }
                else
                {
                    status.VersionFileMissing = true;
                }
            }
            else
            {
                status.UpdateAvailable = false;
            }

            if (repository.CurrentVersion == "0.0.0")
            {
                status.CurrentVersion = status.DownloadedVersion;
            }

            status.LatestCached = downloadedVersion != new Version("0.0.0") && downloadedVersion >= currentVersion;

            return status;
        }

        private static void GetRepositoryDownloadStatus(string directory, out string version, out bool downloaded)
        {
            version = "0.0.0"; 
            downloaded = false;

            if (!Directory.Exists(directory)) { return; }

            string[] files = Directory.GetFiles(directory);
            if (files.Length > 0)
            {
                Version latestVersion = new Version();
                foreach (string v in files)
                {
                    Version versionFromPackage = ParseVersionText(v);
                    if (versionFromPackage > latestVersion)
                    {
                        latestVersion = versionFromPackage;
                    }
                }
                version = latestVersion.ToString();
                downloaded = true;
            }
        }

        internal static void ClearDirectories(string[] directories)
        {
            string logPrefix = "[<color=#ABCDEF>WCA Package Manager</color>]:";

            foreach (string directory in directories)
            {
                try
                {
                    string fullPath = Path.GetFullPath(directory);

                    string metaPath = $"{fullPath}.meta";

                    if (Directory.Exists(fullPath))
                    {
                        Directory.Delete(fullPath, true);
                    }

                    if (File.Exists(metaPath))
                    {
                        File.Delete(metaPath);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogWarning($"{logPrefix} Can't clear directory ({directory})\nException: {e}");

                    continue;
                }

                Debug.Log($"{logPrefix} Cleared directory ({directory})");
            }

            AssetDatabase.Refresh();
        }

        internal static WCAData LoadWCAData()
        {
            WCAData wcaData = UnityEngine.Resources.Load<WCAData>("Data/WCAData");

            if (wcaData != null)
            {
                UpdateWCAData(wcaData);

                SaveAsset(wcaData);

                return wcaData;
            }

            return CreateWCAData();
        }

        private static void UpdateWCAData(WCAData wcaData)
        {
            DefaultData defaultData = UnityEngine.Resources.Load<DefaultData>("Data/DefaultData");

            SyncCommunityTools(defaultData, wcaData);

            SyncUPMPackages(defaultData, wcaData);

            SyncPrefabRepositories(defaultData, wcaData);
        }

        private static void SyncCommunityTools(DefaultData defaultData, WCAData wcaData)
        {
            if (defaultData.DefaultCommunityToolRepositories.Count == wcaData.CommunityTools.Count) { return; }

            wcaData.CommunityTools.Clear();

            foreach (DataStructs.RepositoryInfo repoInfo in defaultData.DefaultCommunityToolRepositories)
            {
                wcaData.CommunityTools.Add(new DataStructs.Repository(repoInfo.Name, repoInfo.Author, repoInfo.Description, repoInfo.RequiresUdonSharp, repoInfo.SDK3Only, directories: repoInfo.Directories));
            }
        }

        private static void SyncUPMPackages(DefaultData defaultData, WCAData wcaData)
        {
            if (defaultData.DefaultUPMPackages.Count == wcaData.UPMPackages.Count) { return; }

            wcaData.UPMPackages.Clear();

            foreach (DataStructs.UPMPackage package in defaultData.DefaultUPMPackages)
            {
                wcaData.UPMPackages.Add(new DataStructs.UPMPackage { Name = package.Name });
            }
        }

        private static void SyncPrefabRepositories(DefaultData defaultData, WCAData wcaData)
        {
            if (defaultData.DefaultPrefabRepositories.Count == wcaData.PrefabRepositories.Count) { return; }

            wcaData.PrefabRepositories.Clear();

            foreach (DataStructs.RepositoryInfo repoInfo in defaultData.DefaultPrefabRepositories)
            {
                wcaData.PrefabRepositories.Add(new DataStructs.Repository(repoInfo.Name, repoInfo.Author, repoInfo.Description, repoInfo.RequiresUdonSharp, repoInfo.SDK3Only, directories: repoInfo.Directories));
            }
        }

        private static WCAData CreateWCAData()
        {
            WCAData wcaData = ScriptableObject.CreateInstance<WCAData>();

            UpdateWCAData(wcaData);

            AssetDatabase.CreateAsset(wcaData, "Assets/Varneon/WorldCreatorAssistant/Resources/Data/WCAData.asset");

            SaveAsset(wcaData);

            return wcaData;
        }

        internal static string GetVRCSDKDownloadLink(DataStructs.SDKVariant variant)
        {
            switch (variant)
            {
                case DataStructs.SDKVariant.SDK3Worlds:
                    return "https://vrchat.com/download/sdk3-worlds";
                case DataStructs.SDKVariant.SDK3Avatars:
                    return "https://vrchat.com/download/sdk3-avatars";
                case DataStructs.SDKVariant.SDK2:
                    return "https://vrchat.com/download/sdk2";
                default:
                    return string.Empty;
            }
        }

        internal static string ParseFileSize(long fileLength)
        {
            string[] sizes = { "bytes", "KB", "MB", "GB" };
            int i = 0;

            while (fileLength > 1024 && i < sizes.Length)
            {
                fileLength /= 1024;

                i++;
            }
            return ($"{fileLength} {sizes[i]}");
        }
    }
}
