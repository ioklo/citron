# 회의 / 결정 노트

Date: 2026-04-03
Title: `MTopLevel_*` 도입 및 `labelId` 정책 정리

Status
- current

Summary
- `MLoc_Materialize`의 임시 수명 경계를 MIR에서 더 잘 드러내기 위해 `MFullExpr_*` 대신 `MTopLevel_*` 계열을 도입한다.
- `MStmt_Scope`는 lexical scope를 나타내며, `MLoc_Materialize`의 수명은 `MTopLevel_*`이 담당한다.
- `MTopLevel_*`는 독립 IR 계층보다는 `MStmt` 전용 top-level consumer wrapper로 본다.
- label은 source에서는 이름으로 쓰이지만, MIR에서는 함수 단위 unique한 `labelId`로 resolve해서 사용한다.
- label 이름 shadowing은 유연성은 있지만 가독성이 크게 떨어지므로 금지하는 쪽을 우선 선호한다.

Context
- 기존 논의에서 MIR의 목표는 "Syntax(Text)에 숨어 있는 의미를 implicit 없이 드러내는 것"으로 정리되었다.
- 동시에 사람이 MIR를 봤을 때 source와 크게 동떨어져 보이지 않는 것도 부목표로 유지하고 싶었다.
- `MStmt_Scope`를 도입해 lexical scope를 보이게 하는 방향은 구현과 디버깅 측면에서 편했지만,
  `MLoc_Materialize`의 수명을 `MStmt_Scope`에 싣는 것은 의도하지 않았다.
- 문제의 핵심은 expression-like statement들의 top-level consumer에서 생성되는 임시 객체의 수명 경계를
  어디에 귀속시키는지가 불분명하다는 점이었다.

Decision
## 1) `MStmt_Scope`의 역할
- `MStmt_Scope`는 lexical scope를 나타낸다.
- local visibility, block boundary, label visibility 같은 lexical 정보는 `MStmt_Scope` 기준으로 해석한다.
- `MLoc_Materialize`의 수명은 `MStmt_Scope`가 아니라 `MTopLevel_*` 기준으로 해석한다.

## 2) `MTopLevel_*` 도입
- `MFullExpr_*`라는 이름 대신 `MTopLevel_*`를 사용한다.
- 이유:
  - 이 wrapper들은 일반 expression tree 전체를 위한 타입이 아니라,
    `MStmt`가 직접 소비하는 top-level payload를 감싸는 용도에 가깝다.
  - `full-expression`은 semantics 설명 용어로는 유효하지만, 타입 이름으로는 과하게 넓은 느낌이 있었다.

정의한 타입:
- `MTopLevel_Read`
- `MTopLevel_Create`
- `MTopLevel_Loc`
- `MTopLevel_Assign`
- `MTopLevel_Call`
- `MTopLevel_Command`

의도:
- `MTopLevel_*`는 top-level evaluation boundary를 드러낸다.
- 해당 `MTopLevel_*` 내부에서 도달 가능한 `MLoc_Materialize`는 그 top-level evaluation이 끝날 때까지 산다.

## 3) `MTopLevel_*`를 쓰는 위치
- `MTopLevel_Read`
  - `MStmt_If.cond`
- `MTopLevel_Create`
  - `MStmt_LocalVarDeclInit_Create.create`
  - `MStmt_Return.create`
  - `MStmt_Exp.create`
  - `MStmt_Foreach.iterCreate`
  - `MStmt_Yield.valueCreate`
- `MTopLevel_Loc`
  - `MStmt_LocalRefDecl.loc`
- `MTopLevel_Assign`
  - `MStmt_Assign.assign`
- `MTopLevel_Call`
  - `MStmt_Call.call`
- `MTopLevel_Command`
  - `MStmt_Command.command`

메모:
- `MStmt_Command`는 처음엔 `MTopLevel_Read`에 넣을 수도 있다고 보았지만,
  `@{ ... }` 구문은 단순 read보다 command block/imperative block 성격이 더 강하다고 판단했다.
- 따라서 `MTopLevel_Command`를 별도 타입으로 둬서
  top-level boundary 정보와 기존 `MRead_Loc` 기반 payload 정보를 함께 보존한다.

## 4) 타입 배치 위치
- `MTopLevel_*`는 별도 헤더로 분리하지 않고 우선 `MStmt.h` 안에 둔다.
- 이유:
  - 현재 용도는 `MStmt` 전용 helper type에 가깝다.
  - 실험 단계에서 별도 계층처럼 보이게 만들 필요가 없다.
  - 나중에 usage가 넓어지면 그때 분리할 수 있다.

## 5) 표현 방식
- `MTopLevel_*`는 포인터 기반 다형 계층이나 공통 `variant`로 만들지 않는다.
- 각 `MStmt`가 필요한 concrete `MTopLevel_*` struct를 직접 멤버로 가진다.
- 이유:
  - 소유권과 수명 관리가 단순하다.
  - `MFactory`와 별도 allocation이 필요 없다.
  - 현재 단계에서는 독립 IR 노드보다 stmt payload wrapper에 가깝다.

## 6) 검증/테스트에 대한 입장
- `MTopLevel_*`는 correctness를 완전히 보장하는 장치가 아니라,
  MIR에서 top-level evaluation boundary를 더 읽기 쉽게 드러내는 marker에 가깝다.
- 따라서:
  - 상세 semantic correctness는 여전히 테스트가 필요하다.
  - verifier도 최소한은 필요하다.
- 다만 `MTopLevel_*`를 도입해도 테스트와 verifier가 필요하다는 사실은
  도입 반대 근거가 아니라, 역할 분담의 문제로 본다.

Rationale
- `MStmt_Scope`와 `MLoc_Materialize` 수명은 서로 다른 축의 정보다.
  - `MStmt_Scope`: lexical scope
  - `MTopLevel_*`: temporary/full-expression-like boundary
- 이 둘을 구조적으로 완전히 합치려 하면 오히려 verifier 부담이 커지고,
  `for` desugaring 같은 곳에서 syntax fidelity와 semantic explicitness가 불필요하게 충돌한다.
- 현재 구조는 구현자가 block과 top-level consumer를 덜 까먹게 해 준다는 장점이 있다.
- 따라서 둘을 분리하되, 역할을 명확히 나누는 쪽이 더 실용적이라고 판단했다.

## 7) label 이름과 `labelId`
- source syntax에서 label은 문자열 이름으로 쓴다.
- MIR에서는 문자열 대신 resolved `labelId`를 쓴다.
- `labelId`는 함수 단위로 unique하게 발급한다.

권장 형태:
```cpp
struct MLabelId
{
    int value;
};
```

이유:
- MIR에서 중요한 것은 "이 break/continue가 어느 target을 가리키는가"이지, 이름 자체가 아니다.
- 함수 단위 unique id는 비교와 verifier가 단순하다.
- depth/scope-path 기반 id는 lowering 구조가 바뀌면 의미가 흔들릴 수 있다.

## 8) label 이름의 유일성 규칙
- source label 이름은 lexical visibility 기준으로 해석한다.
- 다만 shadowing은 허용하지 않는 쪽을 우선 선호한다.

이유:
- 변수 shadowing과 달리 label shadowing은 control-flow target을 바꾸므로 혼동이 크다.
- 예: 중첩된 `foo:` 안에서 `break foo;`가 어느 `foo`를 가리키는지 사람이 다시 계산해야 한다.
- label은 자주 쓰는 이름 공간이 아니므로, 엄격하게 두더라도 실사용 불편이 크지 않을 가능성이 높다.

권장 정책:
- translator는 함수 단위 label env를 관리한다.
- 새 label 선언 시 기존 visible label과 이름이 겹치면 에러를 낸다.
- MIR에는 resolved `labelId`만 남긴다.
- printer/debug 필요 시 원래 label 이름은 부가 정보로만 유지할 수 있다.

Open points
- `MTopLevel_*`에 대한 최소 verifier invariant를 어디까지 둘지
- `MStmt_For`, `MStmt_While`, `MStmt_Directive`에도 top-level wrapper를 확장할지
- label shadowing을 lexical scope 단위 허용으로 완화할지, 함수 전체 중복 금지로 더 강화할지
- printer에서 `labelId`를 그대로 숫자로 출력할지, debug name mapping을 둘지

Action Items
- [ ] `MPrinter`를 `MTopLevel_*` 구조에 맞게 갱신
- [ ] translator에서 `MStmt_Call`, `MStmt_Assign`, `MStmt_Return`, `MStmt_Exp` 생성 경로가 `MTopLevel_*`을 사용하도록 정리
- [ ] label 선언/참조 resolve 규칙을 translator 쪽에서 함수 단위 `labelId`로 정리
- [ ] label shadowing 허용 여부를 실제 syntax/diagnostic 정책으로 최종 확정
