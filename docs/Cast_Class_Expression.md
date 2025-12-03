대응하는 syntax 없음. static cast.

%%NOTTEST%%
```
CastClassExp(Exp source, ClassSymbol symbol)
```

%%BEGIN_EMBED(Cast_Class_Expression_Upcast)%%
```cs
//@ 
class B { }
class C : B { }

void Main()
{
	var c = new C(); 
	B b = c; // CastClassExp(NewClassExp(C, []), B)
}

```
%%END_EMBED%%

# Reference
[Expressions](Expressions.md)
[Class](Class.md)