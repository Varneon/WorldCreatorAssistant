#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    public static class GUIElements
    {
        public static bool Foldout(bool open, string foldoutText, Action drawContent, string contextButtonText = null, Action contextButtonAction = null)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(foldoutText, GUIStyles.BlockHeaderButton))
            {
                open ^= true;
            }
            if (open)
            {
                if (contextButtonAction is Action && GUILayout.Button(new GUIContent(contextButtonText), GUIStyles.FlatStandardButton, GUILayout.Width(120)))
                {
                    contextButtonAction.Invoke();
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical();
                drawContent.Invoke();
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            return open;
        }
        public static void DrawWarningBox(string text)
        {
            GUI.color = Color.yellow;
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(text, GUIStyles.WrappedText);
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        public static void DrawHintPanel(string text)
        {
            GUI.color = new Color(0.5f, 0.75f, 1f);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(text, GUIStyles.WrappedText);
            GUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        public static void LanguageSelection(Action loadActiveLanguage)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Language");
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            int selectedLanguage = EditorGUILayout.Popup(DictionaryLoader.ActiveLanguageIndex, DictionaryLoader.LanguageNames, GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
            {
                DictionaryLoader.ChangeLanguage(selectedLanguage);

                loadActiveLanguage();
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif
