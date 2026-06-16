# Nullable Type

Status: draft current
Area: language, type system
Keywords: nullable, null, optional, some bind, nullable inplace

## Current Rules
- Nested nullable은 의미적으로 병합하지 않는다.
- 타입 계층에서 `nullable<nullable T>`를 허용한다.
- `C?`는 reference-like/class-like 타입을 위한 compressed nullable representation으로 본다.
- 일반 `T?`는 tagged nullable representation으로 유지한다.
- Generic `T?`는 `T`가 reference-like 타입으로 확정되더라도 compressed representation으로 자동 전환하지 않는다.

## Representation Names
- Compressed nullable representation 이름은 `NullableInplace<T>`를 사용한다.
- 일반 tagged nullable representation 이름은 `Nullable<T>`를 사용한다.

개념:
```text
C?
  -> NullableInplace<C>

T?
  -> Nullable<T>

nullable<nullable<T>>
  -> nested nullable, not flattened
```

## Nullable Pattern
Nullable 값 검사와 binding은 `is some` pattern을 사용한다.

```citron
if (exp is some value)
{
    use(value);
}
```

현재 규칙:
- `some alias`는 checked bind를 뜻한다.
- `alias!`는 unchecked type alias / target alias를 명시하는 표기다.
- `some alias!`는 unchecked bind를 뜻한다.
- Checked target alias는 local target에서만 보장한다.
- Non-local target에 대해 `alias!` 없는 checked request를 하면 경고를 낸다.

`if` / `is` binding 전체 규칙은 별도 page로 옮길 예정이다.

## Open Points
- `C?`가 정확히 어떤 type kind에 대해 `NullableInplace<C>`가 되는지
- `Nullable<T>` / `NullableInplace<T>`의 standard library surface
- `some alias` / `alias!`의 최종 parser spelling
- Nullable pattern과 general `is` binding page의 책임 분리

## History
- `ai/specs/language/nullable-and-iteration.md`
- `ai/specs/language/if-and-is.md`
