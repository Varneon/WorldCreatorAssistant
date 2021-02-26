#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Varneon.WorldCreatorAssistant
{
    public class Settings
    {
        string packageCacheDirectory;
        DataStructs.Skills skills;

        public void Draw()
        {
            packageCacheDirectory = EditorGUILayout.TextField(packageCacheDirectory);
        }
    }
}
#endif