# 인스턴스 구조체 멤버 함수 호출

```csharp
struct S
{
    public void F()
    {        
        ...
    }

    public void G()
    {
        F(); // 1. 내부에서 호출 this.F();
    }
}

var s = new S();
s.F();  // 2. 외부에서 호출

```

