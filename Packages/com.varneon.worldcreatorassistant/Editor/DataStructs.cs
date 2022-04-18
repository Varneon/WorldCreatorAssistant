using System;
using System.Collections.Generic;

namespace Varneon.WorldCreatorAssistant
{
    public static class DataStructs
    {
        [Serializable]
        public struct AssetStorePackage
        {
            public string Name;
            public string Author;
            public string Path;
            public long DownloadDate;
            public long Size;
            public string PublishDate;
            public string Version;
            public string Category;
            public string ID;

            public AssetStorePackage(string author, string name, string path, long downloadDate = 0, long size = 0, string publishDate = "", string version = "", string category = "", string id = "")
            {
                Author = author;
                Name = name;
                Path = path;
                DownloadDate = downloadDate;
                Size = size;
                PublishDate = publishDate;
                Version = version;
                Category = category;
                ID = id;
            }
        }

        [Serializable]
        public struct UPMPackage
        {
            public string Name;
        }

        [Serializable]
        public struct Repository
        {
            public string Name;
            public string Author;
            public string Description;
            public bool Imported;
            public bool Downloaded;
            public string ImportedVersion;
            public string DownloadedVersion;
            public string CurrentVersion;
            public bool UpdateAvailable;
            public bool LatestCached;
            public bool VersionFileMissing;
            public long LastRefreshed;
            public bool RequiresUdonSharp;
            public bool SDK3Only;
            public List<string> Directories;

            public Repository(string name, string author, string description = "", bool requiresUdonSharp = false, bool sdk3Only = false, List<string> directories = null)
            {
                Name = name;
                Author = author;
                Description = description;
                RequiresUdonSharp = requiresUdonSharp;
                SDK3Only = sdk3Only;
                Imported = false;
                Downloaded = false;
                ImportedVersion = "0.0.0";
                DownloadedVersion = "0.0.0";
                CurrentVersion = "0.0.0";
                UpdateAvailable = false;
                LatestCached = false;
                VersionFileMissing = false;
                LastRefreshed = 0;
                Directories = directories;
            }

            public void UpdateRepositoryStatus(string packageCacheDirectory)
            {
                RepositoryStatus status = this.GetRepositoryStatus(packageCacheDirectory);

                Imported = status.Imported;
                Downloaded = status.Downloaded;
                UpdateAvailable = status.UpdateAvailable;
                LatestCached = status.LatestCached;
                ImportedVersion = status.ImportedVersion;
                DownloadedVersion = status.DownloadedVersion;
                VersionFileMissing = status.VersionFileMissing;
            }
        }

        [Serializable]
        public struct ImportResponse
        {
            public string Version;
            public bool Succeeded;
        }

        [Serializable]
        public struct RepositoryStatus
        {
            public bool Imported;
            public bool Downloaded;
            public bool UpdateAvailable;
            public bool LatestCached;
            public bool VersionFileMissing;
            public string ImportedVersion;
            public string DownloadedVersion;
            public string CurrentVersion;
        }

        [Serializable]
        public struct RepositoryInfo
        {
            public string Author;
            public string Name;
            public string Description;
            public bool SDK3Only;
            public bool RequiresUdonSharp;
            public List<string> Directories;
        }

        #region Resources
        [Serializable]
        public struct ResourceList
        {
            public Resource[] Resources;

            public ResourceList(Resource[] resources)
            {
                Resources = resources;
            }
        }

        [Serializable]
        public struct Resource
        {
            public string Name;
            public string URL;
            public string Description;
            public ResourceType Type;

            public Resource(string name, string url, string description, ResourceType type)
            {
                Name = name;
                URL = url;
                Description = description;
                Type = type;
            }
        }

        [Serializable]
        public enum ResourceType
        {
            Software,
            Website,
            Tutorial,
            Asset,
            Community,
            Inspiration,
            Information
        }

        [Serializable]
        public struct FAQTopic
        {
            public string Question;
            public List<FAQAnswer> Answers;

            public FAQTopic(string question, List<FAQAnswer> answers)
            {
                Question = question;
                Answers = answers;
            }
        }

        [Serializable]
        public struct FAQAnswer
        {
            public string Description;
            public string URL;

            public FAQAnswer(string description, string url = "")
            {
                Description = description;
                URL = url;
            }
        }
        #endregion

        #region VRCSDK
        [Serializable]
        public enum SDKVariant
        {
            None,
            SDK3Worlds,
            SDK3Avatars,
            SDK2
        }

        [Serializable]
        public struct VRCSDKVersions
        {
            public string SDK2;
            public string SDK3Worlds;
            public string SDK3Avatars;
        }

        [Serializable]
        public struct VRCApiConfig
        {
            public ConfigDownloadUrls downloadUrls;
        }

        [Serializable]
        public struct ConfigDownloadUrls
        {
            public string sdk2;
            public string sdk3_worlds;
            public string sdk3_avatars;
        }
        #endregion

        #region GitHub API
        [Serializable]
        public struct GitHubApiRelease
        {
            public string html_url;
            public string tag_name;
            public string name;
            public Assets[] assets; 
        }

        [Serializable]
        public struct Assets
        {
            public string browser_download_url;
        }

        [Serializable]
        public struct GitHubApiStatus
        {
            public int RequestsRemaining;
            public int RequestLimit;
            public DateTime ResetDateTime;
        }

        [Serializable]
        public struct GitHubApiRateLimit
        {
            public RateLimitResources resources;
        }

        [Serializable]
        public struct RateLimitResources
        {
            public RateLimitCore core;
        }

        [Serializable]
        public struct RateLimitCore
        {
            public int limit;
            public int remaining;
            public int reset;
            public int used;
        }
        #endregion

        [Serializable]
        public enum UpdateCheckStatus
        {
            Unchecked,
            VersionFileMissing,
            OutOfRequests,
            CouldNotFetchRelease,
            UpToDate,
            UpdateAvailable
        }
    }
}
