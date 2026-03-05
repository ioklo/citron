# 회의 / 실험 노트

Date: 2026-03-05
Title: void 타입을 tuple<>와 분리 유지

Summary
- `void`를 `tuple<>`로 동일시하지 않고, 언어/IR에서 별도 타입으로 유지하기로 정리했다.
- 제네릭에서 `void`를 type argument로 받는 케이스는 지원하되, 값 연산은 제한하는 규칙을 둔다.
- 호출 표현은 반환 카테고리에 따라 분리하고, `void` 호출은 `MStmt_Call`로 다루는 방향을 채택한다.

Decisions
- `void`는 별도 타입으로 유지한다.
- `tuple`은 1개 이상 원소를 갖는 형태를 기본으로 하고, `tuple<>`를 `void`의 별칭으로 두지 않는다.
- `Generics`에서 `T=void`는 허용하되, `T`를 값으로 생성/저장/복사하는 경로는 컴파일타임 규칙으로 제한한다.
- MIR에서 호출은 반환 성격으로 분리한다.
  - BC 반환: `MExp_Call`
  - NBC 반환: `MInitExp_Call`
  - `void` 반환: `MStmt_Call`

Rationale
- `void=tuple<>`는 타입 이론은 단순해지지만, 실행 모델에서 unit 값 저장(최소 1바이트)과 alias/ABI 고려가 계속 필요하다.
- 별도 `void` 타입은 런타임 오버헤드를 피하고, 호출 규약 및 lowering을 단순하게 유지할 수 있다.
- 현재 `MRead/MCreate/MLoc` 중심으로 재정렬 중인 MIR 방향과도 충돌이 적다.

Action Items
- [ ] `MStmt_Call` 노드 추가 및 call lowering 경로에서 `void` 반환 분기 연결
- [ ] `SExp` call 번역에서 반환 타입 기반(`BC/NBC/void`) 노드 선택 규칙 명문화
- [ ] 제네릭 타입체크에서 `T=void`일 때 금지/허용 연산 표 정리

Additional Decisions (follow-up)
- 표면 언어에서 `void`는 별도 타입으로 유지한다.
- 제네릭 인자에서 `T=void`는 허용하고, 인스턴스화 시 컴파일러 내부 전용 대체 타입(`__VoidSubst`)으로 자동 치환한다.
- `__VoidSubst`는 사용자에게 노출하지 않는다(문서/이름해석/오류메시지에서 숨김).
- `__VoidSubst`는 empty struct 기반으로 취급하되, 현재 정책상 크기 1바이트를 갖는다.
- 제네릭 경계/호출 경계에서 아래 자동 변환을 허용한다.
  - `void -> __VoidSubst`
  - `__VoidSubst -> void`
- 변환은 일반 식 문맥에서 무제한 허용하지 않고, `T=void` 인스턴스화 경계에서만 암시 적용한다.

Example
- `T G<T>() { return T(); }`
- `void F() { return G<void>(); }`
- lowering 개념: `G<void>`를 내부적으로 `G<__VoidSubst>`로 인스턴스화하고, 반환은 `void`로 재해석한다.

Nullable policy update
- 중첩 nullable(`??`)는 의미적으로 절대 병합하지 않는다.
- 현재 문법에서는 `(C?)?` 표기가 직접 불가능하므로 유지한다.
- 대신 타입 표현 계층에서는 `nullable<nullable C>`를 허용하고, 기존의 인위적 금지를 해제한다.
- `C?`는 전용 nullable 표현(압축/인플레이스)으로 취급하고, 나머지 `T?`는 일반 tagged nullable로 유지한다.
- 제네릭 `T?`는 `T`가 ref로 확정되더라도 표현을 압축형으로 자동 전환하지 않는다(항상 tagged 유지).

Naming update proposal
- 기존 `NullableRef` / `NullableValue` 이름은 의미가 구현 세부와 섞여 있어 교체한다.
- 새 이름 후보:
  - `NullableInplace<T>`: 인플레이스/압축 표현(nullable payload와 null 상태를 함께 표현)
  - `Nullable<T>`: 일반 tagged nullable

Implications
- `(C?)?` 의미는 타입 계층에서 `Nullable<NullableInplace<C>>`로 표현 가능해진다.
- `(S?)?`는 `Nullable<Nullable<S>>`로 표현한다.
- 제네릭 코드의 예측 가능성을 위해 `T?`는 인스턴스화 전후 표현이 바뀌지 않는다.

Foreach policy update
- `foreach`는 `for`로 완전 desugaring하지 않고, MIR에서 의미 보존 노드(`MStmt_Foreach`)로 유지한다.
- 기존 `MStmt_ForeachCast`는 별도 노드로 유지하지 않는 방향을 우선 검토한다(단일 `MStmt_Foreach`로 통합).
- 캐스팅 의미는 IR에서 보이도록 유지하되, 별도 stmt kind 분리 대신 `MStmt_Foreach` 내부 경로로 표현한다.

Foreach shape (draft)
- `MStmt_Foreach(iteratorType, iteratorName, iteratorExp, itemType, itemName, nextExp, body)`
- `iteratorExp`는 진입 시 1회 평가되어 `iteratorName`에 바인딩된다.
- 각 iteration에서 `nextExp`를 실행해 진행 여부를 판단하고 item 바인딩을 수행한다.

Next/Cast rule
- `nextExp`는 raw item 생산 및 필요 cast를 내부에 포함하는 방향을 채택한다.
- `iterator.GetNext` 자체는 raw item을 제공하되, 최종 `itemType itemName` 바인딩이 가능하도록 `nextExp`에서 cast를 완료한다.
- `nullable<T>` 반환 방식 기반 next 계약은 채택하지 않는다(중첩 nullable/표현 충돌 회피).

Iterator.GetNext signature decision
- iterator protocol의 기본 next 시그니처를 아래로 둔다.
  - `nullable<TItem> GetNext();`
  - `nullable<TItem> GetNext() TError;`  // iteration 중 오류 전파가 필요한 경우

Semantics
- `GetNext`의 `value` 결과는 다음 item의 존재를 의미한다.
- `GetNext`의 `null` 결과는 정상 종료(end-of-iteration)를 의미한다.
- `TError` 채널은 종료와 별개의 실패 의미이며, `null`과 혼용하지 않는다.

Type policy alignment
- 제네릭 iterator의 `nullable<TItem>`은 기본적으로 tagged nullable 의미를 따른다.
- 비제네릭 경계에서의 압축 nullable 최적화는 별도 최적화 규칙으로 다룬다.

not_null talias safety policy
- `if (exp is not_null(alias))` 형태를 지원한다.
- `alias`에 `!`를 붙여 unchecked talias를 명시할 수 있다.
  - 예: `if (a.s is not_null(s!))`

Rules
- `not_null(alias)`:
  - checked talias를 요청한다.
  - 로컬 대상(local var)은 checked 검사를 수행한다.
  - 비로컬 대상(field/indexer/deref 등)은 checked 보장을 할 수 없으므로 경고를 낸다.
- `not_null(alias!)`:
  - unchecked talias를 강제한다.
  - 비로컬 대상에서는 경고 없이 허용한다.
  - 로컬 대상에서는 불필요한 unchecked 사용으로 경고를 낸다.

Diagnostics
- 비로컬 + `!` 없음: "checked talias를 보장할 수 없음, `alias!`로 의도 명시" 경고.
- 로컬 + `!` 있음: "로컬 대상에 unchecked talias는 불필요" 경고.

Notes
- 이 정책은 포인터 연산과 유사하게, 안전성 완화(unchecked)를 문법으로 명시하게 한다.
- v1에서는 완전한 전역 invalidation 추적 대신, local checked + non-local unchecked 명시 모델을 채택한다.
