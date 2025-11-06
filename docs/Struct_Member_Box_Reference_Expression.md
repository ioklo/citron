%%NOTTEST%%
```
StructMemberBoxRefExp(Exp holder, StructMemberVarSymbol symbol)
```


%%TEST(Basic, 5)%%
```
struct A { int i; }
struct S { A a; }

void Main()
{
	box var* s = box S(A(3));
	box var* x = &s->a.i; // StructMemberBoxRefExp(StructIndirectMemberBoxRefExp(s, S.a), S.i)
	*x = 5;

	@${s->a.i}
}

```

# Reference
[Expressions](Expressions.md)
[Struct_Member_Variable](Struct_Member_Variable.md)