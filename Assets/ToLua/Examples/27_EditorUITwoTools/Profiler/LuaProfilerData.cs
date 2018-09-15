using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LuaMonitor
{
    public class LuaProfilerData
    {
        public string funcname;
        public string source;
        public string totaltime;
        public string average;
        public string relative;
        public string called;
    }
}
