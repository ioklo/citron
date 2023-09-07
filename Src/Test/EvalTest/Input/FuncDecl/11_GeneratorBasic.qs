// 123

seq int Func()
{
    yield 1;
    yield 2;
    yield 3;
    return;
}

void Main()
{
    foreach(var i in Func())
        @$i
}