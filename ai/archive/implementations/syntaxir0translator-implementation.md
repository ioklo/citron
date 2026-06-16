# SyntaxIR0Translator 구현 가이드

이 문서는 `Syntax`에서 MIR(IR0)과 `RDecl`을 생성하는 흐름을 구체적으로 설명합니다.

주요 진입점
- `src/SyntaxIR0Translator/PhaseManager.cpp` — 단계 관리
- `src/SyntaxIR0Translator/GlobalContext.*`, `FuncContext.*` — 심볼/스코프 상태

핵심 흐름(요약)
1. `TextAnalysis`의 출력(구문 트리)을 받아 `PhaseManager`가 변환 파이프라인을 시작합니다.
2. 모듈 수준 선언은 `ModuleDecls`가 모아 `GlobalContext`에 등록합니다. 이 과정에서 `NDecl` → `RDecl` 매핑이 생성됩니다.
3. 함수/구조체 등 개별 단위는 `*Task` (예: `GlobalFuncTask`, `StructTask`)로 분해되어 처리됩니다.
4. 표현식/문장은 `SExpToMExpTranslation.*` 계열 파일에서 MIR 노드로 변환됩니다(중간 단계: ImExp, ReExp, IrExp).

코드 참조 포인트(탐색 팁)
- `GlobalFuncTask.cpp`를 열어 `NDecl`이 `RDecl`로 등록되는 지점을 찾으세요.
- `SExpToMExpTranslation.cpp`에서 구체적인 표현식 변환 케이스를 확인하세요.

테스트
- 변환 기능을 수정한 뒤 `TextAnalysis.Tests` 또는 관련 모듈 테스트에 케이스를 추가하세요.
