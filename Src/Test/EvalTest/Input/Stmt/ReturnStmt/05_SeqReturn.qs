// 01234

seq int F()
{
    for(int i = 0; i < 10; i++)
    {
        yield i;
        if (i == 4) return;
    }
}

foreach(var e in F())
{
    @$e
}