using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LuaMonitor
{
    public class LuaMonitorWindow : EditorWindow
    {
        static Vector2 s_minSize = new Vector2(200, 200);

        [MenuItem("Lua/Lua Monitor Window", priority = 100)]
        static void OpenWindow()
        {
            LuaMonitorWindow window = EditorWindow.GetWindow<LuaMonitorWindow>();
            window.minSize = s_minSize;
            window.Show();
        }

        int p_mode = 0;
        const int c_toolbarHeight = 40;

        LuaSnapshotView ssWindow;
        LuaProfilerWin pfWindow;

        private void OnEnable()
        {
            ssWindow = new LuaSnapshotView(this);
            pfWindow = new LuaProfilerWin(this);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            p_mode = GUILayout.Toolbar(p_mode, new string[2] { "Profiler", "Snapshot" }, GUILayout.Height(c_toolbarHeight - 5));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            Rect winRect = new Rect(0, c_toolbarHeight, position.width, position.height - c_toolbarHeight);
            EditorGUI.DrawRect(new Rect(winRect.x, winRect.y - 1, winRect.width, 1), Color.gray);

            if (p_mode == 0)
            {
                pfWindow.DrawWindow(winRect);
            }
            else if (p_mode == 1)
            {
                ssWindow.DrawWindow(winRect);
            }
        }
    }
}