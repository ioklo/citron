# this 참조

```csharp
class C
{
    int x;

    void F()
    {
        this.x = 3; // 클래스 멤버 함수에서 this 참조
    }
}

struct S
{
    int x;

    void F()
    {
        this.x = 3; // 구조체 멤버 함수에서 this참조
    }
}

```