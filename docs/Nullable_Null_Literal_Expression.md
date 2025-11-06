hint type이 nullable 타입일때, `null`은 해당 nullable 타입의 null값이 된다
%%NOTTEST%%
```
null
```

%%NOTTEST%%
```
NullableNullLiteralExp(Type innerType)
```

%%TEST(Basic, )%%
```cs
void Main()
{
	int? i = null;
}
```

%%TEST(CantInferType, $Error)%%
```cs
void Main()
{
	var? i = null;
}
```

# Reference

[Types](Types.md)