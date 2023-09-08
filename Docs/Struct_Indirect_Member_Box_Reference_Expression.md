%%NOTTEST%%
```
StructIndirectMemberBoxRefExp(Exp holder, StructMemberVarSymbol symbol)
```

%%TEST(Basic, 2)%%
```
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

# Reference
[Expressions](Expressions.md)
[Struct_Member_Variable](Struct_Member_Variable.md)