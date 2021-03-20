# nullable 값으로 분기하기

```csharp
void Func(nullable<int> i)
{
    // 1. 기본형
    if (int v = i as int) // i값이 v에 복사되지 않는다
    {
        print(v);
    }

    // 2. 축약형
    if (i is null) // i == null은 지원 안할 예정이다
    {

    }

    // 3. not null 축약형
    if (i is not null)
    {

    }

    // 4. 그냥 타입을 써주던가
    if (i is int)
    {

    }
    
}
```