```
SharedExp(MCreate innerExp)
```

SharedExp는 
- innerExp를 계산한 값이 들어갈 크기의 공간을 힙에 할당하고 innerExp의 결과물을 직접 그 공간에 만듭니다

<!--BEGIN_EMBED(Shared_Expression_Basic)-->
```cs
//@ 5
void Main()
{
	@${*(shared 5)}
}
```
<!--END_EMBED-->


# Reference
[Expressions](Expressions.md)