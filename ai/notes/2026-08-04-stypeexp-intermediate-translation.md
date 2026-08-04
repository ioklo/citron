# 회의 / 설계 노트

날짜: 2026-08-04

제목: `STypeExp`의 intermediate / resolved 번역 단계

## 목적

`STypeExp`를 일반 타입(`RType*`) 또는 적용된 trait declaration으로 번역하는 경로를,
value expression의 `SExp -> ImExp -> ReExp` 단계 구조와 대응시킨다.

```text
STypeExp -> ImTypeExp -> ReTypeExp
SExp     -> ImExp     -> ReExp
```

여기서 `ReTypeExp`는 runtime expression이라는 뜻이 아니라 `ReExp`와 대응하는
"해석 완료 번역 단계"라는 뜻이다. 최종 결과는 다음 둘의 합으로 본다.

```text
RType*
RAppliedDecl<RTraitDecl>
```

## `ImTypeExp`의 canonical 표현

초기에는 `ImTypeExp_Class`, `ImTypeExp_Struct`와 `ImTypeExp_Type { RType* }`를 함께
두었다. 그러나 같은 `C<int>`가 declaration 기반 variant와 `RType_Class` 기반 variant
양쪽으로 나타날 수 있었다. 그러면 이후 단계가 어떤 경로로 왔는지에 따라 qualified
member lookup 가능 여부가 달라질 수 있다.

따라서 현재 intermediate의 variant는 다음으로 제한한다.

```text
ImTypeExp_Namespaces
ImTypeExp_Type  { RType* type; }
ImTypeExp_Trait { RAppliedDecl<RTraitDecl> appliedDecl; }
```

- class, struct, enum, enum element, primitive, `void`, type parameter 등 trait이 아닌
  타입 결과는 모두 `ImTypeExp_Type` 하나로 나타낸다.
- `string`은 struct이지만 다른 nominal type과 마찬가지로 `RType_Struct`를 담는다.
- trait은 `RType`이 아니므로 적용된 trait declaration으로 별도 보관한다.
- namespace는 chain 중간에만 유효하며 최종 타입/trait 결과가 될 수 없다.

이 구분은 "현재 member가 있는가"가 아니라 intermediate가 보관하는 canonical semantic
표현에 따른 것이다. 따라서 이후 `ImTypeExp` consumer가 member lookup 외의 일을 맡아도
의미가 유지된다.

## Identifier와 member 번역

### Identifier

`STypeExp_Id`는 다음 순서로 처리한다.

1. 예약 타입을 확인한다.
2. 아니면 현재 type resolve context에서 이름을 찾아 `SmTypeRes`를 얻는다.
3. explicit type arguments를 적용해 `ImTypeExp`를 만든다.

nominal type은 이때 이미 canonical `RType*`로 materialize한다. 따라서 finalization에서
class/struct를 다시 분기해 factory를 호출하지 않는다.

### Member

`STypeExp_Member`는 base를 먼저 `ImTypeExp`로 번역한 뒤 다음처럼 처리한다.

- namespace base는 namespace child lookup을 수행한다.
- `ImTypeExp_Type` base는 `RType` visitor로 qualified type-member lookup을 수행한다.
  `RType_Class`와 `RType_Struct`는 applied declaration과 applied arguments를 사용해
  `GetTypeMember`를 호출한다.
- primitive, `void`, tuple, function, pointer, nullable, shared, box 등 type member를
  제공하지 않는 타입은 `Error_ResolveIdentifier_TypeCantHaveTypeMember`를 낸다.
- type parameter는 향후 trait constraint의 associated/nested type lookup을 지원할 수
  있으므로 현재는 별도 후속 구현 지점으로 둔다.
- trait은 현재 nested type을 지원하지 않으므로 `TraitCantHaveMember`를 낸다.

전체 applied arguments는 `outerTypeArgs + memberTypeArgs`다. 이 규칙은 nested trait에도
동일하게 적용하며, trait의 explicit member type arguments를 버리지 않는다.

type parameter는 lexical binder이지 qualified member가 아니므로 `S<int>.T` 같은 projection은
허용하지 않는다.

## Type constructor 경계

`Nullable`, `Shared`, `Box`, `Ptr`, `Local` 같은 type constructor는 identifier/member
chain intermediate에 넣지 않는다. 각 operand를 재귀적으로 `RType*`로 번역한 후 factory를
적용한다. 따라서 trait 결과에는 이 constructor들을 적용할 수 없다.

## `RAppliedDecl`의 적용 기준

`RAppliedDecl<T>`은 `T` 자신에게 남은 type parameter가 없는 fully-applied declaration
relation이다. `ROuterAppliedDecl<T>`은 lexical outer arguments만 적용되어 있고, member
자신의 type arguments를 더 받아야 하는 lookup 상태다.

- class/struct/enum 같은 nominal type name lookup은 explicit member type arguments를
  받아야 하므로 `ROuterAppliedDecl` 상태를 거친다.
- variable, enum element, 그리고 현재처럼 자기 own type parameter가 없는 lambda는 outer
  arguments까지 적용되면 `RAppliedDecl`로 보관한다.
- function overload group은 explicit function type arguments와 inference가 남아 있으므로
  `RAppliedDecl`로 성급하게 닫지 않고 partial-application 모델을 유지한다.

## Expression 번역기와의 관계

두 경로는 단계 구조만 대응시키고 구현을 통합하지 않는다.

- expression 번역은 value/location/call/assignment, overload matching, MIR materialization을
  다룬다.
- type 번역은 namespace/type-member lookup과 generic argument application을 다룬다.

공유가 필요해지면 `outerTypeArgs + memberTypeArgs` 결합, arity 검사, namespace child lookup
같은 작은 helper만 공통화한다.

## 명명 메모

SmTranslator 소유 자료형에 `Sm` prefix를 붙이는 방향과 기존 `ImExp`/`ReExp`는 완전히
일관되지는 않는다. 기계적인 `SmImExp`/`SmReExp` 대신 장기 rename 후보는 다음과 같다.

```text
ImExp -> SmIntermediateExp
ReExp -> SmResolvedExp
```

다만 이름 변경은 영향 범위가 넓으므로 현재 type translation 및 `RAppliedDecl` 정리와는
분리한다.
