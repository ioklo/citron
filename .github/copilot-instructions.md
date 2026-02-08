<!-- Citron 컴파일러 작업을 위한 Copilot/AI 에이전트 안내서 (한국어) -->

**목적**: 이 레포지토리에서 AI 코딩 에이전트가 빠르게 생산적으로 일할 수 있도록 핵심 구조, 관행, 빌드/테스트 흐름을 요약합니다.

**전체 파이프라인(정확한 순서)**
- TextAnalysis: Text -> Syntax
- SyntaxIR0Translator: Syntax -> MIR (그리고 Symbol 생성: `RDecl`)
- IR0IR1Translator: MIR -> QIR
- QIrLLVMTranslator: QIR -> LLVM

즉, 일반적으로는 Text -> Syntax -> MIR(IR0) -> QIR(IR1) -> LLVM 순으로 변환됩니다.

**Declaration / Symbol 모델 (중요)**
- 컴파일 중 생성되는 심볼/선언은 세 가지 형태가 있습니다:
	- `EDecl`: 모듈에서 외부로 노출되는 선언(External 선언)
	- `NDecl`: 소스 코드로부터 직접 만들어지는 선언(Normal/Source 선언)
	- `RDecl`: 컴파일 과정에서 내부적으로 사용하는 선언(런타임/중간 표현용)
- 관계: `EDecl`은 `REDecl` 형태로 감싸져 `RDecl` 인터페이스를 구현하고,
	`NDecl`은 `RDecl`을 상속(구현)하는 형태로 제공됩니다. 즉, `RDecl`이 컴파일 단계에서 공통 인터페이스 역할을 합니다.

**핵심 디렉터리(먼저 읽어야 할 파일들)**
- `src/CMakeLists.txt` — 빌드 타깃과 모듈 순서 ([src/CMakeLists.txt](src/CMakeLists.txt#L1)).
- `src/RULES.md` — 코드 스타일/관습 (`e_`/`o_` 접두사 등) ([src/RULES.md](src/RULES.md#L1)).
- `src/TODO.md` — 진행 중 이슈와 작업 우선순위 ([src/TODO.md](src/TODO.md#L1)).
- `docs/` — 언어 명세와 예제들(언어 의미론 이해에 중요).

참고: AI 관련 문서는 [ai/index.md](ai/index.md#L1)에서 시작하세요. 일반 언어 명세는 `docs/`에 남아 있습니다.

**빌드 & 테스트 (macOS 예시)**
필수: `cmake`, C++23 지원 컴파일러, 선택: `vcpkg`.

```bash
cd src
mkdir -p build && cd build
cmake -DCMAKE_BUILD_TYPE=Debug ..
cmake --build . -- -j$(sysctl -n hw.ncpu)
ctest --output-on-failure
```

`vcpkg`를 쓴다면 `-DCMAKE_TOOLCHAIN_FILE=/path/to/vcpkg.cmake`를 `cmake`에 추가하세요. 의존성은 `vcpkg.json`과 `vcpkg-configuration.json`을 확인합니다.

**프로젝트 관행(명시적)**
- 반환형 표기: `std::expected` 결과는 `e_` 접두사, `std::optional`은 `o_` 접두사 변수명을 사용합니다. (예: `auto e_mLoc = GetMLoc();`) — [src/RULES.md](src/RULES.md#L1).
- 모듈/패스: 각 컴파일 단계는 `src/` 아래에 독립된 CMake 서브디렉터리로 구성됩니다. 새 패스나 테스트 추가 시 `src/CMakeLists.txt`에 등록합니다.
- 번역기(Translator)들은 주로 단방향 변환을 목표로 합니다(AST -> MIR -> QIR ...). 변경은 해당 단계에 국한시키세요.

**통합 포인트 / 외부 의존성**
- `vcpkg`로 관리되는 라이브러리(예: GoogleTest) 사용. 의존성 변경 시 `vcpkg.json`을 업데이트하세요.
- LLVM 관련 작업은 `QIrLLVMTranslator/`에 집중되어 있을 가능성이 높습니다. IR 구조를 변경하면 `RuntimeLibrary/`와 빌더 연결을 검토해야 합니다.

**AI 에이전트용 작업 권장 방식**
- 변경은 영향을 받는 컴파일 단계(디렉터리)에만 국한시키고, 가능하면 해당 단계의 테스트를 추가/수정하세요.
- 변경 후 반드시 `cmake --build`와 `ctest`를 실행하여 회귀를 확인하십시오. 실패 로그를 PR에 포함하면 리뷰어가 빠르게 이해합니다.

**예시 작업 참조**
- 번역기 추가: `SyntaxIR0Translator/` 또는 `IR0IR1Translator/`의 구조를 따라 새 폴더와 CMake 타겟을 추가하고 `src/CMakeLists.txt`에 등록하세요.
- 런타임 함수 추가: `RuntimeLibrary/`에 구현하고 `Builder/` 타겟에서 링크되는지 확인하세요.

더 자세한 예시나 `vcpkg` 설정, 특정 Translator 내부 구조 설명을 원하시면 어느 부분을 더 깊게 풀어쓸지 알려주세요.
