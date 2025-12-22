대응하는 syntax 없음 

```
CastEnumElemToEnumExp(Exp source, EnumSymbol symbol)
```

<!--BEGIN_EMBED(Cast_Enum_Element_To_Enum_Expression_Basic)-->
```cs
//@ 
enum E { First, Second(int i) }

void Main()
{
	E.Second s = E.Second(2);
	E e = s; // CastEnumElemToEnumExp(LoadExp(LocalVarLoc(s)), E)
}

```
<!--END_EMBED-->

# Reference
[Expressions](Expressions.md)
[Enum](Enum.md)