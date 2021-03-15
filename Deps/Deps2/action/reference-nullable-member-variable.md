# nullable 멤버 변수 참조

```csharp
nullable<int> i = 3;

if (i is null)
    return;

if (i is int s)
    s = 7; // i는 값이 변할까 안변할까
```

