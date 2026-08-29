# 회의 / 설계 노트

Date: 2026-08-29
Title: Runtime generic witness ABI와 associated type 방향

Status
- decided direction

## 결정
- Citron의 일반 generic은 type별 monomorphization을 correctness나 ABI의 전제로 삼지 않는다.
- generic function은 하나의 shared code로 생성하고 type metadata와 trait witness를 hidden argument로 전달하는 Swift식 runtime generic ABI를 기본으로 한다.
- type metadata의 value witness는 size, align, copy, move, destroy를 제공한다.
- trait witness는 trait function entry를 제공하며, associated type requirement가 있으면 associated type metadata와 associated conformance witness를 구하는 entry/accessor도 제공한다.
- concrete type을 아는 경우의 specialization은 선택적 최적화다.
- 장래의 명시적 code instantiation은 일반 generic과 분리된 `template` 또는 macro 계층에서 처리한다.
- trait requirement의 `some Trait`는 현재 범위에서 제외한다. 필요해지면 anonymous associated type으로 정의한다.

## Associated type 우선 예제

`foreach`와 독립적으로 associated type core를 검증하기 위해 다음 형태를 우선 사용한다.

```citron
trait Value
{
    int GetValue();
}

trait Producer
{
    type Output : Value;
    Output Produce();
}
```

초기 테스트 범위는 associated type declaration parsing, impl의 explicit type witness, associated type bound 검사, requirement signature substitution, generic projection 사용이다. Associated type inference와 equality constraint는 v1 범위에서 제외한다.

## Foreach 연결

초기 iteration contract는 다음처럼 equality constraint 없이 최소화할 수 있다.

```citron
trait RefEnumerator
{
    type Item;
    Item* Next();
}

trait RefEnumerable
{
    type Enumerator : RefEnumerator;
    Enumerator GetEnumerator();
}
```

generic `foreach` lowering은 `T : RefEnumerable` witness에서 `Enumerator` metadata와 `Enumerator : RefEnumerator` witness를 얻고, 후자의 associated type projection으로 `Item`을 다룬다.

## Runtime generic recursion

`F<T>`가 `F<List<T>>`를 호출해도 새 machine code를 계속 instantiate하지 않는다. 같은 shared function에 `List<T>` metadata를 넘긴다. 따라서 무한 type expansion은 compile-time monomorphization 문제가 아니라 실행되는 경우 runtime recursion과 metadata 생성 문제다. Generic metadata는 identity별로 cache하며 recursive metadata dependency를 처리할 allocation/initialization 상태가 필요하다.
