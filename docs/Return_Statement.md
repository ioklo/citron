%%TEST(ControlFlow, F)%%
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

%%TEST(ReturnValue, F6)%%
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

%%TEST(LambdaReturn, 3)%%
```cs
void Main()
{
    var f = () => {
        return 3;
    };

    @${f()}
}
```

%%TEST(SeqReturn, 01234)%%
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