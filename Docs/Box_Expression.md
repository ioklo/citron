%%NOTTEST%%
```
BoxExp(Exp innerExp)
```

BoxExp는 
- innerExp를 계산한 값이 들어갈 크기의 공간을 힙에 할당합니다
- 실행환경의 result가 할당한 공간을 가리키도록 합니다
- innerExp를 계산합니다
- 실행환경의 result를 원래대로 돌려놓습니다
- 실행환경의 result에 힙에 할당한 위치를 저장한 box 포인터 값을 저장합니다

%%TEST(Basic, 5)%%
```cs
void Main()
{
	@${*(box 5)}
}
```


# Reference
[Expressions](Expressions.md)