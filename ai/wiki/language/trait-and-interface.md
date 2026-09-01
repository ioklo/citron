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

타입 header의 trait도 선언의 외부 계약 접근성 규칙을 따른다. public struct의 header에 private trait를 직접 나열하지 않는다. 내부에서만 필요한 conformance는 타입 header 대신 해당 trait의 접근 범위를 넘지 않는 extension bundle로 분리한다. 상세한 접근 범위 포함 규칙은 `visibility-and-reachability.md`를 본다.

### Generic Canonical Conformance

generic struct의 canonical conformance도 원본 module의 struct header와 대응 `impl`으로 선언한다.

```citron
struct S<T> : Trait<T>
{
}

impl S<U> : Trait<U> { }
```

generic canonical impl의 `S<U>`는 일반 type expression이 아니라 generic conformance target pattern이다. `U`는 impl header가 소스 수준에서 도입하고 소유하는 type parameter이고, `struct S<T>`의 `T`와 이름만 다른 alpha-equivalent parameter다. 이는 signature 비교 도중 compiler가 임시 fresh binder를 만든다는 뜻이 아니다. 같은 이름을 generic arity만 다르게 overload하지 않으므로 target arity는 type resolution으로 정해진다.

- `struct S<T> : Trait<T>`는 모든 well-formed `S<T>`가 `Trait<T>`를 구현한다는 canonical 약속이다.
- 대응 canonical `impl S<U> : Trait<U>`은 그 전체 범위를 덮어야 하며, `S<int>`처럼 일부 specialization만 구현해서는 충족되지 않는다.
- `where` constraint는 `impl S<U> : Trait<U> where U : OtherTrait { }`처럼 target-pattern parameter를 사용한다.

#### Generic Requirement Matching

generic trait requirement와 impl 함수를 비교할 때 type parameter의 source name은
identity가 아니다. 먼저 conformance target을 비교해 struct header binder와 impl
header binder의 대응을 확정하고, 함수 requirement와 impl 함수의 local binder도
같은 위치끼리 대응시킨다.

예를 들어 다음 두 함수를 비교한다고 하자.

```text
trait: ^T3. { Ret = void, Params = [T1, T2, T3*] }
       outer substitution = [T1 => int, T2 => list<T5>]
       local slots        = [T3 => 0]

impl:  ^T6. { Ret = void, Params = [int, list<T5>, T6*] }
       outer substitution = [T4 => T4, T5 => T5]
       local slots        = [T6 => 0]
```

signature 비교는 `RType`을 새로 만들거나 trait의 `T3` 자리에 impl의 `T6`을
주입하지 않는다. 양쪽 `RType`을 재귀적으로 방문하면서 type variable을 다음 순서로
해석한다.

1. 해당 side의 outer substitution domain에 속하는 formal binder이면 argument로
   치환된 type을 상대편과 재귀 비교한다.
2. 아니면 function-local slot lookup을 수행하고, 양쪽 slot index를 비교한다.
3. 어느 쪽에도 속하지 않는 binder는 `RTypeParam*` exact identity로 비교한다.

따라서 `T1`은 `int`, `T2`는 `list<T5>`로 lazy하게 해석되고, `T3`와 `T6`은
각각 local slot 0이므로 대응한다. `$0`을 나타내는 compiler-generated `RType`이나
한쪽 signature의 binder를 포함하는 cross-signature applied type은 만들지 않는다.

`GetGlobalIndex()`가 우연히 같다는 이유로 두 binder를 대응시키지 않는다. exact
identity와 substitution domain membership은 `RTypeParam*`로 판정한다.
`RTypeArguments`의 count와 global index만으로 치환 가능 여부를 판단하면 unrelated
binder를 잘못 치환할 수 있다. 또한 substitution RHS는 이미 target context에 속하므로
원래 outer substitution을 다시 적용하지 않는다. 함수 type parameter 개수와 제약,
인자 개수, 전달 방식도 함께 일치해야 한다.

specialization 또는 조건부 conformance는 canonical `impl`의 변형으로 직접 쓰지 않고, 이름 있는 extension bundle로 선언한다.

```citron
extension IntTrait for S<int> : Trait<int>;

impl IntTrait for S<int> : Trait<int> { }
```

따라서 `struct S<T> : Trait`는 universal canonical conformance에만 쓰고, `S<int> : Trait`처럼 적용 범위를 좁히는 관계는 bundle header가 자신의 accessibility에 맞는 declaration surface로 제공한다.

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
- bundle의 target/trait 및 type arguments는 bundle의 외부 계약 접근성 검사를 받는다. bundle activation으로 private trait의 접근 제한을 우회하지 않는다.
- 현재 trait 구현 전용 extension은 trait/header 접근성, trait 인자·associated type 노출, impl signature 일치 검사로 계약을 보장한다. trusted private 접근 권한이 임의의 public 반환 타입을 추가할 권한을 뜻하지 않는다.

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

구현체는 정식 `type` alias 또는 concrete nested type으로 requirement를 충족한다. unit-local convenience `using` alias와 구분한다.

```citron
impl SEnumerator : RefEnumerator
{
    type Item = int;
    int* Next() { ... }
}
```

Associated type inference는 v1에서 제외한다.

공개 conformance의 associated type witness로 private 타입을 노출하는 것은 금지한다. `impl`이나 witness에 `public` 표기가 없어도, conformance를 통해 노출되는 타입의 접근 범위를 검사한다. 이는 `some Trait` 뒤에 숨겨진 private backing type과 구분한다.

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
- `ai/notes/2026-08-31-declaration-contract-accessibility.md`
- `ai/notes/2026-05-14-trait-refenumerable-foreach-direction.md`
- `ai/notes/2026-05-15-trait-concept-associated-type-design.md`
- `ai/notes/2026-06-16-some-opaque-result-and-cti.md`
- `ai/notes/2026-06-29-accessibility-struct-trait-extension-direction.md`
- `ai/notes/2026-07-15-generic-impl-and-specialized-conformance.md`
- `ai/notes/2026-08-07-applied-decl-tenv-and-trait-matching.md`
- `ai/notes/2026-08-08-generic-application-without-persistent-type-env.md`
- `ai/notes/2026-08-15-lazy-type-substitution-and-smtypeview.md`
- `ai/notes/2026-08-20-generic-application-composition-and-smtype-removal.md`
- `ai/notes/2026-08-24-rtype-trait-function-correspondence.md`
