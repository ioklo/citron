# 정적 구조체 멤버 변수 참조

```csharp
struct S
{
    static int x;

    void Func()
    {
        x = 4; // 1. 내부에서 참조
    }
}

S.x = 3; // 2. 외부에서 타입 이름을 사용하여 참조
var s = new S();
s.x = 45; // 3. 에러, 값의 멤버변수는 인스턴스 멤버변수만 가능

```

