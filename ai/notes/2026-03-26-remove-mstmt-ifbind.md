# 회의 / 실험 노트

Date: 2026-03-26
Title: `MStmt_IfBind` 제거

Summary
- `MStmt_If`와 `MStmt_IfBind`를 분리해 두었지만, 실제 구현을 진행해 보니 stmt kind를 둘로 나눌 만큼의 의미 차이가 없었다.
- `if`의 핵심 의미는 "조건을 읽고 분기한다"는 점이며, bind 발생 여부는 별도 stmt 종류보다 조건 패턴/true branch 진입 규칙으로 다루는 편이 자연스럽다는 결론에 도달했다.
- 따라서 MIR에서는 `MStmt_If`만 유지하고 `MStmt_IfBind`는 제거하는 방향으로 정리한다.

Context
- 기존에는 `if (x is some s)` 같은 패턴 바인딩이 생기는 경우를 일반 `if`와 구분해서 `MStmt_IfBind`로 분리해 둘 수 있다고 보았다.
- 그러나 현재 구현에서는 translator가 실제로 `MStmt_IfBind`를 생성하지 않고, `if`는 모두 `MStmt_If`로 내려가고 있다.
- downstream lowering에서도 `MStmt_IfBind` 전용 의미는 아직 생기지 않았고, 별도 stmt kind를 유지할수록 visitor/translator 표면만 늘어난다.
- 최근 `MRead` 관련 논의에서도 `MStmt_If.cond`는 본질적으로 "`bool`이어야 하는 read"로 정리되고 있어, bind 유무로 stmt를 나누는 방향과 잘 맞지 않는다.

Decision
- `MStmt_IfBind`는 제거한다.
- MIR의 `if` stmt는 `MStmt_If` 하나만 유지한다.
- 조건식 안에서 bind가 발생하는 경우도 별도 stmt kind로 분리하지 않는다.
- bind의 존재는 아래 층위 중 하나에서 표현한다.
  - 조건 패턴/조건 번역 결과의 메타데이터
  - true branch 진입 시 주입되는 local/ref 선언
- 즉, "분기"와 "branch 안에서 유효해지는 바인딩"을 분리해서 다룬다.

Rationale
- 현재 `MStmt_If`와 `MStmt_IfBind`는 필드 구조가 동일하고, 차이는 의도 설명뿐이다.
- translator가 실제로 `MStmt_IfBind`를 만들지 않는다면, 타입 분리는 모델의 의미 차이가 아니라 미사용 표면적만 늘린다.
- lowering 관점에서도 필요한 것은 조건 판정과 scope 진입 시점의 binding 처리이지, stmt 종류의 증식이 아니다.
- `if`를 하나의 stmt로 유지하면 visitor, verifier, lowering, 테스트 표면이 단순해진다.
- `if is some`, type test bind, enum pattern bind 같은 케이스를 앞으로 확장하더라도, 공통 구조를 `MStmt_If` 하나에 모으는 편이 더 일관적이다.

Implications
- `MStmt` 계층에서 `MStmt_IfBind` 정의와 visitor 항목을 제거한다.
- `SyntaxIR0Translator` 쪽의 오래된 "`Bind`가 발생하면 `MStmt_IfBind`" 가정도 함께 제거한다.
- `IR0IR1Translator` 등 downstream의 빈 `Visit(MStmt_IfBind*)`도 제거한다.
- 이후 bind-carrying `if` 구현이 필요하면 stmt kind를 다시 늘리기보다, `MStmt_If` 내부 표현 또는 branch 진입 스텝을 확장하는 쪽을 우선 검토한다.

Action Items
- [ ] `MStmt_IfBind` 정의 및 관련 visitor 엔트리 제거
- [ ] `SyntaxIR0Translator`의 주석/가정 정리
- [ ] `IR0IR1Translator`의 `MStmt_IfBind` 빈 lowering 제거
- [ ] bind가 필요한 `if`를 `MStmt_If` 하나로 표현하는 구체 방식 정리
