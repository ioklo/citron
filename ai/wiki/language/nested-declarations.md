# Nested Declarations

Status: draft current
Area: language, type system, conformance
Keywords: nested class, nested struct, nested trait, generic identity, conformance

## Current Direction
- Nested `class`, `struct`, `trait`는 논리적으로 가능하다.
- 금지하거나 미루는 이유가 있다면 "말이 안 된다"가 아니라 v1 implementation scope와 design stability다.
- Generic outer type 안 nested declaration은 outer generic parameter capture 문제를 만든다.
- 이 문제는 nested trait뿐 아니라 nested class/struct에도 동일하게 적용된다.

## Nested Generic Identity
Outer generic parameter capture를 허용한다면 nested trait/type identity는 아래 요소를 포함한다.

```text
outer declaration identity
outer type arguments
nested declaration identity
nested type arguments
```

예:
```citron
class C<T, U>
{
    public trait R<V>
    {
    }
}
```

```text
C<int, short>.R<bool>
```

는 `C<T, U>`의 `T = int`, `U = short`, `R<V>`의 `V = bool` specialization이다.

## Nested Generic Conformance
Nested generic type conformance는 family-level conformance로 해석한다.

```citron
class C<T>
{
    public struct S<U>
    {
    }

    public trait R<V>
    {
    }

    public extend S<U> : R<T>
    {
    }
}
```

의미:
```text
for all T, U:
  C<T>.S<U> : C<T>.R<T>
```

## Generic Target Extend
Generic target extend에는 type argument를 명시하는 쪽을 선호한다.

```citron
public extend S<U> : R<T>
{
}
```

`extend S : R<T>`는 `S`의 type parameter를 암시적으로 여는지, non-generic `S`를 가리키는지 애매하다.

## Why Nested Types
- 주요 이점은 namespace와 outer generic parameter 공유다.
- Private implementation type은 unit/cti export control로 숨길 수도 있으므로 nested type만의 강한 이유는 아니다.
- `List<T>.Enumerator`처럼 outer generic parameter를 한 곳에서 관리하고 싶은 경우에는 nested type이 의미가 있다.

## Open Points
- Nested class/struct/trait를 v1에서 허용할지
- Generic outer parameter capture를 모든 nested declaration에 허용할지
- Specialized extend를 허용할지
- Generic conformance overlap을 언제 어떻게 진단할지

## History
- `ai/notes/2026-05-22-nested-decl-visibility-and-resolved-cti.md`
