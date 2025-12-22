```
<enum-id>.<enum> // standalone
<enum-id>.<enum-element-name>(<argument>, ...) // with arguments
```

```
NewEnumElemExp(EnumElemSymbol symbol, [Argument] args)
```

<!--BEGIN_EMBED(New_Enum_Element_Expression_Basic)-->
```cs
//@ 
enum E { First, Second(int i) }
void Main()
{
	var e = E.First;
	e = E.Second(2);
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(New_Enum_Element_Expression_Shorthand)-->
```cs
//@  
enum E { First, Second(int i) }
void Main()
{
	E e = .First;
	e = .Second(2);
}
```
<!--END_EMBED-->

# Reference
[Enum_Element](Enum_Element.md)
[Argument](Argument.md)