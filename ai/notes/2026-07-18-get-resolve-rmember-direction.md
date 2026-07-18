# 회의 / 설계 노트

Date: 2026-07-18
Title: `Get*`, `Resolve*`, `RMember`의 책임 분리

## 목적

declaration tree의 현재 scope 조회와 lexical outer scope를 포함하는 identifier resolution을 분리한다. type parameter는 lexical binder이며 qualified member가 아니라는 기존 방향을 유지한다.

## 현재 방향

`RDecl`은 현재 declaration scope만 조회하는 공통 `Get*` virtual surface를 갖는다.

```text
GetTypeParam(name)  -> RTypeParam*
GetTypeMember(name) -> RTypeDecl*
GetMember(name)     -> RMember
```

- `GetTypeParam`은 현재 declaration의 generic binder만 찾는다.
- `GetTypeMember`는 현재 declaration의 qualified type child만 찾는다.
- `GetMember`는 현재 declaration의 type child와 일반 named member를 찾되, type parameter는 포함하지 않는다.
- 어느 `Get*`도 outer scope로 올라가거나 outer scope의 child를 탐색하지 않는다.

따라서 `S<int>.T`처럼 type parameter를 qualified member로 projection하는 표기는 계속 허용하지 않는다.

## RMember

`GetMember`가 `RDecl*`를 반환하면 function overload set을 충분히 표현할 수 없다. 따라서 현재 scope의 named member lookup 결과를 나타내는 별도 `RMember`를 둔다.

```text
RMember
- single declaration member
- function overload group
```

`RMember`는 declaration/member tree node가 아니라 현재 scope lookup의 result wrapper다. type parameter를 담지 않는다.

```text
RMember::ToRDeclRes(typeArgs, explicitTypeParamsExceptOuterCount)
```

은 단일 declaration 또는 overload group을 `RDeclRes`로 변환한다. 단일 declaration case는 `RDecl::ToRDeclRes(...)`에 위임할 수 있다.

`RMember`를 pointer/owned object로 둘지, `std::optional<RMember>` 같은 값 result로 둘지는 구현 선택으로 남긴다. 새 allocation/ownership이 필요 없다면 값 wrapper를 우선 검토한다.

## Resolve APIs

현재 scope 조회 결과를 사용하고 miss일 때만 outer lexical scope로 진행하는 공통 API는 다음 이름을 사용한다.

```text
ResolveTypeIdentifier(name)         -> RTypeRes
ResolveTypeIdentifierInHeader(name) -> RTypeRes
ResolveIdentifier(name)             -> RDeclRes
```

의도한 순서는 다음과 같다.

```text
ResolveTypeIdentifier(name):
    GetTypeParam(name)
    GetTypeMember(name)
    outer.ResolveTypeIdentifier(name)

ResolveTypeIdentifierInHeader(name):
    GetTypeParam(name)
    outer.ResolveTypeIdentifier(name)

ResolveIdentifier(name):
    GetTypeParam(name) -> RDeclRes_TypeVar
    GetMember(name)    -> RMember::ToRDeclRes(...)
    outer.ResolveIdentifier(name)
```

`ResolveTypeIdentifierInHeader`에서 outer에는 `ResolveTypeIdentifier`를 호출한다. header restriction은 현재 선언 중인 declaration의 자기 member만 막으며, outer declaration의 정상 type lookup까지 막으면 안 된다.

`ResolveIdentifier`은 type parameter도 찾지만 `GetMember`는 찾지 않는다. 이 차이로 type parameter는 lexical identifier/type lookup에는 참여하면서 qualified member surface에는 참여하지 않는다.

## Result Conversion

`RTypeDecl`에 있던 `ToRDeclRes`는 `RDecl` 공통 virtual surface로 옮기는 방향을 검토한다.

```text
RDecl::ToRDeclRes(...)
```

은 single declaration의 category-to-result conversion만 담당한다. overload aggregation, outer recursion, accessibility/ambiguity policy는 이 메서드의 책임이 아니다. overload aggregation은 `RMember`가 맡는다.

visitor로 `RDeclRes` conversion을 외부에 둘 수 있지만, 현재 leaning은 호출부가 `decl->ToRDeclRes(...)`를 직접 사용하는 member API다. 다만 `RDeclRes`의 category 변화가 declaration hierarchy에 과도한 virtual 변경을 강제하는지 구현 시 다시 평가한다.

## Open Points

- `RMember`의 concrete representation: value wrapper, pointer, 또는 별도 overload-group declaration
- `RDecl::ToRDeclRes` virtual과 external visitor 중 최종 선택
- `GetMember`가 type child와 ordinary member를 결합할 때 same-name collision/overload group을 어떻게 구성할지
- `ResolveIdentifier`의 type argument context와 `RMember::ToRDeclRes` parameter shape

