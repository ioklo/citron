%%NOTTEST%%
```
<exp> is <class-type-exp>
```

%%NOTTEST%%
```
ClassIsClassExp(Exp exp, ClassSymbol symbol)
```

%%BEGIN_EMBED(Class_Is_Class_Expression_Basic)%%
```cs
class B { }
class C : B { }

void Main()
{
	var b = new C();
	
	var t0 = b is B;
	var t1 = b is C;

	@$t0 $t1
}
```
%%END_EMBED%%

%%BEGIN_EMBED(Class_Is_Class_Expression_NotRelated)%%
```cs
class C { }
class D { }

void Main()
{
	var c = new C();
	var t = c is D;
	@$t
}
```
%%END_EMBED%%