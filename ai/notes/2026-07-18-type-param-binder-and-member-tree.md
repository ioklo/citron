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

## Open Points

- generic binder와 nested type member가 공유하는 type-name namespace의 정확한 duplicate/shadowing 진단 규칙
- body type lookup에서 generic signature, nested type member, outer scope를 확인하는 정확한 우선순위
- resolver result wrapper의 shape: `TypeDecl | TypeParam`을 별도 type-resolution result로 둘지, 넓은 declaration result에 둘지
- `RTypeParamDecl`의 `RDecl` 및 `RTypeDecl` 상속을 어떤 순서로 제거/대체할지
- `ResolveType...` API의 최종 명명과 resolver 계층 배치

