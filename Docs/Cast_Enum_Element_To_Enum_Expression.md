대응하는 syntax 없음 

%%NOTTESt%%
```
CastEnumElemToEnumExp(Exp source, EnumSymbol symbol)
```

%%TEST(Basic, )%%
```cs
enum E { First, Second(int i) }

void Main()
{
	E.Second s = E.Second(2);
	E e = s; // CastEnumElemToEnumExp(LoadExp(LocalVarLoc(s)), E)
}

```

# Reference
[Expressions](Expressions.md)
[Enum](Enum.md)