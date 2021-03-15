# constraint로부터 정적 함수 호출

```csharp
interface I
{
    static void StaticFunc(int i);
}

void Func<T>() where T : I
{
    T.StaticFunc();
}
```

