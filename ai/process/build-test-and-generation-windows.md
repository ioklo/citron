# Windows Build, Test, and Generation

이 문서는 기존 절차의 호환 입구다. 현재 기준은 `ai/wiki/process/windows-build-and-test.md`를 따른다.

핵심 규칙:

- 테스트 실행 대상은 `src` 아래의 `*.Tests` 세 프로젝트와 `EvalTests`다.
- GoogleTest 목록과 focused 실행은 빌드된 실행 파일의 `--gtest_list_tests`, `--gtest_filter`를 사용한다.
- platform은 고정 `x64`가 아니다. `x64` 또는 `ARM64`를 현재 Visual Studio/vcpkg 구성에 맞춰 일관되게 사용한다.
- 저장소 상대 include path는 `$(ProjectDir)` 기준으로 유지한다. 공통 props가 `bin/<Platform>-<Configuration>`과 `obj/<Platform>-<Configuration>/<Project>`를 설정하므로 단독 빌드에도 `SolutionDir`를 전달하지 않는다.
- `data/TestData` 변경은 `pp/TestGenerator`를 실행하여 `.g.cpp` 테스트 소스를 재생성한다. 생성 파일은 직접 수정하지 않는다.
- Visual Studio instance ID는 고정하지 않고 `vswhere`로 설치 경로를 찾는다.

상세 명령, TestGenerator의 입력/출력 관계, ARM64 예시는 [Windows Build And Test](../wiki/process/windows-build-and-test.md)를 본다.
