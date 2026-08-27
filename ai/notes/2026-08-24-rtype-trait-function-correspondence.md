# 회의 / 설계 노트

Date: 2026-08-24
Title: SmType 없는 trait function correspondence 비교

## 결론

- trait requirement와 impl function의 generic signature는 `SmType`이나 별도 canonical
  type node를 만들지 않고 canonical `RType`을 read-only로 재귀 비교한다.
- 비교의 각 side는 outer substitution과 function-local binder correspondence를
  가진다.
- outer substitution은 `RTypeArguments*`만으로 유효성을 판단하지 않고 source formal
  `RTypeParam*` identity를 함께 확인한다.
- function-local binder는 `RTypeParam* -> size_t` canonical slot으로 정규화한다.
  requirement의 `T3`와 impl의 `T6`이 모두 slot 0이면 alpha-equivalent하다.
- requirement binder 자리에 impl binder를 넣어 cross-signature `RType` 또는
  `RAppliedDecl`을 만들지 않는다.
- substitution RHS는 이미 target context에 속하는 type이므로 원래 outer
  substitution으로 다시 해석하지 않는다.

이 결정은 `ai/notes/2026-08-20-generic-application-composition-and-smtype-removal.md`의
"requirement를 impl binder 기준으로 치환한 뒤 exact comparison" 우선안을 대체한다.
일반 `RAppliedDecl` application의 complete `RTypeArguments` 정규형은 그대로 유지한다.

## 예시

```citron
class C1<T1>
{
    trait Tr<T2>
    {
        void F<T3>(T1 t1, T2 t2, T3* t3);
    }
}

class C2<T4>
{
    impl S<T5> : C1<int>.Tr<list<T5>>
    {
        void F<T6>(int i, list<T5> l, T6* t6) { ... }
    }
}
```

비교 context는 다음과 같다.

```text
left signature:
  ^T3. { Ret = void, Params = [T1, T2, T3*] }
  outer substitution = [T1 => int, T2 => list<T5>]
  local slots        = [T3 => 0]

right signature:
  ^T6. { Ret = void, Params = [int, list<T5>, T6*] }
  outer substitution = [T4 => T4, T5 => T5]
  local slots        = [T6 => 0]
```

재귀 비교 결과는 다음과 같다.

```text
T1 vs int
  -> int vs int

T2 vs list<T5>
  -> list<T5> vs list<T5>

T3* vs T6*
  -> local slot 0 vs local slot 0
```

## 비교 Context

개념적인 context는 다음 두 lookup을 제공한다.

```cpp
struct CorrespondTypeContext
{
    // source formal binder identity를 확인한 뒤 argument를 반환한다.
    const RTypeSubstitution* outerSubstitution;

    // 현재 함수가 직접 소유한 binder의 alpha-equivalence slot이다.
    const RTypeParamToIndex* localBinderSlots;
};
```

`RTypeSubstitution`은 적어도 다음 정보를 보존해야 한다.

```text
source formal binders: [T1, T2]
arguments:             [int, list<T5>]
```

`TryApply(T)`는 `T`가 source formal binder 중 하나와 pointer identity로 일치할 때만
argument를 반환한다. 다음 판정은 허용하지 않는다.

```text
T.GetGlobalIndex() < arguments.GetCount()
```

서로 무관한 declaration의 binder가 같은 flattened index를 가질 수 있기 때문이다.

## Type Variable 비교 순서

양쪽 type variable을 비교할 때 각 side에서 다음 순서로 lookup한다.

1. outer substitution의 정확한 source formal binder인가?
2. function-local correspondence slot을 갖는가?
3. 둘 다 아니면 원래 `RTypeParam*` identity가 같은가?

outer substitution으로 type variable을 argument에 치환한 경우, 그 argument subtree는
원래 substitution의 range, 즉 target context에 속한다. 따라서 그 subtree를 비교할
때 같은 outer substitution을 다시 적용하지 않는다. local binder slot lookup은
별도로 유지할 수 있지만, outer argument RHS에 나타나는 binder와 local slot key가
겹치지 않는다는 construction invariant를 검사한다.

## 비교 대상

return/parameter type 외에도 다음 signature 요소를 함께 검사한다.

- function type parameter count와 constraint
- return kind
- parameter count, passing kind, variadic 여부
- trait semantics상 correspondence에 포함되는 `static`, sequence 등의 modifier

## 모델 경계

이 comparison context는 `RType`이나 `RAppliedDecl`에 저장되는 persistent type
environment가 아니다. trait signature correspondence 한 번 동안만 사용하는
transient read-only view다. 따라서 `SmType`, `SmAppliedDecl`, application용
`SmTypeEnv`를 제거한다는 기존 결정과 충돌하지 않는다.

