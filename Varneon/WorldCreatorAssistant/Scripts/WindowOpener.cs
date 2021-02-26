#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;

namespace Varneon.WorldCreatorAssistant
{
    public class WindowOpener : EditorWindow
    {
        [MenuItem("Varneon/World Creator Assistant")]
        public static void Init()
        {
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
            EditorWindow window = CreateInstance<WorldCreatorAssistant>();
            window.titleContent.image = AssetDatabase.LoadAssetAtPath<SpriteAtlas>("Assets/Varneon/WorldCreatorAssistant/Resources/Icons/Icons.spriteatlas").GetSprite($"World_{(EditorGUIUtility.isProSkin ? "W" : "B")}").texture;
            window.titleContent.text = "WCA";
#else
            EditorWindow window = CreateInstance<ProjectSetupWizard>();
            window.titleContent.text = "Project Setup Wizard";
#endif
            window.minSize = new Vector2(512f, 512f);
            window.Show();
        }
    }
}
#endif