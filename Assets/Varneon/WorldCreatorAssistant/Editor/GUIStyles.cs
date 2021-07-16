using UnityEditor;
using UnityEngine;

namespace Varneon.WorldCreatorAssistant
{
    public static class GUIStyles
    {
        public static GUIStyle BlockHeaderButton { get; }
        public static GUIStyle ButtonHyperlink { get; }
        public static GUIStyle CenteredBoldLabel { get; }
        public static GUIStyle CenteredHeaderLabel { get; }
        public static GUIStyle CenteredLabel { get; }
        public static GUIStyle FlatStandardButton { get; }
        public static GUIStyle HeaderPageSelection { get; }
        public static GUIStyle LeftGreyLabel { get; }
        public static GUIStyle NonPaddedButton { get; }
        public static GUIStyle ResourceTypeText { get; }
        public static GUIStyle UpdateLabel { get; }
        public static GUIStyle VersionLabel { get; }
        public static GUIStyle WrappedText { get; }

        private static Texture2D ColorTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);

            tex.SetPixel(0, 0, color);

            tex.Apply();

            return tex;
        }

        static GUIStyles()
        {
            BlockHeaderButton = new GUIStyle()
            {
                active = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.2f, 0.65f, 1f) : new Color(0f, 0f, 0.75f)), background = Texture2D.blackTexture },
                padding = new RectOffset(6, 6, 6, 6),
                fontStyle = FontStyle.Bold,
                normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
            };

            ButtonHyperlink = new GUIStyle()
            {
                padding = new RectOffset(6, 6, 6, 6),
                normal = { textColor = (EditorGUIUtility.isProSkin ? new Color(0.2f, 0.65f, 1f) : Color.blue) },
                wordWrap = true
            };

            CenteredBoldLabel = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
            };

            CenteredHeaderLabel = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(6, 6, 6, 6),
                fontSize = 16,
                normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
            };

            CenteredLabel = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
            };

            FlatStandardButton = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Overflow,
                margin = new RectOffset(4, 4, 3, 4),
                padding = new RectOffset(4, 4, 4, 4),
                active = { background = ColorTexture(new Color(0.2f, 0.65f, 1f)) },
                onActive = { background = ColorTexture(new Color(0.2f, 0.65f, 1f)) },
                normal = { background = (EditorGUIUtility.isProSkin ? ColorTexture(new Color(0.2f, 0.2f, 0.2f)) : ColorTexture(new Color(0.76f, 0.76f, 0.76f))) },
                onNormal = { background = (EditorGUIUtility.isProSkin ? ColorTexture(new Color(0.2f, 0.2f, 0.2f)) : ColorTexture(new Color(0.76f, 0.76f, 0.76f))) }
            };

            HeaderPageSelection = new GUIStyle(GUI.skin.box)
            {
                fixedHeight = 32,
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                active = { textColor = Color.white, background = Texture2D.blackTexture },
                onNormal = { textColor = new Color(0.2f, 0.65f, 1f), background = Texture2D.blackTexture },
                onActive = { textColor = new Color(0.2f, 0.65f, 1f), background = (EditorGUIUtility.isProSkin ? ColorTexture(new Color(0.25f, 0.25f, 0.25f)) : Texture2D.whiteTexture) },
                normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black), background = (EditorGUIUtility.isProSkin ? ColorTexture(new Color(0.25f, 0.25f, 0.25f)) : Texture2D.whiteTexture) }
            };

            LeftGreyLabel = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.gray }
            };

            NonPaddedButton = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(2, 2, 2, 2)
            };

            ResourceTypeText = new GUIStyle()
            {
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(6, 6, 6, 6),
                normal = { textColor = Color.grey }
            };

            UpdateLabel = new GUIStyle()
            {
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(6, 6, 6, 6),
                normal = { textColor = new Color(0f, 0.75f, 0f) }
            };

            VersionLabel = new GUIStyle()
            {
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(6, 6, 6, 6),
                normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
            };

            WrappedText = new GUIStyle()
            {
                padding = new RectOffset(6, 6, 6, 6),
                wordWrap = true,
                normal = { textColor = (EditorGUIUtility.isProSkin ? Color.white : Color.black) }
            };
        }
    }
}
