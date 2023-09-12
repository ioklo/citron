%%NOTTEST%%
```
if (<type-exp> <id> = <exp>) <embeddable-stmt> else <embeddable-stmt>
```

%%NOTTEST%%
```
IfNullableValueTestStmt(Type type, Name varName, Exp castExp, [Stmt] body, [Stmt] elseBody)
```

%%TEST(EnumElem, 2)
```cs
enum E { First, Second(int i) }

void Main()
{
	E e = E.Second(2);

	if (E.First ef = e)
	{
		@wrong
	} else if (E.Second es = e)
	{
		@${es.x}
	}
}

```


# Reference
[Locations](Locations.md)
[Enum_Element](Enum_Element.md)
[Statements](Statements.md)