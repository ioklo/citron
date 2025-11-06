: Stmt, Exp

%%BEGIN_EMBED(For_Statement_Basic)%%
```cs
//@ 01234
void Main()
{
    for(int i = 0; i < 5; i++)
        @$i
}
```
%%END_EMBED%%

%%BEGIN_EMBED(For_Statement_Initializer)%%
```cs
//@ hi234
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
%%END_EMBED%%

%%BEGIN_EMBED(For_Statement_EmptyInitializer)%%
```cs
//@ 01234
void Main()
{
    int i = 0;

    for(; i < 5; i++)
        @$i
}

```
%%END_EMBED%%

%%BEGIN_EMBED(For_Statement_Scope)%%
```cs
//@ 001224364834
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
%%END_EMBED%%

%%BEGIN_EMBED(For_Statement_EmptyCond)%%
```cs
//@ 01234
void Main()
{
    for(int i = 0; ; i++)
    {
        if (5 <= i) break;
        @$i
    }
}
```
%%END_EMBED%%

%%BEGIN_EMBED(For_Statement_EmptyContinueExp)%%
```cs
//@ 01234 
void Main()
{
    for(int i = 0; i < 5;)
    {
        @$i
        i++;
    }
}
```
%%END_EMBED%%

%%BEGIN_EMBED(For_Statement_EmptyAll)%%
```cs
//@ 01234
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
%%END_EMBED%%