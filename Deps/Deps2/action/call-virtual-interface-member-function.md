# 인터페이스 멤버 가상 함수 호출

```csharp
interface I
{
    void F();    
}

void MyFunc(I i)
{
    i.F();
}

///// 1.
class C : I
{
    // void I.F() {}를 사용할 수 있다. 외부에서 호출 불가능
    void F() { }    
}

var c = new C();
MyFunc(c); // 내부에서 c.F 호출

////// 2.
struct S : I
{
    void F() { } 
}

var s = box S(); // box<S>, 오직 boxing되었을 때만 interface 로 캐스팅이 가능하다
MyFunc(s); // 내부에서 box<S>.Value.F(); 호출

```