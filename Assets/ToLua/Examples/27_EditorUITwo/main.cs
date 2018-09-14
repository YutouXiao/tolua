using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.EventSystems;
using System;
using System.Runtime.InteropServices;
using LuaMonitor;

namespace LuaEditorUI
{
    public class main : MonoBehaviour
    {
        private static main _instance;
        public List<LuaProfilerData> dataList = new List<LuaProfilerData>();
        
        public static main Instance
        {
            get
            {
                return _instance;
            }
        }

        void OnEnable()
        {
			Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
			Application.logMessageReceived -= HandleLog;
        }

        void Start()
        {
            _instance = this;
        }

        void OnDestroy()
        {
        }

        void Update()
        {
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
                    dataList.Add(new LuaProfilerData
                    {
                        funcname = logMember[0],
                        source = logMember[1],
                        totaltime = logMember[2],
                        average = logMember[3],
                        relative = logMember[4],
                        called = logMember[5],
                    });
                }
            }
        }
    }
}
