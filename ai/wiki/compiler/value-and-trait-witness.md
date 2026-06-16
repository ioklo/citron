# Value And Trait Witness

Status: draft current
Area: compiler, abi
Keywords: value witness, trait witness, metadata, opaque result, some

## Value Witness
Value witness는 타입 값을 다루기 위한 기본 연산 테이블이다.

개념:
```text
ValueWitness
  size
  align
  copy(dest, src)
  move(dest, src)
  destroy(value)
```

컴파일러가 concrete type layout을 모를 때도 value witness를 통해 local storage와 lifetime을 관리할 수 있다.

사용처:
- `some Trait` opaque result local storage allocation
- unknown-size value copy/move/destroy
- opaque sret result cleanup

## Trait Witness
Trait witness는 trait requirement를 backing type 구현으로 연결하는 테이블이다.

개념:
```text
TraitWitness<MyTrait>
  TraitMethod(self, ...)
```

`some MyTrait` 값에서 `MyTrait` method를 호출할 때 trait witness를 사용한다.

## Difference
- value witness는 값 자체의 storage/lifetime 연산이다.
- trait witness는 trait surface operation dispatch다.

## Open Points
- value witness table의 정확한 ABI
- copy/move/destroy와 BC/NBC 구분의 연결
- trait witness lookup을 opaque metadata에 포함할지 별도 accessor로 둘지
- callable `func<>`의 Invoke witness shape

## History
- `ai/notes/2026-06-16-some-opaque-result-and-cti.md`
