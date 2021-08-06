using UnityEditor;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    public class WindowOpener
    {
        [MenuItem("Varneon/World Creator Assistant")]
        public static void OpenWindow()
        {
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
            WorldCreatorAssistant window = EditorWindow.GetWindow<WorldCreatorAssistant>();
            window.titleContent.image = UnityEngine.Resources.Load<Texture>($"Icons/World_{(EditorGUIUtility.isProSkin ? "W" : "B")}");
            window.titleContent.text = "WCA";
#else
            ProjectSetupWizard window = EditorWindow.GetWindow<ProjectSetupWizard>();
            window.titleContent.text = "Project Setup Wizard";
#endif
            window.minSize = new Vector2(512f, 512f);
            window.Show();

#if !UNITY_2019_4
            IncorrectUnityEditorVersionPrompt prompt = EditorWindow.GetWindow<IncorrectUnityEditorVersionPrompt>();
            prompt.titleContent.text = "Wrong Editor Version!";
            prompt.minSize = new Vector2(512f, 256f);
            prompt.Show();
#endif
        }
    }
}
