# Member Translation / String Value Model 논의 (2026-02-28)

## 목적
- `IrExpAndMemberName...` 계열 번역기의 공통 체크 구조를 어떻게 정리할지 합의합니다.
- `SExp -> MSharedExp` 경로 재구성과 `string`의 언어/IR 모델 방향을 정리합니다.

## 배경
- `IrExpAndMemberNameToMSharedExpTranslation.cpp`에는 여러 `Visit` 케이스가 있고, 멤버 조회 / kind 확인 / static-instance 제약 / base 변환 같은 공통 체크가 반복됩니다.
- 기존 `SExp -> IrExp -> MSharedExp/MLoc` 경로는 재귀적이라 어떤 단계가 필요한지 코드만으로 파악하기 어렵다는 문제가 있습니다.
- `MCreate`가 도입된 이후 `MExp_String`이 계속 남아 있는 구조가 notes 상의 value-category 정책과 얼마나 맞는지도 검토가 필요했습니다.

## 논의 요약

### 1) `IrExpAndMemberName...` 공통 체크 구조
- 8개 `Visit` 케이스는 완전히 같지 않아서 전면 통합은 어렵습니다.
- 다만 공통 체크가 여러 곳에 흩어져 있어 누락 위험이 큽니다.
- 따라서 완전 통합보다, 공통 "해석(resolution)" 단계만 helper로 추출하는 방향이 적절합니다.
- 특히 `IrExp_Static*`에 대한 `Visit`들은 대부분 같은 해석을 한 뒤 마지막 결과 생성만 다르므로, `ResolveMemberOnStaticBase(...)` 같은 함수를 두고 builder만 각 translator에서 다르게 두는 구조가 적합합니다.

### 2) static / instance 체크 위치
- static/instance 체크는 dependency가 거의 없어 중간에 빠져도 발견이 어렵다는 문제가 있습니다.
- 이를 별도 verifier 패스로 옮기는 아이디어를 검토했지만, 사용자-facing 의미 오류를 너무 늦게 발견하게 되는 단점이 있습니다.
- 결론적으로:
  - 사용자-facing 제약(static/instance, kind mismatch 등)은 현재 번역 단계에서 계속 즉시 진단합니다.
  - 대신 check 누락을 잡기 위한 verifier는 debug/CI용 안전망으로 추가하는 것이 유효합니다.
- 즉 verifier는 주 경로를 대체하는 것이 아니라, 번역기 invariant 누락을 잡는 백업 레이어입니다.

### 3) `GetMember`와 typed getter 방향
- `GetMember`에 `static only` / `instance only` 같은 정책 인자를 넣는 아이디어는 좋지만, `virtual` 전면 수정 비용이 큽니다.
- 현실적인 방향은 기존 `GetMember`를 유지하면서, 상위 래퍼나 typed getter를 도입하는 것입니다.
- 예: `GetClassVarMember(...)`, `GetStructVarMember(...)`
- 같은 이름의 멤버가 있는데 타입이 다를 경우 `NotFound`로 뭉개지 말고 `WrongKind`로 구분하는 것이 더 좋습니다.
- 따라서 이런 typed getter는 `optional<T>`보다 `expected<T, Error>` 계열이 적합합니다.
- `RMember_NotFound` 같은 sentinel을 성공 타입 안에 넣는 방식은 피합니다.

### 4) `SExp -> MSharedExp` 경로 재구성
- 기존 경로는 우선 전체를 `SExp -> IrExp`로 만든 뒤, 최종 단계에서 `IrExp -> MSharedExp` 또는 `IrExp -> MLoc`로 내립니다.
- 이 방식은 재귀적이고 간접 단계가 많아, 어느 단계가 실제로 필요한지 파악하기 어렵습니다.
- 새 방향은 `SExp_Member`의 base 부분에만 `IrExp`를 적용하고, 그 결과에 대해:
  - `IrExp, name -> IrExp`
  - `IrExp, name -> MSharedExp`
  로 직접 연결하는 것입니다.
- 이 방식은 경로가 더 명시적이라 코드 의도가 잘 드러납니다.
- 다만 `IrExp,name -> IrExp`와 `IrExp,name -> MSharedExp`는 최종 산출물이 달라 완전 통합은 어렵고, 공통 해석만 공유하는 것이 적절합니다.

### 5) 공통 해석 함수 위치
- `ResolveMemberOnStaticBase(...)` 같은 helper는 `SyntaxIR0Translator` 내부 전용으로 두는 것이 맞습니다.
- `RSymbol`로 올리기보다, 번역기 계층 내부 공용 파일에 두는 것이 계층상 더 적절합니다.
- 현재 작업 기준으로는 `IrExpAndMemberNameTranslation.cpp/h` 같은 공용 파일에 두는 방향이 자연스럽습니다.

### 6) visitor `ResultType`
- 일부 visitor는 현재 `unexpected`를 실제로 반환하지 않더라도, 의미상 진단 가능한 실패가 개입될 수 있는 translator라면 `expected<T, DiagPtr>`를 유지하는 편이 낫습니다.
- 같은 계층의 translator들과 시그니처 일관성이 유지되고, 나중에 체크가 추가될 때도 자연스럽습니다.

### 7) `MExp_String`, `MCreate`, string lowering
- notes 기준으로 `MCreate`는 "어떤 place를 어떤 의미 이벤트로 초기화하느냐"를 표현하는 축입니다.
- 따라서 일반 non-bitwise struct라면 장기적으로 `MCreate` / `Materialize` 경로를 타는 것이 더 일관적입니다.
- 하지만 현재 MIR에는 string ctor에 필요한 저수준 인자 표현(shared char* 등)이 없습니다.
- 이 상태에서 `MCreate_StructCtor`로 string을 직접 맞추려 하면 MIR가 하위 표현(QIR/런타임 ABI)에 끌려갑니다.
- 현실적인 방향은:
  - MIR에서는 string 전용 의미 노드(`MExp_String` 또는 향후 `MCreate_String`)를 유지하고
  - QIR lowering에서 실제 문자열 생성 절차를 푸는 것입니다.
- 장기적으로 생성 이벤트를 `MCreate` 축으로 정리하고 싶다면 `MCreate_String`이 `MExp_String`보다 notes 방향과 더 잘 맞습니다.

### 8) bitwise literal과 struct ctor의 위치
- `bool`, `int` 같은 bitwise literal은 `MExp`에 두고
- 일반 non-bitwise struct ctor는 `MCreate`에 두는 구분은 자연스럽습니다.
- 기준은 "값으로 바로 들고 다녀도 의미 손실이 없는가"입니다.
  - bitwise-copyable 값: `MExp`
  - 생성 이벤트가 본질인 값: `MCreate`

### 9) `string`을 `struct`로 할지 `class`로 할지
- 현재 구현/MIR/QIR 정합성만 보면 `class-like builtin`이 쉽습니다.
- 하지만 장기 언어 설계 관점에서는 `string`이 값처럼 다뤄지는 경우가 대부분이라 `immutable struct`가 더 좋은 모델이라는 결론에 도달했습니다.
- `class`의 장점은 주로 구현 편의이고, `immutable struct`의 장점은 언어 의미 품질입니다.
- 특히 값 의미, equality, generic, capture, API 추론 측면에서 `immutable struct`가 자연스럽습니다.
- Swift의 `String`도 struct이지만, 이는 강한 런타임/optimizer/CoW 설계를 전제로 한다는 점을 확인했습니다.

## 최종 합의
- `IrExpAndMemberName...` 계열은 전면 통합하지 않고, 공통 해석(helper)만 추출하는 방향으로 정리합니다.
- static/instance 같은 의미 제약은 번역 단계에서 계속 즉시 진단합니다.
- verifier는 debug/CI에서 invariant 누락을 잡는 안전망으로 고려합니다.
- `GetMember`의 전면 인터페이스 변경보다 typed getter / wrapper를 우선 검토합니다.
- `SExp -> MSharedExp`는 base에만 `IrExp`를 적용하는 새 경로가 더 명시적이므로 이 방향으로 진행합니다.
- `string`의 언어 표면 모델은 **immutable struct**로 정합니다.
- 다만 lowering/런타임은 당분간 special builtin 경로를 허용합니다. 즉 "의미는 struct, 구현은 한동안 특수 처리" 전략을 택합니다.

## 남은 결정 사항
- `string`의 MIR 표현을 당분간 `MExp_String`으로 유지할지, `MCreate_String`으로 옮길지 결정이 필요합니다.
- `ResolveMemberOnStaticBase(...)` 수준의 공통 helper를 어디까지 확장할지(예: `IrExp_ClassVar`, `IrExp_SharedStructVar` 등) 범위를 정해야 합니다.
- typed getter의 오류 모델(`NotFound`, `WrongKind`, `Ambiguous`)을 구체화할 필요가 있습니다.

## Action Items
- [ ] `IrExpAndMemberNameTranslation.cpp/h`에 공통 resolution helper 초안 작성
- [ ] `IrExp_Static*` 케이스부터 공통 helper 적용 여부 검토
- [ ] typed getter / wrapper API 초안 정리
- [ ] `string`의 MIR 표현(`MExp_String` vs `MCreate_String`) 임시 정책 결정
