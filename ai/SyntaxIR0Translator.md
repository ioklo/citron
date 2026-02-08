# SyntaxIR0Translator 구현 안내

목적: `Syntax` 단계로부터 MIR(IR0)과 컴파일 중 사용되는 선언(`RDecl`)를 생성하는 모듈의 내부 구조와 기여 방법을 설명합니다.

핵심 요약
- 입력: `TextAnalysis`가 만든 구문 트리(`Syntax` 자료구조).
- 출력: MIR(IR0) 표현(파일명ㆍ디렉터리: `MIR/` 관련 구조체)과 런타임/중간 선언 `RDecl`(심볼).
- 주요 책임: 구문(문장/표현식)을 MIR로 변환하며, 필요 시 `RDecl` 계열 심볼을 생성/등록.

주요 파일 및 클래스(살펴볼 것)
- `PhaseManager.cpp` / `PhaseManager.h`: 변환 파이프라인 제어와 단계 실행.
- `GlobalContext.*`, `FuncContext.*`, `ScopeContext.*`: 각 스코프/함수/모듈 수준의 상태와 심볼 테이블 관리.
- `ModuleDecls.*`: 모듈 단위 선언(외부 노출 등) 처리.
- 변환 유틸/핵심 변환들: `SExpToMExpTranslation.*`, `ImExpToReExpTranslation.*`, `IrExpToMExpTranslation.*`, `SStmtToMStmtTranslation.*` 등 — 표현식/문장별 세부 변환 로직.
- 작업 단위(Task): `GlobalFuncTask.*`, `StructTask.*`, `EnumElemVarTask.*` 등은 병렬/단계별로 처리되는 단위 작업 예시.

`RDecl`, `NDecl`, `EDecl` 관계
- `NDecl`: 소스 코드로부터 직접 만들어지는 선언(예: 소스의 함수/변수 선언).
- `EDecl`: 모듈에서 외부로 노출되는 선언(모듈 경계를 넘어 공개되는 심볼).
- `RDecl`: 컴파일 과정에서 내부적으로 사용하는 선언 인터페이스.
  - 구현 관계: `NDecl`은 `RDecl`을 상속(구현)하고, `EDecl`은 `REDecl` 형태로 래핑되어 `RDecl` 인터페이스를 구현합니다.
  - `SyntaxIR0Translator`는 MIR 생성과 동시에 필요한 `RDecl`(예: 타입정보, 임시심볼)을 만들어 등록합니다.

코드 변경/기능 추가 시 체크리스트
1. 새로운 Syntax 노드 추가 → `Syntax/`에 문법 정의 추가
2. 구문 노드에 대한 변환 구현 추가
   - SExp/ SStmt 관련 변환은 `SExpToMExpTranslation.*`, `SStmtToMStmtTranslation.*` 등에 구현
   - 표현식 단계별로 ImExp/ReExp/IrExp 등 중간 표현을 거치므로 관련 변환 파일들을 함께 갱신
3. 심볼이 필요하면 `GlobalContext`나 적절한 `*Context`에서 `RDecl`을 생성/등록
4. 테스트 추가: 적절한 `*.Tests` (예: `TextAnalysis.Tests` 또는 모듈별 테스트)에 케이스 추가
5. 빌드 및 테스트 실행: `cmake --build` 후 `ctest --output-on-failure` 실행

간단한 예시(함수 추가 흐름)
1. `Syntax/`에서 함수 선언 노드 추가
2. `ModuleDecls`와 `GlobalFuncTask`에서 해당 선언을 읽어 `NDecl`을 만들고 `RDecl`로 등록
3. `FuncContext`에서 바디를 처리하며 `SStmtToMStmtTranslation` 등으로 MIR 생성
4. 생성된 MIR는 후속 패스(`IR0IR1Translator`)로 전달

디버깅 팁
- 변환 단계별로 로그(디버그 출력)를 확인: `PhaseManager` 또는 각 `*Context`의 상태 출력 지점에 로그 주석 추가
- 특정 노드의 변환 결과를 파일로 덤프하여 후속 단계 입력과 비교

참고 파일
- `src/SyntaxIR0Translator/PhaseManager.cpp`
- `src/SyntaxIR0Translator/GlobalContext.*`, `FuncContext.*`, `ModuleDecls.*`
- 변환기 예시: `src/SyntaxIR0Translator/SExpToMExpTranslation.*`, `ImExpToReExpTranslation.*`

더 필요한 내용
- 예제 코드 스니펫, 특정 함수 흐름(예: `GlobalFuncTask`의 전체 호출 그래프), 또는 `RDecl` 생성 코드 위치를 원하면 알려주세요. 상세히 문서화해 드리겠습니다.

*** 끝 ***
