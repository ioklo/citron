# 정적 클래스 멤버 변수 참조

```csharp
class C
{
    public static int x;

    void F()
    {
        x = 2; // 내부에서 참조 
    }
}

C.x = 4; // 외부에서 참조

var c = new C();
c.x = 4; // 에러

```

