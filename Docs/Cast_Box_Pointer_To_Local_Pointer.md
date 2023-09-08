
%%TEST(NotFixed, $Error)%%
```cs
void Main()
{
    int* s = box 3; // 에러, box가 고정되지 못함
    @${*s}
}
```

%%TEST(Fixed, 3)%%
```cs
void Main()
{
	box var* b = box 3;
	fixed(b)
	{
		int* s = b; // 가능, fixed
		@${*s}
	}
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