%%NOTTEST%%
```
LoadExp(Loc source)
```
Location을 입력으로 받아서 그 위치의 Value를 환경의 result에 저장합니다.

s-expression은 r-location 또는 r-expression으로 해석됩니다. 만약 r-location으로 해석된 s-expression을 r-expression이 필요로 하는 위치에서 사용할 경우 load명령어가 동반됩니다.

%%TEST(LoadLocalVar, 1)%%
```cs
void Main()
{
	int a = 1;

	// a는 Local_Variable_Location이고, b에 대입하기 위해 내부적으로 load expression을 사용하게 됩니다
	// local_var_decl_stmt("b", load_exp(local_var_loc("a")))
	int b = a;

	@$b
}
```

%%모든 Location에 대해서 테스트를 진행할지 생각해 보아야 합니다%%

# Reference
[Locations](Locations.md)