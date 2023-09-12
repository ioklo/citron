box pointer를 local pointer로 변환하려면, box pointer 값이 local pointer의 생명 주기 동안 계속 살아있어야 한다는 보장이 있어야 합니다.
그래서 캐스팅은 일반적인 경우에 사용할 수는 없고, 함수 호출시 인자 변환의 형태로만 사용할 수 있습니다.

%%TEST(NotFixed, $Error)%%
```cs
void Main()
{
    int* s = box 3; // 에러, box가 고정되지 못함
    // 가비지 컬렉터가 돌면서 'box 3'를 힙에서 제거
    @${*s}
}
```

%%TEST(AsArgument, )%%
```cs
void F(int *s)
{
	*s = 3;
}

void Main()
{
	F(box 3); // box 3의 결과는 temporary variable에 저장되고, statement가 끝날때 사라집니다.
}

```