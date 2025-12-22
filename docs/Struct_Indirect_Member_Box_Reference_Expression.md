```
StructIndirectMemberBoxRefExp(Exp holder, StructMemberVarSymbol symbol)
```

<!--BEGIN_EMBED(Struct_Indirect_Member_Box_Reference_Expression_Basic)-->
```cs
//@ 2
struct S
{
	int i;
}

void Main()
{
	box S* bs = new S(3);
	box int* x = &bs->i; // StructIndirectMemberBoxRefExp(bs, S.i)

	*x = 2;

	@${bs->i}
}
```
<!--END_EMBED-->

# Reference
[Expressions](Expressions.md)
[Struct_Member_Variable](Struct_Member_Variable.md)