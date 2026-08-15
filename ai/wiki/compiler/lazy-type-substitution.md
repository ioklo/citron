# Lazy Type Substitution

Status: current design; implementation pending
Area: compiler, RSymbol, SmTranslator, generics
Keywords: RType, SmTypeView, substitution, type environment, binder, trait matching

## Roles

- `RType` is the canonical semantic type expression.
- `RType_TypeVar` identifies a bound type variable with `RTypeParam*` identity.
- `SmTypeView` is a transient SmTranslator view of an `RType` under a lazy
  substitution. It is not another complete type hierarchy.
- A lexical type context answers which binders are visible and which constraints
  apply. It is distinct from a substitution that maps callee formals to actual types.

## Working Copy Transition

The working copy currently contains the WIP prototype merged from `e84dc27c`:

- a mirrored `SmType` variant hierarchy
- `SmType_TypeVar { size_t index }`
- `SmTypeEnv { typeVarCount }`
- eager `RTypeToSmType` and `RAppliedDeclToSmAppliedDecl` conversion
- `SmAppliedDecl<std::vector<SmType*>>`

These files are retained as migration scaffolding, not as the authoritative design.
They do not yet preserve the substitution context required by chained applications,
and comparing only type-variable indices cannot establish binder correspondence across
different declaration chains. The implementation is to be replaced by the view and
substitution model below. Until that migration is complete, source implementation and
this current design intentionally differ.

## Representation Direction

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

The `actual` side is itself a `SmTypeView`, not a bare `RType*`, because an actual
type expression may still depend on the caller's outer substitution. Substitution
objects must outlive every view that refers to them; the exact arena/ownership design
is still open.

## Evaluation Rules

- Keep the original canonical `RType` unchanged.
- Extend the substitution chain when a generic declaration is applied or member
  lookup crosses a generic owner.
- Force substitution only when an observer needs a result, such as parameter
  projection, type-kind inspection, member lookup, or equality.
- Parameter projection follows `param_i(D[sigma]) = param_i(D)[sigma]`.
- If a type variable has no entry in the active chain, it remains the same
  `RType_TypeVar` with the same `RTypeParam*` binder identity.
- Sequential composition follows
  `T[sigma1][sigma2] -> R[sigma2]` when `sigma1(T) = R`. Do not reapply the
  already-consumed `sigma1` to its replacement.
- Substitution order is significant in general.

Example:

```citron
class C1<T1> { void F<T2>(T1, T2); }
class C2<T3> { C1<list<T3>> x; }
class C3<T4> { C2<T4> y; }
```

For `C3<int>.y.x.F`, the first parameter is observed through this chain:

```text
C1.T1 -> list<C2.T3> -> list<C3.T4> -> list<int>
```

The second parameter remains `F.T2` until the function's own type argument is
applied. A parameter type is not wrapped in a separate `^T2` type function; the
function declaration owns that binder.

## Declaration Application

- `ROuterAppliedDecl<T>` represents a declaration whose lexical outer arguments are
  known while the declaration's own type arguments are not all applied.
- `RAppliedDecl<T>` remains the canonical RSymbol relation for a declaration with all
  required argument slots supplied. Supplied arguments may still contain open type
  variables, so applied does not mean closed.
- SmTranslator may use `SmAppliedDecl<T>` or an equivalent contextual relation while
  actual arguments are lazy views. If present, its arguments are `SmTypeView`, not a
  mirrored `SmType` hierarchy.
- A persistent application-site `RTypeEnv` is not part of `RType` or
  `RAppliedDecl` identity. Lexical lookup context and lazy substitution are separate
  mechanisms.

## Equality And Trait Signature Matching

Exact type equality uses declaration identity and `RTypeParam*` binder identity after
resolving only the substitutions demanded by comparison.

For two corresponding generic callable signatures:

1. Compare generic arity and constraints.
2. Map each requirement-owned binder to the corresponding impl-owned binder by
   ordinal position.
3. Combine this binder mapping with the applied trait's outer substitutions.
4. Compare the requirement signature view with the impl signature view lazily.

For the representative requirement
`C1<int>.Tr<list<impl.T5>>.F<T3>` and implementation `F<T7>`, the comparison
substitution contains:

```text
C1.T1     -> int
Tr.T2     -> list<impl.T5>
trait.F.T3 -> impl.F.T7
```

This reduces alpha-equivalence to exact contextual type comparison without creating
a fresh binder or a fully instantiated signature tree.

## Materialization Boundary

`RType::Apply` or an equivalent rebuilding operation may remain at boundaries that
require a canonical materialized type, such as interning, serialization, or a later
lowering contract. It is not the preferred path for ordinary generic member
projection or signature comparison.

## Open Implementation Points

- substitution ownership, allocation, and lifetime
- the exact boundary between canonical `RAppliedDecl` and transient SmTranslator views
- lazy observation/equality APIs and caching policy
- representation of inference variables and skolem variables, which are not the same
  as declaration-owned `RTypeParam` binders

## History

- `../../notes/2026-08-15-lazy-type-substitution-and-smtypeview.md`
- `../../notes/2026-08-08-generic-application-without-persistent-type-env.md`
- `../../notes/2026-08-07-applied-decl-tenv-and-trait-matching.md`
