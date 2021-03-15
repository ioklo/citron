# 열거형 항목 멤버 변수 참조

```csharp
enum E
{
    First,
    Second(int x)
}

var e = E.Second(2);

if (e is Second s)
    s.x = 2; // E.Second의 x멤버 참조

```