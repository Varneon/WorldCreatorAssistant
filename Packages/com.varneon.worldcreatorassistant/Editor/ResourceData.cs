using System.Collections.Generic;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    public class ResourceData : ScriptableObject
    {
        public List<DataStructs.Resource> Resources = new List<DataStructs.Resource>();
        public List<DataStructs.FAQTopic> Questions = new List<DataStructs.FAQTopic>();
    }
}
