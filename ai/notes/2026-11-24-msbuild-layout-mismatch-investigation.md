# SmPhaseManager 레이아웃 불일치 의심 및 증분 빌드 조사

Date: 2026-11-24
Status: Rebuild로 증상 해소. 오래된 obj가 일반 빌드에서 갱신되지 않은 원인은 미확정이며, 후속 재현은 실패했다.
Environment: Windows x64 Debug, Visual Studio 2026 / MSVC v145 (14.52.37025)
Context: HEAD 2a112356 이후 작업 트리에서 SmPhaseManager 등록 API를 AddTask로 전환 중이었다.

## 당시 변경과 오류

- 기존 단계별 shared_ptr 소유 방식을 phaseTasks의 unique_ptr 소유 + 단계별 raw interface pointer 목록으로 변경했다.
- 공통 소유 vector가 추가되면서 SmPhaseManager의 크기와 멤버 offset이 달라졌다.
- 사용자가 EvalTests의 Boolean.BinOp 실행 중 phase7Tasks.push_back에서 오류를 보고했다.
- 기존 실행 파일로 `EvalTests.exe --gtest_filter=Boolean.BinOp`을 실행해 SEH 0xC0000005를 재현했다.
- cdb에서 std::_Construct_in_place<ISmPhase7Task*>가 주소 0에 쓰기를 시도하는 것을 확인했다.
- 호출 경로: SmTranslateGlobalFunc → SmPhaseManager::AddTask<SmGlobalFuncTask> → phase7Tasks.push_back → vector 내부 원소 생성.
- 당시 phase7Tasks는 size=0이지만 비정상적으로 큰 capacity를 표시했다. 정상적인 빈 vector 상태가 아니었다.
- 이 오류는 task 등록 중 발생했다. phase 순회 중 같은 vector에 추가해 iterator가 무효화되는 문제로 확인된 것은 아니다.

## 확인한 산출물과 해석의 한계

재빌드 전 obj 수정 시각(2026-11-24, KST):

| 파일 | 수정 시각 |
|---|---|
| SmTranslator.obj | 10:56:12 |
| SmPhaseManager.obj | 11:18:01 |
| SmGlobalFuncTranslation.obj | 11:27:01 |

- SmTranslator.cpp는 TranslateSyntax 안에서 SmPhaseManager를 지역 객체로 생성한다.
- 오래된 SmTranslator.obj와 새 레이아웃으로 컴파일된 task 등록 코드가 섞였을 가능성이 높은 정황이었다.
- 다만 당시 모든 translation unit의 컴파일 시점 레이아웃을 직접 비교한 것은 아니다. 메모리 손상 원인을 독립적으로 완전히 입증한 기록도 없다.
- 최초 답변에서 obj 레이아웃 불일치를 원인으로 단정했지만, 정확히 확정한 것은 접근 위반 재현, 비정상 vector 상태, obj 시각 차이, Rebuild 후 통과다.

## 실제로 수행한 해결 조치

소스 변경 없이 Visual Studio developer shell에서 **SmTranslator 프로젝트만 Rebuild**했다. 일반 Build가 아니며, 전체 solution Rebuild도 아니다.

```powershell
msbuild src/SmTranslator/SmTranslator.vcxproj /t:Rebuild `
    /p:BuildProjectReferences=false `
    /p:Configuration=Debug /p:Platform=x64 /nologo /v:minimal

& ./bin/x64-Debug/EvalTests.exe --gtest_filter=Boolean.BinOp
```

- Rebuild 성공 후 동일 테스트 1개가 통과했다.
- 에이전트는 이 오류 해결을 위해 소스·프로젝트 설정을 변경하지 않았다.
- Rebuild가 당시 추적 기록을 갱신했으므로, 증상 발생 시점의 tlog/빌드 판단 기록은 별도 보존하지 못했다.

## Ctrl+F7 가설과 후속 시험

- 사용자는 평소 Visual Studio의 현재 파일 컴파일(Ctrl+F7)을 사용한다고 답했다.
- Ctrl+F7은 선택한 파일의 컴파일이다. 다른 cpp의 헤더 의존성을 모두 갱신하거나 링크하는 프로젝트 빌드와 구분한다.
- **그러나 이후 정상적인 프로젝트/솔루션 Build를 실행했다면 변경된 헤더에 의존하는 cpp도 재컴파일되어야 한다.** Ctrl+F7을 사용했다는 사실만으로 이번 문제를 설명할 수 없다.
- 최초에 Ctrl+F7 사용을 원인으로 설명한 것은 성급한 추론이었다. 당시 어떤 경로로 최종 DLL이 링크됐는지는 확인하지 못했다.
- 사용자 재현 시도: SmPhaseManager의 기존 멤버 앞에 임시 uint64_t layoutProbe를 추가해 offset/크기를 바꾸고, SmGlobalFuncTranslation.cpp에서 Ctrl+F7 후 Boolean.BinOp 실행. 사용자는 문제가 재현되지 않았다고 보고했다.
- 해당 사용자 시험의 상세 빌드 로그와 각 obj 갱신 시각은 수집하지 않았다. 원래 상황과 같은 조건이었다고 단정하지 않는다.
- layoutProbe는 재현용 제안이며 영구 변경이나 확정 설계가 아니다. 제거 여부는 이 기록에서 확인하지 않았다.

## 증분 빌드 조사 결과

- 유효 MSBuild 설정에서 TrackFileAccess=true를 확인했다.
- Rebuild 이후 SmTranslator.tlog/CL.read.1.tlog에는 SmTranslator.cpp → SmPhaseManager.h 의존성이 등록되어 있었다. 이는 오류 당시에도 정상이었다는 증거는 아니다.
- IntDir는 프로젝트별 obj/x64-Debug/SmTranslator이고, 현재 조사에서 잘못된 공통 IntDir 사용을 원인으로 확인하지 않았다.
- 같은 MSVC v145의 별도 임시 static library 프로젝트에서 공통 layout.h를 두 cpp가 포함하도록 구성했다.
- 첫 Build 후 layout.h의 struct 멤버를 추가하고 일반 Build를 실행하니 해당 두 obj가 갱신되고, 무관한 cpp의 obj는 유지됐다.
- 시험 프로젝트는 TEMP 아래여서 MSB8029 경고가 있었다. 실제 Citron 프로젝트 및 Visual Studio IDE의 빌드 흐름까지 검증한 시험은 아니다. 시험 디렉터리는 삭제했다.
- 프로젝트의 헤더 의존성 추적이 항상 실패한다는 증거는 얻지 못했다. 설정 변경은 하지 않았다.

## 다음 발생 시 조사 순서

1. **Clean/Rebuild, obj/tlog 삭제, 소스 timestamp 변경을 하기 전에** 상태를 보존한다. 실행 중인 테스트/디버거와 빌드 진행 여부도 기록한다.
2. HEAD, git diff/status, Configuration/Platform, VS/MSVC 버전, 실행한 테스트와 실제 로드된 exe/DLL 경로를 기록한다.
3. 문제가 발생한 exe/DLL/PDB, 관련 obj, obj/x64-Debug/SmTranslator의 tlog 폴더와 *.lastbuildstate, *.log, 관련 dependency JSON을 별도 조사 폴더에 복사한다. 이 기록의 후보 파일: SmTranslator.obj, SmPhaseManager.obj, SmGlobalFuncTranslation.obj.
4. 관련 cpp/header/obj/DLL의 LastWriteTimeUtc를 수집하고, 최근 편집·저장·Ctrl+F7·일반 Build·테스트 실행 순서 및 빌드 확인창의 선택을 기록한다.
5. 보존한 CL.read.*.tlog에서 각 cpp의 SmPhaseManager.h 의존성이 있는지, CL.write.*.tlog와 CL.command.*.tlog에서 출력 경로와 컴파일 옵션이 일치하는지 확인한다. _ITERATOR_DEBUG_LEVEL 등 vector 레이아웃 관련 설정 차이도 후보로 확인한다.
6. 상태 복사 후 일반 Build를 /v:diag 및 /bl로 실행한다. 해당 cpp를 최신으로 판단한 이유와 실제 컴파일/링크 입력을 확인한다. VS에서만 발생한다면 IDE의 Build 출력과 프로젝트 up-to-date 판단 기록도 확보한다.
7. 진단 빌드 이후 동일 테스트를 실행한다. 오류가 지속되면 cdb/VS로 vector 내부 포인터와 SmPhaseManager의 주소·멤버 offset·호출 스택을 조사한다.
8. 필요한 증거를 확보한 뒤에 Rebuild로 복구하고 동일 테스트로 결과를 비교한다.

진단 빌드 예시(Visual Studio developer shell, 저장 경로는 별도 조사 폴더로 지정):

```powershell
msbuild src/SmTranslator/SmTranslator.vcxproj /t:Build `
    /p:BuildProjectReferences=false `
    /p:Configuration=Debug /p:Platform=x64 `
    /v:diag /bl:<조사폴더>/SmTranslator.binlog `
    /fl /flp:logfile=<조사폴더>/SmTranslator.diag.log
```

## 관련 기록

- [Phase 명명 논의와 이전 코드 검토](2026-11-24-phase-number-naming.md)
- [Windows 빌드 및 테스트](../wiki/process/windows-build-and-test.md)
- [Visual Studio의 Build/Compile 구분](https://learn.microsoft.com/visualstudio/ide/building-and-cleaning-projects-and-solutions-in-visual-studio)
