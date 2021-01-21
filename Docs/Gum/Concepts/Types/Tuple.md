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

[Tuple을 함수 인자로 사용하기](Using%20tuples%20as%20function%20arguments.md)