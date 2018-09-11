using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Consolation
{
    /// <summary>
    /// A console to display Unity's debug logs in-game.
    /// </summary>
    class ProfilerUI : MonoBehaviour
    {
        #region Inspector Settings

        /// <summary>
        /// The hotkey to show and hide the console window.
        /// </summary>
        public KeyCode toggleKey = KeyCode.BackQuote;


        /// <summary>
        /// Filter string
        /// </summary>
        public string textToFilter = "";

        /// <summary>
        /// Whether to open the window by shaking the device (mobile-only).
        /// </summary>
        public bool shakeToOpen = true;

        /// <summary>
        /// The (squared) acceleration above which the window should open.
        /// </summary>
        public float shakeAcceleration = 3f;

        #endregion

        Vector2 scrollPosition;
        bool visible;

        /// <summary>
        /// class to output lua memory message
        /// </summary>
        public class SnapMsg
        {
            public string func;
            public string source;
            public string total;
            public string average;
            public string relative;
            public string called;
        }
        private List<SnapMsg> snapMsgs = new List<SnapMsg>();
        private List<SnapMsg> screenMsgs = new List<SnapMsg>();
        // Visual elements:

        /// <summary>
        /// Set readonly color type
        /// </summary>
        static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
        {
            { LogType.Assert, Color.white },
            { LogType.Error, Color.red },
            { LogType.Exception, Color.red },
            { LogType.Log, Color.white },
            { LogType.Warning, Color.yellow },
        };

        const string windowTitle = "Console";
        const int margin = 20;

        //Original GUIContent
        static readonly GUIContent filterContent = new GUIContent("Filter", "Filter the content.");
        static readonly GUIContent report = new GUIContent("Report", "Report the properties .");
        static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the content of the log.");

        readonly Rect titleBarRect = new Rect(0, 0, 10000, 20);
        Rect windowRect = new Rect(margin * 2, margin * 3, Screen.width - (margin * 4), Screen.height - (margin * 6));


        void OnEnable()
        {
#if UNITY_5
			Application.logMessageReceived += HandleLog;
#else
            Application.RegisterLogCallback(HandleLog);
#endif
        }

        void OnDisable()
        {
#if UNITY_5
			Application.logMessageReceived -= HandleLog;
#else
            Application.RegisterLogCallback(null);
#endif
        }

        void Start()
        {

        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                visible = !visible;
                windowRect = new Rect(margin * 2, margin * 3, Screen.width - (margin * 4), Screen.height - (margin * 6));
            }

            if (shakeToOpen && Input.acceleration.sqrMagnitude > shakeAcceleration)
            {
                visible = true;
            }
        }

        /// <summary>
        /// On GUI Start 
        /// </summary>
        void OnGUI()
        {
            if (!visible)
            {
                return;
            }

            windowRect = GUILayout.Window(1, windowRect, DrawConsoleWindow, windowTitle);
        }

        /// <summary>
        /// Displays a window that lists the recorded logs.
        /// </summary>
        /// <param name="windowID">Window ID.</param>
        void DrawConsoleWindow(int windowID)
        {
            DrawLogsList();
            DrawToolbar();

            // Allow the window to be dragged by its title bar.
            GUI.DragWindow(titleBarRect);
        }

        /// <summary>
        /// Displays a scrollable list of logs.
        /// </summary>
        void DrawLogsList()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            LabelPrint();

            GUILayout.EndScrollView();

            // Ensure GUI colour is reset before drawing other components.
            GUI.contentColor = Color.white;
        }

        /// <summary>
        /// Displays options for filtering and changing the logs list.
        /// </summary>
        void DrawToolbar()
        {
            //Draw the components on the GUI
            GUILayout.BeginHorizontal();
            textToFilter = GUILayout.TextField(textToFilter, 50);
            if (GUILayout.Button(filterContent))
            {
                FilterContent();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(clearLabel))
            {
                snapMsgs.Clear();
                screenMsgs.Clear();
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Records a log from the log callback.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="stackTrace">Trace of where the message came from.</param>
        /// <param name="type">Type of message (error, exception, warning, assert).</param>
        void HandleLog(string message, string stackTrace, LogType type)
        {
            string[] logList = message.Split('|');
            if (logList.Length > 1 && logList[1].Contains("Report Message"))
            {
                for (int i = 2; i < logList.Length; i++)
                {
                    string[] logMember = logList[i].Split('+');
                    snapMsgs.Add(new SnapMsg
                    {
                        func = logMember[0],
                        source = logMember[1],
                        total = logMember[2],
                        average = logMember[3],
                        relative = logMember[4],
                        called = logMember[5],
                    });
                    screenMsgs.Add(new SnapMsg
                    {
                        func = logMember[0],
                        source = logMember[1],
                        total = logMember[2],
                        average = logMember[3],
                        relative = logMember[4],
                        called = logMember[5],
                    });
                }
            }
        }

        /// <summary>
        /// print UI label
        /// </summary>
        void LabelPrint()
        {
            if (screenMsgs != null && screenMsgs.Count > 0)
            {
                float win = windowRect.width * 0.95f;
                //{0,15}:{1,20}:{2,15}:{3,15}:{4,15}:{5,15}
                float w1 = win * 0.15f; var w2 = win * 0.2f; var w3 = win * 0.15f; var w4 = win * 0.15f; var w5 = win * 0.15f; var w6 = win * 0.15f;
                for (int i = 0; i < screenMsgs.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(screenMsgs[i].func, GUILayout.Width(w1));
                    GUILayout.Label(screenMsgs[i].source, GUILayout.Width(w2));
                    GUILayout.Label(screenMsgs[i].total, GUILayout.Width(w3));
                    GUILayout.Label(screenMsgs[i].average, GUILayout.Width(w4));
                    GUILayout.Label(screenMsgs[i].relative, GUILayout.Width(w5));
                    GUILayout.Label(screenMsgs[i].called, GUILayout.Width(w6));
                    GUILayout.EndHorizontal();
                }
            }
        }

        /// <summary>
        /// filter button 
        /// </summary>
        void FilterContent()
        {
            screenMsgs.Clear();
            if (snapMsgs != null && snapMsgs.Count > 0)
            {
                screenMsgs.Add(snapMsgs[0]);
                for (int i = 1; i < snapMsgs.Count; i++)
                {
                    if (snapMsgs[i].func.Contains(textToFilter))
                    {
                        screenMsgs.Add(snapMsgs[i]);
                    }
                }
            }
        }
    }
}


