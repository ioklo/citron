# EvalTests 실패 시 중간 표현 출력

Date: 2026-11-09
Status: 구현 및 focused 검증 완료

- 사용자 요청에 따라 EvalTests의 GoogleTest 실패 메시지에 Source, Syntax(JSON), MIR, QIR을 연결했다.
- 각 단계가 성공한 직후 `SCOPED_TRACE`를 추가하므로 실패 지점까지 생성된 결과만 표시한다. 성공 테스트에는 출력하지 않는다.
- 파싱 실패 시 입력 소스가 포함되는지, 최종 출력 불일치 시 네 단계의 내용이 모두 포함되는지 검증하는 회귀 테스트 두 개를 추가했다.
- C++ assert/abort 또는 접근 위반으로 프로세스가 종료되는 경우의 출력은 이 변경의 범위에 포함하지 않는다.
- 변경 범위는 `src/EvalTests/EvalTests.cpp`다. 생성 파일과 docs는 수정하지 않았다.
- ARM64 Debug 의존성 포함 빌드는 기존 Infra/Logging에서 C1902(PDB 관리자 불일치)로 실패했다. EvalTests 단독 빌드도 project의 고정 /Zi 설정을 사용하므로, 해당 빌드 프로세스에만 `_CL_=/Z7`을 지정하고 `/p:BuildProjectReferences=false`로 기존 의존성 바이너리를 사용해 빌드 성공했다. 저장소의 빌드 설정은 변경하지 않았다.
- 실행 검증: "EvalTestDiagnostics.*:Assign_Expression.*" 필터로 신규 회귀 테스트 2개와 기존 성공 테스트 2개, 총 4개 통과. 현재 MSBuild 작업 경로에 대응하는 CTest 구성은 없어 GoogleTest 실행 파일을 직접 실행했다. 전체 테스트는 실행하지 않았다.
