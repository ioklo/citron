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
struct S<T> : Trait<T>
{
}

impl S<U> : Trait<U> { }
```

generic canonical impl의 `S<U>`는 일반 type expression이 아니라 generic conformance target pattern이다. `U`는 impl header가 도입하는 fresh type parameter이고, `struct S<T>`의 `T`와 이름만 다른 alpha-equivalent parameter다. 같은 이름을 generic arity만 다르게 overload하지 않으므로 target arity는 type resolution으로 정해진다.

- `struct S<T> : Trait<T>`는 모든 well-formed `S<T>`가 `Trait<T>`를 구현한다는 canonical 약속이다.
- 대응 canonical `impl S<U> : Trait<U>`은 그 전체 범위를 덮어야 하며, `S<int>`처럼 일부 specialization만 구현해서는 충족되지 않는다.
- `where` constraint는 `impl S<U> : Trait<U> where U : OtherTrait { }`처럼 target-pattern parameter를 사용한다.

specialization 또는 조건부 conformance는 canonical `impl`의 변형으로 직접 쓰지 않고, 이름 있는 extension bundle로 선언한다.

```citron
extension IntTrait for S<int> : Trait<int>;

impl IntTrait for S<int> : Trait<int> { }
```

따라서 `struct S<T> : Trait`는 universal canonical conformance에만 쓰고, `S<int> : Trait`처럼 적용 범위를 좁히는 관계는 bundle header가 public declaration surface로 제공한다.

외부 module은 이름 있는 `extension` bundle로 conformance를 선언한다.

```citron
public extension MyBundle for S : Trait3, Trait4;

impl MyBundle for S : Trait3 { }
impl MyBundle for S : Trait4 { }

extension GenericBundle<T> for S<T> : Trait<T>;
impl GenericBundle<U> for S<U> : Trait<U> { }
```

- `extension`의 accessor가 bundle의 accessibility를 결정한다.
- `impl`에는 accessor를 붙이지 않는다.
- 한 module은 같은 target에 여러 이름의 bundle을 선언할 수 있다.
- 원본 module이 이미 선언한 canonical `(S, Trait)`는 외부 bundle이 재선언할 수 없다.
- external extension implementation은 target의 private member에 접근할 수 있는 trusted augmentation으로 본다.

외부 conformance는 import만으로 자동 활성화되지 않는다. 소비 파일은 bundle을 명시적으로 활성화한다. bundle header가 target mapping을 이미 갖고 있으므로, 기본 activation은 bundle 이름만 쓴다.

```citron
import ExtModule;

extend MyBundle;
```

generic bundle과 선택 activation은 다음처럼 쓴다.

```citron
extension Bundle<T> for S<T> : Trait<T>, Trait2<T>;

extend Bundle;                       // 모든 instantiation, 모든 trait entry
extend Bundle<int>;                  // `int` instantiation의 모든 trait entry
extend<U> Bundle<U> : Trait<U>;      // 모든 U에 대한 Trait<U> entry만 선택
```

- `extend Bundle;`는 bundle family 전체 activation이다.
- `extend Bundle<int>;`의 `int`는 concrete type argument다.
- generic activation pattern은 `extend<U>`에서 parameter를 명시적으로 도입한다. 뒤의 `Bundle<U>`와 trait selector는 그 parameter를 type argument로 적용한다.
- nested declaration에서는 parameter clause 없이 lexical generic context의 parameter를 capture할 수 있다. 예: `class C<U> { extend Bundle<U>; }`의 `U`는 outer `C<U>`의 parameter다.
- `for S<...>` target은 bundle header에서 결정되므로 activation에서 쓰지 않는다.
- trait selector가 없으면 bundle의 모든 trait entry를 활성화한다.

trait entry별 activation selection은 유지한다. 예를 들어 `extend<U> Bundle<U> : Trait<U>;`는 `Trait<U>` entry만 활성화한다.

`extend` directive는 file-local이며 bundle provider는 소비 module의 직접 dependency여야 한다. 활성화된 bundle entry들이 같은 concrete `(type, trait)`에 매칭될 수 있으면 동시 활성화를 금지한다. 이 overlap은 conformance 사용 지점까지 미루지 않고 activation 시점에 error로 진단한다. 사용 편의보다 explicit activation을 우선하므로, 필요한 trait entry만 selector로 명시해 활성화해야 한다. generic pattern의 `where` constraint를 포함한 정확한 overlap 판정은 후속 설계 항목이다.

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
