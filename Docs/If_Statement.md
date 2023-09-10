%%NOTTEST%%
```
if (<exp>) <embeddable-stmt>

if (<exp>) <embeddable-stmt> else <embeddable-stmt>
```

%%NOTTEST%%
```
IfStmt(Exp cond, [Stmt] body, [Stmt] elseBody)
```

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

# Reference
[Expressions](Expressions.md)
[Statements](Statements.md)