# 타입/소유 모델 스펙

Updated: 2026-03-06
Status: current

Void
- `void`는 `tuple<>`와 별도 타입으로 유지한다.
- `tuple<>`를 `void`의 별칭으로 두지 않는다.
- 제네릭 인자에서 `T=void`를 허용한다.
- 내부 인스턴스화에서는 `__VoidSubst`를 사용하고, 사용자에게는 노출하지 않는다.
- call MIR는 반환 종류에 따라 `MExp_Call`, `MInitExp_Call`, `MStmt_Call`로 나눈다.

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
