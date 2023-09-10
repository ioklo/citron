%%NOTTEST%%
```
<struct-id>(<argument>, ...)
```

%%NOTTEST%%
```
NewStructExp(StructConstructorSymbol symbol, [Argument] args)
```

%%TEST(Basic, 3)%%
```cs
struct S
{
	int x;
}

void Main()
{
	var s = S(3);
	@${s.x}
}
```


# Reference
[Struct_Constructor](Struct_Constructor.md)
[Argument](Argument.md)