# 익명 seq 값 생성

```csharp
seq int F()
{
    yield 3;
    yield 4;
}

// l은 익명 seq 타입이다
var l = F();

```