: Stmt, Exp

%%TEST(Basic, 01234)%%
```cs
void Main()
{
    for(int i = 0; i < 5; i++)
        @$i
}
```

%%TEST(Initializer, hi234)%%
```

void F()
{
    @hi
}

void Main()
{
    int i = 2;
    for(F(); i < 5; i++)
        @$i
}
```

%%TEST(EmptyInitializer, 01234)%%
```cs
void Main()
{
    int i = 0;

    for(; i < 5; i++)
        @$i
}

```

%%TEST(Scope, 001224364834)%%
```cs
void Main()
{
    int i = 3, j = 4;

    for(int i = 0; i < 5; i++)
    {
        int j = i * 2;
        @$i$j
    }

    @$i$j
}
```

%%TEST(EmptyCond, 01234)%%
```cs
void Main()
{
    for(int i = 0; ; i++)
    {
        if (5 <= i) break;
        @$i
    }
}
```

%%TEST(EmptyContinueExp, 01234 )%%
```cs
void Main()
{
    for(int i = 0; i < 5;)
    {
        @$i
        i++;
    }
}
```

%%TEST(EmptyAll, 01234)%%
```cs
void Main()
{
    int i = 0;
    for(;;)
    {
        if (5 <= i) break;
        @$i
        i++;
    }
}
```