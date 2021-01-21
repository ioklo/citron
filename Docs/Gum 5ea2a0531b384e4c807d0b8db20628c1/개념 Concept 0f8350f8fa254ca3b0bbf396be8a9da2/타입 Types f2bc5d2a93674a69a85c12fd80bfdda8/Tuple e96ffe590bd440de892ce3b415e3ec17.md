# Tuple

이름이 붙어 있는 값의 묶음입니다. 크기와 타입이 고정되어있습니다. 

`struct`와 다른 점은 타입만 맞으면 대입이 가능합니다. 

멤버 이름은 해당 변수의 타입에 쓰여진 이름을 따릅니다.

```csharp
// 이름을 붙인 튜플 타입
type TupleType = (int Number, string Name, bool bValid);

// 생성
TupleType tuple1 = (1, "hello", false);
var tuple2 = (2, "Hello", true);

// 사용
Assert(tuple1.Number == 1);

// 이름이 없으면 ItemN을 사용합니다.
Assert(tuple2.Item1 == "Hello");

```

[Tuple을 함수 인자로 사용하기](Tuple%20e96ffe590bd440de892ce3b415e3ec17/Tuple%E1%84%8B%E1%85%B3%E1%86%AF%20%E1%84%92%E1%85%A1%E1%86%B7%E1%84%89%E1%85%AE%20%E1%84%8B%E1%85%B5%E1%86%AB%E1%84%8C%E1%85%A1%E1%84%85%E1%85%A9%20%E1%84%89%E1%85%A1%E1%84%8B%E1%85%AD%E1%86%BC%E1%84%92%E1%85%A1%E1%84%80%E1%85%B5%20936fc6e7835143df9220984f4c2df1d8.md)