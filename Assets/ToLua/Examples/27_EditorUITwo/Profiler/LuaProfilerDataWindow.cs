using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using LuaEditorUI;
using UnityEditor;

namespace LuaMonitor
{
    public class LuaProfilerDataWindow
    {
        LuaProfilerWin pfWindow;
        public string textToFilter = "";
        static readonly GUIContent filterContent = new GUIContent("Filter", "Filter the content.");
        static readonly GUIContent reportLabel = new GUIContent("Report", "Report the message.");
        static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the content of the log.");
        private List<LuaProfilerData> dataList;
        private List<LuaProfilerData> iniList;

        public LuaProfilerDataWindow(LuaProfilerWin _pfWindow)
        {
            this.pfWindow = _pfWindow;
        }

        public void DrawWindow(Rect rect)
        {
            GUILayout.BeginArea(rect);
            DataLabel(rect);
            ButtonLabel(new Rect(0, rect.height - 30, rect.width, 30));
            GUILayout.EndArea();
        }

        void ButtonLabel(Rect rect)
        {
            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            textToFilter = GUILayout.TextField(textToFilter, 50, GUILayout.MinHeight(20));
            if (GUILayout.Button(filterContent))
            {
                FilterContent();
            }
            if (GUILayout.Button(reportLabel))
            {
                if (EditorApplication.isPlaying)
                {
                    if (main.Instance != null)
                    {
                        iniList = new List<LuaProfilerData>(main.Instance.dataList);
                        dataList = new List<LuaProfilerData>(iniList);
                    }
                }
            }
            if (GUILayout.Button(clearLabel))
            {
                if (iniList != null && dataList != null)
                {
                    iniList.Clear();
                    dataList.Clear();
                }
            }
            EditorGUILayout.Space();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void FilterContent()
        {
            if (iniList != null && iniList.Count > 0)
            {
                dataList.Clear();
                dataList.Add(iniList[0]);
                for (int i = 1; i < iniList.Count; i++)
                {
                    if (iniList[i].funcname.Contains(textToFilter))
                    {
                        dataList.Add(iniList[i]);
                    }
                }
            }
        }

        void DataLabel(Rect rect)
        {
            if (dataList != null && dataList.Count > 0)
            {
                float win = rect.width * 1.1f;
                //{0,15}:{1,20}:{2,15}:{3,15}:{4,15}:{5,15}
                float w1 = win * 0.15f; var w2 = win * 0.2f; var w3 = win * 0.15f; var w4 = win * 0.15f; var w5 = win * 0.15f; var w6 = win * 0.15f;
                for (int i = 0; i < dataList.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(dataList[i].funcname, GUILayout.Width(w1));
                    GUILayout.Label(dataList[i].source, GUILayout.Width(w2));
                    GUILayout.Label(dataList[i].totaltime, GUILayout.Width(w3));
                    GUILayout.Label(dataList[i].average, GUILayout.Width(w4));
                    GUILayout.Label(dataList[i].relative, GUILayout.Width(w5));
                    GUILayout.Label(dataList[i].called, GUILayout.Width(w6));
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
