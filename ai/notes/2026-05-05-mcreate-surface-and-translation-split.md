# 회의 / 설계 노트

Date: 2026-05-05
Title: `MCreate`는 MIR 표면에 유지하고 translation primitive만 분리

Status
- current

Summary
- `MCreate`는 당분간 MIR 표면 타입으로 유지한다.
- 대신 lowering/translation의 중심 primitive에서는 `TranslateMCreate-*`를 축으로 보지 않고, `TranslateMExp`, `TranslateMLoc`, `TranslateMInitExp`로 역할을 분리한다.
- `MCreate_BC`는 create-context payload로는 유지하되, translation에서는 본질적으로 read/value 경로로 처리한다.
- `MCreate_NBC`는 계속 destination-aware init 경로로 처리한다.
- 따라서 현재 변경 방향은 “`MCreate` 제거”가 아니라, “`MCreate`를 일괄 lowering하던 부분에서 `MCreate_BC`를 분리하여 read primitive를 타게 하는 것”이다.

Context
- 최근 논의에서 다음 방향이 유력해졌다.
  - BC는 value/read 중심
  - NBC는 init/place 중심
  - `MRead`의 1차 축은 `BC/NBC`가 아니라 `Exp/Loc`
  - QIR 쪽 slot 표현 체계는 크게 바꾸지 않고, 필요시 `managed_ptr` annotation만 추가
- 이 흐름에서 `MCreate`를 완전히 제거하는 방안도 검토했으나, `MStmt_Return`, `MStmt_Exp`, local init, `MLoc_Materialize` 같은 create-consumer들은 여전히 “BC/NBC를 한 자리에서 받는 MIR surface 타입”을 필요로 한다는 점이 다시 확인되었다.
- 반면 translation primitive 관점에서는:
  - `MInitExp`는 destination을 받아 처리하는 쪽이 자연스럽고
  - `MExp`는 destination 없이 value result를 만드는 쪽이 자연스럽다.
- 즉 `MCreate_BC`와 `MCreate_NBC`를 동일한 lowering 인터페이스에 억지로 맞추는 것이 오히려 부자연스럽다는 점이 드러났다.

Decision
## 1) `MCreate`는 MIR 표면에서 유지
- 다음 consumer들은 계속 `MCreate`를 받는 형태를 유지한다.
  - `MStmt_Return`
  - `MStmt_Exp`
  - local var init (`MStmt_LocalVarDeclInit_Create`)
  - `MLoc_Materialize`
- 이유:
  - MIR surface에서는 여전히 “이 자리는 create-context다”라는 문맥 정보가 유용하다.
  - 이 consumer들의 필드 타입까지 한 번에 바꾸면 되돌리기 비용이 커진다.

## 2) `TranslateMCreate-*`는 중심 primitive에서 내린다
- lowering의 중심 primitive는 다음 셋으로 둔다.
  - `TranslateMExp(...)`
  - `TranslateMLoc(...)`
  - `TranslateMInitExp(initExp, dest, ...)`
- `TranslateMRead(...)`는 필요하면 public entry로 유지하되, 내부적으로 다음처럼 adapter로 동작시킨다.
  - `MRead_Exp` -> `TranslateMExp`
  - `MRead_Loc` -> `TranslateMLoc` 결과를 read result로 적응
- `TranslateMCreate(...)`가 남더라도, 공통 lowering primitive가 아니라 얇은 dispatch wrapper 수준으로만 본다.

## 3) `MCreate_BC`는 translation에서 read/value primitive로 처리
- `MCreate_BC`는 MIR surface에서는 계속 `MCreate`의 BC variant로 남긴다.
- 다만 translation에서는 이를 “destination-aware BC create primitive”로 보지 않는다.
- 대신 본질적으로 “create-context에 들어온 BC payload”로 보고, 내부적으로는 read/value 경로로 처리한다.

권장 해석:
- `MCreate_BC { MExp* exp; }`
- lowering 시 내부적으로 `TranslateMExp(exp, ...)` 호출

주의:
- 이것은 `MCreate_BC`의 타입 자체를 `MRead`로 바꾼다는 뜻이 아니다.
- 또한 `MCreate_BC`를 `MRead_Loc`까지 받는 일반 read wrapper로 확장하지 않는다.
- BC lvalue를 create-context에 넣을 때는 기존처럼 `MExp_Load(loc)`를 거쳐 `MCreate_BC(MExp_Load(loc))`로 정규화한다.

## 4) `MCreate_NBC`는 destination-aware init primitive로 처리
- `MCreate_NBC`는 계속 `MInitExp` wrapper로 유지한다.
- lowering에서는 이를 다음처럼 처리한다.
  - `TranslateMInitExp(initExp, dest, ...)`
- 즉 NBC 쪽은 현재 합의대로 “value result를 돌리는 계산”이 아니라 “어떤 place/destination을 초기화하는 계산”으로 본다.

## 5) `MExp_Load` 역할은 당분간 유지
- `MCreate_BC`를 read primitive로 처리하더라도, `MExp_Load`를 지금 단계에서 함께 축소/제거하지는 않는다.
- 따라서 create-context의 BC lvalue는 계속 아래 형태를 유지한다.
  - `Loc(l) (BC)` -> `MCreate_BC(MExp_Load(l))`
- 이 결정은 `MExp_Load` 축소 논의를 다음 단계로 미룬다는 뜻이며, 이번 변경의 범위를 translation primitive 재배치로 제한한다.

## 6) `MArgument` surface도 유지하고, `MArgument_Create(MCreate_BC)`만 lowering에서 read로 처리
- `MArgument`의 surface shape는 지금 단계에서 바꾸지 않는다.
- 즉 전달 형태 축은 계속 유지한다.
  - `MArgument_Create`
  - `MArgument_Loc`
  - `MArgument_Move`
- 다만 `MArgument_Create(MCreate_BC{...})`는 lowering에서 `TranslateMExp` 또는 BC read/value 경로를 사용해 처리한다.
- 이것은 by-value 전달 semantics를 바꾸는 것이 아니라, BC payload의 구현 primitive만 read/value 쪽으로 정리하는 것이다.

Implications
## `MStmt_Return`
- 계속 `MCreate` consumer로 둔다.
- BC return:
  - surface: `MCreate_BC`
  - lowering: read/value 경로
- NBC return:
  - surface: `MCreate_NBC`
  - lowering: destination-aware init 경로

## `MStmt_Exp`
- 계속 `MCreate` consumer로 둔다.
- BC expression statement는 내부적으로 value evaluation/read primitive를 사용해 처리한다.
- NBC expression statement는 dummy destination 또는 discard-aware init 경로를 사용한다.

## local init
- `MStmt_LocalVarDeclInit_Create`는 유지한다.
- BC local init은 local slot을 대상으로 BC value result를 기록하는 식으로 처리한다.
- NBC local init은 local storage를 destination으로 하여 init한다.

## `MLoc_Materialize`
- 계속 `MCreate`를 받는다.
- BC materialize는 BC value를 temp storage에 기록하는 경로를 사용한다.
- NBC materialize는 `MInitExp`를 temp destination에 init하는 경로를 사용한다.

## `MRead`
- 문맥 타입으로서의 `MRead`는 유지한다.
- 다만 translation implementation에서는:
  - `MRead_Exp` -> `TranslateMExp`
  - `MRead_Loc` -> `TranslateMLoc`
- 즉 `MRead`는 lowering primitive라기보다 read-context adapter 성격이 강해진다.

Rationale
- `MCreate`를 MIR에서 없애지 않으면 consumer field churn을 줄일 수 있다.
- 반면 lowering primitive를 `MExp` / `MLoc` / `MInitExp(dest)`로 분리하면, BC/NBC의 계산 모델 차이를 더 자연스럽게 반영할 수 있다.
- 특히 다음 비대칭을 억지로 숨기지 않는 것이 중요하다.
  - BC: value-driven
  - NBC: destination-driven
- `MCreate_BC`는 이 차이를 지우는 “대칭적인 create primitive”라기보다, create-context에 BC payload를 태우는 adapter로 보는 것이 더 자연스럽다.

Non-goals for this change
- 이번 단계에서 `MCreate` 자체를 제거하지 않는다.
- 이번 단계에서 `MStmt_Return`, `MStmt_Exp`, `MStmt_LocalVarDeclInit_Create`, `MLoc_Materialize`의 MIR surface 타입을 바꾸지 않는다.
- 이번 단계에서 `MExp_Load` 역할 축소/제거를 함께 진행하지 않는다.
- 이번 단계에서 `MArgument` variant 구조를 바꾸지 않는다.

Recommended implementation order
1. `TranslateMExp`, `TranslateMLoc`, `TranslateMInitExp(dest)` primitive를 정리한다.
2. `TranslateMRead`를 내부 adapter로 정리한다.
3. `MCreate` consumer lowering에서 `MCreate_BC`와 `MCreate_NBC`를 즉시 분기한다.
4. 기존 `TranslateMCreate-*` 경로는 제거하거나 얇은 dispatch wrapper로 축소한다.
5. 이후 실제 구현 경험을 바탕으로 `MCreate` surface 축소 필요성을 재평가한다.

Open points
- `TranslateMExp`의 정확한 반환 shape를 `slot only`로 고정할지, `const` 결과까지 포함할지 최종 명시 필요
- `MStmt_Exp`의 BC discard 경로에서 별도 result slot을 항상 둘지, expression kind별 fast path를 허용할지 결정 필요
- `MArgument_Create(MCreate_BC)` lowering에서 call argument slot materialization 정책을 공통 helper로 둘지 검토 필요
- `TranslateMCreate` 이름을 남길 경우, public helper인지 translator-private dispatch인지 역할을 명확히 할 필요가 있음
