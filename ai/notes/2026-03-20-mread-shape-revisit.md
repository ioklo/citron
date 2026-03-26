# 회의 / 설계 노트

Date: 2026-03-20
Title: `MRead` 형태 재검토 - `BC/NBC` 고정보다 `Exp/Loc` 표현 축으로 복원

Status
- current

Summary
- 기존 `MRead_BC { MExp* }`, `MRead_NBC { MLoc* }` 구조는 `read`의 의미 축과 `Exp/Loc` 표현 축을 결합한 형태였다.
- 이 구조는 `BC loc read`를 `MExp_Load(loc)`로 억지 승격하게 만들고, 이후 lowering에서 불필요한 임시/슬롯 생성 논의를 반복하게 만들 수 있다.
- 이번 논의에서는 `MRead`를 다시 `Exp/Loc` 축으로 보는 쪽이 더 자연스럽다는 데 의견이 모였다.
- 방향:
  - 일반 읽기 자리는 `MRead`
  - loc 기반 읽기만 허용하는 자리는 `MRead_Loc`
  - 진짜 place 자체가 필요한 자리는 `MLoc*`
- 즉, 필드 타입에서 `BC/NBC` 표식을 제거하고, 그 제약은 underlying type 및 verifier/생성 규칙으로 관리하는 쪽을 선호한다.

Context
- 이전 결정에서는 다음 정규화를 사용했다.
  - BC read는 `MRead_BC(MExp*)`
  - NBC read는 `MRead_NBC(MLoc*)`
  - BC lvalue를 읽어야 하면 `MExp_Load(loc)`로 승격
  - NBC rvalue를 읽어야 하면 `MLoc_Materialize(...)` 후 `MRead_NBC`
- 이 구조는 naming 상으로는 깔끔했지만, 실제로는 다음 두 축을 합쳐 버렸다.
  - 값 성질 축: `BC / NBC`
  - 입력 표현 축: `Exp / Loc`
- 특히 `if` 조건처럼 본질적으로는 "read"만 요구하는 자리에서도 `MRead_BC`가 들어가다 보니,
  `MLoc`에서 오는 BC 읽기를 모두 `MExp_Load`로 감싸게 되었고,
  MIR의 read-wrapper와 QIR의 실제 temp/materialize 책임이 섞이는 문제가 드러났다.

Related notes
- `ai/notes/2026-03-03-remove-mcreate-direction.md`
- `ai/notes/2026-03-09-sexp-translation-read-create-loc.md`
- `ai/notes/2026-03-09-reexp-stmtcall-top-level-policy.md`
- `ai/notes/2026-03-12-store-src-read-not-create.md`
- `ai/implementations/mir-value-model.md`

Decision
## 1) `MRead`의 주 축은 `BC/NBC`보다 `Exp/Loc`
- `MRead`는 "읽기 문맥의 입력"을 나타내는 타입으로 본다.
- 따라서 `MRead`의 1차 분류 축은 값 성질(`BC/NBC`)이 아니라 입력 표현(`Exp/Loc`)이다.

권장 형태:
- `MRead_Exp { MExp* exp; }`
- `MRead_Loc { MLoc* loc; }`
- `using MRead = variant<MRead_Exp, MRead_Loc>;`

## 2) `BC/NBC`는 필드 타입 이름에서 제거
- `MStmt_If.cond` 같은 필드는 더 이상 `MRead_BC`를 사용하지 않는다.
- 대신 `MRead`를 사용하고, 이 자리가 실제로 허용하는 타입 제약은 semantic rule / verifier / 생성 규칙에서 판단한다.
- 즉 필드 타입이 말하는 것은:
  - 이 자리는 `read` 문맥인가
  - 이 자리는 `loc-based read`만 허용하는가
  - 이 자리는 진짜 `loc` 자체가 필요한가
  뿐이다.

## 3) `MRead_Loc`은 `NBC`를 뜻하지 않는다
- `MRead_Loc`은 "loc 형태로 읽는다"만 뜻한다.
- `MRead_Loc` 안의 loc가 BC 타입일 수도 있고 NBC 타입일 수도 있다.
- 따라서 `MRead_NBC -> MRead_Loc`은 부분적으로는 맞지만, 의미상 완전한 rename은 아니다.
- 중요한 것은 필드마다 다음 중 무엇이 필요한지를 구분하는 것이다.
  - 아무 read나 가능: `MRead`
  - loc 형태 read만 가능: `MRead_Loc`
  - place 자체가 필요: `MLoc*`

## 4) `MLoc*`와 `MRead_Loc`은 구분 유지
- `MLoc*`는 "저장 위치 자체"를 뜻한다.
- `MRead_Loc`은 "그 위치를 읽기 입력으로 사용"함을 뜻한다.
- 따라서 read/create/loc 문맥 분리를 유지하려면 둘을 다시 하나로 합치지 않는다.

Examples
- `MStmt_If.cond`
  - 기존: `MRead_BC`
  - 새 방향: `MRead`
- `MExp_Store.src`
  - 기존 후보: `MRead_BC`
  - 새 방향: `MRead`
- NBC assign source처럼 "반드시 location 기반 read"인 자리
  - 기존: `MRead_NBC`
  - 새 방향: `MRead_Loc`
- `MStmt_LocalRefDecl.loc`
  - 계속 `MLoc*`
  - 이유: 이 자리는 read가 아니라 loc 자체를 요구한다

Rationale
## `MStmt_If`는 본질적으로 `BC read`가 아니라 `read`
- `if`가 요구하는 것은 "조건을 읽는다"는 문맥 의미다.
- 추가 제약은 결과 타입이 `bool`이어야 한다는 점이지, 필드 타입 이름이 `BC`를 직접 들고 있어야 한다는 뜻은 아니다.
- `MRead_BC`를 필드 타입으로 박아 두면 문맥 제약과 값 성질 제약이 한 타입에 과하게 결합된다.

## `MExp_Load`를 read-wrapper로 억지 사용하지 않게 한다
- `BC loc read`를 모두 `MExp_Load(loc)`로 승격하는 정책은 표현 정규화에는 도움이 되었지만,
  MIR에서의 "read"와 lowering에서의 "실제 load/temp 생성" 책임을 흐리게 만들었다.
- `MRead_Loc`를 허용하면 BC loc read를 더 직접적으로 표현할 수 있다.

## `BC/NBC`는 invariant로 충분하다
- `MRead_Exp`에는 BC만 허용
- NBC rvalue를 read 문맥에 넣어야 하면 `MLoc_Materialize(...)` 후 `MRead_Loc`
- `MRead_Loc`는 BC/NBC 모두 가능
- 이 규칙은 필드 타입 이름보다 생성 규칙과 verifier에서 관리하는 편이 더 자연스럽다.

Field guideline
다음 기준으로 필드 타입을 고른다.

- `MRead`
  - 읽기 문맥이고, `Exp`/`Loc` 둘 다 허용 가능할 때
  - 예: `if` 조건, BC store source 등

- `MRead_Loc`
  - 읽기 문맥이지만, location 형태 입력만 허용할 때
  - 예: NBC assign source처럼 materialize/borrow된 place를 직접 읽는 자리

- `MLoc*`
  - 읽기가 아니라 place 자체가 필요한 자리
  - 예: ref 선언 대상, assign/store destination, member base location 등

Implications
- `MRead_BC`, `MRead_NBC` naming은 장기적으로 유지하지 않는 쪽을 선호한다.
- `MRead` 계층을 `Exp/Loc` 축으로 재정렬하면, 기존 `MRead_NBC` 사용처는 일괄 rename 대상이 아니라 의미별 재분류 대상이 된다.
- 따라서 마이그레이션은 "기계적 rename"보다 각 필드의 문맥을 다시 분류하는 방식으로 진행해야 한다.

Open points
- `MRead_Exp` 허용 범위를 생성 시점에 막을지, verifier에서 막을지 최종 결정 필요
- 기존 `MExp_Load`를 완전히 축소/제거할지, 일부 expression 자리에서만 유지할지 결정 필요
- 현재 `MRead_BC` / `MRead_NBC`를 쓰는 MIR 필드 전체를 `MRead` / `MRead_Loc` / `MLoc*`로 재분류한 표를 별도 문서로 정리할 필요가 있음

Action Items
- [ ] `MRead_BC`, `MRead_NBC` 사용 필드 재분류 표 작성
- [ ] `MRead = Exp | Loc` 기준 초안 반영
- [ ] `MRead_Exp` / `MRead_Loc` invariant를 생성 규칙 또는 verifier 규칙으로 명문화
- [ ] `MExp_Load`의 역할을 "read wrapper"에서 축소할지 검토
