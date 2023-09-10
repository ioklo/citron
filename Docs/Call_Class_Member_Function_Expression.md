%%NOTTEST%%
```
<exp>(<argument>, ...)
```

%%NOTTEST%%
```
CallClassMemberFuncExp(ClassMemberFuncSymbol symbol, Loc? instance, [Argument] args)
```

%%TEST(Instance, 2 4)%%
```cs
class X
{
    int x;
    
	public void F(int i)
    {
        @$x $i
    }
}

void Main()
{
	X x = new X(2);
    x.F(4);
}
```

%%TEST(Static, X: 3)%%
```cs
class X
{
	public static void Print(int a)
	{
		@X: $a
	}
}

void Main()
{
	X.Print(3);
}
```

# Referece
[Class_Member_Function](Class_Member_Function.md)
[Locations](Locations.md)
[Argument](Argument.md)