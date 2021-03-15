# 열거형 정의

```csharp
enum E<T>  // 타입 변수
{
    First,         // 인자가 없는 Element, Constructor 역할도 한다.
    Second(T s), // 인자가 있는 Element
    Third(bool b)
}

enum E2 : E<int> // E의 모든 Element를 그대로 가져온다
{
    Fourth,
    Fifth(int num),
    Sixth(string p, int x)
}
```