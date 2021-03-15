# nullable 값으로 분기하기

```csharp
void Func(nullable<int> i)
{
    if (i is null) // i == null은 지원 안할 예정이다
    {

    }

    if (i is int v) // i값이 v에 복사되지 않는다. if(i is var v) 해도 된다
    {
        print(v);
    }
}
```