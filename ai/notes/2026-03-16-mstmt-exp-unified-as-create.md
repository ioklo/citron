# 회의 / 결정 노트

Date: 2026-03-16
Title: `MStmt_Exp`를 `MCreate` 기반 statement wrapper로 통합

Status
- current

Summary
- top-level expression statement wrapper는 `MStmt_InitExp`와 `MStmt_Exp`로 나누지 않고, `MStmt_Exp` 하나로 통합한다.
- `MStmt_Exp`의 내부 표현은 `MCreate`로 둔다.
- 즉 statement 위치에서 허용되는 value-producing expression은 BC/NBC 구분과 무관하게 모두 `Create`로 정규화해서 소비한다.
- `MStmt_Exp`는 내부 식이 원래 `read`였는지 `create`였는지를 따로 저장하거나 판별하지 않는다.

Context
- 과거 `MInitExp` 도입 이전에는 `MStmt_Exp`가 있었고, C#처럼 top-level expression statement를 제한적으로 허용했다.
- 최근 `SExp` 번역은 `Loc`, `InitExp`, `Exp`, `Stmt_Assign`, `Stmt_Call` 중 하나로 자연스럽게 번역하는 방향으로 정리되었다.
- 또한 `read/create/loc`는 식 자체의 영구 속성이라기보다, 해당 식을 소비하는 문맥의 의미라는 점이 이미 정리되어 있다.
  - 참고: `2026-03-09-sexp-translation-read-create-loc.md`
  - 참고: `2026-03-12-store-src-read-not-create.md`
- 이 상태에서 top-level `SStmt_Exp` consumer가 `Exp`와 `InitExp`를 각각 다른 statement wrapper로 감쌀 필요가 있는지 재검토했다.

Problem
- `Exp`와 `InitExp`를 statement 위치에서 소비할 때, wrapper가 내부 식이 원래 `read`용인지 `create`용인지를 알아야 하는지 혼동이 생겼다.
- 그러나 statement expression wrapper의 역할은 “어떤 값을 평가/생성하고 결과를 버린다”이지, “읽기 입력을 받는다”가 아니다.
- 따라서 wrapper가 `Read` 관점으로 내부를 분류하는 것은 의미 축이 맞지 않는다.

Decision
- `MStmt_InitExp`를 별도 유지하지 않는다.
- `MStmt_Exp` 하나만 사용한다.
- `MStmt_Exp`의 payload는 `MCreate`로 둔다.
- top-level statement에서:
  - `ReExp_Exp`는 `MCreate_BC`로 감싸 `MStmt_Exp`로 변환한다.
  - `ReExp_InitExp`는 `MCreate_NBC`로 감싸 `MStmt_Exp`로 변환한다.
  - `ReExp_StmtCall`은 `MStmt_Call`을 그대로 사용한다.
  - `ReExp_StmtAssign`은 `MStmt_Assign`을 그대로 사용한다.
  - 그 외 variant는 진단한다.

Rationale
- statement wrapper가 소비하는 의미는 `Read`가 아니라 `Create`다.
  - `Read`는 이미 존재하는 값을 읽기 입력으로 넘기는 모델이다.
  - 반면 expression statement는 식을 평가시켜 side effect를 발생시키고 결과를 버리는 모델이다.
- `Exp`와 `InitExp`는 statement 위치에서 모두 “결과를 버리는 value-producing evaluation”로 수렴한다.
  - BC 결과는 `MCreate_BC`
  - NBC 결과는 `MCreate_NBC`
- 따라서 top-level consumer는 `Exp`/`InitExp`의 원래 출신을 구분해도 되지만, 최종 statement MIR에서는 둘 다 `MCreate`로 통일하는 것이 더 자연스럽다.
- 이렇게 하면 `MStmt_Exp`는 내부 식이 원래 `read`였는지 `create`였는지 같은 추가 메타정보를 가질 필요가 없다.

Meaning of `MStmt_Exp`
- `MStmt_Exp`의 의미는 다음처럼 본다.
  - “이 statement는 어떤 값을 생성하게 하고, 그 결과는 버린다.”
- 즉 `evaluate-and-discard` statement이며, `read` wrapper가 아니다.

Translator Rule
- 권장 top-level consumer는 다음 규칙으로 구현한다.
  1. `SStmt_Exp` 내부 `SExp`를 `ReExp`로 번역한다.
  2. `ReExp`를 top-level statement 정책에 따라 소비한다.
  3. value-producing variant는 모두 `MCreate`로 정규화한 뒤 `MStmt_Exp`로 감싼다.

Pseudo mapping
- `ReExp_Exp(exp)` -> `MStmt_Exp(MCreate_BC(exp))`
- `ReExp_InitExp(initExp)` -> `MStmt_Exp(MCreate_NBC(initExp))`
- `ReExp_StmtCall(stmtCall)` -> `stmtCall`
- `ReExp_StmtAssign(stmtAssign)` -> `stmtAssign`

Implications
- MIR statement 계층에서 “value-producing expression statement”는 `MStmt_Exp` 하나로 표현된다.
- `MStmt_Exp`는 BC/NBC 차이는 유지하되, 이를 `MCreate` 내부 variant로만 구분한다.
- `read/create/loc` 분류는 계속 translator entry 및 개별 MIR 인자의 의미 규정에 사용하고, `MStmt_Exp` 자체의 추가 분류에는 사용하지 않는다.

Follow-up
- [ ] `ReExp -> top-level MStmt` translator에서 `Exp`/`InitExp`를 `MCreate`로 정규화하는 경로 반영
- [ ] 기존 `MStmt_InitExp` 흔적이 있다면 제거 또는 `MStmt_Exp`로 합치기
- [ ] 관련 테스트에서 top-level value-producing expression statement 케이스를 `MStmt_Exp(MCreate)` 기준으로 갱신
