# Enumerable And Foreach

Status: draft current
Area: language, iteration
Keywords: foreach, RefEnumerable, RefEnumerator, ValueEnumerable, iterator, seq

## Current Direction
- `foreach`는 ad-hoc name lookup 규칙이 아니라 trait/conformance 모델 위에서 동작하게 한다.
- 초기 구현은 `trait`를 먼저 도입하고, 그 위에 `RefEnumerable`, `RefEnumerator`, `foreach(var& x in e)`를 올리는 순서를 선호한다.
- Storage-backed collection의 주 순회 contract는 `RefEnumerable` 계열로 본다.
- `ValueEnumerable`은 generator / `seq` / `yield` 계열의 value-producing source에 더 가깝고, 초기 `foreach` 범위에서는 제외한다.
- `foreach(var x in e)`가 `RefEnumerable`에 자동 fallback해서 item을 copy하는 규칙은 두지 않는다.

## Binding Modes
초기 방향:
```text
foreach(var& x in e)
  -> RefEnumerable

foreach(var x in e)
  -> deferred, likely ValueEnumerable later
```

`foreach(var x in e)`를 `RefEnumerable`에 fallback시키지 않는 이유:
- `var&` 누락 실수를 숨길 수 있다.
- struct/NBC item에서 copy 비용이 숨어 들어간다.
- 사용자가 원본을 보고 있다고 착각할 수 있다.

## RefEnumerable
`RefEnumerable`은 내부 element storage를 안정적으로 가리킬 수 있는 collection에 적합하다.

예:
```citron
trait RefEnumerator
{
    type Item;
    Item* Next();
}

trait RefEnumerable
{
    type Item;
    type Enumerator : RefEnumerator
        where This.Item == Enumerator.Item;

    Enumerator GetEnumerator();
}
```

Open point:
- `RefEnumerator::Next`가 최종적으로 `T*`, `T&`, 또는 다른 shape가 될지 확정 필요.

## ValueEnumerable
`ValueEnumerable`은 collection보다 value-producing source에 더 가깝다.

예:
```citron
seq int Gen()
{
    yield 2;
    yield 3;
    yield 7;
}
```

후보 shape는 아직 확정하지 않는다. 이전 논의에서는 아래 후보들이 있었다.

```citron
trait ValueEnumerator
{
    type Item;
    bool Next([out] Item item);
}
```

또는 `optional<Item> Next()` / `MoveNext()` + `Current()` 계열도 후보였지만, v1에서는 결정하지 않는다.

## Deprecated Iterator Contract
기존 deprecated spec에는 아래 contract가 있었다.

```citron
nullable<TItem> GetNext();
nullable<TItem> GetNext() TError;
```

이 모델에서 `null`은 normal end, `TError`는 failure channel이었다. 현재 wiki 기준에서는 이 shape를 current direction으로 보지 않고, trait 기반 `RefEnumerable` 우선 방향을 따른다.

## MIR / Lowering Notes
- `foreach`는 `for`로 완전 desugaring하지 않는다.
- MIR에서 `MStmt_Foreach` 의미 보존 노드를 유지하는 방향이 있었다.
- next/cast/lifetime 의미는 `MStmt_Foreach` 내부 lowering path에서 표현한다.

이 lowering 세부는 compiler wiki로 이관할 예정이다.

## History
- `ai/specs/language/nullable-and-iteration.md`
- `ai/notes/2026-05-14-trait-refenumerable-foreach-direction.md`
- `ai/notes/2026-05-15-trait-concept-associated-type-design.md`
