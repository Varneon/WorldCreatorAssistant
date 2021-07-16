using UnityEditor;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    public class Resources
    {
        bool expandResources, expandQuestions;
        Dictionary.Translations dictionary;
        int selectedQuestion = -1, selectedResource = -1;
        public Texture iconWeb, iconCopy;
        readonly ResourceData resourceData;

        public void Draw()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            if (GUILayout.Button(dictionary.USEFUL_LINKS, GUIStyles.BlockHeaderButton))
            {
                expandResources ^= true;
            }
            if (expandResources)
            {
                for (int i = 0; i < resourceData.Resources.Count; i++)
                {

                    DrawResourceBlock(resourceData.Resources[i], i);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(EditorStyles.helpBox);
            if (GUILayout.Button(dictionary.FREQUENTLY_ASKED_QUESTIONS, GUIStyles.BlockHeaderButton))
            {
                expandQuestions ^= true;
            }
            if (expandQuestions)
            {
                for (int i = 0; i < resourceData.Questions.Count; i++)
                {
                    DrawQuestionBlock(resourceData.Questions[i], i);
                }
            }
            GUILayout.EndVertical();
        }

        public Resources()
        {
            LoadActiveDictionary();

            resourceData = UnityEngine.Resources.Load<ResourceData>("Data/ResourceData");
        }

        public void LoadActiveDictionary()
        {
            dictionary = DictionaryLoader.ActiveDictionary;
        }

        private void DrawResourceBlock(DataStructs.Resource resource, int index)
        {
            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(resource.Name, GUIStyles.BlockHeaderButton))
            {
                selectedResource = (selectedResource == index) ? -1 : index;
            }

            GUILayout.Label(resource.Type.ToString(), GUIStyles.ResourceTypeText, GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
            if (index == selectedResource)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + 30, rect.width, 1), (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : Color.grey));
                GUILayout.Label(resource.Description, GUIStyles.WrappedText);
                if (resource.URL.Length > 0)
                {
                    DrawBlockURL(resource.URL);
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawQuestionBlock(DataStructs.FAQTopic question, int index)
        {
            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button(question.Question, GUIStyles.BlockHeaderButton))
            {
                selectedQuestion = (selectedQuestion == index) ? -1 : index;
            }

            if(index == selectedQuestion)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + 30, rect.width, 1), (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : Color.grey));
                
                GUILayout.Label(question.Answer, GUIStyles.WrappedText);

                if (question.URL.Length > 0)
                {
                    DrawBlockURL(question.URL);
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawBlockURL(string url)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(url, GUIStyles.ButtonHyperlink))
            {
                Application.OpenURL(url);
            }
            else if (GUILayout.Button(new GUIContent(dictionary.COPY_LINK, iconCopy), GUIStyles.FlatStandardButton, new GUILayoutOption[] { GUILayout.MaxWidth(100), GUILayout.MaxHeight(20) }))
            {
                EditorGUIUtility.systemCopyBuffer = url;
            }
            GUILayout.EndHorizontal();
        }
    }
}
