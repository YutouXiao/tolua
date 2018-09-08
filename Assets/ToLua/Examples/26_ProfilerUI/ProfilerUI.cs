using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

public class ProfilerUI : LuaClient
{
    /// <summary>
    /// Debug.log message
    /// </summary>
    struct Log
    {
        public string message;
        public string stackTrace;
        public LogType type;
    }

    #region Inspector Settings

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

        public SnapMsg(string _func, string _source, string _total, string _average, string _relative, string _called)
        {
            this.func = _func;
            this.source = _source;
            this.total = _total;
            this.average = _average;
            this.relative = _relative;
            this.called = _called;
        }
    }
    private SnapMsg receiveSnapMsg;
    private List<SnapMsg> snapMsgs = new List<SnapMsg>();

    /// <summary>
    /// class to store title message
    /// </summary>
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

    /// <summary>
    /// The hotkey to show and hide the console window.
    /// </summary>
    public KeyCode toggleKey = KeyCode.BackQuote;


    /// <summary>
    /// Filter string
    /// </summary>
    public string textToFilter = "";

    /// <summary>
    /// Snap string
    /// </summary>
    public string textToSnap = "";

    /// <summary>
    /// Calculate string
    /// </summary>
    public string textToCal1 = "";
    public string textToCal2 = "";

    /// <summary>
    /// Whether to open the window by shaking the device (mobile-only).
    /// </summary>
    public bool shakeToOpen = true;

    /// <summary>
    /// The (squared) acceleration above which the window should open.
    /// </summary>
    public float shakeAcceleration = 3f;

    /// <summary>
    /// Whether to only keep a certain number of logs.
    ///
    /// Setting this can be helpful if memory usage is a concern.
    /// </summary>
    public bool restrictLogCount = false;

    /// <summary>
    /// Number of logs to keep before removing old ones.
    /// </summary>
    public int maxLogs = 1000;

    #endregion

    readonly List<Log> logs = new List<Log>();
    Vector2 scrollPosition;
    bool visible;
    bool collapse;

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
    static readonly GUIContent TextContent = new GUIContent("Text", "Text Input.");
    static readonly GUIContent filterContent = new GUIContent("Filter", "Filter the content.");
    static readonly GUIContent takeSnap = new GUIContent("TakeSnap", "Take the snapshot content.");
    static readonly GUIContent CalculSnap = new GUIContent("CalculSnap", "Calculate the snapshots.");
    static readonly GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");
    static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the content of the log.");

    readonly Rect titleBarRect = new Rect(0, 0, 10000, 20);
    Rect windowRect = new Rect(margin * 2, margin * 3, Screen.width - (margin * 4), Screen.height - (margin * 6));

    /*
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
    */

    /// <summary>
    /// On GUI Start 
    /// </summary>
    void OnGUI()
    {
        if (!visible)
        {
            return;
        }

        windowRect = GUILayout.Window(123456, windowRect, DrawConsoleWindow, windowTitle);
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

        // Iterate through the recorded logs.
        //for (var i = 0; i < logs.Count; i++)
        //{
        //    var log = logs[i];

        //    // Combine identical messages if collapse option is chosen.
        //    if (collapse && i > 0)
        //    {
        //        var previousMessage = logs[i - 1].message;

        //        if (log.message == previousMessage)
        //        {
        //            continue;
        //        }
        //    }

        //    GUI.contentColor = logTypeColors[log.type];
        //    GUILayout.Label(log.message);
        //}

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
            snapMsgs.Clear();
            FilterSnapshot();
            if (collapse)
            {
                DistinctList();
            }
        }
        collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("the snap name", GUILayout.Width(90));
        textToSnap = GUILayout.TextField(textToSnap, 30);
        if (GUILayout.Button(takeSnap))
        {
            snapMsgs.Clear();
            TakeSnapshot();
            if (collapse)
            {
                DistinctList();
            }
        }
        if (GUILayout.Button(clearLabel))
        {
            snapMsgs.Clear();
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
        logs.Add(new Log
        {
            message = message,
            stackTrace = stackTrace,
            type = type,
        });

        TrimExcessLogs();
    }

    /// <summary>
    /// Removes old logs that exceed the maximum number allowed.
    /// </summary>
    void TrimExcessLogs()
    {
        if (!restrictLogCount)
        {
            return;
        }

        var amountToRemove = Mathf.Max(logs.Count - maxLogs, 0);

        if (amountToRemove == 0)
        {
            return;
        }

        logs.RemoveRange(0, amountToRemove);
    }

    // Use this for initialization
    void Start()
    {                       //todo
        string fullPath = Application.dataPath + "\\ToLua\\Examples\\26_ProfilerUI";
        luaState.AddSearchPath(fullPath);

        luaState.Require("profiler");
    }

    // Update is called once per frame
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

    void TakeSnapshot()
    {
        LuaFunction func1 = luaState.GetFunction("profiler.start");
        if (func1 != null)
        {
            func1.BeginPCall();
            func1.PCall();
            //int num = (int)func.CheckNumber();
            func1.EndPCall();
        }

        LuaTable receiveTable = new LuaTable(0, luaState);
        LuaFunction func2 = luaState.GetFunction("profiler.report");
        if (func2 != null)
        {
            func2.BeginPCall();
            func2.Push(textToSnap);
            func2.PCall();
            receiveTable = func2.CheckLuaTable();
            //int num = (int)func.CheckNumber();
            func2.EndPCall();
        }

        List<LuaTable> innerTables = new List<LuaTable>();
        for (int i = 1; i <= receiveTable.Length; i++)
        {

            innerTables.Add(receiveTable.RawGet<int, LuaTable>(i));
        }

        for (int j = 0; j < innerTables.Count; j++)
        {
            receiveSnapMsg = new SnapMsg(
                innerTables[j].RawGet<string, string>("func"),
                innerTables[j].RawGet<string, string>("source"),
                innerTables[j].RawGet<string, string>("total"),
                innerTables[j].RawGet<string, string>("average"),
                innerTables[j].RawGet<string, string>("relative"),
                innerTables[j].RawGet<string, string>("called")
                );
            snapMsgs.Add(receiveSnapMsg);
        }
        
        LuaFunction func3 = luaState.GetFunction("profiler.stop");
        if (func3 != null)
        {
            func3.BeginPCall();
            func3.PCall();
            //int num = (int)func.CheckNumber();
            func3.EndPCall();
        }
    }

    void FilterSnapshot()
    {
        LuaTable receiveTable = new LuaTable(0, luaState);
        LuaFunction func = luaState.GetFunction("profiler.luafilter");
        if (func != null)
        {
            func.BeginPCall();
            func.Push(textToSnap);
            func.Push(textToFilter);
            func.PCall();
            receiveTable = func.CheckLuaTable();
            //int num = (int)func.CheckNumber();
            func.EndPCall();
        }

        List<LuaTable> innerTables = new List<LuaTable>();
        for (int i = 1; i <= receiveTable.Length; i++)
        {

            innerTables.Add(receiveTable.RawGet<int, LuaTable>(i));
        }

        for (int j = 0; j < innerTables.Count; j++)
        {
            receiveSnapMsg = new SnapMsg(
                innerTables[j].RawGet<string, string>("func"),
                innerTables[j].RawGet<string, string>("source"),
                innerTables[j].RawGet<string, string>("total"),
                innerTables[j].RawGet<string, string>("average"),
                innerTables[j].RawGet<string, string>("relative"),
                innerTables[j].RawGet<string, string>("called")
                );
            snapMsgs.Add(receiveSnapMsg);
        }
    }

    /// <summary>
    /// print UI label
    /// </summary>
    void LabelPrint()
    {
        if (preMesList != null && preMesList.Count > 0)
        {
            for (int i = 0; i < preMesList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(preMesList[i].title, GUILayout.Width(windowRect.width * 0.4f));
                GUILayout.Label(preMesList[i].message, GUILayout.Width(windowRect.width * 0.5f));
                GUILayout.EndHorizontal();
            }
        }
        if (snapMsgs != null && snapMsgs.Count > 0)
        {
            float win = windowRect.width * 0.95f;
            //{0,15}:{1,10}:{2,15}:{3,20}:{4,20}:{5,20}
            float w1 = win * 0.15f; var w2 = win * 0.1f; var w3 = win * 0.15f; var w4 = win * 0.2f; var w5 = win * 0.2f; var w6 = win * 0.2f;
            for (int i = 0; i < snapMsgs.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(snapMsgs[i].func, GUILayout.Width(w1));
                GUILayout.Label(snapMsgs[i].source, GUILayout.Width(w2));
                GUILayout.Label(snapMsgs[i].total, GUILayout.Width(w3));
                GUILayout.Label(snapMsgs[i].average, GUILayout.Width(w4));
                GUILayout.Label(snapMsgs[i].relative, GUILayout.Width(w5));
                GUILayout.Label(snapMsgs[i].called, GUILayout.Width(w6));
                GUILayout.EndHorizontal();
            }
        }
    }

    /// <summary>
    /// distinct label message list 
    /// </summary>
    void DistinctList()
    {
        int checkState = 0;
        for (int i = 0; i < snapMsgs.Count; i++)
        {
            checkState = 0;
            for (int j = 0; j < snapMsgs.Count; j++)
            {
                if (snapMsgs[i].func == snapMsgs[j].func)
                {
                    checkState += 1;
                }
            }
            if (checkState >= 2)
            {
                snapMsgs.Remove(snapMsgs[i]);
            }
        }
    }
}
