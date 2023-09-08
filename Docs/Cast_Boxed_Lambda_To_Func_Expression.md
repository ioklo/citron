%%NOTTEST%%
```
CastBoxedLambdaToFuncExp(Exp exp, FuncType funcType)
```

%%TEST(Basic, 3)%%
```
void Main()
{
	var bf = box () => { return 3; }
	func<int> f = bf; // CastBoxedLambdaToFuncExp(LoadExp(LocalVarLoc("bf")), func<int>)

	@${f()}
}
```