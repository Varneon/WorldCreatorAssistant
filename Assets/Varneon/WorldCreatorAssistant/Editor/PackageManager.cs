using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace Varneon.WorldCreatorAssistant
{
    internal class PackageManager
    {
        private static PackageManager instance = null;
        internal static PackageManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new PackageManager();
                }
                return instance;
            }
        }

        private const string LogPrefix = "[<color=#009999>WCA Package Manager</color>]:";

        private static readonly string[] VRCSDKFolderPaths =
        {
            "Assets/VRCSDK",
            "Assets/Udon",
            "Assets/VRChat Examples"
        };

        DataStructs.GitHubApiRateLimit rateLimit;
        DataStructs.GitHubApiRelease latestRelease;
        internal float Progress = 0f;
        private readonly string tempPath = Path.GetTempPath();
        private string vrcApiResponse;

        #region GitHub

        internal DataStructs.GitHubApiStatus GetGitHubApiRateLimit()
        {
            rateLimit = new DataStructs.GitHubApiRateLimit();
            var reader = GitHubAPIRateLimit("https://api.github.com/rate_limit");
            while (reader.MoveNext()) { }
            return new DataStructs.GitHubApiStatus() 
            { 
                RequestsRemaining = rateLimit.resources.core.remaining,
                RequestLimit = rateLimit.resources.core.limit,
                ResetDateTime = UtilityMethods.GetDateTimeFromUnix(rateLimit.resources.core.reset)
            };
        }

        IEnumerator GitHubAPIRateLimit(string url)
        {
            var request = UnityWebRequest.Get(url);

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

        internal string DownloadLatestRepository(string cacheDir, string author, string name)
        {
            string apiUrl = UtilityMethods.GetGitHubApiLatestReleaseURL(author, name);

            Debug.Log($"{LogPrefix} Checking the latest version: {apiUrl}");

            string url = GetLatestReleaseDownloadURL(apiUrl);

            string path = $"{cacheDir}/{name}{url.Substring(url.LastIndexOf('/'))}";

            Debug.Log($"{LogPrefix} Starting download: {url}");

            var downloader = GitHubDownloader(url, path);

            while (downloader.MoveNext()) { }

            return path;
        }

        internal DataStructs.ImportResponse DownloadAndImportLatestRepository(string cacheDir, string author, string name)
        {
            return ImportPackage(DownloadLatestRepository(cacheDir, author, name), returnVersion: true);
        }

        internal System.Version GetLatestReleaseVersion(string author, string name)
        {
            Debug.Log($"{LogPrefix} Checking the latest version of {name}...");

            var reader = GitHubLatestRelease(UtilityMethods.GetGitHubApiLatestReleaseURL(author, name));

            while (reader.MoveNext()) { }

            if(!latestRelease.Equals(null))
            {
                return new System.Version(latestRelease.tag_name.ToLower().Trim('v'));
            }

            return new System.Version();
        }

        private string GetLatestReleaseDownloadURL(string url)
        {
            var reader = GitHubLatestRelease(url);

            while (reader.MoveNext()) { }

            if (!latestRelease.Equals(null))
            {
                return latestRelease.assets[0].browser_download_url;
            }

            return string.Empty;
        }

        IEnumerator GitHubLatestRelease(string url)
        {
            var request = UnityWebRequest.Get(url);

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

        IEnumerator GitHubDownloader(string url, string path)
        {
            var request = UnityWebRequest.Get(url);

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
                CreateDirectoryIfNotFound(path);
                File.WriteAllBytes(path, request.downloadHandler.data);
            }
        }
        #endregion

        #region VRCSDK
        internal void DownloadSDK(DataStructs.SDKVariant variant, bool cleanInstall = false)
        {
            if (cleanInstall)
            {
                Debug.Log($"{LogPrefix} Cleaning VRCSDK install...");

                foreach (string s in VRCSDKFolderPaths)
                {
                    string fullPath = Path.GetFullPath(s);

                    string metaPath = $"{fullPath}.meta";

                    if (Directory.Exists(fullPath))
                    {
                        Directory.Delete(Path.GetFullPath(s), true);
                    }

                    if (File.Exists(metaPath))
                    {
                        File.Delete(metaPath);
                    }
                }
            }

            string path = $"{tempPath}/{variant}.unitypackage";

            string url = UtilityMethods.GetVRCSDKDownloadLink(variant);

            Debug.Log($"{LogPrefix} Downloading VRC{variant}...");

            var downloader = SDKDownloader(url, path);

            while (downloader.MoveNext()) { }

            ImportPackage(path);
        }

        IEnumerator SDKDownloader(string url, string path)
        {
            var request = UnityWebRequest.Get(url);

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

        internal string GetVRCSDKConfig()
        {
            vrcApiResponse = null;

            var reader = VRCAPIConfigReader();

            while (reader.MoveNext()) { }

            return vrcApiResponse;
        }

        IEnumerator VRCAPIConfigReader()
        {
            var request = UnityWebRequest.Get("https://vrchat.com/api/1/config");

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
                vrcApiResponse = request.downloadHandler.text;
            }
        }
        #endregion

        #region Asset Store
        internal DataStructs.AssetStorePackage[] GetDownloadedUASPackages()
        {
            List<DataStructs.AssetStorePackage> downloadedPackages = new List<DataStructs.AssetStorePackage>();
            string path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Unity\Asset Store-5.x";
            if (!Directory.Exists(path)) { return new DataStructs.AssetStorePackage[0]; }
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

        #region Unity Package Manager
        internal void AddUPMPackage(string package)
        {
            Debug.Log($"{LogPrefix} Importing UPM Package: {package}");

            AddRequest Request = Client.Add(package);

            while (!Request.IsCompleted) { };

            if(Request.Error != null) { Debug.LogError(Request.Error.message); return; }
        }
        #endregion

        internal DataStructs.ImportResponse ImportPackage(string path, bool returnVersion = false)
        {
            DataStructs.ImportResponse response = new DataStructs.ImportResponse();

            if (File.Exists(path))
            {
                Debug.Log($"{LogPrefix} Importing package: {path}");

                AssetDatabase.ImportPackage(path, false);

                if (!returnVersion) { return response; }

                response.Version = Regex.Replace(path.Substring(path.LastIndexOf('/')), "[^0-9.]", "").TrimStart('.').TrimEnd('.');

                response.Succeeded = true;

                return response;
            }
            else
            {
                Debug.LogError($"{LogPrefix} Could not find Unitypackage to import at {path}");

                return response;
            }
        }

        private void CreateDirectoryIfNotFound(string path)
        {
            string dir = path.Substring(0, path.LastIndexOf('/'));

            if (!Directory.Exists(dir))
            {
                Debug.Log($"{LogPrefix} Directory ({dir}) doesn't exist, creating it...");

                Directory.CreateDirectory(dir);
            }
        }
    }
}
