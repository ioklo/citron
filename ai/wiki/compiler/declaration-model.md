# Declaration Model

Status: draft current
Area: compiler, symbol
Keywords: RDecl, RNode, NDecl, EDecl, REDecl, declaration, skeleton, fdecl, symbol tree

## Current Rules
- Runtime/intermediate declaration interface is unified as `RDecl`.
- Source declarations and externally exposed declarations remain distinct as `NDecl` and `EDecl`.
- Compilation phases use `RDecl` as the common interface.
- Common declaration-related sum types such as `RFuncDecl`, `NFuncDecl`, `RDeclRes`, and `BodyRes` use thin wrapper classes over `std::variant` rather than public type aliases.
- Prefer domain methods on those wrappers (`GetRDecl()`, `GetRFuncDecl()`, `GetFuncDeclWithOuterTypeArgs()`, etc.) over scattering free helper functions at call sites.
- Ongoing direction: treat the semantic declaration tree as a dedicated node model (`RNode`) rather than letting category views and source payload types also act as tree APIs.
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

## Related Open Points
- Exact `RNode` shape and migration path from current `RDecl`.
- Exact fields filled at fdecl / decl / impl states for each declaration kind.
- How `cti` generated declaration surface maps into `EDecl` / `REDecl`.
- How opaque result identity for `some` return attaches to declaration identity.

## History
- `git history: ai/implementations/decl-model.md`
- `ai/notes/2026-06-27-rdecl-rnode-and-lookup-direction.md`
- `ai/notes/2026-05-12-module-visibility-and-internal-fdecl-direction.md`
