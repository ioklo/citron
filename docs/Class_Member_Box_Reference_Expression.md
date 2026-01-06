```
ClassMemberBoxRefExp(Loc holder, ClassMemberVarSymbol symbol)
```

<!--BEGIN_EMBED(Class_Member_Box_Reference_Expression_Basic)-->
```cs
//@ 4
class C
{
	int x;
	
	shared int GetX()
	{
		return &x; // ClassMemberBoxExp(this, C.x)
	}

	void PrintX()
	{
		@$x
	}
}

void Main()
{
	var c = new C(3);
	shared var pX = c.GetX();
	*pX = 4;
	
	c.PrintX();
}

```
<!--END_EMBED-->




# Reference
[Locations](Locations.md)
[Class_Member_Variable](Class_Member_Variable.md)