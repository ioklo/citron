%%NOTTEST%%
```
LocalVarRefExp(Loc inner)
```

%%TEST(Basic, 3)%%
```cs
void Main()
{
    int s = 3;
    var* i = &s; // LocalVarRefExp(LocalVar("i"))

    @{${*i}}
}
```

%%TEST(Nested, 4)%%
```cs
void Main()
{
	int s = 3;
	int* i = &s;  
	int** j = &i;
	**j = 4;
}
@$s
```

%%TEST(Uninitialized, $Error)%%
```cs
void Main()
{
	int i;       // uninitialized
	int* p = &i; // 에러, uninitialized는 포인터로 가리킬 수 없습니다
	@{$p} 
}
```

# Reference
[Locations](Locations.md)