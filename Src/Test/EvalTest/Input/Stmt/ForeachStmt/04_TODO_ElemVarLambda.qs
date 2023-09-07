// 1234

void Main()
{
    var fs = new list<func<void>>();

    foreach(var x in [1, 2, 3, 4])
        fs.Add(() => @{$x});

    foreach(var f in fs)
        f();
}