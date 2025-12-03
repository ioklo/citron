%%NOTTEST%%
```
<struct-id>(<argument>, ...)
```

%%NOTTEST%%
```
NewStructExp(StructConstructorSymbol symbol, [Argument] args)
```

%%BEGIN_EMBED(New_Struct_Expression_Basic)%%
```cs
//@ 3
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
%%END_EMBED%%


# Reference
[Struct_Constructor](Struct_Constructor.md)
[Argument](Argument.md)