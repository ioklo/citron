# 열거형 값 생성

```csharp
enum E
{
    First,
    Second(int x)
}

var e1 = E.First;     // 1. 인자 없는 열거형 생성자
var e2 = E.Second(2); // 2. 인자 있는 열거형 생성자

E e3 = First;         // 3. 타입 힌트
E e4 = Second(2);     // 4. 타입 힌트
```

