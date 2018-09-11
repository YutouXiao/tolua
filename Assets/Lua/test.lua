test = {}
function report()
    local profiler = require 'perf.profiler'
    profiler.start()
	print("Hello World! ")
	print(profiler.report())
	profiler.stop()
end

test.report = report