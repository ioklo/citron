```
CastBoxedLambdaToFuncExp(Exp exp, FuncType funcType)
```

<!--BEGIN_EMBED(Cast_Boxed_Lambda_To_Func_Expression_Basic)-->
```cs
//@ 3
void Main()
{
	var bf = box () => { return 3; }
	func<int> f = bf; // CastBoxedLambdaToFuncExp(LoadExp(LocalVarLoc("bf")), func<int>)

	@${f()}
}
```
<!--END_EMBED-->