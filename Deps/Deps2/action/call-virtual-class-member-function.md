# 가상 클래스 멤버 함수 호출

```csharp
class Base
{
    public virtual void Func()
    {
        ...
    }

    public void G()
    {
        Func(); // 1. this.Func(), this가 실제로 Base냐 Derived냐에 따라서 무엇을 호출할지 달라진다
    }
}

class Derived
{
    public override void Func()
    {
        ...
    }
}

Base b = new Derived();
b.Func(); // 2. Derived.Func 호출

```
