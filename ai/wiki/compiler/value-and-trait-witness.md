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
- shared generic code의 type parameter value storage/lifetime 연산
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

일반 generic은 monomorphization을 전제로 하지 않는다. 예를 들어 `F<T>(T value) where T : MyTrait`의 shared entry point는 개념적으로 `T` metadata와 `T : MyTrait` witness를 hidden argument로 받는다.

Associated type requirement가 있으면 trait witness는 다음 runtime projection을 제공할 수 있어야 한다.

```text
TraitWitness<Producer>
  Produce(self, resultStorage)
  OutputMetadata()
  Output_Value_Witness()
```

`OutputMetadata`는 associated type의 type metadata를, `Output_Value_Witness`는 `Output : Value` 같은 associated conformance의 trait witness를 돌려준다. 정확한 entry/accessor ABI는 QIR lowering 설계에서 확정한다.

## Difference
- value witness는 값 자체의 storage/lifetime 연산이다.
- trait witness는 trait surface operation dispatch다.

## Open Points
- value witness table의 정확한 ABI
- copy/move/destroy와 BC/NBC 구분의 연결
- trait witness lookup을 opaque metadata에 포함할지 별도 accessor로 둘지
- callable `func<>`의 Invoke witness shape
- generic function hidden metadata/witness argument의 logical slot과 physical calling convention
- associated type metadata 및 associated conformance accessor의 정확한 witness-table layout

## History
- `ai/notes/2026-06-16-some-opaque-result-and-cti.md`
