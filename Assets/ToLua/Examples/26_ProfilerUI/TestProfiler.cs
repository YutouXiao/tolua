using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

public class TestProfiler : LuaClient
{
    // Use this for initialization
    void Start()
    {                            //todo
        string fullPath = Application.dataPath + "\\Lua";
        luaState.AddSearchPath(fullPath);

        luaState.Require("test");

        LuaFunction func = luaState.GetFunction("test.report");
        if (func != null)
        {
            func.BeginPCall();
            func.PCall();
            //int num = (int)func.CheckNumber();
            func.EndPCall();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
