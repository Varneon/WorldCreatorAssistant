using System.Collections.Generic;
using UnityEditor;

namespace Varneon.WorldCreatorAssistant
{
    public static class DictionaryLoader
    {
        public static Dictionary.Translations ActiveDictionary { get; internal set; }

        public static string[] LanguageNames { get; }

        public static int ActiveLanguageIndex { get; internal set; }

        private static readonly Dictionary dictionary;

        static DictionaryLoader()
        {
            dictionary = UnityEngine.Resources.Load<Dictionary>("Data/Dictionary");

            LanguageNames = GetAvailableLanguages();

            if (EditorPrefs.HasKey(EditorPreferenceKeys.Language))
            {
                ActiveLanguageIndex = EditorPrefs.GetInt(EditorPreferenceKeys.Language);
            }

            LoadActiveDictionary();
        }

        private static string[] GetAvailableLanguages()
        {
            List<string> languageList = new List<string>();

            foreach (Dictionary.Localization loc in UnityEngine.Resources.Load<Dictionary>("Data/Dictionary").Languages)
            {
                languageList.Add(loc.Language);
            }

            return languageList.ToArray();
        }

        private static void LoadActiveDictionary()
        {
            int languageIndex = 0;

            if (EditorPrefs.HasKey(EditorPreferenceKeys.Language))
            {
                languageIndex = EditorPrefs.GetInt(EditorPreferenceKeys.Language);
            }

            int languageCount = dictionary.Languages.Count;

            if (languageIndex >= languageCount) { languageIndex = 0; }

            ActiveDictionary = dictionary.Languages[languageIndex].Translations;
        }

        public static void ChangeLanguage(int index)
        {
            EditorPrefs.SetInt(EditorPreferenceKeys.Language, index);

            ActiveLanguageIndex = index;

            LoadActiveDictionary();
        }
    }
}
