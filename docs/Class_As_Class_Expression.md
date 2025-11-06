%%NOTTEST%%
```
<exp> as <class-type-exp>
```

%%NOTTEST%%
```
ClassAsClassExp(Exp exp, ClassSymbol symbol)
```

%%TEST(Basic, 2)%%
```cs
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

%%TEST(NotRelated, ok)%%
```cs
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