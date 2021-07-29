using System.Collections.Generic;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    public class DefaultData : ScriptableObject
    {
        public List<DataStructs.RepositoryInfo> DefaultCommunityToolRepositories = new List<DataStructs.RepositoryInfo>();
        public List<DataStructs.UPMPackage> DefaultUPMPackages = new List<DataStructs.UPMPackage>();
        public List<DataStructs.RepositoryInfo> DefaultPrefabRepositories = new List<DataStructs.RepositoryInfo>();
    }
}
