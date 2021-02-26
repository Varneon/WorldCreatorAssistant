#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

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
                this.Author = author;
                this.Name = name;
                this.Path = path;
                this.DownloadDate = downloadDate;
                this.Size = size;
                this.PublishDate = publishDate;
                this.Version = version;
                this.Category = category;
                this.ID = id;
            }
        }

        [Serializable]
        public struct RepositoryList
        {
            public Repository[] Repositories;

            public RepositoryList(Repository[] repositories)
            {
                this.Repositories = repositories;
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
            public bool UpdateDownloaded;
            public long LastRefreshed;
            public bool RequiresUdonSharp;
            public bool SDK3Only;

            public Repository(string name, string author, string description = "", bool requiresUdonSharp = false, bool sdk3Only = false)
            {
                this.Name = name;
                this.Author = author;
                this.Description = description;
                this.RequiresUdonSharp = requiresUdonSharp;
                this.SDK3Only = sdk3Only;
                this.Imported = false;
                this.Downloaded = false;
                this.ImportedVersion = "0.0.0";
                this.DownloadedVersion = "0.0.0";
                this.CurrentVersion = "0.0.0";
                this.UpdateAvailable = false;
                this.UpdateDownloaded = false;
                this.LastRefreshed = 0;
            }
        }

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
                this.Name = name;
                this.URL = url;
                this.Description = description;
                this.Type = type;
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
            public string Answer;
            public string URL;

            public FAQTopic(string question, string answer, string url = "")
            {
                Question = question;
                Answer = answer;
                URL = url;
            }
        }

        [Serializable]
        public enum Skill
        {
            Beginner,
            Intermediate,
            Expert
        }

        [Serializable]
        public struct Skills
        {
            public Skill Unity;
            public Skill Blender;
            public Skill CSharp;
            public Skill SubstancePainter;
        }

        [Serializable]
        public struct Lesson
        {

        }

        [Serializable]
        public struct Task
        {
            bool Skippable;
        }

        [Serializable]
        public struct Icon
        {
            public IconType IconType;
            public Texture Texture;

            public Icon(IconType iconType, Texture texture)
            {
                IconType = iconType;
                Texture = texture;
            }
        }

        [Serializable]
        public enum IconType
        {
            Checkmark,
            Copy,
            Download,
            GitHub,
            Import,
            Notification,
            Web
        }

        [Serializable]
        public enum SDKVariant
        {
            None,
            SDK3Worlds,
            SDK3Avatars,
            SDK2
        }

        #region VRCSDK
        public static string SDKDownloadLink(SDKVariant variant)
        {
            switch (variant)
            {
                case SDKVariant.SDK3Worlds:
                    return "https://vrchat.com/download/sdk3-worlds";
                case SDKVariant.SDK3Avatars:
                    return "https://vrchat.com/download/sdk3-avatars";
                case SDKVariant.SDK2:
                    return "https://vrchat.com/download/sdk2";
            }
            return string.Empty;
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

        #region GitHub
        [Serializable]
        public struct GitHubApiRelease
        {
            public string html_url;
            public string tag_name;
            public string name;
            public assets[] assets; 
        }

        [Serializable]
        public struct assets
        {
            public string browser_download_url;
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

        #region GUIStyles
        public static readonly GUIStyle ButtonHyperlink = new GUIStyle()
        {
            padding = new RectOffset(6, 6, 6, 6),
            normal = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.2f, 0.65f, 1f) : Color.blue) },
            wordWrap = true
        };

        public static readonly GUIStyle VersionLabel = new GUIStyle()
        {
            alignment = TextAnchor.MiddleRight,
            padding = new RectOffset(6, 6, 6, 6),
            normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
        };

        public static readonly GUIStyle UpdateLabel = new GUIStyle()
        {
            alignment = TextAnchor.MiddleRight,
            padding = new RectOffset(6, 6, 6, 6),
            normal = { textColor = new Color(0f, 0.75f, 0f) }
        };

        public static readonly GUIStyle WrappedText = new GUIStyle()
        {
            padding = new RectOffset(6, 6, 6, 6),
            wordWrap = true,
            normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
        };

        public static readonly GUIStyle ResourceTypeText = new GUIStyle()
        {
            alignment = TextAnchor.MiddleRight,
            padding = new RectOffset(6, 6, 6, 6),
            normal = { textColor = Color.grey }
        };

        public static readonly GUIStyle BlockHeaderButton = new GUIStyle()
        {
            active = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.2f, 0.65f, 1f) : new Color(0f, 0f, 0.75f)), background = Texture2D.blackTexture },
            padding = new RectOffset(6, 6, 6, 6),
            fontStyle = FontStyle.Bold,
            normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
        };

        public static readonly GUIStyle FlatStandardButton = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Overflow,
            margin = new RectOffset(4, 4, 3, 4),
            padding = new RectOffset(4, 4, 4, 4),
            active = { background = ColorTexture(new Color(0.2f, 0.65f, 1f)) },
            onActive = { background = ColorTexture(new Color(0.2f, 0.65f, 1f)) },
            normal = { background = (EditorGUIUtility.isProSkin ? ColorTexture(new Color(0.2f, 0.2f, 0.2f)) : ColorTexture(new Color(0.76f, 0.76f, 0.76f))) },
            onNormal = { background = (EditorGUIUtility.isProSkin ? ColorTexture(new Color(0.2f, 0.2f, 0.2f)) : ColorTexture(new Color(0.76f, 0.76f, 0.76f))) }
        };

        public static readonly GUIStyle CenteredHeaderLabel = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(6, 6, 6, 6),
            fontSize = 16,
            normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
        };

        public static readonly GUIStyle CenteredLabel = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
        };

        public static readonly GUIStyle CenteredBoldLabel = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
        };

        public static readonly GUIStyle LeftGreyLabel = new GUIStyle()
        {
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.gray }
        };

        public static readonly GUIStyle NonPaddedButton = new GUIStyle(GUI.skin.button)
        {
            margin = new RectOffset(2, 2, 2, 2)
        };

        public static readonly GUIStyle HeaderPageSelection = new GUIStyle(GUI.skin.GetStyle("toolbarButton"))
        {
            fixedHeight = 32,
            fontSize = 12,
            active = { textColor = Color.white, background = Texture2D.blackTexture },
            onNormal = { textColor = new Color(0.2f, 0.65f, 1f), background = Texture2D.blackTexture },
            onActive = { textColor = new Color(0.2f, 0.65f, 1f), background = (EditorGUIUtility.isProSkin ? ColorTexture(new Color(0.25f, 0.25f, 0.25f)) : Texture2D.whiteTexture) },
            normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black), background = (EditorGUIUtility.isProSkin ? ColorTexture(new Color(0.25f, 0.25f, 0.25f)) : Texture2D.whiteTexture) }
        };
        #endregion

        public static Texture2D ColorTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public static string ParseFileSize(long fileLength)
        {
            string[] sizes = { "bytes", "KB", "MB", "GB" };
            int i = 0;

            while (fileLength > 1024 && i < sizes.Length)
            {
                fileLength = fileLength / 1024;
                i++;
            }
            return ($"{fileLength} {sizes[i]}");
        }
    }
}
#endif