# 클래스 값의 타입으로 분기하기

```csharp
class Derived : Base { public int x; }

void Func(Base b)
{
    // 1. 기본형
    if (var d = b as Derived)
    {
        // d는 scope내에서 Derived 타입이다.
        assert(d.x == 4);
    }

    // 2. 테스트 대상이 지역변수일 경우의 축약형
    if (b is Derived) // 스코프 내에서 b는 Derived 타입이다
    {
        b = new Base(); // derived 타입이니 불가, 이걸 하고 싶으면 var d = b as derived를 해야 한다        
    }
}
```