%%TEST(Basic, 1234)%%
```cs
void Main()
{
    foreach(var e in [1, 2, 3, 4])
        @$e
}
```

%%TEST(WithSeqFunc,  HelloWorld)%%
```cs
void Main()
{
    seq string F()
    {
        yield "Hello";
        yield "World";
    }

    foreach(var e in F())
        @$e
}
```

%%TEST(Scope, 7)%%
```cs
void Main()
{
    int i = 7;

    foreach(var i in [1, 2, 3, 4]);

    @$i
}
```

%%TEST(LambdaAsItem, 12343)%%
```cs
void Main()
{
    list<func<void>> fs = [];

    foreach(var x in [1, 2, 3, 4])
        fs.Add(() => @{$x});

    int a = 1, b = 2;
    fs.Add(() => ${a + b}); // a, b두개가 캡쳐되서 들어간다

    foreach(var f in fs)
        f();
}
```

