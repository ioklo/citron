# Windows Build, Test, and Generation

## 개발 환경 시작
Visual Studio 개발자 셸 환경이 먼저 필요하다.
비대화형 실행에서도 같은 PowerShell 세션 앞부분에 아래 두 줄을 먼저 실행하면 된다.

```powershell
Import-Module "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell 627cbcf5 -SkipAutomaticLocation -DevCmdArguments "-arch=x64 -host_arch=x64"
```

터미널 앱에서 직접 여는 원본 명령은 다음과 같다.

```powershell
pwsh.exe -NoExit -Command "&{Import-Module """C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"""; Enter-VsDevShell 627cbcf5 -SkipAutomaticLocation -DevCmdArguments """-arch=x64 -host_arch=x64"""}"
```

## 개별 프로젝트 빌드
예를 들어 `MIR` 프로젝트가 `src/MIR` 아래에 있다면, 해당 디렉터리에서 다음처럼 빌드한다.

```powershell
msbuild .\MIR.vcxproj /t:Build /p:Configuration=Debug /p:Platform=x64
```

프로젝트 파일 이름은 생략 가능하다.

```powershell
msbuild /t:Build /p:Configuration=Debug /p:Platform=x64
```

## 전체 솔루션 빌드
프로젝트 전체를 빌드하려면 `src` 디렉터리에서 다음처럼 실행한다.

```powershell
msbuild Citron.sln /t:Build /p:Configuration=Debug /p:Platform=x64
```

솔루션 파일 이름도 생략 가능하다.

```powershell
msbuild /t:Build /p:Configuration=Debug /p:Platform=x64
```

## 테스트 프로젝트
테스트 프로젝트는 현재 `src` 아래의 `.Tests` 디렉터리에 있다.

- `src/Builder.Tests`
- `src/QEvaluator.Tests`
- `src/TextAnalysis.Tests`

각 테스트 프로젝트는 GoogleTest 기반의 `Application` 타입 실행 파일이다.
따라서 각 디렉터리에서 일반 프로젝트처럼 `msbuild`로 빌드한 뒤, 생성된 `.exe`를 직접 실행하는 방식이 기본이다.

예:

```powershell
cd src\Builder.Tests
msbuild .\Builder.Tests.vcxproj /t:Build /p:Configuration=Debug /p:Platform=x64
```

다른 테스트도 동일하다.

```powershell
cd src\QEvaluator.Tests
msbuild .\QEvaluator.Tests.vcxproj /t:Build /p:Configuration=Debug /p:Platform=x64

cd src\TextAnalysis.Tests
msbuild .\TextAnalysis.Tests.vcxproj /t:Build /p:Configuration=Debug /p:Platform=x64
```

빌드 후 테스트 실행 파일을 직접 실행한다. 출력 경로는 Visual Studio 프로젝트 설정에 따르며, 보통 `x64\Debug` 또는 `Debug` 계열 디렉터리 아래에 생성된다.

예상 예:

```powershell
.\x64\Debug\Builder.Tests.exe
.\x64\Debug\QEvaluator.Tests.exe
.\x64\Debug\TextAnalysis.Tests.exe
```

특정 GoogleTest만 실행하고 싶으면 `--gtest_filter`를 사용한다.

```powershell
.\x64\Debug\Builder.Tests.exe --gtest_filter=MPrinter*
```

## `pp` 디렉터리와 생성 파일 규칙
루트에는 `src` 외에 `pp` 디렉터리가 있다.
`pp`는 보통 preprocess/code generation 및 docs 업데이트를 담당한다.

### .g.cpp, .g.h 파일 규칙
이 문서에서 .g라는 표현은 확장자 이름이 아니라, .g.cpp와 .g.h 파일을 묶어 부르는 약칭이다.
- `.g.cpp`, `.g.h` 파일은 직접 수정하지 않는다.
- 해당 파일을 바꾸려면 `pp` 아래의 generator 소스를 수정한 뒤 generator를 실행해야 한다.
- generator 실행 결과로 `src` 아래의 `.g.cpp`, `.g.h`가 갱신된다.

### generator 실행 방식
보통 `pp` 쪽에서 `CodeGenerator.exe ..\..\..` 형태로 실행한다.
인자는 git root 경로이며, generator는 그 경로를 기준으로 상대적으로 `src` 등을 찾아가서 파일을 갱신한다.

예:

```powershell
CodeGenerator.exe ..\..\..
```

### 리포지토리 정책
- `.g.cpp`, `.g.h`는 ignore 대상이 아니다.
- 필요할 때 generator를 실행해서 생성 결과를 리포지토리에 포함하는 방식이다.
- 따라서 여기서 말하는 `.g` 파일 변경은 `.g.cpp`, `.g.h` 생성 파일 변경을 뜻하며, 생성물이라도 무시하지 않고 함께 검토하고 커밋해야 한다.

## 작업 규칙 요약
- Windows 빌드/테스트를 돌릴 때는 먼저 VS Dev Shell 환경을 준비한다.
- 개별 프로젝트는 각 프로젝트 디렉터리에서 `msbuild`로 빌드한다.
- 전체 빌드는 `src`에서 `msbuild Citron.sln`으로 진행한다.
- 테스트는 `.Tests` 프로젝트를 빌드한 뒤 생성된 GoogleTest 실행 파일을 직접 실행한다.
- `.g.cpp`, `.g.h`는 직접 수정하지 않고, 반드시 `pp`의 generator를 수정하고 실행해서 갱신한다.
- 여기서 말하는 `.g` 파일은 `.g.cpp`, `.g.h` 생성 파일 묶음을 뜻하며, 생성물이지만 리포지토리에 포함되는 대상이다.


