# Declaration Model

Status: draft current
Area: compiler, symbol
Keywords: RDecl, NDecl, EDecl, REDecl, declaration, skeleton, fdecl

## Current Rules
- Runtime/intermediate declaration interface is unified as `RDecl`.
- Source declarations and externally exposed declarations remain distinct as `NDecl` and `EDecl`.
- Compilation phases use `RDecl` as the common interface.
- Common declaration-related sum types such as `RFuncDecl`, `NFuncDecl`, `RDeclRes`, and `BodyRes` use thin wrapper classes over `std::variant` rather than public type aliases.
- Prefer domain methods on those wrappers (`GetRDecl()`, `GetRFuncDecl()`, `GetFuncDeclWithOuterTypeArgs()`, etc.) over scattering free helper functions at call sites.

## Declaration Kinds
- `NDecl` is created directly from source.
- `EDecl` is an externally exposed declaration.
- `REDecl` wraps `EDecl` and implements the `RDecl` interface.
- `RDecl` is the common declaration interface used by translation and intermediate representation phases.

## Pipeline Implications
- Later phases, including `SyntaxIR0Translator`, create/register required declarations through `RDecl`.
- External declarations and source declarations can be handled through one runtime-facing interface.
- Skeleton/fdecl collection can establish stable declaration identity before complete surface/body is known.
- Wrapper-based sum types keep declaration-specific operations close to the type while still allowing internal `Visit(...)` dispatch where a real sum-type branch is needed.

## Related Open Points
- Exact fields filled at fdecl / decl / impl states for each declaration kind.
- How `cti` generated declaration surface maps into `EDecl` / `REDecl`.
- How opaque result identity for `some` return attaches to declaration identity.

## History
- `git history: ai/implementations/decl-model.md`
- `ai/notes/2026-05-12-module-visibility-and-internal-fdecl-direction.md`
