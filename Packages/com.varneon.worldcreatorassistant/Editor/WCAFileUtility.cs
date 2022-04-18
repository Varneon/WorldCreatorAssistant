using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace Varneon.WorldCreatorAssistant
{
    /// <summary>
    /// Utility class for checking the validity of WCA files and managing them to assist with troubleshooting
    /// </summary>
    internal static class WCAFileUtility
    {
        private static readonly FileDefinition[] ScriptFileDefinitions = new FileDefinition[]
        {
            new FileDefinition("DataStructs.cs", "a86e844f00685db4ba8f1df5e4577e7e"),
            new FileDefinition("DefaultData.cs", "efde2e7da5ba45a4bb5fd202188125b6"),
            new FileDefinition("Dictionary.cs", "215a6ed57c7bac7408e8bf6757420e36"),
            new FileDefinition("DictionaryLoader.cs", "b3f2c2cf712a02a4e93edb1044cf14a7"),
            new FileDefinition("EditorPreferenceKeys.cs", "1121866af2e203943b6e0d68952fd1fe"),
            new FileDefinition("GUIElements.cs", "999613815213d7048971c40be3612315"),
            new FileDefinition("GUIStyles.cs", "a8225749c7c27b649840c0df37b4f548"),
            new FileDefinition("Importer.cs", "6acd73af308a7dd448f2d1114f6007d4"),
            new FileDefinition("IncorrectUnityEditorVersionPrompt.cs", "53ad0a89ac899054cbcb0dfed3792181"),
            new FileDefinition("PackageManager.cs", "093db119226359b42be2884146bbefc9"),
            new FileDefinition("ProjectSetupWizard.cs", "ff95cb1dea4985e42a2b42e6de00090b"),
            new FileDefinition("ResourceData.cs", "ddbdbe91c50970f4e88a472a0de4ce42"),
            new FileDefinition("Resources.cs", "1f82aa8a1d9f99943931271996e99bb1"),
            new FileDefinition("UtilityMethods.cs", "b1234bd5fb068cc45b23c149a8d999c0"),
            new FileDefinition("WCAData.cs", "9aef4cf80468af34ca97ce003bca6c0b"),
            new FileDefinition("WindowOpener.cs", "2ee74ed6763ee63488a5805290243579"),
            new FileDefinition("WorldCreatorAssistant.cs", "e8a62ef5fd971ee4fb86e7dbf2306464"),
        };

        private static readonly FileDefinition[] DataAssetDefinitions = new FileDefinition[]
        {
            new FileDefinition("DefaultData.asset", "8db252d8640a1474ebcad18df45ad390", WCAFileType.Data),
            new FileDefinition("Dictionary.asset", "8ab9f056ae5147545866e08d0f5175e2", WCAFileType.Data),
            new FileDefinition("ResourceData.asset", "c4143893d59cb9143af3992ae05c4e9d", WCAFileType.Data)
        };

        private const string EditorFolderPath = "Assets/Varneon/WorldCreatorAssistant/Editor/";

        private const string DataFolderPath = "Assets/Varneon/WorldCreatorAssistant/Resources/Data/";

        private const string LogPrefix = "[<color=grey>WCA File Utility</color>]:";

        private struct FileDefinition
        {
            internal string Name;
            internal string GUID;
            internal WCAFileType FileType;

            internal FileDefinition(string name, string guid, WCAFileType fileType = WCAFileType.Script)
            {
                Name = name;
                GUID = guid;
                FileType = fileType;
            }
        }

        internal struct FileValidityReport
        {
            internal bool Verified;
            internal int InvalidGUIDCount;
            internal int InvalidDirectoryCount;
        }

        private enum WCAFileType
        {
            Script,
            Data
        }

        private enum FileStatus
        {
            Verified,
            InvalidGUID,
            InvalidDirectory
        }

        private static FileStatus CheckFileValidity(FileDefinition fileDefinition)
        {
            string filePath = AssetDatabase.GUIDToAssetPath(fileDefinition.GUID);

            if (string.IsNullOrEmpty(filePath)) 
            {
                Debug.LogWarning($"{LogPrefix} <color=red>{fileDefinition.Name}</color>'s has an invalid GUID!");
                
                return FileStatus.InvalidGUID; 
            }
            else if (!filePath.Equals(Path.Combine(fileDefinition.FileType == WCAFileType.Script ? EditorFolderPath : DataFolderPath, fileDefinition.Name)))
            {
                Debug.LogWarning($"{LogPrefix} <color=red>{fileDefinition.Name}</color> is located in an invalid directory!");

                return FileStatus.InvalidDirectory;
            }

            Debug.Log($"{LogPrefix} <color=green>{fileDefinition.Name}</color> Successfully verified!");

            return FileStatus.Verified;
        }

        internal static FileValidityReport CheckValidityOfWCAFiles()
        {
            FileValidityReport report = new FileValidityReport();

            List<FileDefinition> filesToCheck = new List<FileDefinition>();

            filesToCheck.AddRange(ScriptFileDefinitions);
            filesToCheck.AddRange(DataAssetDefinitions);

            foreach(FileDefinition fileDefinition in filesToCheck)
            {
                FileStatus status = CheckFileValidity(fileDefinition);

                switch (status)
                {
                    case FileStatus.InvalidDirectory:
                        report.InvalidDirectoryCount++;
                        break;
                    case FileStatus.InvalidGUID:
                        report.InvalidGUIDCount++;
                        break;
                }
            }

            if(report.Verified = report.InvalidDirectoryCount == 0 && report.InvalidGUIDCount == 0)
            {
                Debug.Log($"{LogPrefix} All files successfully verified!");
            }
            else
            {
                if(report.InvalidDirectoryCount > 0) { Debug.LogWarning($"{LogPrefix} {report.InvalidDirectoryCount} Files are in invalid directories! WCA might become corrupted after automatic update! Please import WCA into the standard directory in the future to ensure intended functionality."); }
            
                if(report.InvalidGUIDCount > 0) { Debug.LogWarning($"{LogPrefix} {report.InvalidGUIDCount} Files have invalid GUIDs, did you manually move WCA into the project without the .meta files? Invalid GUID prevents the file from being found and impossible to guarantee for WCA to remain functional after automatic update."); }
            }

            return report;
        }

        internal static void DeleteAllWCAFiles(string newWCAUnitypackagePath = null, EditorWindow windowToClose = null)
        {
            if(windowToClose != null) { windowToClose.Close(); }

            DeleteWCAData();

            List<FileDefinition> filesToDelete = new List<FileDefinition>();

            filesToDelete.AddRange(ScriptFileDefinitions);
            filesToDelete.AddRange(DataAssetDefinitions);

            AssetDatabase.StartAssetEditing();

            foreach (FileDefinition fileDefinition in filesToDelete)
            {
                string path = AssetDatabase.GUIDToAssetPath(fileDefinition.GUID);

                if (string.IsNullOrEmpty(path) || !path.EndsWith(fileDefinition.Name)) { Debug.LogError($"{LogPrefix} <color=red>{fileDefinition.Name}</color> couldn't be found!"); continue; }

                AssetDatabase.DeleteAsset(path);
            }

            try
            {
                AssetDatabase.StopAssetEditing();
            }
            catch(Exception e)
            {
                Debug.LogWarning($"{LogPrefix} {e}");
            }

            if (File.Exists(newWCAUnitypackagePath))
            {
                AssetDatabase.ImportPackage(newWCAUnitypackagePath, false);
            }
        }

        internal static void DeleteWCAData()
        {
            string[] wcaDataAssetPaths = AssetDatabase.FindAssets("t:Varneon.WorldCreatorAssistant.WCAData WCAData");

            foreach (string path in wcaDataAssetPaths)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(path));
            }
        }
    }
}
