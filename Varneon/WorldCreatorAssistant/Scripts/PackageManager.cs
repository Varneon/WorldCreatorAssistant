#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using System;

namespace Varneon.WorldCreatorAssistant
{
    public class PackageManager
    {
        #region Variables
        public float Progress = 0f;
        string tempPath = Path.GetTempPath();
        DataStructs.GitHubApiRelease latestRelease;
        DataStructs.GitHubApiRateLimit rateLimit;
        static AddRequest Request;
        #endregion

        #region Static

        private static readonly string logPrefix = "<color=#009999>[WCA Package Manager]</color>:";

        private static readonly string[] vrcsdkFolderPaths = new string[]
        {
            "Assets/VRCSDK",
            "Assets/Udon",
            "Assets/VRChat Examples"
        };

        #endregion

        #region GitHub
        private static string gitHubApi(string author, string name)
        {
            return $"https://api.github.com/repos/{author}/{name}/releases/latest";
        }

        public DataStructs.GitHubApiRateLimit GetGitHubApiRateLimit()
        {
            var reader = GitHubAPIRateLimit("https://api.github.com/rate_limit");
            while (reader.MoveNext()) { }
            return rateLimit;
        }

        IEnumerator GitHubAPIRateLimit(string url)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.SendWebRequest();

                while (!request.isDone)
                {
                    Progress = request.downloadProgress;
                    yield return null;
                }

                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    rateLimit = JsonUtility.FromJson<DataStructs.GitHubApiRateLimit>(request.downloadHandler.text);
                }
            }
        }

        public string DownloadRepositoryLatest(string cacheDir, string author, string name)
        {
            string apiUrl = gitHubApi(author, name);
            Debug.Log($"{logPrefix} Checking the latest version: {apiUrl}");
            string url = getLatestReleaseDownloadURL(apiUrl);
            string path = $"{cacheDir}/{name}{url.Substring(url.LastIndexOf('/'))}";
            Debug.Log($"{logPrefix} Starting download: {url}");
            var downloader = GitHubDownloader(url, path);
            while (downloader.MoveNext()) { }
            ImportPackage(path);
            return Regex.Replace(url.Substring(url.LastIndexOf('/')), "[^0-9.]", "").TrimStart('.').TrimEnd('.');
            /*
            string version = Regex.Replace(url.Substring(url.LastIndexOf('/')), "[^0-9.]", "").TrimStart('.').TrimEnd('.');
            string versionPath = $"Assets/{name}/version.txt";
            createDirectoryIfNotFound(versionPath);
            if (!File.Exists(versionPath))
            {
                StreamWriter writer = new StreamWriter(versionPath);
                writer.Write(version);
                writer.Close();
            }
            return version;
            //return string.Empty;
            */
        }

        public void DownloadRepository(string cacheDir, string url, string name, System.Version version = null)
        {
            string path = $"{cacheDir}/{name}/{name}_{version}.unitypackage";
            //packagePath = $"{cacheDir}/{name}/{name}_{version}.unitypackage";
            Debug.Log($"URL: {url} | Path: {path}");
            //var downloader = GitHubDownloader(url, path);
            //while (downloader.MoveNext()) { }
            //import(path);
        }

        public System.Version GetLatestReleaseVersion(string author, string name)
        {
            Debug.Log($"{logPrefix} Checking the latest version of {name}...");
            var reader = GitHubLatestRelease(gitHubApi(author, name));
            while (reader.MoveNext()) { }
            if(!latestRelease.Equals(null))
            {
                return new System.Version(latestRelease.tag_name.ToLower().Trim('v'));
            }
            else
            {
                return new System.Version();
            }
        }

        private string getLatestReleaseDownloadURL(string url)
        {
            var reader = GitHubLatestRelease(url);
            while (reader.MoveNext()) { }
            if (!latestRelease.Equals(null))
            {
                return latestRelease.assets[0].browser_download_url;
            }
            else
            {
                return string.Empty;
            }
        }

        IEnumerator GitHubLatestRelease(string url)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.SendWebRequest();

                while (!request.isDone)
                {
                    Progress = request.downloadProgress;
                    yield return null;
                }

                if(request.isHttpError || request.isNetworkError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    latestRelease = JsonUtility.FromJson<DataStructs.GitHubApiRelease>(request.downloadHandler.text);
                }
            }
        }

        IEnumerator GitHubDownloader(string url, string path)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.SendWebRequest();

                while (!request.isDone)
                {
                    Progress = request.downloadProgress;
                    yield return null;
                }

                if(request.isHttpError || request.isNetworkError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    createDirectoryIfNotFound(path);
                    File.WriteAllBytes(path, request.downloadHandler.data);
                }
            }
        }
        #endregion

        #region VRCSDK
        public void DownloadSDK(DataStructs.SDKVariant variant, bool cleanInstall = false)
        {
            if (cleanInstall)
            {
                Debug.Log($"{logPrefix} Cleaning VRCSDK install...");
                foreach (string s in vrcsdkFolderPaths)
                {
                    string fullPath = Path.GetFullPath(s);
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
            }

            string path = $"{tempPath}/{variant}.unitypackage";
            string url = DataStructs.SDKDownloadLink(variant);
            Debug.Log($"{logPrefix} Downloading VRC{variant}...");
            var downloader = SDKDownloader(url, path);
            while (downloader.MoveNext()) { }
            ImportPackage(path);
        }

        IEnumerator SDKDownloader(string url, string path)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.SendWebRequest();

                while (!request.isDone)
                {
                    Progress = request.downloadProgress;
                    yield return null;
                }

                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    File.WriteAllBytes(path,
                        request.downloadHandler.data);
                }
            }
        }
        #endregion

        #region Asset Store
        public DataStructs.AssetStorePackage[] GetDownloadedUASPackages()
        {
            List<DataStructs.AssetStorePackage> downloadedPackages = new List<DataStructs.AssetStorePackage>();
            string path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Unity\Asset Store-5.x";
            string[] publishers = Directory.GetDirectories(path);
            string[] packages = Directory.GetFiles(path, "*.unitypackage", SearchOption.AllDirectories);
            foreach (string s in packages)
            {
                downloadedPackages.Add(new DataStructs.AssetStorePackage(
                    Path.GetFileNameWithoutExtension(Path.GetFullPath(Path.Combine(s, @"..\..\").TrimEnd('\\'))),
                    Path.GetFileNameWithoutExtension(s),
                    s,
                    size: new FileInfo(s).Length
                    ));
            }
            return downloadedPackages.ToArray();
        }
        #endregion

        #region Package Manager
        public void AddUPMPackage(string package)
        {
            Request = Client.Add(package);
            EditorApplication.update += UPMProgress;
        }

        static void UPMProgress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + Request.Result.packageId);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= UPMProgress;
            }
        }

        public DataStructs.UPMPackage[] GetUPMPackages()
        {
            List<DataStructs.UPMPackage> upmPackages = new List<DataStructs.UPMPackage>();
            return new DataStructs.UPMPackage[0];
        }
        #endregion

        public void ImportPackage(string path)
        {
            if (File.Exists(path))
            {
                Debug.Log($"{logPrefix} Importing package: {path}");
                AssetDatabase.ImportPackage(path, false);
            }
            else
            {
                Debug.Log($"{logPrefix} Could not find Unitypackage to import at {path}");
            }
        }

        private void createDirectoryIfNotFound(string path)
        {
            string dir = path.Substring(0, path.LastIndexOf('/'));
            if (!Directory.Exists(dir))
            {
                Debug.Log($"{logPrefix} Directory ({dir}) doesn't exist, creating it...");
                Directory.CreateDirectory(dir);
            }
        }
    }
}
#endif