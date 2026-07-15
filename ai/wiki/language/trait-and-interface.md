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

## Conformance Declaration And Implementation

타입의 원본 module은 타입 선언부에 canonical trait conformance를 선언한다. Struct는 concrete struct를 상속하지 않으므로 `:` 뒤에는 trait만 올 수 있다.

v1에서 `impl` target은 `struct`로 한정한다. `class`, `enum`, tuple/nullable/pointer/ownership wrapper 같은 다른 type category의 conformance는 후속 설계에서 단계적으로 검토한다.

```citron
struct S : Trait1, Trait2
{
}

impl S : Trait1 { }
impl S : Trait2 { }
```

각 `(type, trait)` declaration에는 같은 module 안에 정확히 하나의 `impl` block이 필요하다.

### Generic Canonical Conformance

generic struct의 canonical conformance도 원본 module의 struct header와 대응 `impl`으로 선언한다.

```citron
struct S<T> : Trait
{
}

impl S : Trait { }
```

`impl S : Trait`는 source-level 생략형이다. `S`가 generic struct이면 compiler는 struct의 전체 generic arity에 맞춘 fresh type parameter를 도입해, 개념적으로 `impl<T> S<T> : Trait`와 같은 universal conformance schema로 정규화한다. 같은 이름을 generic arity만 다르게 overload하지 않으므로 이 생략형의 target arity는 type resolution으로 정해진다.

- `struct S<T> : Trait`는 모든 well-formed `S<T>`가 `Trait`를 구현한다는 canonical 약속이다.
- 대응 canonical `impl S : Trait`은 그 전체 범위를 덮어야 하며, `S<int>`처럼 일부 specialization만 구현해서는 충족되지 않는다.
- 초기 구현은 이 생략형을 먼저 지원한다. full generic impl 표기와 `where` constraint는 후속 단계로 둔다.
- full form은 `impl<T> S<T> : Trait where T : OtherTrait { }`처럼 generic signature와 target type expression을 명시한다. `where`가 필요한 impl에는 생략형을 쓰지 않는다.

specialization 또는 조건부 conformance는 canonical `impl`의 변형으로 직접 쓰지 않고, 이름 있는 extension bundle로 선언한다.

```citron
extension IntTrait for S<int> : Trait;

impl IntTrait for S<int> : Trait { }
```

따라서 `struct S<T> : Trait`는 universal canonical conformance에만 쓰고, `S<int> : Trait`처럼 적용 범위를 좁히는 관계는 bundle header가 public declaration surface로 제공한다.

외부 module은 이름 있는 `extension` bundle로 conformance를 선언한다.

```citron
public extension MyBundle for S : Trait3, Trait4;

impl MyBundle for S : Trait3 { }
impl MyBundle for S : Trait4 { }
```

- `extension`의 accessor가 bundle의 accessibility를 결정한다.
- `impl`에는 accessor를 붙이지 않는다.
- 한 module은 같은 target에 여러 이름의 bundle을 선언할 수 있다.
- 원본 module이 이미 선언한 canonical `(S, Trait)`는 외부 bundle이 재선언할 수 없다.
- external extension implementation은 target의 private member에 접근할 수 있는 trusted augmentation으로 본다.

외부 conformance는 import만으로 자동 활성화되지 않는다. 소비 파일에서 target과 필요한 trait를 모두 명시한다.

```citron
import ExtModule;

extend MyBundle for S : Trait3;
extend MyBundle for S : Trait3, Trait4;
```

`extend` directive는 file-local이며 bundle provider는 소비 module의 직접 dependency여야 한다. 동일한 concrete `(type, trait)`에 둘 이상의 활성 bundle이 매칭되면 conformance 사용 지점에서 ambiguity error를 낸다. Target 또는 trait 목록 생략형은 v1에서 허용하지 않는다.

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
impl SEnumerator : RefEnumerator
{
    type Item = int;
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
- `ai/notes/2026-06-29-accessibility-struct-trait-extension-direction.md`
- `ai/notes/2026-07-15-generic-impl-and-specialized-conformance.md`
