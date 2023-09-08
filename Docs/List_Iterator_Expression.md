No corresponding s-expression

%%NOTTEST%%
```
ListIterExp(Loc list)
```

%%TEST(Basic, 123)%%
```
var l = [1, 2, 3]
foreach(var i in l) // ListIterExp(LocalVarLoc("l"))
{
	@$i
}
```

# Reference
[Locations](Locations.md)