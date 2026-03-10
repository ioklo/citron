# ReExp `StmtCall` / Top-Level `SStmt_Exp` 정책 정리 (2026-03-10)

## 목적
- `SExp` 번역 중 `void` 반환 call이 기존 `ReExp -> { MExp, MInitExp, MLoc }` 축에 들어가지 않는 문제를 정리합니다.
- `SStmt_Exp`가 top-level 문맥에서 어떤 `ReExp`를 허용하는지 명확히 합니다.
- top-level assign의 의미를 call로 뭉개지 않고 유지하는 방향을 정리합니다.
- `MRead` / `MCreate` naming을 BC/NBC 축으로 정리합니다.
- member access와 nested decl에서 `typeArgs` 명칭을 정리합니다.

## 배경
- 현재 `SExp`는 보통 다음 중 하나로 해석됩니다.
  - `MExp`
  - `MInitExp`
  - `MLoc`
- 그런데 `SExp_Call`이 `void`를 반환하면, 이는 값/생성/위치 어느 쪽에도 속하지 않습니다.
- 그렇다고 `ReExp`에 일반 `MStmt` 전체를 넣으면 의미가 너무 넓어집니다.
- 현재 언어 정책상 `1;`, `x;` 같은 순수 표현식 statement는 허용하지 않습니다.
- `SStmt_Exp`에서는 top-level side effect 표현식만 허용합니다.
  - assign expression
  - call expression

## 논의 요약

### 1) 일반 `ReExp_Stmt` 대신 구체 variant를 둔다
- 현재 필요한 statement 성격 결과는 소수이며, 아직 공통 `Stmt` sum으로 묶기에는 이릅니다.
- 따라서 당장은 다음처럼 구체 variant를 둡니다.
  - `ReExp_StmtCall`
  - `ReExp_StmtAssign`
- 장점:
  - 의미가 직접 드러납니다.
  - `ReExp`가 임의의 `MStmt`를 담는다는 오해를 막습니다.
  - 나중에 variant가 늘어나면 그때 `ReExp_Stmt{...}` 같은 상위 묶음 도입 여부를 재검토할 수 있습니다.

### 2) `MStmt_Call`과 value-returning call의 구분
- `MStmt_Call`은 **void 반환 call 전용**입니다.
- 반환값이 있는 call은 statement 위치에서 쓰이더라도 다음 중 하나로 남아야 합니다.
  - `MExp_Call`
  - `MInitExp_Call`
- 이유:
  - 반환값이 있는 call은 결과를 담을 공간 / 생성 의미가 필요합니다.
  - `MStmt_Call`은 그런 의미를 갖지 않는 순수 void-call statement입니다.

### 3) assign은 call로 뭉개지지 않게 유지한다
- `a = b;` 같은 top-level assign은 statement 문맥에서 side effect를 가지지만,
  그 의미를 단순 `call`로 낮추면 상위 의미가 사라집니다.
- 특히 NBC assign을 내부적으로 `copy_assign` / `move_assign` call로 lowering하더라도,
  MIR 상위 계층에서는 `assign` 의미를 유지하는 편이 낫습니다.
- 따라서 assign 성격 결과는 `ReExp_StmtAssign`으로 유지합니다.

### 4) bitwise assign과 non-bitwise assign
- 예시: `a = b;`
- BC(bitwise-copyable)인 경우:
  - 기존처럼 `MExp_Assign(dest, src)` 형태로 `ReExp_Exp`에 남길 수 있습니다.
- NBC인 경우:
  - 내부적으로 `copy_assign` / `move_assign` 호출을 쓸 수 있더라도,
    상위 해석 결과는 `ReExp_StmtAssign`으로 유지하는 쪽이 적절합니다.

### 5) NBC assign을 `MLoc`로 올려 chain을 허용할 것인가
- 한때 NBC assign을 `MLoc` 쪽으로 올리면 assign chain이 가능해질 수 있다는 검토가 있었습니다.
- 하지만 이는 원래 의도와 어긋납니다.
- NBC assign chain을 허용하면 `a = b = c` 같은 매우 단순한 표면 문법 뒤에
  copy/move/assign 의미가 연쇄적으로 숨어버립니다.
- 이를 “작성자가 감수해야 할 비용”으로 넘기기에는 `=` 문법이 너무 가볍습니다.
- BC도 일반화해서 허용 기준을 넓히기보다,
  기존 의도대로 primitive 중심의 제한적 chain 허용 정책을 유지하는 편이 낫습니다.
- 결론:
  - NBC assign은 `MLoc`로 승격해 chain을 허용하지 않습니다.
  - NBC assign은 `ReExp_StmtAssign`으로 유지합니다.
  - assign chain 허용은 기존 primitive 중심 정책을 유지합니다.

### 6) `MStmt_Assign`의 src는 `MRead_NBC`
- NBC assign은 초기화가 아니라 갱신(assign)입니다.
- 따라서 src는 `MCreate`가 아니라 읽기 입력이어야 합니다.
- 또한 NBC assign의 src는 항상 location 기반으로 정규화됩니다.
  - lvalue는 그대로 location 읽기
  - rvalue는 materialize 후 location 읽기
- 이 의미를 타입에 드러내기 위해 `MStmt_Assign`의 src는 `MRead_NBC`로 둡니다.
- `MLoc*`만 두는 것보다 다음 의미를 이름으로 드러낼 수 있습니다.
  - read 문맥이다
  - NBC 입력이다

### 7) `MRead` / `MCreate` naming을 BC/NBC 축으로 통일
- 기존 이름 후보:
  - `MRead_Value`, `MRead_Location`
  - `MCreate_Bitwise`, `MCreate_Init`
- 하지만 현재 정책의 핵심 축은 “표현 형태”보다 “BC / NBC”입니다.
- 따라서 naming도 다음처럼 맞춥니다.
  - `MRead_BC { MExp* exp; }`
  - `MRead_NBC { MLoc* loc; }`
  - `MCreate_BC { MExp* exp; }`
  - `MCreate_NBC { MInitExp* initExp; }`
- 장점:
  - `MRead`와 `MCreate`가 같은 의미 축으로 정렬됩니다.
  - 내부 표현보다 타입군/의미가 이름에 직접 드러납니다.
  - verifier 및 invariant 문서화가 쉬워집니다.

### 8) `typeArgs` / `outerTypeArgs` / `memberTypeArgs`
- nested member/type의 type arguments는 다음 naming으로 정리합니다.
  - 전체 적용 인자: `typeArgs`
  - 바깥 축 인자: `outerTypeArgs`
  - 현재 멤버 자신의 인자: `memberTypeArgs`
- 관계는 다음과 같습니다.
  - `typeArgs = outerTypeArgs + memberTypeArgs`
- 이 naming을 택한 이유:
  - 기존 Citron 관습에서 `TypeArguments` / `typeArgs`는 전체 적용 인자를 뜻하는 경우가 많습니다.
  - `fullTypeArgs = outerTypeArgs + typeArgs`처럼 두면 `typeArgs`가 전체인지 현재 멤버 것인지 혼동되기 쉽습니다.
  - `memberTypeArgs`는 현재 nested member/type 자신에게 직접 붙은 인자라는 뜻을 가장 안정적으로 전달합니다.

## `SStmt_Exp` 해석 규칙

### 전체 흐름
1. `SStmt_Exp` 내부의 `SExp`를 `ReExp`로 변환합니다.
2. top-level statement 문맥 전용 consumer가 `ReExp`를 `MStmt`로 변환합니다.

### 권장 consumer
- `TranslateReExpToTopLevelMStmt(...)` 같은 전용 translator를 둡니다.
- `SStmtToMStmtTranslation` 안에서 직접 variant를 계속 분기하지 않고, top-level 정책을 한 곳에 모읍니다.

### 허용 규칙
- `ReExp_Exp`
  - top-level assign expression 또는 value-returning call이면 `MStmt_Exp(...)`
- `ReExp_InitExp`
  - top-level init-returning call이면 `MStmt_InitExp(...)`
- `ReExp_StmtCall`
  - 내부의 `MStmt_Call`을 그대로 사용
- `ReExp_StmtAssign`
  - 내부의 `MStmt_Assign`을 그대로 사용
- 그 외
  - top-level statement로 허용하지 않고 진단

## 설계 결론
- `ReExp`는 계속 `ResolvedExp` 의미로 사용합니다.
- 하지만 일반 statement 전체를 담도록 넓히지 않습니다.
- 필요한 최소 확장으로 다음 variant를 추가합니다.
  - `ReExp_StmtCall`
  - `ReExp_StmtAssign`
- `SStmt_Exp`는 top-level 전용 `ReExp -> MStmt` translator를 통해 소비합니다.
- 반환값이 있는 call은 `MStmt_Call`로 낮추지 않습니다.
- NBC assign은 `call`로 뭉개지지 않게 `Assign` 의미를 유지합니다.
- NBC assign은 `MLoc`로 승격해 chain을 허용하지 않습니다.
- `MRead` / `MCreate`는 `Value/Location`, `Bitwise/Init`보다 `BC/NBC` 축의 naming을 사용합니다.
- nested member/type의 type arguments는 `typeArgs`, `outerTypeArgs`, `memberTypeArgs`로 구분합니다.

## 후속 작업
- [ ] `ReExp`에 `ReExp_StmtCall` variant 추가
- [ ] `ReExp`에 `ReExp_StmtAssign` variant 추가
- [ ] `void` 반환 `SExp_Call`을 `ReExp_StmtCall`로 만드는 경로 추가
- [ ] NBC top-level assign을 `ReExp_StmtAssign`으로 만드는 경로 추가
- [ ] `MRead_Value`, `MRead_Location` 사용처를 `MRead_BC`, `MRead_NBC`로 rename
- [ ] `MCreate_Bitwise`, `MCreate_Init` 사용처를 `MCreate_BC`, `MCreate_NBC`로 rename
- [ ] `typeArgs` 관련 이름을 `outerTypeArgs`, `memberTypeArgs` 기준으로 정리
- [ ] `ReExpToTopLevelMStmtTranslation` 초안 작성
- [ ] `SStmtToMStmtTranslation`의 top-level 처리에서 새 translator 사용
