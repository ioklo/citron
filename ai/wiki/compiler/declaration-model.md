# Declaration Model

Status: implemented current
Area: compiler, symbol
Keywords: RDecl, RNode, NDecl, EDecl, REDecl, declaration, skeleton, fdecl, symbol tree

## Current Rules
- Runtime/intermediate declaration interface is unified as `RDecl`.
- Source declarations and externally exposed declarations remain distinct as `NDecl` and `EDecl`.
- Compilation phases use `RDecl` as the common interface.
- Common declaration-related sum types such as `RFuncDecl`, `NFuncDecl`, `RDeclRes`, and `BodyRes` use thin wrapper classes over `std::variant` rather than public type aliases.
- Prefer domain methods on those wrappers (`GetRDecl()`, `GetRFuncDecl()`, `GetFuncDeclWithOuterTypeArgs()`, etc.) over scattering free helper functions at call sites.
- The semantic declaration tree reconfiguration around `RNode` is complete; it is no longer an active migration task.
- `RTypeDecl`, `RFuncDecl`, `RTypeDeclOuter`, and `RFuncDeclOuter` are better understood as category views over semantic declarations than as the tree node abstraction itself.
- Terminology: use `outer` for the containing element in the symbol tree, `base` for inheritance relationships, and avoid `parent` because it is ambiguous between those axes.
- Current leaning: `RNode` should be a lightweight tree/name carrier first. Richer declaration identity such as generic arity and callable signature belongs in declaration/category metadata rather than the node name key itself.
- Type declarations follow a C++-like rule: do not allow same-name generic type families distinguished only by generic arity in the same scope.

## Declaration Kinds
- `NDecl` is created directly from source.
- `EDecl` is an externally exposed declaration.
- `REDecl` wraps `EDecl` and implements the `RDecl` interface.
- `RDecl` is the common declaration interface used by translation and intermediate representation phases.
- Current leaning is that `NDecl` is weaker as an independent tree abstraction and stronger as a source-origin contract/payload surface.

## Pipeline Implications
- Later phases, including `SyntaxIR0Translator`, create/register required declarations through `RDecl`.
- External declarations and source declarations can be handled through one runtime-facing interface.
- Skeleton/fdecl collection can establish stable declaration identity before complete surface/body is known.
- Wrapper-based sum types keep declaration-specific operations close to the type while still allowing internal `Visit(...)` dispatch where a real sum-type branch is needed.
- Member lookup (`GetMember`, `GetTypeMember`) is a candidate to migrate toward semantic tree-node storage and shared child-based lookup logic.
- Identifier resolution is increasingly viewed as a resolver/scope responsibility rather than an intrinsic declaration-node method.
- `GetMember` is leaning toward a broad "everything name-visible in this scope" surface, while type-only resolution is a separate resolver concern rather than a narrower declaration storage model.
- Accessibility policy is likely to split between module/namespace member rules and type-member/inheritance rules, so `RNode` should not assume a single tree-only access algorithm.

## Impl Payload Direction

- `impl`은 이름을 바인딩하지 않으므로 `RDecl` tree child로 만들지 않는다.
- `RStructDecl`과 `RExtensionDecl`은 external declaration surface를 나타내는 named symbol로 유지한다.
- canonical impl의 실제 witness implementation은 `NStructInfo`, extension impl의 실제 witness implementation은 `NExtensionInfo` 같은 typed internal payload에 둔다.
- public conformance header와 witness identity는 external module consumer가 알아야 하므로 declaration surface에 남긴다.
- category-specific `N*Info`는 `RDecl` base의 universal tag보다 concrete `R*Decl`에 typed하게 붙이는 쪽을 선호한다.
- generic conformance header는 단순 `(trait, traitTypeArgs)`가 아니라 generic signature, owner struct의 formal parameter에 적용하는 self type-argument pattern, trait type arguments, constraint를 함께 나타내야 한다. 예를 들어 `impl<T> S<T> : Trait`의 self pattern은 `[T]`이고, `impl Bundle for S<int> : Trait`의 self pattern은 `[int]`다.
- canonical conformance의 witness는 `NStructInfo`에, specialized/conditional bundle conformance의 witness는 `NExtensionInfo`에 둔다. specialized bundle의 public header는 `RExtensionDecl` surface가 소유하며 conformance resolver가 활성화된 bundle과 canonical header를 함께 조회한다.

## Related Open Points
- Exact fields filled at fdecl / decl / impl states for each declaration kind.
- How `cti` generated declaration surface maps into `EDecl` / `REDecl`.
- How opaque result identity for `some` return attaches to declaration identity.

## History
- `git history: ai/implementations/decl-model.md`
- `ai/notes/2026-06-27-rdecl-rnode-and-lookup-direction.md`
- `ai/notes/2026-05-12-module-visibility-and-internal-fdecl-direction.md`
- `ai/notes/2026-07-15-generic-impl-and-specialized-conformance.md`
