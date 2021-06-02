// 1234

var fs = new List<(void => void)>();

foreach(var x in [1, 2, 3, 4])
    fs.Add(() => @{$x});

foreach(var f in fs)
    f();