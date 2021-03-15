# foreach를 사용한 루프

```csharp
var l = Enumerate();

// l은 seq<> constraint를 가진 값
foreach(var e in l) // 매 이터레이션 마다 l.Next() constraint instance call을 한다
{
    // e는 값 복사를 하지 않는다
}
```
