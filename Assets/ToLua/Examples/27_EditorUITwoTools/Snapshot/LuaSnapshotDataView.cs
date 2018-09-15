using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using LuaEditorUI;
using UnityEditor;
using System;

namespace LuaMonitor
{
    public class LuaSnapshotDataView
    {
        LuaSnapshotView ssWindow;
        public string textToFilter = "";
        public string textToSnap = "";
        public string textToCal1 = "";
        public string textToCal2 = "";
        private Vector2 scrollPosition;

        //Original readonly GUIContent
        static readonly GUIContent filterContent = new GUIContent("Filter", "Filter the content.");
        static readonly GUIContent takeSnap = new GUIContent("TakeSnap", "Take the snapshot content.");
        static readonly GUIContent CalculSnap = new GUIContent("CalculSnap", "Calculate the snapshots.");
        static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the content of the log.");
        private List<LuaSnapshotData> dataList = new List<LuaSnapshotData>();
        
        public class PreMes
        {
            public string title;
            public string message;

            public PreMes(string t, string m)
            {
                title = t;
                message = m;
            }
        }
        private List<PreMes> preMesList = new List<PreMes>();

        public LuaSnapshotDataView(LuaSnapshotView _ssWindow)
        {
            this.ssWindow = _ssWindow;
        }

        public void DrawWindow(Rect rect)
        {
            Rect DataRect = new Rect(0, 0, rect.width, rect.height - 70);
            GUILayout.BeginArea(DataRect);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            DataLabel(DataRect);
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            Rect buttonRect = new Rect(0, rect.height - 70, rect.width, 70);
            GUILayout.BeginArea(buttonRect);
            ButtonLabel(buttonRect);
            GUILayout.EndArea();
        }

        /// <summary>
        /// add first two lines message
        /// </summary>
        void AddTitle()
        {
            if (preMesList != null && MemoryUI.Instance != null)
            {
                if (preMesList.Count == 0)
                {
                    preMesList.Add(new PreMes("snapshot key: ", textToSnap));
                    preMesList.Add(new PreMes("total memory: ", MemoryUI.Instance.GetTotalMemory()));
                }
                if (preMesList.Count == 2)
                {
                    preMesList[0].title = "snapshot key: ";
                    preMesList[0].message = textToSnap;

                    preMesList[1].title = "total memory: ";
                    preMesList[1].message = MemoryUI.Instance.GetTotalMemory();
                }
            }
        }

        void ButtonLabel(Rect rect)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            textToFilter = GUILayout.TextField(textToFilter, 50, GUILayout.MinHeight(21));
            if (GUILayout.Button(filterContent))
            {
                if (MemoryUI.Instance != null)
                {
                    dataList.Clear();
                    dataList = MemoryUI.Instance.FilterSnapshot(textToSnap, textToFilter);
                }
            }
            if (GUILayout.Button(clearLabel))
            {
                if (MemoryUI.Instance != null)
                {
                    preMesList.Clear();
                    dataList.Clear();
                }
            }
            EditorGUILayout.Space();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUILayout.Label("the snap name", GUILayout.Width(90));
            textToSnap = GUILayout.TextField(textToSnap, 30, GUILayout.MinHeight(21));
            if (GUILayout.Button(takeSnap))
            {
                if (MemoryUI.Instance != null)
                {
                    dataList.Clear();
                    AddTitle();
                    dataList = MemoryUI.Instance.TakeSnapshot(textToSnap);
                }
            }
            EditorGUILayout.Space();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            GUILayout.Label("snap1", GUILayout.Width(40));
            textToCal1 = GUILayout.TextField(textToCal1, 30, GUILayout.MinHeight(21));
            GUILayout.Label("snap2", GUILayout.Width(40));
            textToCal2 = GUILayout.TextField(textToCal2, 30, GUILayout.MinHeight(21));
            if (GUILayout.Button(CalculSnap))
            {
                if (MemoryUI.Instance != null)
                {
                    dataList.Clear();
                    preMesList.Clear();
                    dataList = MemoryUI.Instance.CalculationSnap(textToCal1, textToCal2);
                }
            }
            EditorGUILayout.Space();
            GUILayout.EndHorizontal();
        }
        
        void DataLabel(Rect rect)
        {
            if (preMesList != null && preMesList.Count > 0)
            {
                for (int i = 0; i < preMesList.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(preMesList[i].title, GUILayout.Width(rect.width * 0.4f));
                    GUILayout.Label(preMesList[i].message, GUILayout.Width(rect.width * 0.5f));
                    GUILayout.EndHorizontal();
                }
            }
            if (dataList != null && dataList.Count > 0)
            {
                float win = rect.width * 1.2f;
                //{0,15}:{1,20}:{2,15}:{3,15}:{4,15}
                float w1 = win * 0.15f; var w2 = win * 0.1f; var w3 = win * 0.1f; var w4 = win * 0.15f; var w5 = win * 0.3f;
                for (int i = 0; i < dataList.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(dataList[i].name, GUILayout.Width(w1));
                    GUILayout.Label(dataList[i].size, GUILayout.Width(w2));
                    GUILayout.Label(dataList[i].type, GUILayout.Width(w3));
                    GUILayout.Label(dataList[i].id, GUILayout.Width(w4));
                    GUILayout.Label(dataList[i].info, GUILayout.Width(w5));
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
