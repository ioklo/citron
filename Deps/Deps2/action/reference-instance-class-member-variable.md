# 인스턴스 클래스 멤버 변수 참조

```csharp
class C
{
    public int x;
    public default construtor;    

    public void F()
    {
        x = 4; // 1. this.x 참조        
    }
}

var c = new C();
c.x = 1; // 2. 외부에서 클래스 값으로 참조

```

