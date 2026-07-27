# 2026-07-23 RNode / RDecl / RNamespace Separation

## Decision

The semantic symbol tree is made explicit.

```text
RModule                         // tree owner; not an RNode
└─ RNamespace root : RNode      // canonical root namespace
   ├─ RNamespace : RNode
   └─ RDecl : RNode
      ├─ RTypeDecl / RFuncDecl / ...
      └─ RImplTraitDecl / ...
```

- `RNode` is the common base of declaration-space tree nodes.
- `RDecl` derives from `RNode` and represents actual declarations.
- `RNamespace` derives from `RNode`, but is not an `RDecl`.  It is a canonical
  named semantic scope, not a source declaration occurrence.
- `RModule` owns the root `RNamespace` but is not an `RNode`; it remains the
  module registry/build/cache/CTI ownership boundary.
- Body locals, parameters and syntax AST nodes are not `RNode`s.

## Canonical namespaces

There is one `RNamespace` for each `(RModule, namespace path)`.  Translating a
namespace syntax block obtains the existing node or creates it, then attaches
the block's child declarations to it.  It does not create a namespace
declaration per unit or per syntax occurrence.

`RNamespaceDeclGroup` is therefore unnecessary and will be removed.  Unit
state is held by the unit context rather than namespace fragments/groups.

## RNode API direction

`RNode` owns common tree identity/navigation:

```text
RNodeKey / GetNodeKey()
GetOuterNode()
TryGetName()
GetIdentifier() and internal Fill/AppendIdentifier
```

`RNodeKey` generalizes the former `RDeclKey`: it is the exact key of a direct
tree child and may represent a namespace name, declaration signature, special
ctor/dtor/impl key, or root-namespace key.  `RIdentifier` is consequently a
canonical global tree-node path; resolving it may yield an `RNode`, while a
declaration-only consumer checks for `RDecl`.

All `RNode`s are declaration-space lookup nodes.  Accordingly `Get*` and
`Resolve*` declaration-space APIs live directly on `RNode` rather than on an
additional `RDeclSpace` layer.  Default `GetTypeParam`/`GetTypeMember`/
`GetMember` behavior is empty.  `ResolveInheritedTypeMember` and
`ResolveInheritedMember` are protected virtual hooks on `RNode` with default
`nullopt`; `RClassDecl` is the relevant override.

`RDeclRes` continues to mean *declaration-space resolution result*, not
“result containing only an `RDecl*`”.  Its namespace alternative changes from
`RNamespaceDecl*` to `RNamespace*`; it must retain outer-applied declaration
and overload-group alternatives rather than reuse raw `RMember`.

