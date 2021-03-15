# 클래스 정의

```csharp
class Base { }
interface I { }

class C<T> : Base, I  // 타입 변수, 부모 타입, 구현할 인터페이스
{
    default constructor;

    // 멤버 함수
    public static void StaticFunc();
    protected void Func(); // 액세스 한정자 
    virtual void VirtFunc();

    // 멤버 변수
    static int v;
    int x;

    // 멤버 타입
    class NestedC
    {
        ...
    }    
}
```
