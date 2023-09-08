%%NOTTEST%%
```
[0-9]+ // 단 범위를 넘어가지 않는 선에서
```
환경의 result에 int값을 넣습니다

%%TEST(Basic, 123456)%%
```
void Main()
{
	@${123456}
}
```

%%TEST(OverTheLimit, $Error)%%
```
void Main()
{
	@${12345678901234567890123456789012345678901234567890}
}
```

# Reference
[Integer](Integer.md)