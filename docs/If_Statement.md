%%NOTTEST%%
```
if (<exp>) <embeddable-stmt>

if (<exp>) <embeddable-stmt> else <embeddable-stmt>
```

%%NOTTEST%%
```
IfStmt(Exp cond, [Stmt] body, [Stmt] elseBody)
```

%%BEGIN_EMBED(If_Statement_Basic)%%
```cs
//@ good
void Main()
{
    if (1 < 2) @good

    if (1 > 2)
    { 
        @bad
    }
}
```
%%END_EMBED%%

%%BEGIN_EMBED(If_Statement_BasicElse)%%
```cs
//@ pass
void Main()
{
    if (2 < 1) { }
    else @{pass}
}
```
%%END_EMBED%%

%%BEGIN_EMBED(If_Statement_NestedIf)%%
```cs
//@ completed
void Main()
{
    if (false)
        if (true) {}
        else @wrong

    @completed
}
```
%%END_EMBED%%

# Reference
[Expressions](Expressions.md)
[Statements](Statements.md)