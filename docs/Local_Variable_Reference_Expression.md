```
LocalVarRefExp(Loc inner)
```

<!--BEGIN_EMBED(Local_Variable_Reference_Expression_Basic)-->
```cs
//@ 3
void Main()
{
    int s = 3;
    var& i = s;

    @{${*i}}
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Local_Variable_Reference_Expression_Nested)-->
```cs
//@ 4
void Main()
{
	int s = 3;
	int& i = s;  
	int& j = i;
	j = 4;
}
@$s
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Local_Variable_Reference_Expression_Uninitialized)-->
```cs
//@ $Error
void Main()
{
	int i;       // uninitialized
	int& p = i; // 에러, uninitialized는 포인터로 가리킬 수 없습니다
	@{$p} 
}
```
<!--END_EMBED-->

# Reference
[Locations](Locations.md)