%%NOTTEST%%
```
<exp>(<argument>, ...)
```

%%NOTTEST%%
```
CallStructMemberFuncExp(StructMemberFuncSymbol symbol, Loc? instance, [Argument] args)
```

%%BEGIN_EMBED(Call_Struct_Member_Function_Expression_Instance)%%
```cs
//@ hello
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
%%END_EMBED%%

%%BEGIN_EMBED(Call_Struct_Member_Function_Expression_Static)%%
```cs
//@ hello
struct S
{
	static void Print()
	{
		@hello
	}
}
```
%%END_EMBED%%

# Reference
[Struct_Member_Function](Struct_Member_Function.md)
[Locations](Locations.md)
[Argument](Argument.md)