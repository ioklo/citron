```
<exp>(<argument>, ...)
```

```
CallClassMemberFuncExp(ClassMemberFuncSymbol symbol, Loc? instance, [Argument] args)
```

<!--BEGIN_EMBED(Call_Class_Member_Function_Expression_Instance)-->
```cs
//@ 2 4
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
<!--END_EMBED-->

<!--BEGIN_EMBED(Call_Class_Member_Function_Expression_Static)-->
```cs
//@ X: 3
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
<!--END_EMBED-->

# Referece
[Class_Member_Function](Class_Member_Function.md)
[Locations](Locations.md)
[Argument](Argument.md)