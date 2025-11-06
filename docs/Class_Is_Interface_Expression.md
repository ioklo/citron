%%NOTTEST%%
```
<exp> is <interface-type-exp>
```

%%NOTTEST%%
```
ClassIsInterfaceExp(Exp exp, InterfaceSymbol symbol)
```

%%TEST(Basic, true)%%
```cs
interface I { }

class B { }
class C : I { }

void Main()
{
	var b = new C();
	var t = b is I;

	@$t
}
```

%%TEST(NotRelated, false)%%
```cs
interface I { }
class C { }

void Main()
{
	var c = new C();
	var t = c is I;
	@$t
}
```