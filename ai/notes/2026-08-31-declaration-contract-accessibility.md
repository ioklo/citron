# 선언 외부 계약의 접근성

Date: 2026-08-31
Status: confirmed; 사용자 요청으로 기록, 소스 구현은 미변경

## 변경 배경

앞선 `2026-08-31-name-access-and-dll-export.md`에서는 C++식 name-use 규칙을 채택하여 private 타입을 public signature/alias로 노출하고 소비자가 `var`로 받는 것을 허용했다.

언어별 비교 후, private은 외부가 의존하지 않는 구현 세부사항으로 읽히는 편이 자연스럽고 public 계약은 소비자도 접근할 수 있는 타입으로 설명되어야 한다는 방향으로 재검토했다. 구현 타입을 숨겨야 하면 기존 `some Trait` 계약을 사용한다.

## 확정된 변경

1. 선언 D의 외부 계약에 노출되는 타입·symbol S는 `Access(D) ⊆ Access(S)`를 만족해야 한다. 선언 위치에서 S를 쓸 권한이 있는지와는 별도의 조건이다.
2. 접근 범위는 enclosing declaration을 반영한다. protected끼리도 owner가 다르면 범위가 다르므로 modifier 서열만으로 비교하지 않는다. `Outer.Token`과 `Outer.Inner.Make` 예제에서 Inner만 상속한 Client는 Make만 접근할 수 있으므로 `protected Token Make()`를 거부한다.
3. public signature에 private 타입을 쓰거나 public type alias가 private target을 가리키는 것을 금지한다. private alias가 public 타입인 `int`를 가리키더라도 public signature에 그 alias 이름을 쓰면 안 된다. 정식 type alias의 접근성 검사를 normalization으로 없애지 않는다.
4. 검사 범위는 반환·매개변수·필드 타입, base/trait, generic arguments·constraints, alias target 등 외부 계약이다. 함수 body와 타입의 private 구현 멤버까지 따라가지는 않는다. `some Trait`의 private backing type은 직접적인 공개 signature 노출과 구분한다.
5. public struct의 header에 private trait를 직접 넣지 않는다. 내부용 conformance는 접근 범위가 맞는 bundle로 분리한다. bundle 사용으로 private trait의 접근성을 높이지 않는다.
6. 공개 conformance의 associated type witness로 private 타입을 노출하는 것은 거부한다. impl/witness 자체의 public 표기 유무가 기준은 아니다.
7. 현재 extension은 trait requirement를 구현하므로 임의의 public 반환 타입을 추가하는 상황을 가정하지 않는다. trait/header 접근성, trait 인자·associated type 노출, impl signature 일치 검사로 계약을 보장한다. trusted private 접근은 구현 내부 사용 권한이며 공개 권한을 추가하지 않는다.

## 유지되는 규칙과 미확정 범위

- lookup 뒤 접근성을 검사하며 inaccessible candidate도 shadowing에 참여한다는 규칙은 유지한다.
- 소멸자는 항상 public이다. CTI metadata reachability와 native export의 구분 및 DLL export 방식 보류도 유지한다.
- unit-local convenience `using`은 정식 `type` alias declaration과 구분한다. `using` 자체에 public/private modifier를 부여하지 않는다.
- 접근 범위 비교 API, alias provenance 보존 방식, 접근성 검사의 정확한 phase는 구현 설계가 남아 있다.
- 앞서 논의한 alias/base 해석 스케줄링, phase rename, 상속절에서 alias 금지, 선언 순서 강제, unit import 복원은 이번 확정 사항이 아니다.

## 언어별 조사 요약

외부에서 실제 private 타입을 public API로 얻는 경우를 비교했다. 경로가 접근 불가능한 public 타입과 실제 private 타입은 구분한다.

| 언어 | 조사 결과 |
|---|---|
| C++ | public 함수의 private 타입 반환과 public alias 노출 허용. auto/decltype/public alias로 얻은 타입의 public 멤버 사용 가능. 생성자·소멸자 등의 개별 접근성 검사는 별도다. |
| C# | 접근 범위가 부족한 반환 타입은 선언 오류 CS0050. var로 우회 불가. using alias는 공개 타입 선언이 아니다. |
| Java | private 타입 반환 선언과 var로 받기는 허용하지만 private 클래스에 선언된 public 멤버 호출은 제한된다. 공개 interface 등으로 사용 범위를 표현하는 경우는 별개다. |
| Rust | 일반 public 함수/alias의 private 타입 노출은 기본 private_interfaces 경고, 외부에서 그 타입을 실제 사용하면 오류. 공개 associated type 노출은 E0446. private module 안 pub 타입의 unnameable 상태와 구분한다. |
| Swift | public signature 및 public typealias가 private 타입을 노출하는 것을 선언 단계에서 금지. some Protocol의 backing type은 숨길 수 있다. |

로컬 실험: MSVC 19.51에서 auto/public alias/decltype은 통과하고 private 이름 직접 사용은 C2248. 로컬 C# Add-Type 컴파일에서 public 함수의 private 반환 타입은 CS0050. Java/Rust/Swift는 공식 명세·문서로 확인했고 로컬 컴파일은 수행하지 않았다.

Sources:
- [C++ member access](https://eel.is/c++draft/class.access.general)
- [C# accessibility constraints](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/basic-concepts#755-accessibility-constraints)
- [C# using aliases](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/namespaces#1462-using-alias-directives)
- [Java access control](https://docs.oracle.com/javase/specs/jls/se26/html/jls-6.html#jls-6.6.1)
- [Java local variable typing](https://docs.oracle.com/javase/specs/jls/se26/html/jls-14.html#jls-14.4.1)
- [Rust private_interfaces](https://doc.rust-lang.org/rustc/lints/listing/warn-by-default.html#private-interfaces)
- [Rust type privacy RFC](https://rust-lang.github.io/rfcs/2145-type-privacy.html)
- [Rust unnameable_types](https://doc.rust-lang.org/rustc/lints/listing/allowed-by-default.html#unnameable-types)
- [Rust E0446](https://doc.rust-lang.org/stable/error_codes/E0446.html)
- [Swift access control](https://docs.swift.org/swift-book/documentation/the-swift-programming-language/accesscontrol/)
- [Swift opaque types](https://docs.swift.org/swift-book/documentation/the-swift-programming-language/opaquetypes/)

## 현재 문서

- `../wiki/language/visibility-and-reachability.md`
- `../wiki/language/type-aliases.md`
- `../wiki/language/trait-and-interface.md`
- `../wiki/current-decisions.md`

기존 history의 결론을 소급 삭제하지 않고 현재 규칙에서 교체했다. 이번 작업은 문서 기록만 수행하며 compiler source와 `docs/`는 수정하지 않았다.
