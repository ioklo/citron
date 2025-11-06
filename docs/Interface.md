# Interface

인터페이스는 타입 멤버 구성의 규약입니다.

```csharp
public interface IB
{
    int G();    // public 인스턴스 함수 G를 포함해야 합니다
}

public interface I : IB // 모듈 외부에서 노출합니다, IB를 상속으로 받습니다
{
    I(int i, string s); // 이 인터페이스를 구현하는 타입은 int, string을 인자로 받는 public 생성자가 있어야 합니다
    
    static void F();    // public 정적 함수 F를 포함합니다
    void H<T>();        // public H<T>를 포함해야 합니다
}
```

Generic에서 constraint로 사용할 수 있습니다

```csharp
class D1 : I
{
    public D1(int i, string s) { ... }
    public static void F() { ... }
    public int G() { ... }
    public void H<T>() { ... } 
}

// Generic에서 constraint로 사용할 수 있습니다
T Func<T>() where T : I
{
    var x = new T(2, "34");
    T.F();    

    var i = x.G();
    x.H<string>();        

    return x;
}

var d1 = Func<D1>();
```

```csharp
struct D2 : I // 인터페이스는 ref상태일때만 캐스팅 가능하다. D2* -> I
{
    public D2(int i, string s) { ... }
    public static void F() { ... }
    public int G() { ... }
    public void H<T>() { ... } 
}

void Func2(I x)
{
    var i = x.G();
    x.H<int>();
}

I i = new D1();
i.G(); // D1.G 호출

i = ref new D2();
i.G(); // D2.G 호출
```