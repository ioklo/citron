```
[0-9]+ // 단 범위를 넘어가지 않는 선에서
```
환경의 result에 int값을 넣습니다

<!--BEGIN_EMBED(Integer_Literal_Expression_Basic)-->
```cs
//@ 123456
void Main()
{
	@${123456}
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Integer_Literal_Expression_OverTheLimit)-->
```cs
//@ $Error
void Main()
{
	@${12345678901234567890123456789012345678901234567890}
}
```
<!--END_EMBED-->

# Reference
[Integer](Integer.md)