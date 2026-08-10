# 회의 / 설계 노트

Date: 2026-08-08
Title: GetGlobalIndex 기반 generic application과 persistent TypeEnv 제거 방향

## 결론

- `RTypeEnv`/`tenv`를 `RAppliedDecl`이나 `RType`에 영구적으로 보관하지 않는다.
- `RAppliedDecl`은 `decl + complete RTypeArguments`로 유지한다.
- `RTypeParam::GetGlobalIndex()`는 lexical outer부터 현재 declaration까지
  평탄화한 complete type-argument slot으로 사용한다.
- exact equality는 `RTypeParam*` binder identity를 비교한다.
- alpha-equivalence는 서로 대응하는 generic signature에만 적용한다. global
  index를 canonical slot으로 비교하거나, 한쪽 binder를 다른 쪽 binder로 치환한
  뒤 exact equality를 사용할 수 있다.
- source 이름 해석을 위한 현재 type-parameter scope는 translator의 임시 lookup
  context로 둘 수 있지만 semantic type/application의 일부는 아니다.

## GetGlobalIndex의 의미

```citron
struct X<T1>
{
    trait Tr<T2>
    {
        T2 F<T3>(T3 t3);
    }

    struct S<T4> : Tr<list<T4>> { }

    impl S<T5> : Tr<list<T5>>
    {
        list<T5> F<T6>(T6 t6) { ... }
    }
}
```

각 type parameter의 flattened index는 다음과 같다.

```text
X.T1       = 0
Tr.T2      = 1
Tr.F.T3    = 2

S.T4       = 1
impl.T5    = 1
impl.F.T6  = 2
```

`GetGlobalIndex()`는 compiler 전체에서 유일한 identity가 아니다. 서로 다른
generic declaration의 parameter도 같은 lexical slot이면 같은 index를 가진다.
실제 binder identity는 `RTypeParam*`로 구분한다.

## Trait Header 비교

```text
X<T1>.Tr<list<S.T4>>
X<T1>.Tr<list<impl.T5>>
```

두 conformance target `S<T4>`와 `impl S<T5>`가 대응한다는 사실에서
`S.T4 <-> impl.T5`를 확정한다. 그 뒤 두 trait application을 비교하면
`T4`, `T5`는 모두 global index 1이어서 다음 canonical form을 갖는다.

```text
X<$0>.Tr<list<$1>>
```

단, global index가 같다는 사실만으로 임의의 두 type parameter를 exact-equal로
보지 않는다.

## Trait Function Requirement 적용

requirement의 complete formal slots는 다음과 같다.

```text
[X.T1, Tr.T2, Tr.F.T3]
```

impl 쪽 binder로 requirement를 전개할 complete arguments는 다음과 같다.

```text
[
    X.T1,          // global index 0
    list<impl.T5>, // global index 1
    impl.F.T6      // global index 2
]
```

`RType_TypeVar::Apply`는 자신의 global index로 이 배열을 조회한다. requirement
signature를 적용하면 반환형과 인자형은 다음이 된다.

```text
return: list<impl.T5>
param:  impl.F.T6
```

이제 impl 함수의 반환형과 인자형을 `RTypeParam*` identity까지 포함해 exact
비교할 수 있다. 이 방식에서는 별도의 tenv 정규화 자료구조가 필요하지 않다.

## Base Member Application

```citron
class B<T1>
{
    struct S<T2> { }
}

class C<T3> : B<list<T3>>
{
    S<int> e;
}
```

base application의 complete arguments `[list<C.T3>]`를
`B<T1>.S<>`의 outer arguments에 적용하면 다음 결과를 얻는다.

```text
B<list<C.T3>>.S<>
```

그 뒤 member argument `int`를 붙이면:

```text
B<list<C.T3>>.S<int>
```

`C.T3`의 binder identity가 `RType_TypeVar` 안에 남으므로 결과 위치의 별도
`TypeEnv`를 보관할 필요가 없다.

## 비교 규칙

- exact type/application equality: declaration, type constructor, type arguments,
  `RTypeParam*` identity를 비교한다.
- alpha-equivalence: 두 generic declaration/signature가 대응한다는 전제 아래
  global index canonicalization 또는 explicit binder substitution을 사용한다.
- 단순히 global index가 같다는 이유만으로 서로 관련 없는 type parameter를 exact
  equal로 보지 않는다.

## 후속 검토

- trait requirement comparison을 global-index canonicalization으로 구현할지,
  requirement를 impl binder로 apply한 뒤 exact comparison으로 구현할지
- 기존 `RTypeEnv` 실험 코드를 제거할 범위와 시점
- `GetGlobalIndex()`가 compiler-global identity로 오해되지 않도록 이름 또는 주석을
  보강할지

## 이전 논의

- `ai/notes/2026-08-07-applied-decl-tenv-and-trait-matching.md`는 AppliedDecl에
  tenv를 결합하는 후보안의 논의 이력이다. 이 노트의 persistent TypeEnv 제거
  방향이 현재 결론이다.
