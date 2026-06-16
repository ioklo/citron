# Types And References

Status: draft current
Area: language, type system
Keywords: type, pointer, reference, ownership, T&, T*

## Current Rules
- Type layer의 기본 축은 value/handle 의미의 `T`와 raw pointer 의미의 `T*`다.
- `T&`는 일반 first-class type constructor가 아니다.
- `T&`는 제한된 surface slot에서만 허용하는 reference/alias 표기다.

허용 후보:
- function parameter
- function return
- local alias declaration
- struct instance method의 implicit `this`

## Non-Goals
- Rust 수준의 전역 lifetime / borrow safety
- `T&`를 field, generic type argument, container element type 등에 자유롭게 쓰는 모델
- 모든 reference return의 dangling-free 보장
- 모든 container의 stable reference 보장

## Notes
- `T*`는 raw pointer로 계속 허용한다.
- container mutation에 따른 reference/iterator invalidation은 각 타입의 contract로 둔다.
- `foreach`와 collection API도 전역 memory safety보다 API contract 중심으로 설계한다.

## History
- `ai/notes/2026-05-14-reference-and-memory-safety-policy.md`
