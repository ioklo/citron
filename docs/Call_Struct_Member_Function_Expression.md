%%NOTTEST%%
```
<exp>(<argument>, ...)
```

%%NOTTEST%%
```
CallStructMemberFuncExp(StructMemberFuncSymbol symbol, Loc? instance, [Argument] args)
```

%%TEST(Instance, hello)%%
```cs
struct S
{
	string s;
	void Print()
	{
		@$s
	}
}

void Main()
{
	var s = S("hello");
	s.Print();
}
```

%%TEST(Static, hello)%%
```cs
struct S
{
	static void Print()
	{
		@hello
	}
}
```

# Reference
[Struct_Member_Function](Struct_Member_Function.md)
[Locations](Locations.md)
[Argument](Argument.md)