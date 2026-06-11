# 회의 / 설계 노트

Date: 2026-05-14
Title: trait / RefEnumerable / foreach 초기 방향 정리

Status
- draft

Summary
- `trait`와 `interface`는 분리하는 방향을 선호한다.
- 초기 구현은 `trait`를 먼저 도입하고, 그 위에 `RefEnumerable`, `RefEnumerator`를 올린 뒤 `foreach(var& x in e)`를 구현하는 순서로 진행한다.
- storage-backed collection(`List<T>` 등)의 순회 본체는 `RefEnumerable` 계열로 본다.
- `ValueEnumerable`은 `seq` / `yield` 계열의 value-producing source에 더 가깝고, 초기 `foreach` 구현 범위에서는 제외한다.
- `foreach(var x in e)`가 `RefEnumerable`에 자동 fallback하는 규칙은 두지 않는 쪽을 선호한다.

Context
- `T&`는 type layer의 일반 타입이 아니라 제한된 surface form으로 두는 방향을 이미 택했다.
- 따라서 enumeration 설계에서도 "일반 reference 시스템"을 먼저 완성하려고 하기보다, 필요한 순회 contract를 따로 세우는 편이 자연스럽다.
- `struct`와 `class` 모두 내부에 addressable element storage를 가진 collection이라면 ref enumeration을 제공하는 것이 자연스럽다.
- 반면 generator / `seq` function은 기존 저장소의 원소를 빌려주는 모델보다, 값을 그때그때 생성해 내보내는 모델에 가깝다.

Decisions
## 1) `trait`와 `interface`는 분리한다
- `trait`
  - static contract
  - 주 사용처는 `struct` 중심 generic/static polymorphism
  - associated type을 허용하는 방향을 선호한다
- `interface`
  - dynamic contract
  - 주 사용처는 `class` 중심 dynamic dispatch
  - object-like 저장성과 runtime dispatch를 더 직접적인 목표로 둔다

메모:
- 기존 노트의 "declaration 하나 + dynamic/static 소비 모드 구분" 안도 있었지만, 현재는 `trait`와 `interface`를 분리하는 쪽이 더 자연스럽다고 본다.
- 특히 associated type과 dynamic 저장성 문제를 같은 declaration kind 안에서 동시에 풀기보다, 두 계약 모델을 분리하는 편이 명확하다.

## 2) naming은 `trait`와 `interface`를 다르게 간다
- `trait` 이름에는 `I` 접두사를 붙이지 않는 방향을 선호한다.
  - 예: `RefEnumerable`, `RefEnumerator`
- `interface` 쪽은 별도 naming rule을 나중에 정한다.
  - 필요하면 `IRefEnumerable`, `IRefEnumerator` 같은 이름을 사용할 수 있다.

## 3) storage-backed collection의 본체는 `RefEnumerable`
- `AList<TItem>`이 `struct`이든, `BList<TItem>`이 `class`이든,
  내부 원소 storage를 순회하는 collection이라면 `RefEnumerable` / `RefEnumerator`를 구현하는 것이 자연스럽다.
- receiver가 value type인지 handle type인지는 부차적이다.
- 핵심은 "element location을 안정적으로 가리킬 수 있는 순회 contract를 제공하는가"다.

## 4) `ValueEnumerable`은 generator / `seq` 계열에 더 가깝다
- `ValueEnumerable` / `ValueEnumerator`는 collection보다 value-producing source에 더 적합하다.
- 예:
```citron
seq int Gen()
{
    yield 2;
    yield 3;
    yield 7;
}
```
- 이런 형태의 source는 `RefEnumerable`보다 `ValueEnumerable`과 더 잘 맞는다.
- 따라서 초기 개발 순서에서는 `ValueEnumerable`을 뒤로 미룬다.

## 5) `foreach(var x in e)`는 `RefEnumerable`에 자동 fallback하지 않는다
- `foreach(var x in e)`가 `RefEnumerable`에 fallback하여 각 iteration마다 복사/copy-ctor를 수행하는 규칙은 실수 유발 가능성이 높다.
- 특히 struct/NBC item에서는:
  - 복사 비용이 숨겨질 수 있고
  - 원본을 보는 것처럼 읽히지만 실제로는 복사본을 다루게 되며
  - `var&`를 빠뜨린 실수를 늦게 발견할 가능성이 있다
- 따라서 초기 방향에서는 다음처럼 분리하는 쪽을 선호한다.
  - `foreach(var& x in e)` : `RefEnumerable`
  - `foreach(var x in e)` : 나중에 `ValueEnumerable` 쪽을 설계한 뒤 별도로 도입

## 6) 초기 구현 순서는 아래로 둔다
1. `trait`
2. `RefEnumerable`, `RefEnumerator`
3. `foreach(var& x in e)`

추가 메모:
- `foreach`는 처음부터 모든 binding mode를 한꺼번에 넣지 않는다.
- 우선 `var&` binding만 지원하는 최소 루프를 구현하고,
  `var`, `ptr`, `seq`, `yield`, `ValueEnumerable`은 이후 단계로 넘긴다.

Rationale
- 현재 가장 확신이 큰 축은 storage-backed collection의 ref iteration이다.
- `ValueEnumerable`을 먼저 고정하려고 하면 nullable/return shape, generator surface, copy semantics 등 아직 열린 문제가 함께 커진다.
- 반면 `RefEnumerable`과 `foreach(var&)`는 `List<T>` 같은 실제 컨테이너 설계와 직접 연결되며, 현재의 `T&` 정책과도 잘 맞는다.
- 초기 slice를 좁게 잡으면 이후 `ValueEnumerable`이나 dynamic `interface` enumeration을 붙일 때도 contract 경계가 더 분명해진다.

Deferred
- `ValueEnumerable`, `ValueEnumerator`의 정확한 shape
- `seq` function의 declaration surface와 return contract
- `foreach(var x in e)` 규칙
- dynamic enumeration용 `interface` 설계
- `RefEnumerator::Next`가 `T*` / `T&` / 기타 형태 중 무엇이 될지에 대한 최종 확정

Action Items
- [ ] `trait` declaration/conformance 최소 규칙 초안 작성
- [ ] `RefEnumerable`, `RefEnumerator`의 associated type 및 requirement shape 초안 작성
- [ ] `foreach(var& x in e)`의 surface syntax, lowering, scope rule 초안 작성
