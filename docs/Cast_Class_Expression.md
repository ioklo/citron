대응하는 syntax 없음. static cast.

%%NOTTEST%%
```
CastClassExp(Exp source, ClassSymbol symbol)
```

%%TEST(Upcast, )%%
```cs
class B { }
class C : B { }

void Main()
{
	var c = new C(); 
	B b = c; // CastClassExp(NewClassExp(C, []), B)
}

```

# Reference
[Expressions](Expressions.md)
[Class](Class.md)