--主入口函数。从这里开始lua逻辑
function Main()					
	-- local snapshot = require "snapshot"
	-- local S1 = snapshot()

	-- local tmp = {}

	-- local S2 = snapshot()
	-- print("[compare]:")
	-- for k,v in pairs(S2) do
		-- if S1[k] == nil then
			-- print(k,v)
		-- end
	-- end 	
	-- local memory = require 'perf.memory'
	-- print('total memory:', memory.total())
	-- print(memory.snapshot())
	
	-- local profiler = require 'perf.profiler'
	-- profiler.start()
	-- print('hello world')
	-- print(profiler.report())
	-- print('hello Earth')
	-- print(profiler.report())
	-- profiler.stop()
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end