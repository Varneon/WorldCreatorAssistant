using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace Varneon.WorldCreatorAssistant
{
    public class WindowOpener
    {
        [MenuItem("Varneon/World Creator Assistant")]
        public static void OpenWindow()
        {
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
            WorldCreatorAssistant window = EditorWindow.GetWindow<WorldCreatorAssistant>();
            window.titleContent.image = UnityEngine.Resources.Load<SpriteAtlas>("Icons/Icons").GetSprite($"World_{(EditorGUIUtility.isProSkin ? "W" : "B")}").texture;
            window.titleContent.text = "WCA";
#else
            ProjectSetupWizard window = EditorWindow.GetWindow<ProjectSetupWizard>();
            window.titleContent.text = "Project Setup Wizard";
#endif
            window.minSize = new Vector2(512f, 512f);
            window.Show();
        }
    }
}
