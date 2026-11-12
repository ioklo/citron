# 셸 스크립팅 적합성 평가와 전환 보류

Date: 2026-11-09
Status: 평가 기록 및 사용자 확정 보류

## 논의 배경

- Citron의 과거 개발 목표 중 하나는 셸 스크립트 용도까지 지원하는 것이었다.
- 사용자는 현재 사용하는 스크립트를 Citron으로 작성하면 어떨지 검토하고자 했다.
- 사용자가 설명한 `@`로 시작하는 `Stmt_Command`의 원래 용도는 스크립트 명령 조합이다. 현재 소스의 대응 구문 노드는 `SStmt_Command`다.

## 현재 구현에서 확인한 사항

- 단일 행 `@...`, 여러 행 `@{ ... }`, `$변수` 및 `${표현식}` 문자열 보간을 지원한다.
- 명령 구문은 문자열 목록으로 번역되며, MIR의 `MTopLevel_Command` / `MStmt_Command`를 거쳐 QIR의 `Command_Item`으로 이어진다.
- 인터프리터는 `IEvalQDataCommandHandler::Execute(const std::string&)`에 문자열을 전달한다. 반환형은 `void`이며 명령 전용 종료 코드나 표준 출력 반환 경로가 없다.
- 평가 테스트의 핸들러는 명령 문자열을 이어 붙여 예상 문자열과 비교한다. 외부 프로세스 실행을 검증하는 테스트는 아니다.
- 네이티브 런타임의 `citron_command`는 현재 문자열을 `printf`로 출력한다.
- `foreach`는 `SmTranslator/SStmtToMStmt.cpp`에서 미구현 상태이며, 현재 agenda에서도 associated type 및 enumeration contract 관련 후속 작업을 검토 중이다.
- 이번 평가는 기록과 소스 확인에 기반한다. 빌드 및 실행 테스트는 수행하지 않았다.

관련 소스와 기록:

- `src/TextAnalysis/StmtParser.cpp`: `ParseSingleCommand`, `ParseCommandStmt`
- `src/SmTranslator/SStmtToMStmt.cpp`: `SStmt_Command`, `SStmt_Foreach` 번역
- `src/MqTranslator/MStmtToQInsts.cpp`: 명령별 `Command_Item` 생성
- `src/QEvaluator/Public/QEvaluator/QEvaluation.h`: 명령 핸들러 인터페이스
- `src/QEvaluator/QEvaluation.cpp`: 명령 핸들러 호출
- `src/EvalTests/EvalTests.cpp`: 문자열 누적 테스트 핸들러
- `data/TestData/EvalTests/Command_Statement_*.ct`: 단일 행, 블록, 보간 예제
- `src/RuntimeLibrary/RuntimeLibrary.cpp`: `citron_command`
- `ai/notes/2026-04-03-mtoplevel-and-label-id-design.md`: 명령 블록을 별도 `MTopLevel_Command`로 둔 이력

## 평가와 미확정 설계 쟁점

Citron의 조건문, 반복문, 함수와 `@` 명령 구문을 조합하는 형태는 빌드·테스트·파일 변환 등의 자동화 스크립트에 자연스럽다. 다만 현재 상태에서 실용적으로 사용하려면 프로세스 실행과 결과 처리, 파일·경로·환경변수 API, 스크립트 실행 도구 등의 기반이 더 필요하다.

아래 항목은 평가 과정에서 제시한 쟁점과 후보이며, 채택된 언어 규칙이나 구현 계획이 아니다.

- 실행 모델: 보간된 문자열을 특정 셸에 넘길지, 실행 파일과 인자 배열로 해석할지 결정해야 한다. 현재처럼 하나의 문자열로 합치면 보간값의 인자 경계를 나중에 복원하기 어렵다.
- 인용과 이스케이프: 공백·특수문자가 포함된 값의 전달 규칙, 셸 해석 여부 및 플랫폼 차이를 정해야 한다. 단순히 `system()`을 연결하는 것만으로 최종 설계를 대신하기는 어렵다.
- 실행 문맥: `@{ ... }`는 현재 행마다 별도 문자열로 전달된다. 각 행을 독립된 셸 프로세스로 실행하면 `cd`가 다음 행에 영향을 주지 않는다. 블록 전체를 셸에 넘길지, Citron이 작업 디렉터리를 관리할지 정해야 한다.
- 결과와 실패: 종료 코드 확인, 실패 시 중단/계속 정책, 표준 출력·오류 캡처 및 스크립트 종료 상태가 필요하다. 결과 없는 statement는 실행 후 실패 시 중단하는 용도에 맞을 수 있지만, 출력 캡처와 실패 분기에는 별도 API 또는 표현식도 검토할 수 있다.
- 명령 조합: 파이프, 리다이렉션, 스트리밍 출력 및 파이프 내 실패 판정은 추가 설계 대상이다.
- 재개 시의 범위 후보: 외부 도구를 순서대로 실행하는 빌드·자동화 스크립트를 먼저 목표로 삼고, 병렬 실행이나 대화형 셸 전체 기능은 후속 범위로 둘 수 있다.

## 사용자 확정 결론

- 현재 스크립트를 Citron으로 전환하는 작업과 이를 위한 스크립팅 지원 확장은 보류한다.
- 필요한 기반 기능이 많으므로 당분간 Citron 컴파일러 개발에 집중한다.
- 이번 논의로 `@` 실행 의미론이나 프로세스 API를 새로 확정하지 않았다. 기존 구문을 변경하거나 제거하는 작업도 진행하지 않는다.

이번 작업은 노트 기록만 수행했으며 소스 수정과 빌드·테스트는 하지 않았다.
