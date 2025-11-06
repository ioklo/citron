%%NOTTEST%%
```
<exp> is <interface-type-exp>
```

%%NOTTEST%%
```
ClassIsInterfaceExp(Exp exp, InterfaceSymbol symbol)
```

%%BEGIN_EMBED(Class_Is_Interface_Expression_Basic)%%
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
%%END_EMBED%%

%%BEGIN_EMBED(Class_Is_Interface_Expression_NotRelated)%%
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
%%END_EMBED%%