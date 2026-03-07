# MIR 값 모델 스펙

Updated: 2026-03-06
Status: current

Summary
- MIR은 bitwise-copyable(BC) 값과 non-bitwise-copyable(NBC) 값의 의미 이벤트를 분리해 표현한다.
- 읽기(`MRead`)와 생성(`MCreate`)을 구분하고, NBC 값은 `MInitExp`와 `MLoc_Materialize`를 통해 다룬다.

BC vs NBC
- BC 값은 값으로 들고 다녀도 의미 손실이 없는 타입이다.
  - primitive
  - `[BitwiseCopy] struct`
  - class, interface handle 값
- NBC 값은 ctor/copy/move/nullable wrapping 같은 의미 이벤트를 명시해야 하는 타입이다.
  - 일반 struct
  - struct를 포함하는 tuple
  - nullable struct 등

Core Rules
- `MExp`는 항상 BC 결과만 표현한다.
- `MInitExp`는 항상 NBC 결과만 표현한다.
- 읽기 입력은 `MRead`로 표현한다.
  - BC 읽기: `MRead_Value(MExp*)`
  - NBC 읽기: `MRead_Location(MLoc*)`
- 생성 계획은 `MCreate`로 표현한다.
  - BC 생성: `MCreate_Bitwise(...)`
  - NBC 생성: `MCreate_Init(...)`
- rvalue가 place로 필요할 때는 `MLoc_Materialize`를 사용한다.
- NBC 반환 call은 항상 sret(dest-passing) 규약으로 lowering한다.

Bitwise Policy
- bitwise-copyable 타입만 bitwise copy/assign 경로를 사용한다.
- 일반 struct에는 암시적 bitwise load/copy를 허용하지 않는다.
- `[BitwiseCopy] struct`는 멤버 전체가 bitwise-copyable이어야 한다.
- chain assign은 bitwise-assignable 타입에서만 허용한다.

Construction and Assign
- 초기화와 대입은 MIR에서 구분한다.
- NBC 초기화는 ctor/copy/move/RVO 의미 이벤트를 명시적으로 드러낸다.
- ctor/call RHS를 기존 초기화된 dest에 대입할 때는 materialize 후 assign 경로를 사용한다.

FollowUp
- verifier 또는 생성 시점 검사로 BC/NBC 불변식을 강제한다.
- `MOperand` 사용처는 `MRead` 기준으로 점진 이관한다.
