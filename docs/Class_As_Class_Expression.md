%%NOTTEST%%
```
<exp> as <class-type-exp>
```

%%NOTTEST%%
```
ClassAsClassExp(Exp exp, ClassSymbol symbol)
```

%%BEGIN_EMBED(Class_As_Class_Expression_Basic)%%
```cs
//@ 2
class B { }
class C : B { public int x; }

void Main()
{
	var b = new C(2);
	
	var c = b is C; // c는 nullable C 타입
	if (c != null)	
		@${c.x}
}
```
%%END_EMBED%%

%%BEGIN_EMBED(Class_As_Class_Expression_NotRelated)%%
```cs
//@ ok
class C { }
class D { }

void Main()
{
	var c = new C();
	var d = c as D;
	
	if (d == null)
		@ok
}
```
%%END_EMBED%%