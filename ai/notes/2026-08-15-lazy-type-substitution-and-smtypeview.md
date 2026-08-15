# Lazy Type Substitution And SmTypeView

Date: 2026-08-15
Status: design discussion recorded; source migration pending

## Working Copy State

After this discussion, `e84dc27c` was merged into the working branch. It contains the
earlier WIP implementation based on a mirrored `SmType` hierarchy,
`SmType_TypeVar { index }`, a count-only `SmTypeEnv`, and eager `RTypeToSmType`
conversion. The files were intentionally retained as implementation scaffolding, but
they are not the current design authority. The implementation is expected to be
rewritten according to the `SmTypeView` and lazy substitution direction recorded in
this note and `ai/wiki/compiler/lazy-type-substitution.md`.

## Question

Generic declaration을 적용하고 trait requirement 함수와 impl 함수를 비교할 때,
compiler가 매번 치환된 `RType` tree를 새로 만들지 않으면서 type variable의 binder
identity를 어떻게 보존할 것인가를 논의했다.

예제의 오타를 바로잡은 원문은 다음과 같다.

```citron
class C1<T1> {
    trait Tr<T2> {
        void F<T3>(T1 t1, T2 t2, T3* t3);
    }
}

class C2<T4> {
    impl S<T5, T6> : C1<int>.Tr<list<T5>> {
        void F<T7>(int, list<T5>, T7*) { }
    }
}
```

형식적으로는 다음처럼 볼 수 있다.

```text
C1 = ^T1. {
    Tr = ^T2. {
        F = ^T3. (T1, T2, T3*) -> void
    }
}
```

`C1<int>.Tr<list<T5>>`에는 다음 outer substitution이 붙는다.

```text
C1.T1 -> int
Tr.T2 -> list<impl.T5>
```

여기서 `trait.F.T3`와 `impl.F.T7`은 서로 다른 declaration이 소유한 binder다.
`list<T5>`의 `T5`도 impl header binder이므로, 우연히 같은 global index를 가졌다는
이유로 같은 type variable로 취급하면 안 된다.

## Function Quantifier And Parameter Projection

```citron
class C<T1> {
    void F<T2>(T1, T2) { }
}
```

`C<int>.F`를 수학적으로 다음처럼 적을 수 있다.

```text
(^T2. (T1, T2) -> void) [C.T1 -> int]
```

하지만 `^T2`는 함수 signature 전체의 binder이지 parameter type 각각의 일부가
아니다. 따라서 compiler API에서 parameter를 관찰한 결과는 다음과 같다.

```text
param0(C<int>.F) = int
param1(C<int>.F) = RType_TypeVar(F.T2)
```

두 번째 결과는 `^T2.T2`라는 별도의 type function이 아니라, `F.T2` binder가
유효한 function declaration context에서 해석되는 type variable다. 함수 자신의
type arguments가 아직 적용되지 않았으므로 declaration lookup 상태는
`ROuterAppliedDecl<F>`가 자연스럽다.

## Environment And Substitution

일반 함수 계산에서 두 표현은 같은 의미를 다른 방식으로 구현한다.

```text
substitution semantics:
    (lambda x. e) v -> e[x := v]

environment semantics:
    Closure(lambda x. e, rho)를 만들고 rho[x -> v]에서 e를 평가
```

Citron 논의에서 `TypeEnv`라는 말에는 서로 다른 두 책임이 섞여 있었다.

- lexical/binder context `Gamma`: 현재 어떤 binder가 유효하며 어떤 constraint를
  갖는지 나타낸다.
- substitution `sigma`: callee formal `RTypeParam*`을 실제 type expression에
  대응시킨다.

현재 `RType_TypeVar`가 `RTypeParam*`을 직접 보관하므로 binder identity를 위해
별도의 index environment를 영구 저장할 필요는 없다. lexical lookup과 constraint
검사에는 context가 필요하지만, generic application의 의미 값과 동일한 것은 아니다.

반대로 `SmType_TypeVar { index }`처럼 index만 저장하는 별도 type hierarchy를 쓰면
그 index를 해석할 environment가 필수다. 이 방식은 `RType` hierarchy와 역할이
중복되며 현재 signature matching에는 필요하지 않다고 판단했다.

## Current Direction: Explicit Lazy Substitution

Citron은 generic 계산 중 치환된 중간 `RType` tree를 계속 materialize하지 않으려
한다. 따라서 분석의 주 경로는 원본 `RType*`와 아직 계산하지 않은 substitution을
함께 들고 다니는 contextual view로 둔다.

```cpp
class SmTypeSubstitution;

struct SmTypeView
{
    RType* rType;
    const SmTypeSubstitution* subst;
};

struct SmTypeSubstitutionEntry
{
    RTypeParam* formal;
    SmTypeView actual;
};

class SmTypeSubstitution
{
    const SmTypeSubstitution* outer;
    std::vector<SmTypeSubstitutionEntry> entries;
};
```

`SmTypeView`는 두 번째 type algebra가 아니다. canonical `RType`을 SmTranslator가
특정 substitution 아래에서 관찰하기 위한 작고 비소유적인 view다. substitution의
`actual`도 bare `RType*`가 아니라 `SmTypeView`여야 한다. 실제 인자로 들어온 type
expression 자체가 caller의 outer substitution 아래에 있을 수 있기 때문이다.

필요한 관찰 지점에서만 계산한다.

```text
param_i(D[sigma]) = param_i(D)[sigma]
```

예를 들어 `C<int>.F`의 첫 parameter를 요구하면 `C.T1[sigma]`를 `int`로
resolve한다. 두 번째 parameter의 `F.T2`는 substitution에 없으므로 그대로 남는다.

이 방식은 구현 관점에서는 environment/closure evaluator이고, 형식 표기에서는
explicit substitution이다. 동일한 사실을 full `TypeEnv`와 substitution 양쪽에
중복 저장하지 않는다.

## Chained Lazy Substitution

```citron
class C1<T1> { void F<T2>(T1 t1, T2 t2); }
class C2<T3> { C1<list<T3>> x; }
class C3<T4> { C2<T4> y; }

C3<int> c = new C3<int>();
```

`c.y.x.F`의 첫 번째 parameter type을 계산해 보자.

```text
raw type of y       = C2<C3.T4>
raw type of x       = C1<list<C2.T3>>
raw param0 of F     = C1.T1

sigma1 = { C1.T1 -> View(list<C2.T3>, sigma2) }
sigma2 = { C2.T3 -> View(C3.T4, sigma3) }
sigma3 = { C3.T4 -> View(int, identity) }
```

이를 평평하게 표기하면 다음과 같다.

```text
C1.T1
 [C1.T1 -> list<C2.T3>]
 [C2.T3 -> C3.T4]
 [C3.T4 -> int]
-> list<C2.T3>[sigma2][sigma3]
-> list<C3.T4>[sigma3]
-> list<int>
```

따라서 첫 번째 인자는 `list<int>`다. 두 번째 parameter는 아직 함수 type
argument가 없으므로 `F.T2`로 남는다. `F<string>`을 적용하면
`{ F.T2 -> string }`을 chain에 추가한다.

중요한 sequential rule은 다음과 같다.

```text
T[sigma1][sigma2], sigma1(T) = R  =>  R[sigma2]
```

치환 결과 `R`에는 이미 소비한 `sigma1`을 다시 적용하지 않고, 뒤에 남은
substitution만 적용한다. 일반적으로 substitution 순서는 의미가 있다.

## Trait Requirement Signature Matching

`C1<int>.Tr<list<T5>>`의 requirement `F`에는 outer substitution을 붙인다.

```text
C1.T1 -> int
Tr.T2 -> list<impl.T5>
```

그 다음 requirement와 impl 함수의 generic arity와 constraint를 확인하고, 함수가
소유한 binder를 ordinal로 대응시킨다.

```text
trait.F.T3 -> impl.F.T7
```

이 binder mapping을 outer substitution과 결합한 requirement signature view를
impl signature의 identity view와 비교한다. 그러면 양쪽은 다음처럼 관찰된다.

```text
(int, list<impl.T5>, impl.F.T7*) -> void
(int, list<impl.T5>, impl.F.T7*) -> void
```

compiler-generated fresh binder나 완전히 instantiate된 signature tree는 필요하지
않다. alpha-equivalence는 대응이 확인된 requirement binder를 impl binder로
mapping한 뒤 `RTypeParam*` identity를 사용하는 exact view comparison으로
환원한다.

## Relation To Existing Models

- 2026-08-07 논의는 applied declaration에 `tenv`를 붙이는 env-based normalization을
  후보로 검토했다.
- 2026-08-08 논의는 persistent `RTypeEnv`를 제거하고 complete
  `RTypeArguments`와 eager `RType::Apply`를 사용하는 방향으로 정리했다.
- 이번 결론은 persistent `RTypeEnv`를 semantic identity에 넣지 않는다는 점은
  유지하지만, generic 분석 때마다 새 `RType`을 만드는 eager materialization도
  피한다. canonical `RType` 위에 SmTranslator-local `SmTypeView`와 lazy
  substitution을 둔다.

`RType::Apply`는 serialization, interning, lowering input 확정처럼 실제
materialization이 필요한 경계의 helper로 남을 수 있다. 다만 member projection과
signature comparison의 기본 계산 모델은 아니다.

`SmAppliedDecl<T>`가 필요하다면 type argument는 `std::vector<SmType*>`가 아니라
`std::vector<SmTypeView>`여야 한다. `RAppliedDecl`은 canonical RSymbol relation으로
남을 수 있으나, RSymbol과 SmTranslator 사이의 정확한 적용 경계는 아직 구현 설계가
필요하다.

## Open Implementation Questions

- `SmTypeSubstitution` chain의 ownership과 lifetime을 `SmFactory`/arena로 둘지,
  persistent immutable object로 둘지
- canonical `RAppliedDecl`/`RTypeArguments`와 transient
  `SmAppliedDecl`/`SmTypeView` 사이의 책임 경계
- `ResolveTypeVar`, type kind 관찰, member lookup, lazy equality API의 구체적인 shape
- chain flattening, memoization, cycle detection이 언제 필요한지
- bound `RTypeParam`과 inference variable, skolem variable 같은 compiler-generated
  type variable을 같은 모델에 둘지 별도 모델로 둘지

## Related Records

- `ai/notes/2026-08-07-applied-decl-tenv-and-trait-matching.md`
- `ai/notes/2026-08-08-generic-application-without-persistent-type-env.md`
- `ai/wiki/compiler/lazy-type-substitution.md`
