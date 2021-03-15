# 인스턴스 구조체 멤버 변수 참조

```csharp
struct S
{
    int x;    

    void Func()
    {
        x = 3; // 1. 인스턴스 함수 안에서 this참조
    }
}

S s = new S();
s.x = 3;  // 2. 구조체 값으로 밖에서 참조

```

