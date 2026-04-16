# 회의 / 실험 노트

Date: 2026-04-13
Title: ABI layering, internal ABI flavor, external boundary 정리

Summary
- Citron은 우선 하나의 internal ABI 구현으로 시작하되, 구조는 ABI-aware하게 유지하는 방향을 선호한다.
- `BC/NBC`는 언어 의미 축이고, 함수 호출의 direct/indirect 반환·인자 전달은 ABI 축으로 분리해서 본다.
- internal ABI flavor는 symbol에 직접 박지 않고 compile context에서 결정하며, external declaration만 ABI contract를 symbol에 둔다.
- internal ABI flavor가 다르면 direct link는 허용하지 않고, DLL/shared library 같은 외부 경계에서는 explicit external ABI로만 연결한다.

Context
- by-value parameter cleanup convention, QIR lowering, return passing mode, external C++ ABI 연동 논의가 이어지면서, ABI 관련 결정을 한 문서에 묶어 둘 필요가 생겼다.
- 특히 다음 질문들이 연결되어 있었다.
  - 함수 반환을 direct value로 받을지 indirect place로 받을지
  - 함수 인자를 direct/indirect/alias 중 어떤 규약으로 넘길지
  - `BC/NBC`와 call lowering 규칙을 같은 축으로 볼지
  - internal ABI를 symbol에 둘지, compile option에서 정할지
  - internal ABI flavor가 다른 바이너리끼리 직접 링크 가능한지

Decisions
## 1) 구현은 하나의 Citron internal ABI로 시작한다
- 현재 단계에서는 ABI family를 여러 개 동시에 구현하지 않는다.
- 우선은 하나의 `Citron internal ABI`만 실제 구현한다.
- 다만 lowering 인터페이스와 symbol model은 나중에 ABI flavor를 추가할 수 있도록 ABI-aware하게 설계한다.

의도:
- 구현 복잡도를 낮춘다.
- QIR/MIR를 너무 일찍 target-specific하게 쪼개지 않는다.
- 나중에 `MSVC-like`, `Itanium-like` flavor를 policy 추가로 확장할 수 있게 한다.

## 2) `BC/NBC`와 return/parameter passing mode는 다른 축이다
- `BC/NBC`는 값 의미/생성 의미를 구분하는 언어·MIR 축이다.
- direct/indirect 반환, direct/indirect/alias 인자 전달은 호출 ABI 축이다.
- 따라서 함수 호출 lowering 규칙은 `BC/NBC`만으로 직접 결정하지 않고, 별도의 ABI passing mode 개념으로 다룬다.

권장 개념:
- `ReturnPassingMode::DirectValue`
- `ReturnPassingMode::IndirectPlace`
- `ParameterPassingMode::DirectValue`
- `ParameterPassingMode::IndirectValue`
- `ParameterPassingMode::Alias`

Notes:
- `BC/NBC`는 passing mode를 계산할 때 참고 요소가 될 수는 있다.
- 하지만 ABI 스펙의 최종 표현은 `BC/NBC`가 아니라 `Direct/Indirect/Alias` 같은 호출 규약 개념이 더 적절하다.

## 3) 초기 ABI 스펙은 모호한 `large` 대신 명시 규칙을 사용한다
- ABI는 caller와 callee가 독립적으로 같은 결론에 도달해야 하므로, `large type` 같은 모호한 표현으로 두지 않는다.
- 초기 Citron internal ABI에서는 크기 heuristic보다, direct value로 허용하는 타입군을 좁게 열거하는 보수적 정책을 우선 선호한다.

예상 초기 정책 예:
- direct value return / param:
  - `bool`
  - `int`
  - native int
  - raw pointer / reference-like value
- 그 외:
  - indirect place return
  - indirect value parameter

Notes:
- 나중에 size/alignment/triviality 기반 규칙을 넣을 수는 있다.
- 그 경우에도 ABI 문서에는 반드시 수치와 판정 규칙을 명시해야 한다.

## 4) return / parameter 규약은 ABI 스펙에 함께 들어간다
- 함수 반환 규약과 함수 인자 규약은 모두 call boundary contract이므로 ABI 문서에 같이 들어가야 한다.
- ABI 문서에는 최소한 다음이 포함된다.
  - return passing mode
  - parameter passing mode
  - by-value parameter object의 construct/destroy owner
  - alias/reference parameter 규약
  - hidden sret/hidden parameter 유무

## 5) internal ABI flavor는 compile context에서 결정한다
- internal 함수 declaration 자체는 "Citron internal function"이라는 성격만 가지면 충분하다.
- 실제로 이를 어떤 internal ABI flavor로 lower할지는 compile option / target config / compilation context가 결정한다.
- 즉 symbol에 `CitronInternalMSVCLikeABI`, `CitronInternalItaniumLikeABI`를 직접 저장하지 않는다.

정리:
- internal declaration:
  - symbol에는 internal function이라는 성격만 둔다
  - 실제 internal ABI flavor는 compile context가 결정한다
- external declaration:
  - `extern(c)`, `extern(msvc)`, `extern(itanium)` 같은 foreign ABI contract를 symbol에 둔다

Rationale:
- 같은 internal source declaration이 target에 따라 다른 internal ABI flavor로 lowering될 수 있다.
- 이를 symbol에 직접 bake-in하면 declaration model이 target policy에 과하게 오염된다.
- 반대로 external declaration은 API 표면의 ABI contract이므로 symbol에 있어야 한다.

## 6) internal ABI flavor mismatch는 direct link를 허용하지 않는다
- `CitronInternalMSVCLikeABI`와 `CitronInternalItaniumLikeABI`처럼 internal ABI flavor가 다르면, direct internal link-compatible하지 않다고 본다.
- 하나의 link unit 또는 바이너리 경계 안에서는 하나의 Citron internal ABI flavor만 사용한다.
- ABI flavor mismatch는 compile/link 단계에서 에러로 막는 편이 안전하다.

이유:
- parameter passing shape
- return passing shape
- hidden parameter
- cleanup ownership
- 나중에는 mangling/type layout 규칙

등이 달라질 수 있기 때문이다.

## 7) DLL/shared library 같은 외부 경계에서는 explicit external ABI로 연결한다
- internal ABI flavor가 달라도, DLL/shared library/export boundary에서는 별도의 external ABI를 고정하면 상호 운용이 가능할 수 있다.
- 중요한 것은 internal ABI가 아니라 export/import boundary의 ABI contract다.

예상 구조:
- 라이브러리 내부 구현: `CitronInternalItaniumLikeABI`
- export 함수: `extern(c)` 또는 `extern(msvc)` 같은 stable external ABI
- 소비자 쪽 내부 구현: `CitronInternalMSVCLikeABI`
- 양쪽은 internal ABI가 아니라 export ABI를 통해 연결

결론:
- internal ABI flavor mismatch는 direct internal link를 막는다.
- 외부 경계는 explicit thunk / wrapper / export ABI를 통해 연결한다.

Open points
- 초기 Citron internal ABI에서 direct value return/param 허용 타입군을 정확히 어디까지 둘지
- by-value parameter object의 destroy owner를 caller/callee 중 어느 쪽으로 확정할지
- caller-destroy라면 cleanup 시점을 `call 직후`와 `MTopLevel_Call 종료 시점` 중 어디로 둘지
- `ReturnPassingMode` / `ParameterPassingMode`를 ABI policy object에서 계산할지, 일부 cached metadata를 둘지
- compile option에서 internal ABI flavor를 어떻게 표기할지(`citron-internal-msvc-like`, `citron-internal-itanium-like` 등)
- external ABI declaration syntax를 어디까지 허용할지(`extern(c)`, `extern(msvc)`, `extern(itanium)`, future `extern(citron_stable)` 등)
- flavor mismatch를 어떤 단계에서 진단할지(semantic, IR linking, final link)

Action Items
- [ ] Citron internal ABI 초안에 `ReturnPassingMode` / `ParameterPassingMode` / cleanup owner 항목을 표 형태로 정리
- [ ] internal function과 external function declaration이 각각 어떤 ABI metadata를 가져야 하는지 RSymbol 관점에서 정리
- [ ] compile option 기반 internal ABI flavor 선택 지점을 문서화
- [ ] ABI flavor mismatch 진단 시점을 정리

Update: 2026-04-13 (ABI v0 draft)

## 8) Citron internal ABI v0 초안
- 함수 결과와 by-value 인자는 `Direct` 또는 `Indirect` passing mode를 가진다.
- `Direct` mode는 값 자체를 직접 전달한다.
- `Indirect` mode는 caller-provided storage를 통해 전달한다.

### BC 값
- BC 값은 constructor/destructor 의미를 갖지 않는다.
- BC indirect parameter는 caller가 storage를 준비하고 그 storage에 값을 써 넣는다.
- BC indirect return은 caller가 result storage를 준비하고 callee가 그 storage에 결과값을 써 넣는다.
- BC 값의 읽기 가능 여부는 initialize analysis가 보장한다.

### NBC 값
- NBC 값은 object lifetime semantics를 가진다.
- NBC indirect parameter는 caller가 storage를 준비하고 그 위에 parameter object를 construct한다.
- NBC indirect parameter object의 destructor는 callee가 함수 종료 시 호출한다.
- NBC indirect parameter storage의 reclaim은 caller가 담당한다.
- NBC indirect return은 caller가 result storage를 준비하고 callee가 그 위에 return object를 construct한다.
- NBC indirect return object의 destructor와 storage reclaim은 caller가 담당한다.

### 기본 mode
- `int`, `bool`, raw pointer, reference-like value는 기본적으로 `Direct`를 사용한다.
- 그 외의 값 타입은 기본적으로 `Indirect`를 사용한다.

### Alias parameter
- `ref` 계열 parameter는 `Alias` passing mode를 사용한다.
- `Alias` mode의 physical representation은 pointer다.

### Notes
- BC/NBC와 Direct/Indirect는 서로 다른 축이다.
- BC/NBC는 값 의미 및 lifetime semantics를 구분한다.
- Direct/Indirect는 call boundary에서의 ABI 전달 규약을 구분한다.
- 따라서 passing mode는 BC/NBC만으로 직접 치환하지 않고, ABI policy가 별도로 계산한다.
