# 회의 / 설계 노트

Date: 2026-08-20
Title: Generic application 합성과 SmType 제거 방향

## 결론

- generic application의 formal 의미는 type environment 사이의 substitution으로
  설명한다. 다만 구현에서는 formal parameter가 flattened type-argument index로
  정해지므로 substitution을 complete `RTypeArguments` 배열로 erase할 수 있다.
- `RAppliedDecl<D>`의 `typeArgs` 자체가 declaration의 formal slots에서 현재
  문맥으로 가는 substitution이다. declaration을 별도로 instantiate하지 않고
  `decl + complete typeArgs`로 보관하는 형태가 declaration application의 정규형이다.
- 뒤 application이 추가되면 기존 type arguments 각각에 새 substitution을 적용해
  하나의 complete arguments로 합성한다. 별도의 `LazyApply` semantic node는 두지
  않는다.
- declaration-backed가 아닌 pointer, nullable, tuple 등의 type constructor는
  내부 type에 substitution을 재귀적으로 eager apply한다. nominal type은 declaration
  body를 전개하지 않고 자신이 가진 applied declaration의 arguments에만 적용한다.
- `SmType`, `SmAppliedDecl`, application용 `SmTypeEnv`는 제거하는 방향으로 확정한다.
  generic application과 trait requirement matching은 `RType`, `RAppliedDecl`,
  `RTypeArguments`를 직접 사용한다.
- source type-name lookup에 필요한 translator 문맥은 generic application의
  persistent type environment와 별개다.

## Formal Model

type environment를 `Γ`, `Δ`, `Θ`라 하고 substitution을 다음처럼 쓴다.

```text
σ : Γ -> Δ
ρ : Δ -> Θ
```

중첩 application은 다음 합성 법칙을 만족한다.

```text
(x[σ])[ρ] = x[σ ; ρ]
(σ ; ρ)(T) = Apply(σ(T), ρ)
```

예를 들어:

```text
σ = [T1 => int, T2 => list<T5>]
ρ = [T4 => int, T5 => bool, T6 => string]

σ ; ρ = [T1 => int, T2 => list<bool>]
```

이는 두 map의 union이 아니다. 앞 substitution의 각 우변에 뒤 substitution을
적용하는 함수 합성이다.

## GetDeclaredTrait와 Application

`GetDeclaredTrait`이 impl declaration에 저장된 trait relation을 꺼내는 구조적
projection이라면 application과 교환한다.

```text
GetDeclaredTrait(impl[ρ]) = GetDeclaredTrait(impl)[ρ]
```

impl에 저장된 trait application이 `traitDecl[σ]`라면:

```text
GetDeclaredTrait(impl[ρ])
= (traitDecl[σ])[ρ]
= traitDecl[σ ; ρ]
```

이 법칙은 concrete arguments를 보고 specialization, conditional conformance,
activation 또는 ambiguity를 판정하는 `ResolveConformance`류 연산에는 자동으로
적용하지 않는다.

## Erased Implementation Representation

formal substitution은 다음 mapping이다.

```text
[T0 => S0, T1 => S1, ...]
```

각 `Ti`의 flattened index가 `0, 1, ...`로 정해져 있으므로 구현에서는 다음
배열만 보관할 수 있다.

```text
[S0, S1, ...]
```

type variable application과 substitution 합성은 다음과 같다.

```text
Apply(TypeVar(Ti), args) = args[Ti.GetGlobalIndex()]
Compose(args1, args2)[i] = Apply(args1[i], args2)
```

`RAppliedDecl<D> { decl, typeArgs }`에서 `decl`이 source formal slots를 정하고,
`typeArgs`가 그 slots의 complete substitution을 나타낸다. 뒤 substitution이 오면:

```text
Apply(RAppliedDecl(decl, args), next)
= RAppliedDecl(decl, Apply(args, next))
```

로 하나의 applied declaration 정규형을 유지한다.

## Required Invariants

- complete arguments의 개수는 적용 대상 declaration의 flattened formal slot 수와
  일치해야 한다.
- `RType_TypeVar`의 flattened index는 arguments 범위 안에 있어야 한다.
- arguments의 각 `RType*`는 결과 문맥에서 well-scoped해야 한다.
- 치환하지 않는 parameter도 identity type argument로 채워 complete arguments를
  유지한다.
- 서로 무관한 binder가 같은 flattened index를 가질 수 있으므로, index가 같다는
  사실만으로 binder identity나 arbitrary application의 유효성을 판단하지 않는다.
  올바른 source declaration/signature를 먼저 확정한 뒤 그 complete arguments만
  적용한다.

## SmType 제거

기존 검토안의 `SmType`/`SmAppliedDecl`은 `RType`/`RAppliedDecl`을 environment-relative
형태로 다시 표현하려 했다. 별도 Lazy node가 필요하지 않고 application용 type
environment도 complete arguments로 erase할 수 있으므로, 동일한 type algebra를
SmTranslator에 중복해서 유지할 이유가 사라졌다.

SmTranslator는 다음 semantic value를 직접 사용한다.

```text
RType*
RAppliedDecl<D>
RTypeArguments*
```

trait requirement와 impl signature의 alpha-equivalence는 별도 `SmType` 정규형을
만들지 않고, 대응 binder를 먼저 확정한 뒤 requirement를 impl binder 기준의
complete arguments로 치환하여 exact comparison하는 방향을 우선한다.

## 열린 구현 쟁점

- alpha-equivalence를 binder substitution 후 exact comparison으로 통일할지,
  대응 signature에 한해 flattened index canonical comparison도 제공할지
- release representation은 type argument 배열만 유지하되 debug build에서 source
  declaration/signature 또는 expected slot count를 추가 검증할지
- `GetGlobalIndex()`가 compiler-global identity로 오해되지 않도록
  `GetTypeArgumentIndex()` 등의 이름으로 바꿀지
- 향후 associated type projection이나 type-level function처럼 eager structural
  application이 불가능한 연산이 추가될 때 delayed application node를 도입할지

## 이전 논의

- `ai/notes/2026-08-07-applied-decl-tenv-and-trait-matching.md`
- `ai/notes/2026-08-08-generic-application-without-persistent-type-env.md`
