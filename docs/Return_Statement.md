%%BEGIN_EMBED(Return_Statement_ControlFlow)%%
```cs
void F()
{    
    @F

    return;

    @wrong
}

void Main()
{
    F();
}

```
%%END_EMBED%%

%%BEGIN_EMBED(Return_Statement_ReturnValue)%%
```cs
int F(int i)
{    
    @F

    return i * 2;

    @wrong
}

void Main()
{
    @${F(3)}
}
```
%%END_EMBED%%

%%BEGIN_EMBED(Return_Statement_LambdaReturn)%%
```cs
void Main()
{
    var f = () => {
        return 3;
    };

    @${f()}
}
```
%%END_EMBED%%

%%BEGIN_EMBED(Return_Statement_SeqReturn)%%
```cs
seq int F()
{
    for(int i = 0; i < 10; i++)
    {
        yield i;
        if (i == 4) return;
    }
}

void Main()
{
    foreach(var e in F())
    {
        @$e
    }
}
```
%%END_EMBED%%