# Citron 컴파일러용 Codex 에이전트 안내서

본 문서는 Codex가 이 레포에서 작업할 때 따라야 할 최소 지침입니다. Copilot 문서와 겹치는 내용은 줄이고, Codex 입장에서 필요한 핵심만 담았습니다.

## 1. 범위와 기본 원칙
- 목적: 작은 단위로 변경해 리뷰·회귀 위험을 최소화.
- 변경 범위 제한: 영향 단계 디렉터리 안에서 끝내기(예: 문법 변경 → `Syntax/`, `SyntaxIR0Translator/`).
- `docs/` 보호: AI는 `docs/` 트리의 기존 문서를 수정하거나 생성하지 않는다(필요 시 사람에게 위임하거나 별도 승인을 받음).
- 테스트 동반: 기능·버그 수정 시 관련 단위 테스트 추가/갱신.
- 기록: 회의/실험 메모는 `ai/notes/`, 현재 유효한 언어 규칙은 `ai/specs/`, 컴파일러 내부 규약과 구현 흐름은 `ai/implementations/`에 남김.
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
- `RDecl`: 컴파일 단계 공통 인터페이스(런타임/중간 표현용).
- `EDecl`: 외부 노출 선언, `REDecl`로 감싸 `RDecl` 인터페이스 구현.
- `NDecl`: 소스에서 직접 생성, `RDecl`을 상속/구현.
- 최근 결정: 런타임용 선언을 `RDecl`로 통일(참고: `ai/implementations/decl-model.md`).

## 5. 산출물 위치
- 회의/실험 노트: `ai/notes/`.
- 언어 스펙 스냅샷: `ai/specs/`.
- 컴파일러 내부 규약/구현 스냅샷: `ai/implementations/`.
- 프로세스/체크리스트: `ai/process/`, `ai/ai-guidelines/`.
- 문서 템플릿: `ai/templates/`.
- 언어 명세 원문: `docs/` 아래 기존 문서 유지.

## 6. 스타일·명명 규칙(필수)
- `src/RULES.md`를 우선 준수. 주요 예: `std::expected` 결과 변수 `e_`, `std::optional`은 `o_` 접두사.
- 새 모듈/패스 추가 시 `src/CMakeLists.txt`에 타깃 등록.
- 번역기 변경은 해당 단계 테스트를 함께 갱신.

## 7. 작업 흐름 체크리스트
- [ ] 변경 범위와 영향 단계 명시.
- [ ] 관련 테스트 추가/업데이트 후 `ctest --output-on-failure` 실행.
- [ ] 문서 업데이트 여부 확인(`notes/specs/implementations`).
- [ ] PR 설명에 목적·파일·테스트 결과·후속 작업 기재.

## 8. 참고 문서
- Copilot 안내(비교용): `.github/copilot-instructions.md`.
- AI 문서 색인: `ai/index.md`.
- 에이전트 작업 규칙: `ai/ai-guidelines/agent-workflow.md`.
