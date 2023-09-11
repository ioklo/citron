%%NOTTEST%%
```
<enum-id>.<enum> // standalone
<enum-id>.<enum-element-name>(<argument>, ...) // with arguments
```

%%NOTTEST%%
```
NewEnumElemExp(EnumElemSymbol symbol, [Argument] args)
```

%%TEST(Basic, )%%
```
enum E { First, Second(int i) }
void Main()
{
	var e = E.First;
	e = E.Second(2);
}
```

%%TEST(Shorthand,  )%%
```
enum E { First, Second(int i) }
void Main()
{
	E e = .First;
	e = .Second(2);
}
```

# Reference
[Enum_Element](Enum_Element.md)
[Argument](Argument.md)