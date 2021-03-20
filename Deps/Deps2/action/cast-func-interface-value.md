# func<> 인터페이스 캐스팅

```csharp

// 1. func_throw<>로 캐스팅
void F(func<int, string> f)
{
    func_throw<int, string, E> fE = f; // 아무것도 던지지 않으므로 안전
}

```

