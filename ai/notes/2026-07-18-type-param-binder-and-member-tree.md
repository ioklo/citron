# 회의 / 설계 노트

Date: 2026-07-18
Title: type parameter binder, type lookup, 그리고 member tree의 경계

## Summary

- `S<int>.T` 같은 qualified type-parameter projection은 제공하지 않기로 했다.
- 반면 `S<T>`의 member function/body 안에서 `T`를 찾고, `S<int>`의 instantiation environment에서 `int`로 적용하는 것은 지원해야 한다.
- 따라서 type parameter는 type-name lexical scope에는 참여하지만, qualified member surface에는 참여하지 않는 generic binder로 보는 쪽이 현재 가장 자연스럽다.
- `RTypeParamDecl`을 만든 역사적 핵심 이유는 parameter의 identity와 type-argument 적용을 단순 index가 아니라 declaration object로 표현하기 위해서였으며, member tree node여야 한다는 요구는 아니었다.
- member tree는 단순 search 편의가 아니라 declaration containment 및 qualified/addressable member surface를 나타내는 얇은 구조로 한정하는 쪽이 적절하다.

## Example And Intended Semantics

```citron
struct S<T>
{
    struct X<U> { }
    func F(x: T) -> T { ... }
}
```

의도하는 의미는 다음과 같다.

```text
S<int>.X      // 허용: S의 qualified member
S<int>.T      // 불허: T는 S의 qualified member가 아님

S<int>의 F 본문에서 T
  -> lexical generic binder S.T를 resolve
  -> S<int>의 type-argument environment를 적용
  -> int
```

`F` 안에서의 `T -> int`와 `S<int>.T -> int`는 같은 동작이 아니다.

- 전자는 lexical binder lookup 뒤의 substitution이다.
- 후자는 instantiated type의 qualified member lookup이며, 허용하면 generic parameter를 externally projectable member/associated type처럼 만드는 별도 언어 규칙이 된다.

현재는 전자만 필요하고 후자는 원하지 않는다.

## Tree And Generic Signature Model

`S`는 하나의 declaration/member tree node로 두고, generic signature는 그 declaration의 payload로 둔다.

```text
S
├─ generic signature: [T]
└─ member children:
   ├─ X
   │  └─ generic signature: [U]
   └─ F
```

여기서 `T`는 `S`가 도입하고 소유하는 declaration/binder일 수 있지만, `S`의 member child는 아니다. `X`, `F`처럼 qualified access 가능한 declaration만 member edge를 가진다.

generic parameter와 type member는 source type-name namespace를 공유한다. 따라서 `struct S<T> { struct T { } }`처럼 같은 declaration scope에서 충돌하는 이름은 허용하지 않는 쪽이 자연스럽다. 다만 namespace 공유가 곧 member edge 공유를 뜻하지는 않는다.

## Lookup Modes

이번 논의에서 구분된 중요한 lookup 규칙은 다음과 같다.

1. 일반 type lookup
   - current declaration의 generic signature와 type member를 문맥 규칙에 따라 본다.
   - outer lexical scope로 올라갈 수 있다.

2. declaration header에서 시작하는 type lookup
   - `struct S<T> : Trait<T>` 같은 header에서 `T`는 보인다.
   - 하지만 `S`가 선언할 `Item`/`X` 같은 자기 member는 아직 보지 않는다.
   - current declaration의 generic signature만 확인한 뒤 outer scope로 진행한다.

3. general member lookup
   - type뿐 아니라 value/function 등의 일반 item을 문맥 규칙에 따라 찾는다.

기존에 논의한 `ResolveTypeMember`, `ResolveTypeMemberFromHeader`, `ResolveMember` 명명은 type parameter를 member로 취급하는 전제와 연결되어 있었다. 현재 leaning에서는 `TypeMember`가 qualified member만 뜻한다면 type parameter를 포함하지 않는다. 따라서 최종 API 이름은 generic binder lookup과 member lookup의 분리 후에 다시 정한다.

후보 방향:

```text
ResolveType(...)
ResolveTypeFromDeclHeader(...)
ResolveMember(...)
```

단, 이는 resolver 계층의 API 후보이며 아직 확정 규칙은 아니다.

## `RTypeParamDecl` History Investigation

2026-01-02 커밋 `2afbdbe5` (`TypeParamDecl을 decl tree에 올리기`)를 확인했다. 이 커밋의 실질적인 변화는 type parameter의 identity를 index에서 declaration object로 옮긴 것이었다.

```text
이전
  RMember_TypeVar(index)
  RType_TypeVar(index)
  RFactory의 type-var cache key = index

이후
  RMember_TypeVar(RTypeParamDecl*)
  RType_TypeVar(RTypeParamDecl*)
  RFactory의 type-var cache key = RTypeParamDecl*
```

같은 커밋에서 `RDecl::MakeOpenTypeArgs`는 outer declaration chain을 따라 각 formal parameter의 open type을 만들도록 바뀌었고, `globalIndex`는 type-variable application에서 type argument slot을 찾는 데 사용됐다. 이것은 nested generic identity와 substitution을 위한 변화다.

2026-01-03 커밋 `af5a50fc`는 type parameter 동작을 `NGenericsComponent`로 옮겼고, 당시 기존 `GetTypeMember` API가 type parameter도 반환하게 만들었다. 이는 별도 generic-binder lookup API가 없던 상태에서 type namespace lookup을 재사용한 흔적으로 볼 수 있다. type parameter가 qualified member여야 한다는 언어 의미를 확정한 근거로 보기는 어렵다.

최초 구현에서 type parameter는 `outer`를 저장했지만 `GetROuter()`가 `outer` 대신 `this`를 반환하고 있었다. 일반적인 tree-parent traversal이 당시 필수였다면 즉시 문제가 됐을 형태이므로, "tree에 올리기"라는 커밋 제목도 완성된 member-tree 의미론보다 declaration object/identity 체계로의 편입을 가리킨 것으로 해석하는 편이 타당하다.

## Consequences For `RTypeParamDecl`

현재 구현에서 `RTypeParamDecl`의 다음 `RDecl` API는 사실상 의미가 없다.

```text
GetTypeParamCount()    -> 0
GetTypeMember(...)     -> null
ResolveMember(...)     -> miss
ResolveIdentifier(...) -> miss
```

따라서 장기적으로 `RTypeParamDecl`은 `RDecl`/member-tree node일 필요가 없을 수 있다. 필요한 최소 정보는 다음이다.

```text
RTypeParam
- generic owner declaration/node
- name
- slot/index (또는 equivalent identity)
- constraint/source metadata
```

`RType_TypeVar`는 이 parameter object를 가리키고, type resolution 결과 wrapper가 type declaration과 type parameter를 별도 case로 담는 모델을 검토한다.

`RTypeDecl` 상속도 별도 검토 대상이다. type parameter는 custom/nested type declaration이라기보다 `RType_TypeVar`를 만드는 binder이기 때문이다. 다만 기존 factory/visitor 및 result API 영향이 있으므로, `RDecl` 분리와 `RTypeDecl` 분리를 한 번에 강제하지 않는다.

## Member Tree의 목적과 경계

member tree를 유지한다면 목적은 단순 tree search가 아니다.

- declaration containment와 canonical owner
- qualified path 및 addressable member surface
- member-name collision 및 shadowing 판정의 입력
- module declaration surface/CTI에 나타나는 구조
- accessibility policy가 참고할 containment context
- source declaration identity/provenance

반대로 member tree에 넣지 않는 관계는 다음과 같다.

- generic type parameter와 function parameter 같은 lexical binder
- local/lambda local
- inheritance/base relation
- trait conformance
- type argument application

이 경계를 따르면 `RNode`는 lightweight containment/member tree이고, generic signature와 type parameter는 declaration payload/binder scope다. resolver는 member tree를 기계적으로 따라가기만 하는 것이 아니라, current declaration의 generic signature를 확인한 뒤 문맥별 규칙에 따라 member/outer lookup을 수행한다.

## Inheritance Lookup과 Resolver Hook

class의 base class를 통한 lookup은 member tree의 outer/containment 관계와 별개다. base class는 generic instantiation을 동반하므로 bare `RDecl*`를 반환하는 `GetBaseDecl()`만으로 모델링하면 type argument application 정보를 잃는다.

당분간 `RDecl`의 좁은 virtual hook으로 다음을 두는 방향을 검토한다.

```text
ResolveInheritedTypeMember(typeArgs, name)
ResolveInheritedMember(typeArgs, name)
```

일반 resolver는 local direct member lookup과 outer lexical lookup 사이에서 이 hook을 호출한다. hook의 계약은 다음으로 제한한다.

- current declaration의 direct member는 다시 보지 않는다.
- outer lexical scope로 올라가지 않는다.
- base/inheritance chain만 따라간다.
- base edge의 type arguments를 current `typeArgs`에 적용한 뒤, 찾은 declaration result에 그 applied arguments를 붙인다.

`ResolveIdentifierExtra`처럼 일반적인 이름의 hook은 확장 여지를 남기지만 lookup 순서와 책임이 불명확해지기 쉽다. 현재 목적이 inheritance라면 위처럼 `Inherited`를 명시하는 이름이 더 낫다. 장기적으로는 resolver가 declaration category별 inheritance/conformance policy를 직접 다루고, `RDecl` hook은 줄이는 방향을 우선 검토한다.

타 언어의 nested type 상속은 일관되지 않다. C++의 unqualified lookup 및 C#/Java의 member inheritance는 base의 nested type을 볼 수 있는 모델과 친하지만, Swift는 nested type을 일반 inherited member surface로 두지 않고 Rust에는 class inheritance가 없다. Citron이 base class의 type member를 상속시키려면 C#/Java 계열처럼 명시적인 언어 규칙으로 채택해야 한다.

## `Get`과 `Resolve`의 의미

`RDeclRes`를 반환한다는 사실만으로 API를 `Resolve`라고 부르지는 않는다. 결과 object는 declaration에 lexical/outer type arguments, overload candidate group 같은 문맥을 붙이는 표현 형식일 뿐이다.

- `Get...`: 해당 container의 direct/local item을 찾고 필요하면 result context로 감싼다.
- `Resolve...`: generic binder, inherited member, outer lexical scope, overload/shadowing/ambiguity 같은 lookup policy를 적용한다.
- `ToR...Res`: 이미 찾은 raw declaration에 type-argument context를 결합하는 conversion이다.

따라서 `RType`의 `ResolveVar`가 자기 type의 direct variable map만 보고 `outerTypeArgs`를 붙이는 동작이라면 `GetVar`가 더 맞다. 반대로 다른 scope까지 진행하면 `Resolve`다.

## Generic Argument Binding State

type argument 상태는 한 축으로 합치지 않는다.

1. **결합 범위**
   - `unbound`: generic definition/decl만 있고 self type argument가 없다.
   - `outer-applied`: nested/member declaration에 enclosing type arguments만 적용됐다. self type argument는 아직 없다.
   - `fully-applied`: enclosing 및 self type arguments가 모두 적용됐다.
2. **인자의 closure**
   - open: 적용된 argument 안에 type parameter/type variable이 남아 있다.
   - closed: 모든 argument가 concrete/closed type이다.

Citron에서는 첫 상태를 `RType`으로 들고 다니지 않는다. generic definition은 `RTypeDecl`/`RDecl`이고, `RType`은 항상 arguments가 채워진 constructed type이다.

```text
S        -> generic definition declaration (RTypeDecl), not RType
S<T>     -> bound RType, open constructed type
S<int>   -> bound RType, closed constructed type
```

그러므로 `RDeclRes` 안에서 self argument가 없는 상태는 unbound *type*이 아니라 member/overload lookup 중의 declaration context다. `RType`으로 승격하지 않는다. C#은 `S<>`를 unbound generic type이라는 별도 type form으로 다루지만, Citron에 `typeof(S<>)`, generic type constructor, higher-kinded/partial application 같은 기능이 없다면 이를 별도 `RType`으로 도입할 실익은 없다.

공개 용어는 보통 `open/closed type` 또는 `open/closed constructed type`이다. `open/closed type arguments`는 구현상 `RTypeArguments::IsClosed()` 같은 query 이름으로는 자연스럽지만, 모델 설명에서는 arguments 자체보다 그것으로 구성된 type/decl의 성질로 설명한다.

## Declaration Result 표현 후보

`RDeclRes` 전체를 하나의 applied-declaration wrapper로 대체하지는 않는다. declaration을 담는 각 variant payload에만 결합 범위를 명시하는 wrapper를 둔다.

```cpp
template <typename TDecl>
struct ROuterAppliedDecl
{
    TDecl* decl;
    RTypeArguments* outerTypeArgs;
};

template <typename TDecl>
struct ROuterAppliedDeclGroup
{
    RTypeArguments* outerTypeArgs;
    std::vector<TDecl*> items;
};
```

단일 class variable/function 같은 result는 `ROuterAppliedDecl<TDecl>`로, 같은 enclosing type arguments를 공유하는 overload group은 `ROuterAppliedDeclGroup<TDecl>`로 표현한다. overload마다 동일한 `outerTypeArgs`를 반복해 `vector<ROuterAppliedDecl<TDecl>>`로 만들 필요는 없다.

`RBoundDecl`보다 `RAppliedDecl`/`ROuterAppliedDecl`이 더 정확하다. `bound`는 open/closed constructed type의 의미와 혼동될 수 있기 때문이다.

## 2026-07-19 적용 결과: Applied Declaration과 함수 호출 경계

커밋 `e028f227`은 위 논의를 다음 형태로 적용했다.

```cpp
template <typename TDecl>
struct ROuterAppliedDecl
{
    RTypeArguments* outerTypeArgs;
    TDecl* decl;
};

template <typename TFuncDecl>
struct ROuterAppliedFuncDeclGroup
{
    RTypeArguments* outerTypeArgs;
    std::vector<TFuncDecl*> decls;
};

template <typename TDecl>
struct RAppliedDecl
{
    TDecl* decl;
    RTypeArguments* typeArgs;
};
```

- `ROuterAppliedDecl`: nested type declaration처럼 enclosing generic arguments만 적용되고 self arguments는 아직 없는 declaration result다.
- `ROuterAppliedFuncDeclGroup`: overload 후보가 하나의 `outerTypeArgs`를 공유하는 함수 lookup result다. `RDeclRes`의 global/class/struct/trait 함수 variants가 이를 사용한다.
- `RAppliedDecl`: self generic layer가 없는 variable처럼 `typeArgs` 전체가 이미 적용된 declaration이다. class/struct/enum element variable lookup과 그 직접 `GetVar` API에 사용한다.

여기서 함수의 explicit type arguments는 **`RDeclRes`에 넣지 않는다.**

```citron
struct S<X>
{
    void F<T, U, V>(U u, V v) { }
    void G() { F<int>(2, false); }
}
```

`F`의 identifier/member lookup 결과는 `S`의 applied outer arguments와 `F` overload group뿐이다. `<int>`는 호출 expression이 보유하는 member type arguments다. overload마다 type parameter arity와 slot이 다를 수 있으므로, `RDeclRes` 단계에서 group 전체에 `[X, int]`를 부착하면 안 된다.

SmTranslator는 identifier/member를 callable intermediate expression으로 바꿀 때 다음을 만든다.

```cpp
template <typename TFuncDecl>
struct SmPartiallyAppliedFuncDeclGroup
    : ROuterAppliedFuncDeclGroup<TFuncDecl>
{
    RTypeArguments* memberTypeArgs;
};
```

`memberTypeArgs`는 outer를 제외한 source-level explicit function type argument prefix이며, 아직 완전하지 않아도 된다. `MatchFunc`/`MatchArguments`는 candidate별로 이 prefix 뒤의 type parameters를 open type variables로 채운 full argument vector를 만들고, value argument matching 및 inference constraint를 수행한다. 이 단계가 성공하면 `SmFuncMatch`의 full `typeArgs`를 얻는다.

따라서 상태별 책임은 다음과 같다.

```text
RDeclRes / RTypeRes
  declaration/type name lookup + outer generic environment

SmPartiallyAppliedFuncDeclGroup
  lookup result + source의 explicit member/function type arguments

SmFuncMatch
  candidate 선택 및 inference 뒤의 full type arguments
```

이 구분은 function overload group에 type arguments를 중복 저장하지 않고, explicit argument 개수/내용이 candidate별 유효성 판정에 미치는 영향을 call matching 단계에 남긴다.

## 2026-07-19 적용 결과: `Get`과 `Resolve` 명명

`RDeclRes`를 반환한다는 이유만으로 `Resolve`를 쓰지 않기로 확정했다.

- `GetMember`, `GetVar`: 현재 type/declaration의 direct member table만 보고, 필요하면 applied declaration result로 감싼다.
- `ResolveTypeIdentifier`, `ResolveTypeIdentifierInHeader`, `ResolveIdentifier`: current scope의 binder/member 확인 후 inherited 및 outer lexical scope까지 진행하는 lookup policy다.
- `ResolveInheritedTypeMember`, `ResolveInheritedMember`: class base chain만 진행하는 좁은 policy hook이다. base type arguments는 current/derived arguments에 apply한 뒤 result에 붙인다.

그에 따라 이전 `RType::ResolveMember`는 `GetMember`로, direct variable lookup의 `ResolveVar`는 `GetVar`로 변경됐다. 결과 형식과 lookup 범위를 분리해 이름에서 실제 탐색 범위를 드러내는 것이 목적이다.

## Open Points

- generic binder와 nested type member가 공유하는 type-name namespace의 정확한 duplicate/shadowing 진단 규칙
- body type lookup에서 generic signature, nested type member, outer scope를 확인하는 정확한 우선순위
- resolver result wrapper의 shape: `TypeDecl | TypeParam`을 별도 type-resolution result로 둘지, 넓은 declaration result에 둘지
- `RTypeParamDecl`의 `RDecl` 및 `RTypeDecl` 상속을 어떤 순서로 제거/대체할지
- `ResolveType...` API의 최종 명명과 resolver 계층 배치
- inherited type/member lookup을 `RDecl` virtual hook으로 둘 기간과, 장기 resolver policy로 옮길 시점
- `RTypeArguments`의 open/closed query 및 inference constraint가 fully-applied `SmFuncMatch`까지 완료된 뒤의 closure 판정을 어떻게 제공할지
