# Process Wiki

Status: current index
Area: process
Keywords: process, build, test, generated files, generator, windows, workflow

빌드, 테스트, 생성 파일 규칙을 agent가 빠르게 찾기 위한 index다.

## Routing Hints
- Windows 빌드/테스트 절차와 Dev Shell 사용은 `windows-build-and-test.md`
- `.g.cpp`, `.g.h`와 generator 수정 규칙은 `generated-files.md`
- 일반 agent 작업 흐름은 `ai/ai-guidelines/agent-workflow.md`

## Current References
- `ai/wiki/process/windows-build-and-test.md`
- `ai/wiki/process/generated-files.md`
- `ai/ai-guidelines/agent-workflow.md`

Legacy process reference:
- `ai/process/build-test-and-generation-windows.md`

## Current Rules
- Windows 빌드/테스트는 Visual Studio Dev Shell 환경에서 실행한다.
- 테스트 프로젝트는 `src/*.Tests` 아래의 GoogleTest 실행 파일이다.
- `.g.cpp`, `.g.h`는 직접 수정하지 않고 `pp` generator를 수정한 뒤 생성 결과를 포함한다.
- 기능/버그 수정에는 관련 테스트 추가/갱신을 동반한다.
