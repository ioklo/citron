# 타입/소유 모델 스펙

Updated: 2026-04-26
Status: current

Void
- `void`는 `tuple<>`와 별도 타입으로 유지한다.
- `tuple<>`를 `void`의 별칭으로 두지 않는다.
- 제네릭 인자에서 `T=void`를 허용한다.
- 내부 인스턴스화에서는 `__VoidSubst`를 사용하고, 사용자에게는 노출하지 않는다.
- call MIR는 반환 종류에 따라 `MExp_Call`, `MInitExp_Call`, `MStmt_Call`로 나눈다.

BC / NBC Value Semantics
- BC(bitwise-copyable) 값은 bitwise copy가 언어 의미를 바꾸지 않는 값이다.
  - primitive
  - class/interface handle 값
  - `[BitwiseCopy]` 조건을 만족하는 struct
- NBC(non-bitwise-copyable) 값은 object lifetime operation이 의미 이벤트인 값이다.
  - 일반 struct
  - NBC 값을 포함하는 tuple
  - nullable struct 등 생성/소멸/복원 의미가 필요한 값
- BC 값은 여러 번 copy되어도 observable behavior가 보존된다.
- NBC 값은 constructor, copy/move constructor, assignment, destructor 의미를 보존해야 한다.
- NBC rvalue가 place로 필요하면 materialize된 object location을 통해 다룬다.
- `[BitwiseCopy] struct`는 모든 멤버가 bitwise-copyable이어야 한다.
- chain assignment는 bitwise-assignable 타입에서만 허용한다.

String
- `string`의 언어 표면 모델은 immutable struct다.
- lowering/런타임에서는 당분간 special builtin 경로를 허용한다.

Ownership
- `shared<T>`는 shared ownership이다.
- `box<T>`는 unique ownership(move-only)이다.

Address-of
- `&`는 polymorphic 연산자다.
- 기대 타입이 `T*`이면 raw pointer를 생성한다.
- 기대 타입이 `shared T`이면 shared 참조를 생성한다.
- 기대 타입이 없거나 다의적이면 컴파일 에러다.
- owner 값(`shared<T>`, `box<T>`)에 대한 `&`는 `T*` 문맥에서만 허용한다.
- `shared <- &shared`는 금지한다. shared alias는 직접 대입으로 표현한다.
- static shared 참조는 전역 singleton immortal owner를 공유한다.
