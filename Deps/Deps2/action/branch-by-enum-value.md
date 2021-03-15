# 열거형 항목으로 분기하기

```csharp
enum E
{
    First,
    Second(int num);
}

void Func(E e)
{
    if (e is Second s) // 복사가 일어나지 않는다
        print(s.num);
}
```

