hint type이 nullable 타입일때, `null`은 해당 nullable 타입의 null값이 된다
```
null
```

```
NullableNullLiteralExp(Type innerType)
```

<!--BEGIN_EMBED(Nullable_Null_Literal_Expression_Basic)-->
```cs
//@ 
void Main()
{
	int? i = null;
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Nullable_Null_Literal_Expression_CantInferType)-->
```cs
//@ $Error
void Main()
{
	var? i = null;
}
```
<!--END_EMBED-->

# Reference

[Types](Types.md)