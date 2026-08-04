# 회의 / 설계 노트

Date: 2026-08-04
Title: `STypeExp` intermediate / resolved translation 단계

## 목적

`STypeExp`를 `RType*` 또는 applied trait declaration으로 번역하는 경로를,
기존 value expression의 `SExp -> ImExp -> ReExp` 구조와 대응되게 정리한다.

## 결정 전 후보 방향

현재 내용은 구현 전 설계 방향이다. 이후 구현 세부나 기존 API와 충돌하면 다시
검토한다.

```text
STypeExp -> ImTypeExp -> ReTypeExp
SExp     -> ImExp     -> ReExp
```

- `ImTypeExp`는 type identifier/member chain을 계속 해석할 수 있는 intermediate다.
- `ReTypeExp`는 최종 type-expression 결과다.
- `ReTypeExp`는 다음 둘을 하나의 variant로 둔다.

```text
RType*
RAppliedDecl<RTraitDecl>
```

`ReTypeRes`보다 `ReTypeExp`를 택한다. 이는 결과가 runtime expression이라는 뜻이
아니라, `ReExp`와 동등한 "resolved translation stage"임을 이름으로 보이기
위해서다.

## 번역 흐름

### Identifier

`STypeExp_Id`는 다음 순서로 처리한다.

1. 예약 타입을 확인한다.
2. 아니면 현재 type resolve context에서 이름을 찾아 `SmTypeRes`를 얻는다.
3. explicit type arguments를 보존한 `ImTypeExp`를 만든다.

현재의 `SmTypeRes`는 identifier 하나의 lookup 원재료다. `ImTypeExp`는 여기에
member lookup을 이어 갈 수 있는 상태와 explicit member type arguments를 더한
표현이다.

### Member

`STypeExp_Member`는 base를 먼저 `ImTypeExp`로 번역한다. 이어서 base의 namespace
또는 applied declaration context에서 member를 찾아 새 `ImTypeExp`를 만든다.

- 전체 적용 인자는 `typeArgs`
- enclosing 축 인자는 `outerTypeArgs`
- 현재 member 자신이 받은 인자는 `memberTypeArgs`

로 구분하며, 최종 전체 인자는 `outerTypeArgs + memberTypeArgs`다.

type parameter는 lexical binder이지 qualified member가 아니다. 따라서
`S<int>.T` 같은 projection은 이 단계에서 허용하지 않는다.

### Finalization / consumers

`ImTypeExp -> ReTypeExp` 단계에서 다음을 수행한다.

- nominal type과 type variable 및 builtin type은 `RType*`로 완성한다.
- trait declaration은 `RAppliedDecl<RTraitDecl>`로 완성한다.
- namespace가 최종 결과로 남으면 진단한다.

그 뒤 public API는 얇은 consumer가 된다.

```text
TranslateSTypeExpToRType
  -> ReTypeExp를 만들고 RType* variant만 허용

TranslateSTypeExpToRTrait
  -> ReTypeExp를 만들고 RAppliedDecl<RTraitDecl> variant만 허용
```

## Type constructor 경계

`Nullable`, `Shared`, `Box`, `Ptr`, `Local` 같은 type constructor는
identifier/member chain intermediate에 넣지 않는다. 각 operand를 재귀적으로
`RType*`로 번역한 후 type factory를 적용한다. 따라서 trait 결과에는 이
constructor들을 적용할 수 없다.

## Expression translator와의 관계

두 경로는 단계 구조만 대응시키고 implementation을 통합하지 않는다.

- expression translation은 value/location/call/assignment, overload matching,
  MIR materialization을 다룬다.
- type translation은 namespace/type-member lookup과 generic argument application을
  다룬다.

특히 expression의 explicit function type argument는 candidate별 inference 전까지
partial application으로 남지만, type expression의 generic arguments는 type-name
resolution에 직접 사용된다. 공통 `Im*`/`Re*` hierarchy로 합치면 이 서로 다른
책임과 invariant가 섞인다.

공유가 필요해지면 `outerTypeArgs + memberTypeArgs` 결합, arity 검사, namespace child
lookup처럼 작은 helper만 공통화한다.

## 명명 메모

SmTranslator 소유 자료형에 `Sm` prefix를 붙이는 방향과 기존 `ImExp`/`ReExp`는
일관되지 않는다. 기계적인 `SmImExp`/`SmReExp` 대신, 장기 rename 후보는 다음과
같다.

```text
ImExp -> SmIntermediateExp
ReExp -> SmResolvedExp
```

다만 이름 변경은 영향 범위가 넓으므로 이번 type translation 작업과 분리한다.
`ImTypeExp`/`ReTypeExp`는 당분간 기존 단계 명명과 대응시키기 위해 사용한다.
