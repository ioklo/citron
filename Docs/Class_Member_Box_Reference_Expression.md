%%NOTTEST%%
```
ClassMemberBoxRefExp(Loc holder, ClassMemberVarSymbol symbol)
```

%%TEST(Basic, 4)%%
```cs
class C
{
	int x;
	
	box int* GetX()
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
	box var* pX = c.GetX();
	*pX = 4;
	
	c.PrintX();
}

```




# Reference
[Locations](Locations.md)
[Class_Member_Variable](Class_Member_Variable.md)