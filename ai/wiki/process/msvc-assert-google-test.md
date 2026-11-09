# Visual Studio Google Test에서 assert()가 멈추지 않을 때

Status: troubleshooting reference (2026-11-06 대화 정리)
Area: process, Windows, test, debugging
Keywords: assert, abort, MSVC, Visual C++, Visual Studio, GoogleTest, gtest, Break on failure, _set_abort_behavior, 디버거, 중단, 다이얼로그

## 먼저 확인할 설정

Visual Studio에서 테스트 안의 C++ `assert()` 실패 시 기대한 디버거 중단이나 abort 대화상자가 나오지 않으면 다음 설정을 먼저 확인한다.

**Tools → Options → Test Adapter for Google Test → Google Test → Runtime behavior → Break on failure → True**

설정을 바꾼 뒤 Test Explorer에서 해당 테스트를 **Debug**로 실행해 동작을 확인한다. 위 경로는 2026-11-06 대화에서 확인한 UI 기준이다.

실행 파일을 직접 호출하는 경우 대응하는 GoogleTest 옵션은 `--gtest_break_on_failure`다. 기존 Windows 테스트 문서에서 정한 `$RepoRoot`와 `$Platform`을 사용하면 다음과 같다.

```powershell
& "$RepoRoot\bin\${Platform}-Debug\EvalTests.exe" --gtest_filter=Suite.Case --gtest_break_on_failure
```

`Suite.Case`는 확인하려는 실제 테스트 이름으로 바꾼다. CLI에서도 디버거 중단을 확인하려면 디버거를 연결한 상태로 실행한다.

## 원인과 적용 범위

GoogleTest의 Windows/MSVC 테스트 실행 경로에는 `break_on_failure`가 false이면 `_set_abort_behavior(0, _WRITE_ABORT_MSG | _CALL_REPORTFAULT)`로 CRT의 abort 메시지 및 오류 보고를 억제하는 코드가 있다. death test 등의 대화상자를 방지하기 위한 처리다. 이 CRT 상태는 일반 C++ `assert()` 실패 후 abort 동작에도 영향을 줄 수 있다.

`break_on_failure=true`이면 이 억제 호출을 건너뛴다. 이미 다른 코드가 변경한 CRT 설정을 복원하는 옵션은 아니며, 모든 C++ `assert()`에서 자동 중단을 보장하는 것도 아니다. 디버거가 없을 때 `_CrtSetReportMode`를 바꾸는 별도 경로도 있다.

이 옵션은 GoogleTest의 `EXPECT_*`와 `ASSERT_*` 실패 시에도 디버거 중단을 활성화한다. 테스트 디버깅용 설정으로 사용한다.

## 해결되지 않으면

- `NDEBUG` 정의로 C++ `assert()` 자체가 제거되었는지 확인한다.
- 실제 테스트 프로세스에 디버거가 연결되어 있는지 확인한다.
- Debug CRT(`/MDd` 또는 `/MTd`) 사용 여부와 별도의 CRT report/abort 설정 변경을 확인한다.
- 사용 중인 GoogleTest 버전의 `gtest.cc`에서 `_set_abort_behavior`, `break_on_failure`, `_CrtSetReportMode`를 검색한다.

## 출처와 확인 상태

- [원래 대화: Visual C++ assert 동작](chatgpt-conversation://6aed9c42-a1bc-83ec-8791-bcb5d84500fe): 사용자 제공 `gtest.cc` 코드와 마지막 설정 안내를 정리했다. 설정 변경 후 해결되었다는 명시적 재현 결과는 대화에 없다.
- [GoogleTest upstream gtest.cc](https://github.com/google/googletest/blob/main/googletest/src/gtest.cc): 2026-11-06 조회 시 위 조건부 CRT 설정 코드를 확인했다. 로컬 의존성 버전과 다를 수 있다.
- [Windows 빌드·테스트 절차](windows-build-and-test.md)

이번 작업은 기록만 추가했으며 Visual Studio 설정 변경이나 테스트 재현은 수행하지 않았다.
