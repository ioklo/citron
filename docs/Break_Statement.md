%%BEGIN_EMBED(Break_Statement_For)%%
```cs
//@ 123end
void Main()
{
    for (int i = 1; i < 6; i++)
    {
        @$i
        if (i % 3 == 0) break;
    }

    @end
}
```
%%END_EMBED%%

%%BEGIN_EMBED(Break_Statement_Foreach)%%
```cs
//@ 67end
void Main()
{
    foreach (int e in [6, 7, 1, 1, 4])
    {
        @$e
        if (e % 2 == 1) break;
    }

    @end
}
```
%%END_EMBED%%

%%BEGIN_EMBED(Break_Statement_NestedFor)%%
```cs
//@ 6767
void Main()
{
    for(int i = 0; i < 2; i++)
    {
        foreach (int i in [6, 7, 1, 1, 4])
        {
            @$i
            if (i % 2 == 1) break;
        }
    }
}
```
%%END_EMBED%%