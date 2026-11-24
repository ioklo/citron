# Phase 번호 명명 논의와 코드 검토

Date: 2026-11-24
Status: 번호로 식별하고 역할을 주석으로 설명하는 방향은 사용자 합의. 소수점 표기는 논의 후보이며 검토 커밋은 정수 번호를 사용한다.
Review target: `d2d4df79bb8f0c6f6c0f30456114cddae60e282d` (`Phase에서 이름 제거, 번호로 변경`)

## 배경과 합의

- 하나의 phase가 여러 작업을 처리할 수 있으므로 특정 작업명을 phase 식별자에 고정하지 않는다.
- phase는 번호로 식별하고, 담당 작업은 주석으로 설명한다. 작업 추가·이동 시 역할 이름 때문에 phase API까지 바꾸는 부담을 줄인다.
- 문자열 정렬은 명명 기준에서 제외한다. 실행 순서는 코드에서 명시적으로 관리한다.
- 번호만 보이는 호출부에서는 역할 파악이 어려워진다. 단계별 역할과 경계를 한곳에 모아 설명하는 비용을 받아들이는 선택이다.
- 기존 SmPhaseManager 기반 phase 방식을 유지하는 결정과 일치한다. 새 scheduler나 dependency 추적 방식을 채택한 논의는 아니다.

## 검토한 표기 후보

1. `P1_BuildTypeSymbol`, `P15_BuildTrait`, `P175_BuildSomething`처럼 중간 단계를 소수 자릿수로 표현하는 안을 검토했다.
2. ASCII/ordinal 정렬에서는 숫자(48~57)가 `_`(95)보다 앞이므로 `P15_...`, `P175_...`, `P1_...` 순서가 된다. 자연 정렬 등 도구별 규칙은 별개다.
3. `.`은 숫자보다 앞이지만 C++ 식별자에 사용할 수 없다. 표준 ASCII 식별자 문자 중 숫자보다 앞서는 구분자는 없다.
4. 고정 자릿수(`P100`, `P150`, `P175`, `P200`)도 후보였으나 자릿수 확장 시 전체 번호를 맞춰야 한다.
5. 정렬을 포기하고 소수점 의미를 드러내는 `P0_1_BuildTypeSymbol`, 역할을 먼저 쓰는 `BuildTypeSymbol_0_1`도 검토했다.
6. 한 phase가 여러 작업을 담당할 수 있어 최종 대화 예시는 아래처럼 역할명을 제거했다.

```cpp
Phase0_1   // TypeSymbol
Phase0_15  // TypeTrait
Phase0_175 // Something
Phase0_2   // TypeHierarchy, TypeAlias
```

위 예시의 소수점 표기와 별개로, 실제 커밋에서는 `SmPhase2`~`SmPhase7`을 사용한다. 소수점 표기로 다시 바꾸기로 한 결정으로 해석하지 않는다. 번호 끝에 0을 붙이지 않는 규칙은 제안만 있었으며 별도 확정하지 않았다.

## 커밋에서 확인한 구현

- `SmPhaseManager.h`에 Phase 1~7의 역할을 모아 설명한다.
- Phase 1은 `SmTranslateScript` 등의 선언 수집 경로에서 수행한다. 별도 Phase 1 task queue는 없고, 모든 script를 처리한 뒤 `SmPhaseManager::Run()`을 호출한다.
- Run은 Phase 2 → 3 → 4 → 5 → 6 → 7 순서로 task vector를 순회한다. 기존 실행 순서와 오류 전파 방식은 유지됐다.
- task interface를 `ISmPhaseNTask`, 실행 함수를 `RunPhaseN`, 등록 함수를 `AddPhaseNTask`, context를 `SmPhaseNContext(s)`로 변경했다. 기존 TranslationTasks.h는 단계별 헤더로 분리했다.
- Phase 5의 impl 처리에서 Phase 7 body task를 등록하는 경로도 유지됐다.

| 단계 | 현재 역할 또는 주석상 의도 |
|---|---|
| 1 | body-space lambda를 제외한 type symbol 수집 |
| 2 | type hierarchy 구성. 주석에는 type alias resolve도 함께 기재 |
| 3 | type hierarchy 이후 처리. 현재 trait associated type의 trait constraint 해석 |
| 4 | 함수·변수 등 non-type symbol 구성 |
| 5 | non-type symbol 이후 impl 구성 및 conformance 검사 |
| 6 | memberwise constructor 등 implicit symbol 합성 |
| 7 | 함수 body 번역 및 MFuncBody 수집 |

Phase 2 주석은 type alias resolve 구현 완료의 근거가 아니다. alias/base 해석 방식과 관련된 기존 후보·보류 사항을 이번 명명 변경으로 확정하지 않는다.

## 코드 검토 결과

1. **[P2] Phase 2·3 헤더에 중복 포함 방지가 빠졌다.** `ISmPhase2Task.h`, `ISmPhase3Task.h`에 `#pragma once`나 include guard가 없다. 각각 두 번 포함하는 임시 translation unit에서 MSVC C2011(class type redefinition)을 재현했다. 현재 include 경로의 빌드는 통과하지만 간접 포함 경로가 겹치면 실패한다. 각 헤더에 중복 포함 방지를 추가하는 것이 후속 수정 사항이다.
2. **[P2] Phase 7 헤더가 MFuncBody의 선행 선언에 의존한다.** `ISmPhase7Task.h:14`는 `std::expected<MFuncBody, DiagPtr>`를 사용하지만 MFuncBody를 선언하거나 관련 헤더를 포함하지 않는다. 단독 포함 시 MSVC C2065/C2923/C2955를 재현했다. 삭제된 TranslationTasks.h에는 `struct MFuncBody;`가 있었다. 이를 해당 namespace에 복원하거나 필요한 헤더를 직접 포함해야 한다.
3. 프로젝트 등록 정리 사항: `SmTranslator.vcxproj:46`~47 및 filters에서 `ISmPhase2Task.h`, `ISmPhase6Task.h`가 ClInclude 대신 ClCompile로 등록돼 있다. 이번 MSBuild에서는 실패하지 않았으므로 빌드 차단 문제로 단정하지 않는다. 다른 task 헤더와 동일한 ClInclude 등록이 적절하다.

검토 마무리 시 작업 트리에서 사용자가 Phase 2·3 헤더에 `#pragma once`를 추가한 것을 확인했다. 위 지적과 재현은 커밋 기준이며, 사용자 수정 이후 헤더 probe를 재실행하지 않았다. 현재 역할명 제거 자체나 실행 순서 변경에 따른 동작 회귀는 코드 검토에서 발견하지 못했다. 에이전트는 소스 수정은 수행하지 않았다.

## 검증

- Windows x64 Debug, Visual Studio 2026 developer shell에서 SmTranslator.vcxproj Build 통과.
- 동일 구성의 EvalTests.vcxproj Build 통과.
- 빌드된 실행 파일의 `--gtest_list_tests`로 목록 확인 후 다음 filter 실행:
  `Trait.*:TypeAlias.*:Call_Global_Function_Expression.General:Struct_MemberwiseCtor.*`
- 총 8개 중 1개 통과, 7개 실패. 통과: Call_Global_Function_Expression.General.
- 파싱 실패: Struct_MemberwiseCtor.Disable, Struct_MemberwiseCtor.Private, Trait.AssociatedType, Trait.Generics.
- Unknown C++ exception: Struct_MemberwiseCtor.Normal, Trait.Basic, TypeAlias.Basic.
- 부모 커밋에서 동일 테스트를 실행하지 않았으므로 위 7개 실패가 이번 변경의 회귀인지는 확인하지 않았다. 전체 테스트, Release, ARM64 및 CMake 빌드는 미실행이다.
- 새 헤더의 중복/단독 포함을 임시 cpp와 `cl /std:c++latest /Zs /TP`로 검사해 위 두 헤더 문제를 재현했다. 임시 파일은 삭제했다.
- 작업 도중 나타난 미추적 소스 파일 `src/SmTranslator/ISmPhaseTask.h`는 검토 커밋에 포함되지 않으며 변경하지 않았다.

## 관련 기록

- [Coroutine scheduler 전환 철회](2026-11-06-coroutine-scheduler-withdrawal.md)
- [Type alias와 base 해석 후보](2026-08-31-type-alias-and-base-resolution.md)
- [Alias 해석과 병렬화 검토](2026-09-28-alias-resolution-and-parallelism.md)
- [현재 phase/scheduling 방침](../wiki/compiler/translation-task-scheduling.md)
