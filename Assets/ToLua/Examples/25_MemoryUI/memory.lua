-- Tencent is pleased to support the open source community by making xLua available.
-- Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
-- Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
-- http://opensource.org/licenses/MIT
-- Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

memtools = {}
memtools.shots ={}
local preLuaSnapshot = nil
local TYPE_NAME = {'GLOBAL', 'REGISTRY', 'UPVALUE', 'LOCAL'}

local function string_format_report(rp)
    local size = string.format("%-7i", rp.size)
    return string.lower(rp.name), size, TYPE_NAME[rp.type], rp.pointer, rp.used_in
end

--returns the total memory in use by Lua (in Kbytes).
local function total()
    return collectgarbage('count')
end

local function filterstr(key, text)
    local ss = memtools.shots[key]
    
    local output = {}
    local item = 1
	output[1] = {name = "NAME", size="SIZE", type="TYPE", id="ID", info = "INFO"}
    
    for i, rp in ipairs(ss) do
        if string.find(string.lower(rp.name), text) ~= nil then 
            item = item + 1
            output[item] = {}
            output[item]["name"],output[item]["size"],output[item]["type"],output[item]["id"],output[item]["info"] = string_format_report(rp)
        end
    end
    
    return output
end

local function calculation(key1, key2)
    local table1 = memtools.shots[key1]
	local table2 = memtools.shots[key2]
	local ret = {}
    
    local output = {}
    output[1] = {name = "NAME", size="SIZE", type="TYPE", id="ID", info = "INFO"}
    
    if(table1 ~= nil and table2 ~= nil) then
        local checkState = false
        local item = 1
        for k2, v2 in pairs(table2) do
            checkState = false
            for k1, v1 in pairs(table1) do 
                if v2.name == v1.name then 
                    checkState = true
                end
            end
            if checkState == false then 
                ret[item] = v2 
                item = item + 1
            end
        end
    else
        output[2] = {name = "table must not be nil !!!", size="", type="", id="", info = ""}
        return output
    end

    table.sort(ret, function(a, b) return a.size > b.size end)
    
    for i, rp in ipairs(ret) do
        output[i+1] = {}
        output[i+1]["name"],output[i+1]["size"],output[i+1]["type"],output[i+1]["id"],output[i+1]["info"] = string_format_report(rp)
    end

    return output
end

local function takesnap(key)
    local ss = perf.snapshot()
    table.sort(ss, function(a, b) return a.size > b.size end) 
	memtools.shots[key] = ss
    
    local output = {}
	output[1] = {name = "NAME", size="SIZE", type="TYPE", id="ID", info = "INFO"}
    for i, rp in ipairs(ss) do
		output[i+1] = {}
        output[i+1]["name"],output[i+1]["size"],output[i+1]["type"],output[i+1]["id"],output[i+1]["info"] = string_format_report(rp)
    end
    
    return output
end

memtools.calculation = calculation
memtools.takesnap = takesnap
memtools.total = total
memtools.filterstr = filterstr
