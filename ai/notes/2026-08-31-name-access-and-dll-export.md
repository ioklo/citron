# Name access, destructor와 DLL export

Date: 2026-08-31

Status update (2026-08-31): 아래 Confirmed의 C++식 name-use 및 private type의 public signature/alias 노출 허용 항목은 같은 날 후속 결정으로 대체됐다. 현재 규칙은 선언 외부 계약의 접근 범위 포함 관계다. `2026-08-31-declaration-contract-accessibility.md`와 `../wiki/language/visibility-and-reachability.md`를 본다. 아래 내용은 당시 논의 이력으로 보존한다. 소멸자 public 규칙, CTI metadata와 native export의 구분, DLL export 설계 보류는 유지한다.

## Confirmed
- 소멸자는 접근 지정자를 갖지 않고 항상 public이다. 소멸자 accessibility check도 수행하지 않는다.
- 타입은 C++식 name-use accessibility를 사용한다. source에서 이름을 사용하는 문맥을 검사하며 canonical type 자체에 private 제약을 전파하지 않는다.
- private type도 `var`나 accessible transparent alias를 통해 사용할 수 있다. public signature/alias가 less-accessible target을 포함한다는 이유만으로 거부하지 않는다. 선언 문맥에서 target 이름의 접근성은 검사한다.
- CTI는 기존 결정대로 private을 포함한 declaration name과 semantic metadata를 제공한다. 이는 native linker export와 별개다.

## Deferred: Windows DLL exports
- 논의 결론 (2026-08-31): 지금 export 방식을 정하지 않고 보류한다. 아래 안들은 후보로만 남기며 현재 type alias/associated type 작업의 선행 조건으로 삼지 않는다. 초과 시 오류 처리나 자체 table 기본 사용을 채택한 것은 아니다.
- 사용자 제안: (1) export 한도 초과 시 오류, (2) 필요한 것만 export하는 장치.
- MSVC `/EXPORT` 문서는 ordinal 범위를 1~65,535로 정한다. 함수뿐 아니라 data export도 대상이다. 언어 declaration 개수를 그대로 한도와 비교하면 안 된다.
- 외부 extension은 private member에 접근할 수 있으므로 `public`만 export하는 정책으로는 기존 contract를 충족하지 못한다. 미래 consumer가 있는 reusable DLL에서 현재 사용된 항목만 남기는 것도 일반적으로 안전하지 않다.
- 검토 후보: 초기 native export 경로에서는 backend/linker 한도 초과를 진단하고, 선택적 export는 별도 ABI 정책으로 검토한다.
- 다른 후보: 작은 module descriptor accessor만 native export하고, 함수와 metadata는 Citron 자체 table/resolver를 통해 제공한다. witness에 함수 주소를 넣어 전달하는 것과 같은 원리로 PE export entry 수를 줄일 수 있다.
- table 방식은 stable identifier/index, ABI versioning, module lifetime, import binding/cache 및 코드 보존 규칙이 추가로 필요하다. ordinary function call은 witness만으로 자동 해결되지 않는다. C ABI 개별 export는 여전히 플랫폼 한도를 따른다.
- DLL 정책은 아직 확정하지 않았다. 소스 구현은 변경하지 않았다.

## Additional Candidate: DLL 자동 분할
- 사용자 추가 제안: native export 한도 초과 시 하나의 logical module을 여러 physical DLL로 자동 분할하는 fallback.
- language module identity와 CTI declaration surface는 유지하고, backend/link artifact가 symbol별 physical DLL 배치를 관리하는 후보로 검토한다. 아직 채택 결정은 아니다.
- 각 definition의 소유 DLL을 하나로 정해 global state와 canonical metadata를 중복 생성하지 않아야 한다. DLL 간 호출을 위해 추가로 필요한 export도 한도 계산에 포함한다.
- 분할 DLL 간 상호 참조의 import library 생성과 module-level 초기화/종료 순서는 별도 처리가 필요하다.
- symbol이 다른 DLL로 이동하면 기존 consumer binary의 import 대상과 어긋날 수 있다. 재링크를 요구하거나 stable 배치/forwarder 등의 compatibility 정책이 필요하다.
- 모든 symbol을 대표 DLL 하나에서 forwarding하는 방법은 대표 DLL의 export 한도를 다시 만나므로 해결책이 아니다.
- DLL 묶음은 함께 배포/버전 관리하며, source-level export 제한이나 자체 function-table ABI와는 별개의 backend 선택지로 둔다.

## References
- 조사 보충 (2026-08-31): Rust/Swift의 일반 native dynamic library 연결은 자체 universal function table로 PE export를 대체하는 방식이 아니다. runtime metadata/witness와 native symbol binding은 별도 계층이다.
- Rust: executable 의존성은 기본적으로 rlib를 우선하며, dylib는 Rust용 native dynamic library, cdylib는 외부 언어용 dynamic library다. rustc MSVC linker 경로는 symbols.o의 .drectve에 /EXPORT 지시를 넣는다. Bevy 공식 가이드는 Windows dynamic linking에서 export 초과 오류를 피하기 위해 최적화를 안내한다. 최적화는 일반적인 한도 해결 보장이 아니다.
- Swift: 현재 GenDecl.cpp의 getIRLinkage는 DLL storage 사용 시 외부 ABI 정의에 DLLExportStorageClass, 외부 참조에 DLLImportStorageClass를 적용한다. private/hidden linkage 등은 별도 처리하므로 모든 source declaration을 단순 export하는 모델은 아니다.
- Swift의 2025-05 WinUI 사례에는 67,393개 export로 인한 lld-link 오류가 보고되었다. 당시 대응 논의는 불필요한 export 감소, static/dynamic build 분리와 정적 링크였으며 자체 table로 자동 전환하는 해결은 아니었다. 당시 SwiftPM 문제를 현재 모든 버전의 미해결 문제로 일반화하지 않는다.
- 확인한 일반 연결 경로와 문서에서 export 한도 초과 시 자체 table 또는 다중 DLL로 자동 전환하는 기능은 확인하지 못했다. Citron 자체 table은 Swift witness ABI 채택에 자동으로 따라오는 요건이 아니라 별도의 설계 선택이다.
- [Rust linkage reference](https://doc.rust-lang.org/reference/linkage.html)
- [rustc native linker implementation](https://doc.rust-lang.org/stable/nightly-rustc/src/rustc_codegen_ssa/back/linker.rs.html)
- [Bevy dynamic linking guide](https://bevy.org/learn/quick-start/getting-started/setup/#dynamic-linking)
- [Swift IR linkage implementation](https://github.com/swiftlang/swift/blob/main/lib/IRGen/GenDecl.cpp)
- [Swift WinUI export limit discussion, May 2025](https://forums.swift.org/t/the-state-of-winui-and-swift/79963)
- [MSVC /EXPORT](https://learn.microsoft.com/en-us/cpp/build/reference/export-exports-a-function?view=msvc-170)
- [PE format](https://learn.microsoft.com/en-us/windows/win32/debug/pe-format)
