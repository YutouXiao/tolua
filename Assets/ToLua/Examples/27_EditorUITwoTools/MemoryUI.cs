using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

namespace LuaMonitor
{
    public class MemoryUI : LuaClient
    {
        private static MemoryUI _instance;

        private LuaSnapshotData receiveSnapMsg;
        private List<LuaSnapshotData> snapMsgs = new List<LuaSnapshotData>();

        public static MemoryUI Instance
        {
            get
            {
                return _instance;
            }
        }

        // Use this for initialization
        void Start()
        {
            _instance = this;
            string fullPath = Application.dataPath + "\\ToLua\\Examples\\27_EditorUITwoTools";
            luaState.AddSearchPath(fullPath);

            luaState.Require("memory");
        }

        // Update is called once per frame
        void Update()
        {
        }

        public List<LuaSnapshotData> TakeSnapshot(string textToSnap)
        {
            LuaTable receiveTable = new LuaTable(0, luaState);
            LuaFunction func = luaState.GetFunction("memtools.takesnap");
            if (func != null)
            {
                func.BeginPCall();
                func.Push(textToSnap);
                func.PCall();
                receiveTable = func.CheckLuaTable();
                func.EndPCall();
            }

            List<LuaTable> innerTables = new List<LuaTable>();
            for (int i = 1; i <= receiveTable.Length; i++)
            {

                innerTables.Add(receiveTable.RawGet<int, LuaTable>(i));
            }

            for (int j = 0; j < innerTables.Count; j++)
            {
                receiveSnapMsg = new LuaSnapshotData
                { 
                    name = innerTables[j].RawGet<string, string>("name"),
                    size = innerTables[j].RawGet<string, string>("size"),
                    type = innerTables[j].RawGet<string, string>("type"),
                    id = innerTables[j].RawGet<string, string>("id"),
                    info = innerTables[j].RawGet<string, string>("info")
                };
                snapMsgs.Add(receiveSnapMsg);
            }

            return snapMsgs;
        }

        public List<LuaSnapshotData> FilterSnapshot(string textToSnap, string textToFilter)
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
                func.EndPCall();
            }

            List<LuaTable> innerTables = new List<LuaTable>();
            for (int i = 1; i <= receiveTable.Length; i++)
            {

                innerTables.Add(receiveTable.RawGet<int, LuaTable>(i));
            }

            for (int j = 0; j < innerTables.Count; j++)
            {
                receiveSnapMsg = new LuaSnapshotData
                {
                    name = innerTables[j].RawGet<string, string>("name"),
                    size = innerTables[j].RawGet<string, string>("size"),
                    type = innerTables[j].RawGet<string, string>("type"),
                    id = innerTables[j].RawGet<string, string>("id"),
                    info = innerTables[j].RawGet<string, string>("info")
                };
                snapMsgs.Add(receiveSnapMsg);
            }

            return snapMsgs;
        }

        public List<LuaSnapshotData> CalculationSnap(string textToCal1, string textToCal2)
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
                func.EndPCall();
            }

            List<LuaTable> innerTables = new List<LuaTable>();
            for (int i = 1; i <= receiveTable.Length; i++)
            {

                innerTables.Add(receiveTable.RawGet<int, LuaTable>(i));
            }

            for (int j = 0; j < innerTables.Count; j++)
            {
                receiveSnapMsg = new LuaSnapshotData
                {
                    name = innerTables[j].RawGet<string, string>("name"),
                    size = innerTables[j].RawGet<string, string>("size"),
                    type = innerTables[j].RawGet<string, string>("type"),
                    id = innerTables[j].RawGet<string, string>("id"),
                    info = innerTables[j].RawGet<string, string>("info")
                };
                snapMsgs.Add(receiveSnapMsg);
            }

            return snapMsgs;
        }

        public string GetTotalMemory()
        {
            string memory_total = "";
            LuaFunction func = luaState.GetFunction("memtools.total");
            if (func != null)
            {
                func.BeginPCall();
                func.PCall();
                memory_total = func.CheckValue<string>();
                func.EndPCall();
            }

            return memory_total;
        }
    }
}
