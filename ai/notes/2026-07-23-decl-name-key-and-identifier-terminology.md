# 2026-07-23 Declaration Name / Key / Identifier Terminology

## Superseded terminology note

This note originally introduced `RDeclName`.  The later lookup review found
that `RName_CtorParam` and type-parameter names must also be valid resolver
inputs, while an `RDecl` has a name only for lookup exposure.  The current
rule therefore keeps `RName` as the common lookup key and does **not** retain a
separate `RDeclName` abstraction.  See
`ai/notes/2026-07-23-rname-lookup-and-impl-member-lookup.md`.

## Historical decision

declaration identity 이름을 다음 세 층으로 고정한다.

```text
RDeclName       source declaration name / lookup surface
RDeclKey        same-outer exact tree key
RIdentifier     module까지 포함해 declaration 하나를 찾는 global identity
RTypeIdentifier arbitrary type의 global identity
```

예:

```text
void F(int i)

RDeclName:   F
RDeclKey:    F(N$pi)
RIdentifier: Core::N.S.F(N$pi)
```

`RDeclKey`만으로는 declaration을 찾을 수 없다. 해당 key를 가진 direct child를 찾을 outer가 필요하다.

```cpp
outer->GetChild(RDeclKey{...});
```

반대로 `RIdentifier`는 module부터 path를 따라 declaration 하나를 가리킨다.

```cpp
ResolveGlobal(RIdentifier{...});
```

## Rationale

기존 local `RIdentifier`라는 이름은 단독으로 RDecl을 찾을 수 있는 것처럼 보였지만, 실제 역할은 parent container의 key였다. `RDeclKey`가 이 제약을 드러낸다.

`RLocalIdentifier`는 local variable과 혼동되고, `RScopedIdentifier`는 scope path까지 포함하는 것처럼 들리므로 사용하지 않는다. `RMemberIdentifier`도 namespace/type/function을 모두 포괄하는 tree child key로는 좁다.

기존 `RGlobalIdentifier`/`RGlobalTypeIdentifier` 계열은 각각 `RIdentifier`/`RTypeIdentifier`로 정리한다. `Identifier`라는 기본 이름은 실제 global resolution이 가능한 identity에만 쓴다.
