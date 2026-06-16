# Trait And Interface

Status: draft current
Area: language, type system
Keywords: trait, interface, func, lambda, associated type, concept

## Current Rules
- `trait`는 static contract다.
- `interface`는 dynamic/runtime dispatch contract다.
- 일반 `dyn trait`는 도입하지 않는다.
- dynamic dispatch가 필요하면 명시적인 `interface`를 설계한다.
- trait 이름에는 `I` prefix를 붙이지 않는다. interface 이름은 필요하면 `I` prefix를 사용할 수 있다.
- `lambda<>` type expression은 두지 않는다.
- `func<R, Params...>`는 callable static contract type expression이다.

## Trait
`trait`는 generic/static polymorphism에 쓰는 compile-time contract다.

```citron
trait Cloneable
{
    This Clone();
}
```

`This`는 associated type이 아니라 trait body에서 현재 구현 concrete type을 가리키는 special self type이다.

## Associated Type
trait body의 `type X;`는 associated type requirement다.

```citron
trait RefEnumerator
{
    type Item;
    Item* Next();
}
```

구현체는 `using` 또는 nested type으로 requirement를 충족한다.

```citron
extend SEnumerator : RefEnumerator
{
    using Item = int;
    int* Next() { ... }
}
```

Associated type inference는 v1에서 제외한다.

Associated type constraint와 concept는 `concepts-and-constraints.md`를 본다.

## Interface
`interface`는 runtime dynamic dispatch contract다. Associated type과 `This`를 가진 trait를 자동으로 dynamic object로 바꾸지 않는다.

Dynamic shape가 필요하면 별도 interface를 사람이 설계한다.

```citron
interface IRefEnumerator<T>
{
    T* Next();
}
```

## Callable
`func<R, Params...>`는 일반 generic trait 이름이 아니라 parameter passing mode까지 담을 수 있는 builtin callable type expression이다.

```citron
func<void, [in] S&, T>
```

람다 반환은 `some func<...>`로 표현한다.

```citron
some func<void, int> MakeHandler()
{
    return [](int x) { use(x); };
}
```

Dynamic callable이 필요하면 별도 interface 또는 interface type expression을 설계한다. 일반 `dyn func<>`는 현재 도입하지 않는다.

## History
- `ai/notes/2026-05-14-trait-refenumerable-foreach-direction.md`
- `ai/notes/2026-05-15-trait-concept-associated-type-design.md`
- `ai/notes/2026-06-16-some-opaque-result-and-cti.md`
