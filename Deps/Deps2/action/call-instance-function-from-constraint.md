# constraint로부터 인스턴스 함수 호출

```csharp
interface I
{
    void F();
}

void F<T>(T t) where T : I
{
    t.F(); // 1. call-instance-function-from-constraint, not interface-call    
}

struct S : I { }
class C : I { }

box<S> s = box S();
F(s);  // F<box<S>> instantiation, s.Value.F(); 처럼 호출하지 않는다

C c = new C(); 
F(c);  // F<C> instantiation

```