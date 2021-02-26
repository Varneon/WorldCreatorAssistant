#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Varneon.WorldCreatorAssistant
{
    public class Resources
    {
        //List<DataStructs.Resource> resources;
        //List<DataStructs.FAQTopic> questions;
        public Texture iconWeb, iconCopy;
        int selectedQuestion = -1, selectedResource = -1;
        bool expandResources, expandQuestions;
        WCAData wcaData;

        public void Draw()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            if (GUILayout.Button("Useful Links", DataStructs.BlockHeaderButton))
            {
                expandResources ^= true;
            }
            if (expandResources)
            {
                for (int i = 0; i < wcaData.Resources.Count; i++)
                {

                    drawResourceBlock(wcaData.Resources[i], i);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            if (GUILayout.Button("Frequently Asked Questions", DataStructs.BlockHeaderButton))
            {
                expandQuestions ^= true;
            }
            if (expandQuestions)
            {
                for (int i = 0; i < wcaData.Questions.Count; i++)
                {
                    drawQuestionBlock(wcaData.Questions[i], i);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        public Resources()
        {
            #region Content Generation
            /*
            resources = new List<DataStructs.Resource>()
            {
                new DataStructs.Resource("VRChat Documentation", "https://docs.vrchat.com/", "Official documentation for VRChat", DataStructs.ResourceType.Information),
                new DataStructs.Resource("VRChat Canny", "https://vrchat.canny.io/", "Official way to give feedback and report bugs regarding VRChat", DataStructs.ResourceType.Website),
                new DataStructs.Resource("VRChat Discord Server", "https://discord.gg/vrchat", "Official VRChat Discord", DataStructs.ResourceType.Community),
                new DataStructs.Resource("VRChat Forum", "https://ask.vrchat.com/", "Official VRChat Forum", DataStructs.ResourceType.Community),
                new DataStructs.Resource("Unity Documentation", "https://docs.unity3d.com/2018.4/Documentation/Manual/index.html", "Official Unity documentation", DataStructs.ResourceType.Information),
                new DataStructs.Resource("VRCPrefabs Database", "https://vrcprefabs.com/browse", "Spreadsheet of prefabs created by the members of VRCPrefabs", DataStructs.ResourceType.Asset),
                new DataStructs.Resource("VRCPrefabs TLX", "https://tlx.dev/talks", "Creator conference related to VRChat world creation where you can find many useful talks", DataStructs.ResourceType.Information),
                new DataStructs.Resource("Blender", "https://store.steampowered.com/app/365670/Blender/", "Free and open source 3D creation suite", DataStructs.ResourceType.Software),
                new DataStructs.Resource("Substance", "https://www.substance3d.com/substance/", "Inspiring content and intelligent tools to create and apply materials for 3D", DataStructs.ResourceType.Software),
                new DataStructs.Resource("PureRef", "https://www.pureref.com/", "Free program for collecting images on canvases", DataStructs.ResourceType.Software),
                new DataStructs.Resource("ArtStation", "https://www.artstation.com/", "ArtStation provides you with a simple, yet powerful way to show your portfolio and be seen by the right people in the industry.", DataStructs.ResourceType.Inspiration),
                new DataStructs.Resource("Trello", "https://trello.com/", "Trello helps teams work more collaboratively and get more done", DataStructs.ResourceType.Website),
                new DataStructs.Resource("Miro", "https://miro.com/", "Embed digital whiteboards into products to extend workflows", DataStructs.ResourceType.Website),
                new DataStructs.Resource("Desmos", "https://www.desmos.com/", "Advanced graphing calculator implemented as a web application", DataStructs.ResourceType.Website),
                new DataStructs.Resource("PlasticSCM", "https://www.plasticscm.com/", "Plastic SCM is a version control to help teams focus on delivering work, one task at a time", DataStructs.ResourceType.Software)
            };

            questions = new List<DataStructs.FAQTopic>()
            {
                new DataStructs.FAQTopic("Why is my world lagging?", "Most of the issues regarding general performance can be solved by using VRWorldToolkit. You can download VRWorldToolkit from 'Importer' page", "https://github.com/oneVR/VRWorldToolkit"),
                new DataStructs.FAQTopic("Where can I learn Udon?", "Vowgan has created some beginner level Udon Graph and UdonSharp tutorials that can help you to get started", "https://www.youtube.com/VowganVR"),
                new DataStructs.FAQTopic("Can I test worlds locally faster?", "CyanEmu allows you to basic testing of your world inside Unity editor, You can download VRWorldToolkit from 'Importer' page", "https://github.com/CyanLaser/CyanEmu"),
                new DataStructs.FAQTopic("Why am I constantly getting errors?", "1 gave a talk at VRCPrefabs TLX about fixing Unity projects, you can find a lot of answers from their talk", "https://tlx.dev/talks/TLX2020-12/Fixing%20Broken%20Unity%20Projects")
            };
            */
            #endregion

            wcaData = AssetDatabase.LoadAssetAtPath<WCAData>("Assets/Varneon/WorldCreatorAssistant/Scripts/WCAData.asset");
        }

        private void drawResourceBlock(DataStructs.Resource resource, int index)
        {
            Rect rect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(resource.Name, DataStructs.BlockHeaderButton))
            {
                selectedResource = (selectedResource == index) ? -1 : index;
            }

            GUILayout.Label(resource.Type.ToString(), DataStructs.ResourceTypeText, GUILayout.MaxWidth(100));
            GUILayout.EndHorizontal();
            if (index == selectedResource)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + 30, rect.width, 1), (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : Color.grey));
                GUILayout.Label(resource.Description, DataStructs.WrappedText);
                if (resource.URL.Length > 0)
                {
                    drawBlockURL(resource.URL);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void drawQuestionBlock(DataStructs.FAQTopic question, int index)
        {
            Rect rect = EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.BeginVertical();

            if (GUILayout.Button(question.Question, DataStructs.BlockHeaderButton))
            {
                selectedQuestion = (selectedQuestion == index) ? -1 : index;
            }

            if(index == selectedQuestion)
            {
                EditorGUI.DrawRect(new Rect(rect.x, rect.y + 30, rect.width, 1), (EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : Color.grey));
                
                GUILayout.Label(question.Answer, DataStructs.WrappedText);

                if (question.URL.Length > 0)
                {
                    drawBlockURL(question.URL);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void drawBlockURL(string url)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(url, DataStructs.ButtonHyperlink))
            {
                Application.OpenURL(url);
            }
            else if (GUILayout.Button(new GUIContent("Copy Link", iconCopy), DataStructs.FlatStandardButton, new GUILayoutOption[] { GUILayout.MaxWidth(100), GUILayout.MaxHeight(20) }))
            {
                EditorGUIUtility.systemCopyBuffer = url;
            }
            GUILayout.EndHorizontal();
        }
    }
}
#endif