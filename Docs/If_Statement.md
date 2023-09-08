Exp, Stmt

%%TEST(Basic, good)%%
```cs
void Main()
{
    if (1 < 2) @good

    if (1 > 2)
    { 
        @bad
    }
}
```

%%TEST(BasicElse, pass)%%
```cs
void Main()
{
    if (2 < 1) { }
    else @{pass}
}
```

%%TEST(NestedIf, completed)%%
```cs
void Main()
{
    if (false)
        if (true) {}
        else @wrong

    @completed
}
```