# Enum

Discriminated Union, Algebraic Data Type

기존 C/C++의 enum과 비슷하지만, 멤버변수와 함께 넣을 수 있습니다.

```csharp
// 선언
enum Coord2D<T>
{
    Invalid,
    Rect(T x, T y),            // 기본 C syntax와 비슷한 느낌을 주려고 콤마로 구분합니다
    Polar(T radius, T angle),
}

Coord2D<int> m = Rect(20, 30); // 타입힌트가 있어서 Coord2D<int>.Rect로 쓰지 않아도 됩니다

int GetLengthSq(Coord2D<int> m)
{
    if (m is Rect)
        return m.x * m.x + m.y * m.y;

    else if (m is Polar) 
        return m.radius;

    else if (m is Invalid)
        return -1;

    ...
}	
```

# 확장

```csharp
enum Mammal
{
    Cat,
    Dog,
    Invalid,
}

enum Bird
{
    Duck,
    Invalid,
}

// Flatten
enum Animal : Mammal, Bird // Invalid는 쓸 수 없게 됩니다
{
}

enum Animal2 // Invalid는 쓸 수 있겠지만 조금 귀찮습니다
{
    M(Mammal m),
    B(Bird b)
}

var animal = Animal.Duck;
var animal2 = Animal2.B(Bird.Duck);
```