# alias-other-type

다른 곳의 타입을 현재 컨텍스트의 이름공간에서 찾을 수 있도록 한다

```csharp

// 1. 전역 공간에서 IntList를 찾으면 list<int>이다
type IntList = list<int>;

// 2. inline성질의 tuple에 이름을 줄 수 있다
type MyTuple = tuple<string Name, int Age>;

class MyList<T>
{
    // 멤버 타입 alias
    public type ElemType = T; // 3. MyList<int>.ElemType은 int이다

    void Func()
    {
        // 로컬 타입 정의, 스코프 끝까지 유효하다
        type P = int;
    }
}
```