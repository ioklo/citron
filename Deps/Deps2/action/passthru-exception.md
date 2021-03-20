# 예외 통과시키기

```csharp
enum E { NotImplemented }

void InnerFunc() throw E
{
    throw NotImplemented;
}

// E를 제외한 모든 익셉션은 이 함수에서 처리되어야 한다
void Func() throw E
{
    InnerFunc(); // E타입 익셉션은 catch로 잡지 않으면, 바로 던진다
}

```

