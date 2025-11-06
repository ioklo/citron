%%TEST(For, 135)%%
```cs
void Main()
{
    for (int i = 0; i < 6; i++)
    {
        if (i % 2 == 0) continue;
        @$i
    }
}
```

%%TEST(Foreach, 711)%%
```cs
void Main()
{
    foreach (int e in [6, 7, 1, 1, 4])
    {
        if (e % 2 == 0) continue;
        @$e
    }
}
```

%%TEST(NestedFor, 711711)%%
```cs
void Main()
{
    for(int i = 0; i < 2; i++)
    {
        foreach (int i in [6, 7, 1, 1, 4])
        {
            if (i % 2 == 0) continue;
            @$i
        }
    }
}
```