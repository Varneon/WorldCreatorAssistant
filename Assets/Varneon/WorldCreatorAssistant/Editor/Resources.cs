using UnityEditor;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    internal class Resources
    {
        bool expandResources, expandQuestions;
        Dictionary.Translations dictionary;
        int selectedQuestion = -1, selectedResource = -1;
        internal Texture iconWeb, iconCopy;
        readonly ResourceData resourceData;

        internal void Draw()
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

        internal Resources()
        {
            LoadActiveDictionary();

            resourceData = UnityEngine.Resources.Load<ResourceData>("Data/ResourceData");
        }

        internal void LoadActiveDictionary()
        {
            dictionary = DictionaryLoader.ActiveDictionary;
        }

        private void DrawResourceBlock(DataStructs.Resource resource, int index)
        {
            bool isThisResourceSelected = index == selectedResource;

            GUI.color = isThisResourceSelected ? Color.grey : Color.white;
            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.color = Color.white;

            GUILayout.BeginHorizontal();

            GUI.color = isThisResourceSelected ? new Color(0.2f, 0.65f, 1f) : Color.white;
            if (GUILayout.Button(resource.Name, GUIStyles.BlockHeaderButton))
            {
                selectedResource = (selectedResource == index) ? -1 : index;
            }
            GUI.color = Color.white;

            GUILayout.Label(resource.Type.ToString(), GUIStyles.ResourceTypeText, GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();

            if (isThisResourceSelected)
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
            bool isThisQuestionSelected = index == selectedQuestion;

            GUI.color = isThisQuestionSelected ? Color.grey : Color.white;
            Rect rect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.color = Color.white;

            GUI.color = isThisQuestionSelected ? new Color(0.2f, 0.65f, 1f) : Color.white;
            if (GUILayout.Button(question.Question, GUIStyles.BlockHeaderButton))
            {
                selectedQuestion = (selectedQuestion == index) ? -1 : index;
            }
            GUI.color = Color.white;

            if (isThisQuestionSelected)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + 30, rect.width, 1), (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : Color.grey));

                for (int i = 0; i < question.Answers.Count; i++)
                {
                    if (i > 0) { GUILayout.Space(20); }

                    DataStructs.FAQAnswer answer = question.Answers[i];

                    GUILayout.Label(answer.Description, GUIStyles.WrappedText);

                    if (!string.IsNullOrEmpty(answer.URL))
                    {
                        DrawBlockURL(answer.URL);
                    }
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
