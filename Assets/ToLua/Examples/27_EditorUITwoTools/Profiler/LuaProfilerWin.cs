using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LuaEditorUI;

namespace LuaMonitor
{
    public class LuaProfilerWin
    {
        LuaMonitorWindow window;
        LuaProfilerDataWindow dataWindow;

        public LuaProfilerWin(LuaMonitorWindow _window)
        {
            window = _window;
            dataWindow = new LuaProfilerDataWindow(this);
        }

        public void DrawWindow(Rect rect)
        {
            GUILayout.BeginArea(rect);
            
            Rect dataRect = new Rect(0, 0, rect.width, rect.height);
            EditorGUI.DrawRect(new Rect(dataRect.x, dataRect.y - 1, dataRect.width, 1), Color.gray);
            dataWindow.DrawWindow(dataRect);

            GUILayout.EndArea();
        }
    }
}

