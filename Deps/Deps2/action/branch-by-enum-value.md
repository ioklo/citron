# 열거형 항목으로 분기하기

```csharp
enum E
{
    First,
    Second(int num);
}

void Func(E e)
{
    // 1. 기본형
    if (var s = e as E.Second) // 복사가 일어나지 않는다
        print(s.num);

    // 축약형
    if (e is E.Second)
        print (e.num);
}
```

