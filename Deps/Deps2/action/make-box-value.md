# box 값 생성

```csharp
// 1. 값으로 부터 
box<int> i = box 1; // int struct 값
box<string> str = box "hi"; // string class 값

// 2. struct 정의로부터
struct S
{
    int x;
    int y;

    default_constructor;
}
box<S> s = box S(2, 3); // 생성자 호출

// 3. Enum 생성자로부터
enum E
{
    First;
    Second(int x);
}

box<E> e = box First;    // 생성자 호출, 타입 힌트
var e = box E.Second(2); // 생성자 호출

```