No corresponding s-expression

%%NOTTEST%%
```
ListIterExp(Loc list)
```

%%BEGIN_EMBED(List_Iterator_Expression_Basic)%%
```cs
//@ 123
var l = [1, 2, 3]
foreach(var i in l) // ListIterExp(LocalVarLoc("l"))
{
	@$i
}
```
%%END_EMBED%%

# Reference
[Locations](Locations.md)