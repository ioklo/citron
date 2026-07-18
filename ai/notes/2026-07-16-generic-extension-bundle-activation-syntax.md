# 회의 / 설계 노트

Date: 2026-07-16
Title: generic extension bundle과 `extend` activation 문법

## Summary

- generic parameter는 기존 선언 문법처럼 이름 뒤 `<...>`에 둔다. 따라서 generic bundle은 `extension Bundle<T>`로 선언한다.
- canonical `impl S<U> : Trait<U>`의 `U`는 generic conformance target pattern이 도입하는 type parameter다. `struct S<T> : Trait<T>`의 `T`와는 이름만 다르며 alpha-equivalent다.
- generic bundle implementation도 `impl Bundle<U> for S<U> : Trait<U>`처럼 header pattern이 type parameter를 도입한다.
- `extend`는 concrete bundle instantiation과 generic activation pattern을 구별해야 하므로, generic activation에서만 명시적 parameter clause를 사용한다.
- nested declaration은 자기 lexical generic context의 parameter를 capture할 수 있다. 따라서 `extend Bundle<U>;`의 `U`가 바깥 context에서 이미 선언되어 있으면 그 `U`를 type argument로 적용한다.
- bundle header가 target mapping을 이미 갖고 있으므로, 소비자 `extend`에서는 `for` target을 생략한다.
- `extend Bundle;`는 bundle family의 모든 instantiation과 모든 trait entry를 file-local로 활성화하는 shorthand다.
- bundle의 trait entry별 activation selection은 유지한다.
- 활성화된 bundle entry들의 conformance pattern이 겹치면 동시 활성화를 금지한다. conformance 사용 지점까지 미루어 ambiguity로 처리하지 않는다. 사용 편의보다 explicit activation을 우선한다.

## Declaration And Implementation Headers

```citron
struct S<T> : Trait<T> { }
impl S<U> : Trait<U> { }

extension Bundle<T> for S<T> : Trait<T>, Trait2<T>;
impl Bundle<U> for S<U> : Trait<U> { }
impl Bundle<U> for S<U> : Trait2<U> { }
```

`struct S<T>`와 `impl S<U>`는 모두 다음 universal conformance schema를 나타낸다.

```text
for all alpha:
    S<alpha> : Trait<alpha>
```

따라서 coverage 비교는 parameter source name이 아니라 generic parameter position과 header type pattern의 alpha-equivalence로 한다.

## Extend Activation

```citron
// 모든 T, Bundle이 제공하는 모든 trait entry를 활성화한다.
extend Bundle;

// T = int 하나에 대해 Bundle의 모든 trait entry를 활성화한다.
extend Bundle<int>;

// 모든 U에 대해 Trait<U> entry만 활성화한다.
extend<U> Bundle<U> : Trait<U>;

// nested context의 U를 capture한 activation이다.
class C<U>
{
    extend Bundle<U>;
}

// 일부 argument만 generic인 activation pattern도 가능하다.
extend<U> PairBundle<int, U>;
```

규칙은 다음과 같다.

- `extend Bundle;`는 bundle family 전체 activation으로 정규화한다.
- `extend Bundle<int>;`의 `<int>`는 concrete type argument다.
- `extend<U>`가 새 type parameter를 도입한다. 뒤의 `Bundle<U>` 및 trait selector의 `<U>`는 선언된 `U`를 적용하는 type argument다.
- parameter clause가 없더라도 lexical generic context에서 이미 선언된 parameter는 type argument로 capture할 수 있다. 예를 들어 nested `C<U>` 안의 `extend Bundle<U>;`에서 `U`는 fresh binder가 아니다.
- bundle이 제공한 target pattern은 activation에서 재기술하지 않는다. 따라서 `extend ... for S<...>`는 생략한다.
- trait selector가 없으면 bundle의 모든 trait entry를 활성화한다. selector가 있으면 해당 entry들만 활성화한다.

### Overlap

둘 이상의 활성 entry가 같은 concrete `(self type, trait type)`을 제공할 수 있으면 해당 entry들을 같은 file에서 동시에 활성화할 수 없다.

```citron
extension General<T> for S<T> : Trait<T>;
extension IntOnly for S<int> : Trait<int>;

extend General;
extend IntOnly; // 오류: S<int> : Trait<int> entry가 General과 겹침
```

trait selector로 서로 다른 trait entry만 활성화하면 허용된다. generic pattern의 overlap은 activation 시점에 검사한다. 사용 지점에서 witness 선택 ambiguity를 내는 대신, 사용자가 `extend A : OtherTrait;`처럼 필요한 entry만 명시적으로 활성화해야 한다. `where` constraint가 도입된 뒤의 정확한 overlap 판정은 별도 설계 항목으로 남긴다.

## Rationale

bundle은 type은 아니지만 generic bundle identity를 가질 수 있다. `extension Bundle<T>`의 `<T>`는 declaration-name slot의 parameter declaration이며, `impl`/`extend`의 `Bundle<U>`는 이미 선언된 bundle family에 대한 application이다.

`extend`는 기존 bundle을 참조하는 activation directive이므로 `Bundle<U>`만으로는 `U`가 새 generic pattern parameter인지 기존 type argument인지 알 수 없다. `extend<U>`는 이 구별을 표면 문법에서 명확히 한다.

## Open Points

- 같은 이름의 outer parameter가 있을 때 `extend<U>`가 shadowing을 허용할지 금지할지
- trait entry는 독립 conformance/witness identity로 유지하면서, 여러 관련 trait의 구현 body와 helper member를 어떤 scope에서 공유할지
- trait requirement에 없는 member를 `impl` block 안에 허용할지. 현재 선호는 impl block을 witness member로 제한하고, 공용 helper는 module-private function 또는 향후 bundle-private helper scope에 두는 것이다.
