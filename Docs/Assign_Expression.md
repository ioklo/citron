%%NOTTEST%%
```
AssignExp(Loc target, Exp source)
```

assign_expression은 
- target이 가리키는 곳을 먼저 계산해서 알아내고
- 환경의 result를 target이 가리키는 곳으로 설정한 후
- source를 계산합니다. 
- 환경의 result를 원래 위치로 다시 돌려놓습니다
- 환경의 result에 계산된 값을 저장합니다

%%TEST(Basic, 10)%%
```
void Main()
{
	int a = 0;

	a = 10;
	@$a
}
```

문법적으로 assignment는 중첩될 수 있습니다. 한 식에 =가 여러번 나타날때, 왼쪽의 = 를 기준으로 오른쪽 전체를 먼저 계산하게 됩니다. 고로 `a = b = 1`은 `a = (b = 1)`과 같습니다. `b = 1`의 결괏값은 1이므로 a에도 1이 대입됩니다.

%%TEST(Nested, 1 1 1)%%
```
void Main()
{
	int a = 0;
	int b = 0;
	int c = 0;

	a = b = c = 1;

	@$a $b $c
}
```

# Reference
[Locations](Locations.md)
[Expressions](Expressions.md)