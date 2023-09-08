%%TEST(For, 123end)%%
```cs
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

%%TEST(Foreach, 67end)%%
```cs
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

%%TEST(NestedFor, 6767)%%
```cs
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