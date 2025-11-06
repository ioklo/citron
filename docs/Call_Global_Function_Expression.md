%%NOTTEST%%
```
id(exp, params exp)
```

%%NOTTEST%%
```
CallGlobalFuncExp(GlobalFuncSymbol symbol, [Argument] args)
```

%%TEST(General, 1 2 false)%%
```cs
void F(int i, string s, bool b)
{    
    @$i $s $b
}

void Main()
{
	F(1, "2", false);
}
```

%%TEST(Recursive, 345)%%
```cs
void F(int i, int end)
{    
    if (end <= i) return;

    @$i
    F(i + 1, end);
}

void Main()
{
	F(3, 6);
}
```

%%TEST(Generator, 123)%%
```cs
seq int Func()
{
    yield 1;
    yield 2;
    yield 3;
}

void Main()
{
    foreach(var i in Func())
        @$i
}
```

# Reference
[Global_Function](Global_Function.md)
[Argument](Argument.md)