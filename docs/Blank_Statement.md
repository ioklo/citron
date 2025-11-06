%%TEST(For, 01234)%%
```cs
int Add(int i)
{
    @$i
    return i + 1;
}

void Main()
{
    for(int i = 0; i < 5; i = Add(i));
}
```

%%TEST(Foreach, helloworld1)%%
```cs
seq string F()
{
    @hello
    yield "1";
    @world
    yield "2";
    @1
}

void Main()
{
    foreach(var i in F());
}

```