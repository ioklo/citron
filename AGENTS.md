# Citron 컴파일러용 Codex 에이전트 안내서

본 문서는 Codex가 이 레포에서 작업할 때 따라야 할 최소 지침입니다. Copilot 문서와 겹치는 내용은 줄이고, Codex 입장에서 필요한 핵심만 담았습니다.

## 1. 범위와 기본 원칙
- 목적: 작은 단위로 변경해 리뷰·회귀 위험을 최소화.
- 변경 범위 제한: 영향 단계 디렉터리 안에서 끝내기(예: 문법 변경 → `Syntax/`, `SyntaxIR0Translator/`).
- `docs/` 보호: AI는 `docs/` 트리의 기존 문서를 수정하거나 생성하지 않는다(필요 시 사람에게 위임하거나 별도 승인을 받음).
- 테스트 동반: 기능·버그 수정 시 관련 단위 테스트 추가/갱신.
- 기록: 회의/실험 메모는 `ai/notes/`, 현재 유효한 결정과 언어 규칙은 `ai/wiki/`에 남김. 진행 중인 작업 주제와 열린 쟁점은 `ai/current-agenda.md`에 유지한다.
- PR 설명: 목적, 영향 파일, 테스트 결과, 후속 작업을 명시.

## 2. 빌드·테스트 단축 스니펫(macOS 기준)
```bash
cd src
mkdir -p build && cd build
cmake -DCMAKE_BUILD_TYPE=Debug ..
cmake --build . -- -j$(sysctl -n hw.ncpu)
ctest --output-on-failure
```
vcpkg 사용 시 `-DCMAKE_TOOLCHAIN_FILE=/path/to/vcpkg.cmake`를 cmake에 추가. 의존성은 `vcpkg.json`/`vcpkg-configuration.json` 확인.

## 3. 파이프라인(요약)
Text → Syntax → MIR(IR0) → QIR(IR1) → LLVM. 변환기는 단방향을 유지하고 단계 밖 부수효과를 만들지 않는다.

## 4. 선언/심볼 모델(핵심)
- 현재 작업 방향: semantic tree를 `RNode` 중심으로 재구성하고, 충분히 이행되면 기존 `RDecl` 계층은 제거한다.
- `RNode`는 tree/name/member relation을 우선 담당하는 lightweight node로 유지한다.
- accessibility, declaration category, richer metadata는 `RNode` 본체보다 별도 payload / policy / checker 계층으로 분리하는 방향을 우선 검토한다.
- 최신 상태와 열린 쟁점은 `ai/current-agenda.md`, 확정된 규칙은 `ai/wiki/current-decisions.md`를 우선 확인한다.

## 5. 산출물 위치
- 회의/실험 노트: `ai/notes/`.
- 현재 작업 주제와 열린 쟁점: `ai/current-agenda.md`.
- 현재 유효한 결정과 규칙: `ai/wiki/`.
- 프로세스/체크리스트: `ai/process/`, `ai/ai-guidelines/`.
- 문서 템플릿: `ai/templates/`.
- 언어 명세 원문: `docs/` 아래 기존 문서 유지.

## 6. 대화 컨텍스트 확인 규칙
- 설계, 문법, 의미론, 선언 모델처럼 최근 논의 맥락이 중요한 작업은 구현 전에 `ai/current-agenda.md`를 먼저 확인한다.
- `ai/current-agenda.md`에는 현재 붙잡고 있는 주제 1개, 최근 합의 방향, 아직 미확정인 쟁점, 현재 진행 중인 리팩터링 상태를 짧게 유지한다.
- 사용자가 “원래 이야기”, “지난번 결정”, “기록”, “히스토리”를 언급하면 `ai/current-agenda.md`를 생략하지 않는다.
- 현재 주제의 확정 규칙이 필요하면 `ai/wiki/current-decisions.md`와 관련 `ai/wiki/` 문서를 확인한다.
- 세부 히스토리가 더 필요하거나 최근 설계 흐름을 빠르게 복원해야 하면, 관련 키워드를 기준으로 최근 7일 정도의 `ai/notes/`를 우선 참고한다.
- 최근 노트를 기계적으로 전부 읽는 것을 기본 절차로 삼지 않는다. 다만 현재 agenda만으로 맥락이 부족하면 최근 7일 범위를 기본 탐색창으로 삼아 관련 노트를 선별해 읽는다.
- 주제가 바뀌면 `ai/current-agenda.md`를 먼저 갱신하고, 완전히 확정된 내용만 `ai/wiki/current-decisions.md`로 옮긴다.

## 7. 스타일·명명 규칙(필수)
- src/RULES.md를 우선 준수. 주요 예: std::expected 결과 변수 _, std::optional은 o_ 접두사.
- 기존 파일의 줄 끝 형식을 유지한다. 특히 Windows 소스 파일은 기존이 CRLF이면 수정 후에도 CRLF를 유지한다.
- 새 모듈/패스 추가 시 `src/CMakeLists.txt`에 타깃 등록.
- 번역기 변경은 해당 단계 테스트를 함께 갱신.

## 8. 작업 흐름 체크리스트
- [ ] 변경 범위와 영향 단계 명시.
- [ ] 관련 테스트 추가/업데이트 후 `ctest --output-on-failure` 실행.
- [ ] 문서 업데이트 여부 확인(`ai/current-agenda.md`, `ai/wiki/`, `ai/notes/`).
- [ ] PR 설명에 목적·파일·테스트 결과·후속 작업 기재.

## 9. 참고 문서
- Copilot 안내(비교용): .github/copilot-instructions.md
- AI 문서 색인: ai/index.md
- 에이전트 작업 규칙: ai/ai-guidelines/agent-workflow.md
- Windows 빌드/테스트/생성 규칙: ai/process/build-test-and-generation-windows.md
