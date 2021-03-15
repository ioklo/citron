# 클래스 값의 타입으로 분기하기

```csharp
class Derived : Base { public int x; }

void Func(Base b)
{
    if (b is Derived d)
    {
        // d는 scope내에서 Derived 타입이다.
        assert(d.x == 4); 
    }
}

// if (b is Derived) 를 안쓰는 이유
type T = (Base b, int x);
void Func2(T t)
{
    if (t.b is Derived d) // t.b는 변수가 아닌데 어떻게 할 것인가
    {

    }
}
```