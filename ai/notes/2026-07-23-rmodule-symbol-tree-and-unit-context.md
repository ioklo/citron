# 2026-07-23 RModule Symbol Tree and Unit Context

## Decision

Symbol tree ownership is module-scoped, not translation-unit-scoped and not
compiler-global.

```text
Compiler / module registry
├─ RModule A -> one canonical symbol tree
├─ RModule B -> one canonical symbol tree
└─ ...
```

Each `RModule` owns one declaration/symbol tree containing its declarations and
canonical namespace nodes.
All units of that module contribute to this one tree.  A compiler-global
registry locates modules, but it is not itself a single tree that merges their
declarations.

There is no unit-local symbol tree.  Instead each translation unit retains a
separate unit context that points to its syntax declarations and records
unit-local state such as imports, import aliases, `using` aliases, task state,
and incremental/CTI contribution information.

```text
RModule
├─ canonical declaration/symbol tree
└─ RTranslationUnitContext[]
   ├─ syntax declaration pointers
   ├─ import / alias lookup overlay
   ├─ task state
   └─ cache / CTI contribution metadata
```

This keeps declaration identity, duplicate detection, overload grouping and
same-module cross-unit lookup module-wide.  It also keeps an import directive
unit-local without having to merge incompatible import environments into a
module-wide tree.

## Lookup consequence

Resolution begins in the current unit context for unit-local aliases/import
policy, then queries the current module's canonical tree.  A qualified or
imported lookup switches to the selected module's tree; declarations from
different modules are never made siblings in one global tree.

Namespace is a first-class canonical `RNamespace : RNode` scope for now.  The
module owns its root namespace, and namespace syntax gets or creates the node
for its module/path.  There are no namespace declaration groups or unit-local
namespace trees.
