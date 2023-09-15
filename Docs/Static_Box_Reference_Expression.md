%%NOTTEST%%
```
StaticBoxRefExp(Loc loc)
```
struct나 class의 static member variable은 reference시 힌트에 따라서 box pointer가 될 수도, local pointer가 될 수 도 있습니다.
static member variable의 위치를 box pointer로 저장할 필요가 있을 때, 이 컴파일러는 이 expression을 사용합니다.

%%TEST(Basic, 3)%%
```cs
struct C
{
	static int x;
	static C()
	{
		x = 3;
	}
}

void Main()
{
	box var* s = &C.x; // reference operator에 box pointer로 만들어달라는 요청을 줍니다
	@${*s}
}
```



# Reference
[Locations](Locations.md)