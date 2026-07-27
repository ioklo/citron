# 2026-07-23 RName Lookup Surface and Impl Member Lookup

## Identifier construction

`RIdentifier` is assembled by walking the declaration path, but intermediate
`RIdentifier`/`std::string` values must not be created for every outer.

- Public `RDecl::GetIdentifier()` creates the final string value.
- The internal append operation is virtual because the root
  `RNamespaceDecl` must cross the `RModule` boundary and append the
  `ModuleName::` prefix.
- The preferred internal spelling is `AppendIdentifierTo(std::string&)`.
  `FillIdentifier` is acceptable if the codebase prefers it, but it must mean
  append-to-an-existing-buffer rather than clear-and-fill.
- Ordinary declarations append their outer identifier, `.` and their own
  `RDeclKey`; root namespace appends the module identifier and does not add a
  root-namespace path segment.
- `std::ostringstream` is not preferred for this compiler path.  Canonical
  encoders append directly to `std::string`; `reserve` and `std::to_chars` may
  be used where useful.

The module edge is kept only at the root namespace (`RModule* |
RNamespaceDecl*` namespace outer relation).  A forwarding `RDecl::GetModule`
virtual or an `RModule*` stored on every declaration is unnecessary.

## RName and declaration names

The attempted `RDeclName` split exposed that a declaration-only name type is
not the common resolver input:

- `RTypeParam` is not an `RDecl`, but its binder name participates in type and
  identifier lookup.
- `RName_CtorParam` is compiler-generated and not source-spellable, yet a
  memberwise-constructor body must resolve it to a `BodyRes` local variable or
  local reference.

Therefore `RName` remains the common structured lookup key for normal source
names, type parameters, locals, parameters, reserved names, and compiler
generated names.  `ResolveIdentifier`, `ResolveTypeIdentifier`, and
`ResolveTypeIdentifierInHeader` take `RName`.

`RDeclKey` remains separate: it is the same-outer exact declaration/tree key
and is not an overload-family lookup name.  Since an `RDecl` retains a name
only to expose it to lookup, there is currently no separate `RDeclName`
abstraction.  A declaration with no ordinary lookup surface may simply have no
`RName`.

## Impl declarations and member lookup

`RImplTraitDecl` is an internal conformance/witness subtree.  It is found by
its target/trait relation or its `$I(TargetRDeclKey,TraitRTypeIdentifier)`
`RDeclKey`, not by ordinary `RName` lookup.  It therefore does not implement
ordinary `GetMember(InRef<RName>)`.

The implementation of a trait requirement is selected through its
`RTraitFuncDecl` relationship:

```text
RTraitFuncDecl -> RImplTraitFuncDecl
```

not through an impl-local function-name lookup.  Thus a pure
`RImplTraitFuncDecl` also does not need an ordinary `RName` lookup surface.

An unnamed parent can still own named, lookup-visible children in general;
that is not an invariant violation.  If a future extension/private-helper
scope intentionally supports unqualified calls to its own helper functions,
that scope may add a name index then.  This does not justify adding ordinary
member lookup to trait witness impls now.

