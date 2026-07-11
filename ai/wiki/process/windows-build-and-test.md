# Windows Build And Test

Status: current
Area: process, build, test, generation
Keywords: Windows, Visual Studio, MSBuild, GoogleTest, EvalTests, TestGenerator, ARM64, x64

## Scope

Citron의 C++ 테스트 실행 파일은 다음 네 프로젝트다.

- `src/Builder.Tests`
- `src/QEvaluator.Tests`
- `src/TextAnalysis.Tests`
- `src/EvalTests`

모두 GoogleTest 기반이다. CMake는 각 target을 CTest에 등록하지만, 현재 실행 가능한 테스트 목록의 확인과 focused 실행은 빌드된 GoogleTest 실행 파일을 직접 사용한다.

## Select A Platform

플랫폼은 고정 `x64`가 아니다. 현재 머신과 설치된 vcpkg triplet/Visual Studio solution configuration에 맞춰 아래 둘 중 하나를 선택한다.

```powershell
# x64 Windows 머신
$Platform = 'x64'
$DevShellArch = 'x64'

# ARM64 Windows 머신
$Platform = 'ARM64'
$DevShellArch = 'arm64'
```

`$Platform`은 MSBuild의 `/p:Platform=` 값과 출력 디렉터리 이름에 쓴다. `$DevShellArch`는 `Enter-VsDevShell`의 architecture 값이다.

## Start Visual Studio Developer Shell

VS instance ID는 업데이트/재설치로 바뀔 수 있으므로 문서에 고정하지 않는다. `vswhere`로 현재 설치 경로를 찾은 뒤 같은 PowerShell 세션에서 Dev Shell을 초기화한다.

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vsInstallPath = & $vswhere -latest -products * -property installationPath
Import-Module (Join-Path $vsInstallPath 'Common7\Tools\Microsoft.VisualStudio.DevShell.dll')
Enter-VsDevShell -VsInstallPath $vsInstallPath -SkipAutomaticLocation `
    -Arch $DevShellArch -HostArch $DevShellArch
```

프로젝트는 vcpkg MSBuild integration을 사용한다. 이 초기화 뒤에도 `vcpkg.user.props`를 읽지 못하면 해당 사용자 환경의 vcpkg integration과 권한을 먼저 확인한다.

## Build A Test Project

먼저 root 변수를 정한다. 프로젝트의 저장소 상대 include path는 `$(ProjectDir)` 기준으로 표현한다. 모든 `src`/`pp` project는 각 영역의 `Common.props`가 `$(MSBuildThisFileDirectory)` 기준으로 정의하는 공통 artifact directory를 사용하므로, 단독 `.vcxproj` 빌드에 `SolutionDir`를 전달하지 않는다.

```powershell
$RepoRoot = 'Z:\proj\citron'
$SourceRoot = Join-Path $RepoRoot 'src'
$TestProjectDir = Join-Path $SourceRoot 'EvalTests'
Set-Location $TestProjectDir
```

예를 들어 EvalTests는 다음처럼 빌드한다.

```powershell
msbuild .\EvalTests.vcxproj /t:Build `
    /p:Configuration=Debug /p:Platform=$Platform
```

다른 테스트도 동일하다.

```powershell
Set-Location (Join-Path $SourceRoot 'TextAnalysis.Tests')
msbuild .\TextAnalysis.Tests.vcxproj /t:Build `
    /p:Configuration=Debug /p:Platform=$Platform
```

출력 실행 파일은 repo root 아래 `bin\<Platform>-<Configuration>\<Project>.exe`에 생성된다. 예를 들어 ARM64 Debug EvalTests는 `bin\ARM64-Debug\EvalTests.exe`다. 중간 산출물은 `obj\<Platform>-<Configuration>\<Project>` 아래에 생성된다.

## Discover And Run GoogleTest Cases

테스트 목록은 항상 빌드 결과의 실행 파일에서 얻는다.

```powershell
& "$RepoRoot\bin\${Platform}-Debug\EvalTests.exe" --gtest_list_tests
```

하나의 suite 또는 case만 실행할 때는 GoogleTest filter를 사용한다.

```powershell
# Trait suite 전체
& "$RepoRoot\bin\${Platform}-Debug\EvalTests.exe" --gtest_filter=Trait.*

# 정확히 Trait.Basic 하나
& "$RepoRoot\bin\${Platform}-Debug\EvalTests.exe" --gtest_filter=Trait.Basic
```

실행 파일을 인용해 호출해야 `$Platform` 변수와 경로의 특수문자를 안전하게 처리할 수 있다.

## Generate Tests From `data/TestData`

테스트는 런타임에 data 파일을 읽지 않는다. `pp/TestGenerator`가 `data/TestData`를 읽어 committed `.g.cpp` 테스트 소스를 생성하며, 이 생성 파일이 각 테스트 프로젝트와 함께 컴파일된다.

입력과 출력 관계는 다음과 같다.

| Input | Generated test source |
|---|---|
| `data/TestData/EvalTests/*.ct` | `src/EvalTests/EvalTests.g.cpp` |
| `data/TestData/ScriptParserTests/*.in.txt` + `.out.txt` 또는 `.fail.txt` | `src/TextAnalysis.Tests/ScriptParserTests.g.cpp` |
| `data/TestData/StmtParserTests/*.in.txt` + `.out.txt` 또는 `.fail.txt` | `src/TextAnalysis.Tests/StmtParserTests.g.cpp` |
| `data/TestData/ExpParserTests/*.in.txt` + `.out.txt` 또는 `.fail.txt` | `src/TextAnalysis.Tests/ExpParserTests.g.cpp` |
| `data/TestData/TypeExpParserTests/*.in.txt` + `.out.txt` 또는 `.fail.txt` | `src/TextAnalysis.Tests/TypeExpParserTests.g.cpp` |

EvalTests `.ct` 파일은 `Category_Name.ct` 이름을 사용하며 첫 줄에 기대 결과를 `//@ <text>` 형식으로 둔다. `//@ $Error...` 형식은 generator가 현재 EvalTests source를 생성하지 않는다.

테스트 데이터를 추가/수정한 뒤에는 TestGenerator를 같은 platform으로 빌드한다. TestGenerator의 첫 번째 인자는 Git root이므로, 공통 output directory에서 실행할 때는 `..\..`를 전달한다.

```powershell
Set-Location "$RepoRoot\pp\TestGenerator"
msbuild .\TestGenerator.vcxproj /t:Build /p:Configuration=Debug /p:Platform=$Platform

Set-Location "$RepoRoot\bin\${Platform}-Debug"
& .\TestGenerator.exe ..\..
```

생성된 `.g.cpp`는 직접 수정하지 않는다. generator 실행 결과를 검토하고 data 파일 변경과 함께 커밋한다.

## Generate Source Files

`pp/CodeGenerator`는 Syntax와 RSymbol 등의 generated source/header를 생성한다. 대표 생성물은 다음과 같다.

- `src/Syntax/Public/Syntax/Syntaxes.g.h`
- `src/Syntax/Syntaxes.g.cpp`
- `src/RSymbol/Public/RSymbol/RTypeDeclVisitor.g.h`

CodeGenerator의 첫 번째 인자도 Git root다. 공통 output directory에서 실행할 때는 `..\..`를 전달한다.

```powershell
Set-Location "$RepoRoot\pp\CodeGenerator"
msbuild .\CodeGenerator.vcxproj /t:Build /p:Configuration=Debug /p:Platform=$Platform

Set-Location "$RepoRoot\bin\${Platform}-Debug"
& .\CodeGenerator.exe ..\..
```

CodeGenerator는 생성 결과가 기존 파일과 같으면 파일을 다시 쓰지 않아 수정 시간을 보존한다. `.g.cpp`와 `.g.h`는 직접 수정하지 않고, generator 입력 또는 구현을 수정한 뒤 이 명령으로 갱신한다.

## Full Solution Build

전체 빌드는 `src`에서 동일한 platform으로 수행한다.

```powershell
Set-Location $SourceRoot
msbuild Citron.sln /t:Build /p:Configuration=Debug /p:Platform=$Platform
```

## Related Documents

- `generated-files.md`: 생성 파일 일반 규칙
- `../../../process/build-test-and-generation-windows.md`: 기존 절차 호환 입구
