# 인스턴스 클래스 멤버 함수 호출

```csharp
class C
{
    int x;

    public void F()
    {
        print(x); // this.x
    }

    public void G()
    {        
        F(); // 1. this.F();
    }
}

var c = new C();

c.F(); // 2. 인스턴스 클래스 멤버 함수 호출

```