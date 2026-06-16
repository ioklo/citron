# Types And References

Status: draft current
Area: language, type system, ownership
Keywords: type, void, BC, NBC, pointer, reference, ownership, T&, T*, shared, box

## Current Rules
- Type layer의 기본 축은 value/handle 의미의 `T`와 raw pointer 의미의 `T*`다.
- `T&`는 일반 first-class type constructor가 아니다.
- `T&`는 제한된 surface slot에서만 허용하는 reference/alias 표기다.
- `void`는 `tuple<>`와 별도 타입으로 유지한다.
- BC 값은 bitwise copy가 언어 의미를 바꾸지 않는 값이다.
- NBC 값은 constructor/copy/move/assignment/destructor 같은 lifetime operation이 observable behavior인 값이다.

## Void
- `void`는 `tuple<>`와 별도 타입이다.
- `tuple<>`를 `void`의 alias로 두지 않는다.
- Generic argument에서 `T = void`를 허용한다.
- 내부 인스턴스화에서는 `__VoidSubst`를 사용할 수 있지만 사용자에게 노출하지 않는다.
- Call MIR는 return kind에 따라 `MExp_Call`, `MInitExp_Call`, `MStmt_Call`로 나눈다.

## BC / NBC Value Semantics
BC(bitwise-copyable) 값:
- bitwise copy가 언어 의미를 바꾸지 않는다.
- primitive
- class/interface handle value
- `[BitwiseCopy]` 조건을 만족하는 struct

NBC(non-bitwise-copyable) 값:
- object lifetime operation이 의미 이벤트다.
- 일반 struct
- NBC 값을 포함하는 tuple
- nullable struct 등 생성/소멸/복원 의미가 필요한 값

규칙:
- BC 값은 여러 번 copy되어도 observable behavior가 보존된다.
- NBC 값은 constructor, copy/move constructor, assignment, destructor 의미를 보존해야 한다.
- NBC rvalue가 place로 필요하면 materialized object location을 통해 다룬다.
- `[BitwiseCopy] struct`는 모든 member가 bitwise-copyable이어야 한다.
- Chain assignment는 bitwise-assignable type에서만 허용한다.

## String
- `string`의 언어 표면 모델은 immutable struct다.
- Lowering/runtime에서는 당분간 special builtin path를 허용한다.

## Ownership
- `shared<T>`는 shared ownership이다.
- `box<T>`는 unique ownership / move-only ownership이다.

## Address-Of
`&`는 polymorphic operator다.

규칙:
- Expected type이 `T*`이면 raw pointer를 생성한다.
- Expected type이 `shared T`이면 shared reference를 생성한다.
- Expected type이 없거나 ambiguous하면 compile error다.
- Owner value(`shared<T>`, `box<T>`)에 대한 `&`는 `T*` context에서만 허용한다.
- `shared <- &shared`는 금지한다. Shared alias는 직접 대입으로 표현한다.
- Static shared reference는 global singleton immortal owner를 공유한다.

## Reference Surface
`T&`는 non-owning alias/reference 표기다.

현재 허용 surface slot 후보:
- function parameter
- function return
- local alias declaration
- struct instance method의 implicit `this`

예:
```citron
void F([in] S& s);

var& y = x;
```

`T&`는 storage를 소유하지 않으며, lifetime safety를 언어가 정적으로 보장하지 않는다.

## Non-Goals
- Rust 수준의 전역 lifetime / borrow safety
- `T&`를 field, generic type argument, container element type 등에 자유롭게 쓰는 모델
- 모든 reference return의 dangling-free 보장
- 모든 container의 stable reference 보장

## Notes
- `T*`는 raw pointer로 계속 허용한다.
- container mutation에 따른 reference/iterator invalidation은 각 타입의 contract로 둔다.
- `foreach`와 collection API도 전역 memory safety보다 API contract 중심으로 설계한다.
- `return T&`는 허용 가능하지만, dangling 가능성은 언어 모델 일부로 받아들인다.

## History
- `ai/specs/language/types-and-ownership.md`
- `ai/notes/2026-05-14-reference-and-memory-safety-policy.md`
