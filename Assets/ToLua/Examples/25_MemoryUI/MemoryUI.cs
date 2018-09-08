using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

public class MemoryUI : LuaClient
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
    /// class to store lua message
    /// </summary>
    public class SnapMsg
    {
        public string name;
        public string size;
        public string type;
        public string id;
        public string info;

        public SnapMsg(string _name,string _size, string _type, string _id, string _info)
        {
            this.name = _name;
            this.size = _size;
            this.type = _type;
            this.id = _id;
            this.info = _info;
        }
    }
    private SnapMsg receiveSnapMsg;
    private List<SnapMsg> snapMsgs = new List<SnapMsg>();

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

    public delegate List<SnapMsg> String2Paragram(string str1, string str2);
    public delegate List<SnapMsg> stringParam(string str1);
    public delegate string voidParam();

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
        if (GUILayout.Button(clearLabel))
        {
            preMesList.Clear();
            snapMsgs.Clear();
        }
        collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("the snap name", GUILayout.Width(90));
        textToSnap = GUILayout.TextField(textToSnap, 30);
        if (GUILayout.Button(takeSnap))
        {
            snapMsgs.Clear();
            AddTitle();
            TakeSnapshot();
            if (collapse)
            {
                DistinctList();
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("snap1", GUILayout.Width(40));
        textToCal1 = GUILayout.TextField(textToCal1, 30);
        GUILayout.Label("snap2", GUILayout.Width(40));
        textToCal2 = GUILayout.TextField(textToCal2, 30);
        if (GUILayout.Button(CalculSnap))
        {
            preMesList.Clear();
            snapMsgs.Clear();
            CalculationSnap();
            if (collapse)
            {
                DistinctList();
            }
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
    void Start () {                            //todo
		string fullPath = Application.dataPath + "\\ToLua\\Examples\\25_MemoryUI";
        luaState.AddSearchPath(fullPath); 

        luaState.Require("memory");
	}
	
	// Update is called once per frame
	void Update () {
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
        LuaTable receiveTable = new LuaTable(0, luaState);
        LuaFunction func = luaState.GetFunction("memtools.takesnap");
        if (func != null)
        {
            func.BeginPCall();
            func.Push(textToSnap);
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
                innerTables[j].RawGet<string, string>("name"),
                innerTables[j].RawGet<string, string>("size"),
                innerTables[j].RawGet<string, string>("type"),
                innerTables[j].RawGet<string, string>("id"),
                innerTables[j].RawGet<string, string>("info")
                );
            snapMsgs.Add(receiveSnapMsg);
        }
    }

    void FilterSnapshot()
    {
        LuaTable receiveTable = new LuaTable(0, luaState);
        LuaFunction func = luaState.GetFunction("memtools.filterstr");
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
                innerTables[j].RawGet<string, string>("name"),
                innerTables[j].RawGet<string, string>("size"),
                innerTables[j].RawGet<string, string>("type"),
                innerTables[j].RawGet<string, string>("id"),
                innerTables[j].RawGet<string, string>("info")
                );
            snapMsgs.Add(receiveSnapMsg);
        }
    }

    void CalculationSnap()
    {
        LuaTable receiveTable = new LuaTable(0, luaState);
        LuaFunction func = luaState.GetFunction("memtools.calculation");
        if (func != null)
        {
            func.BeginPCall();
            func.Push(textToCal1);
            func.Push(textToCal2);
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
                innerTables[j].RawGet<string, string>("name"),
                innerTables[j].RawGet<string, string>("size"),
                innerTables[j].RawGet<string, string>("type"),
                innerTables[j].RawGet<string, string>("id"),
                innerTables[j].RawGet<string, string>("info")
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
            //{0,20}:{1,10}:{2,15}:{3,25}:{4,30}
            float w1 = win * 0.2f; var w2 = win * 0.1f; var w3 = win * 0.15f; var w4 = win * 0.25f; var w5 = win * 0.3f;
            for (int i = 0; i < snapMsgs.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(snapMsgs[i].name, GUILayout.Width(w1));
                GUILayout.Label(snapMsgs[i].size, GUILayout.Width(w2));
                GUILayout.Label(snapMsgs[i].type, GUILayout.Width(w3));
                GUILayout.Label(snapMsgs[i].id, GUILayout.Width(w4));
                GUILayout.Label(snapMsgs[i].info, GUILayout.Width(w5));
                GUILayout.EndHorizontal();
            }
        }
    }

    /// <summary>
    /// add first two lines message
    /// </summary>
    void AddTitle()
    {
        string memory_total = "";
        LuaFunction func = luaState.GetFunction("memtools.total");
        if (func != null)
        {
            func.BeginPCall();
            func.PCall();
            memory_total = func.CheckValue<string>();
            //int num = (int)func.CheckNumber();
            func.EndPCall();
        }

        if (preMesList != null)
        {
            if (preMesList.Count == 0)
            {
                preMesList.Add(new PreMes("snapshot key: ", textToSnap));
                preMesList.Add(new PreMes("total memory: ", memory_total));
            }
            if (preMesList.Count == 2)
            {
                preMesList[0].title = "snapshot key: ";
                preMesList[0].message = textToSnap;

                preMesList[1].title = "total memory: ";
                preMesList[1].message = memory_total;
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
                if (snapMsgs[i].name == snapMsgs[j].name)
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
