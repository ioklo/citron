```
StructMemberBoxRefExp(Exp holder, StructMemberVarSymbol symbol)
```


<!--BEGIN_EMBED(Struct_Member_Box_Reference_Expression_Basic)-->
```cs
//@ 5
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
<!--END_EMBED-->

# Reference
[Expressions](Expressions.md)
[Struct_Member_Variable](Struct_Member_Variable.md)