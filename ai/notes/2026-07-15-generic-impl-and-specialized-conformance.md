# 회의 / 설계 노트

Date: 2026-07-15
Title: generic `impl` 생략형과 specialized conformance bundle

## Summary

- generic struct의 canonical conformance는 우선 `impl S : Trait` 생략형으로 구현한다.
- `impl S : Trait`는 generic `S`에 대해 전체 generic arity를 fresh parameter로 채운 universal conformance schema로 정규화한다.
- `struct S<T> : Trait`는 모든 well-formed `S<T>`가 `Trait`를 구현한다는 약속이며, 일부 specialization impl만으로는 충족할 수 없다.
- `where` constraint를 포함한 full generic impl은 후속 단계에서 명시적으로 도입한다.
- specialization과 conditional conformance는 direct canonical impl이 아니라 named `extension` bundle로 선언하고, 소비자는 `extend`로 활성화한다.
- generic conformance는 `trait`와 trait type argument만으로 표현할 수 없다. generic signature와 owner self type-argument pattern이 필요하다.

## 1. Canonical Generic `impl`의 첫 surface

초기에는 generic impl의 full syntax를 먼저 도입하지 않는다.

```citron
struct S<T> : Trait
{
}

impl S : Trait { }
```

source의 `impl S : Trait`는 생략형이며 semantic header는 다음과 같은 universal pattern이다.

```text
generic signature: <τ0, ...>
self target:       S<τ0, ...>
trait target:      Trait
constraints:       S<τ0, ...>의 well-formedness 조건
```

type declaration은 same-name generic arity overloading을 허용하지 않는 방향이므로, `S`가 resolve되면 생략형이 채울 arity도 하나로 정해진다.

후속 full form은 source에서 generic parameter와 추가 조건을 드러낸다.

```citron
impl<T> S<T> : Trait
    where T : OtherTrait
{
}
```

`where`가 필요한 경우에는 생략형을 허용하지 않는다. 초기 구현 범위는 full form, conditional conformance, direct specialization을 제외한다.

## 2. Struct Header와 Witness Coverage

```citron
struct S<T> : Trait { }
```

은 모든 valid `S<T>`가 `Trait`라는 canonical conformance declaration이다. 그러므로 다음은 이를 충족하지 못한다.

```citron
impl S<int> : Trait { }
```

후속 checker는 struct header가 선언한 universal conformance schema와 canonical impl header가 같은 target/trait 및 적용 범위를 갖는지 확인해야 한다. trait requirement member 충족 검사는 이 coverage 검사와 별도다.

## 3. Specialized Conformance는 Bundle로 표현

`S<int>`처럼 범위를 좁히는 conformance는 direct `impl`으로 두지 않는다. named extension bundle이 public header와 witness identity를 제공한다.

```citron
extension IntTrait for S<int> : Trait;

impl IntTrait for S<int> : Trait
{
}
```

소비 file은 bundle을 import한 뒤 명시적으로 활성화한다.

```citron
import Provider;

extend IntTrait for S<int> : Trait;
```

`extend`는 file-local이며, import만으로 bundle conformance가 활성화되지는 않는다. 따라서 canonical conformance와 specialized/conditional conformance의 역할은 다음처럼 나뉜다.

| 종류 | declaration header | witness payload | 소비자 활성화 |
|---|---|---|---|
| canonical universal | `struct S<T> : Trait` | `NStructInfo` | 항상 적용 |
| specialized / conditional | `extension Bundle for S<...> : Trait` | `NExtensionInfo` | `extend` 필요 |

## 4. Symbol / Payload Model

`impl`은 이름으로 resolve되는 declaration이 아니므로 `RDecl` tree child가 아니다. 그러나 generic conformance header는 RSymbol surface에서 조회 가능해야 한다.

`NImplTrait` 계열은 단순 trait pointer와 trait argument만 보관해서는 충분하지 않다. 최소한 다음 의미를 나타낼 수 있어야 한다.

```text
generic signature
self type-argument pattern
implemented trait and its type arguments
where constraints
witness members
```

예:

| header | generic signature | self type-argument pattern |
|---|---|---|
| `impl S : Trait` | implicit `<τ0, ...>` | `[τ0, ...]` |
| `impl<T> S<T> : Trait` | `<T>` | `[T]` |
| `impl Bundle for S<int> : Trait` | empty | `[int]` |

canonical witness implementation은 `NStructInfo`에 둔다. Bundle conformance의 witness implementation은 `NExtensionInfo`에 둔다. `RExtensionDecl`의 public header와 canonical struct header는 conformance resolver가 함께 조회하되, bundle은 `extend`로 활성화된 것만 후보가 된다.

## Open Points

- full generic impl의 syntax/AST에서 generic parameter clause와 `where` clause를 정확히 어떻게 표현할지
- `impl S` 생략형의 implicit generic parameter를 impl body가 어떤 source name으로 참조할 수 있는지
- generic conformance header object의 정확한 RSymbol API와 CTI serialization shape
- conditional bundle들의 overlap/ambiguity를 activation 시점에 검사할지, conformance 사용 시점에 검사할지

## 5. Declaration Header의 Type Lookup

`struct`/`class`의 base 및 trait header는 body member lookup과 분리된 전용 type lookup scope에서 해석한다.

```citron
struct S<T> : Trait<T>
{
}

class C<T> : Base, Trait<T>
{
}
```

위 예에서 `T`는 현재 declaration의 generic parameter이므로 header에서 보인다. 반면 현재 선언 중인 type의 child type이나, 먼저 resolve한 `Base`의 child type은 unqualified lookup 후보에 들어가지 않는다.

```citron
struct S<T> : Trait<Item> // 오류: body child type을 header에서 찾지 않음
{
    type Item = T;
}

class C<T> : Base, Trait<Item> // 오류: Base.Item을 자동으로 찾지 않음
{
}
```

base type의 member type을 의도적으로 사용할 때는 일반 qualified type path를 써야 한다.

```citron
class C<T> : Base, Trait<Base.Item>
{
}
```

따라서 이름 해석의 의미상 구분은 세 가지다.

| resolver | 책임 | 후보 범위 |
|---|---|---|
| 일반 type lookup | type expression 해석 | type, type alias, trait, generic parameter, qualified type member |
| 일반 전체 lookup | value/type/function/member 문맥의 identifier 해석 | lexical scope에서 이름으로 접근 가능한 모든 member |
| declaration header type lookup | base 및 trait header type expression 해석 | 현재/outer generic parameter와 lexical type scope |

구현에서는 세 번째를 완전히 별도 탐색 알고리즘으로 만들기보다, 일반 type lookup의 제한 모드 `ResolveHeaderType`으로 두는 방향을 선호한다. 이 모드에서는 self child, base/inherited member, trait requirement, `this`/`Self`를 unqualified 후보에 주입하지 않는다. `Base.Item` 같은 명시 qualification은 일반 qualified type lookup으로 처리한다.
