# Citron Language – Core Semantics Summary (for Codex)

> This document defines the **authoritative core semantics** of the Citron language.  
> Implementation must follow these rules strictly, even if they differ from C++/Rust defaults.

---

## 1. Lifetime Model

- Citron uses **deterministic lifetime management** (RAII-style).
- All objects with destructors **must be destroyed at scope exit**.
- Scope exit includes:
  - normal block end
  - early return
- No GC, no deferred destruction.

---

## 2. Structs and Trivial Constructors

### 2.1 Definition of Trivial Constructor

A struct `S` has a **trivial constructor** if and only if:

- All fields of `S` are **default-initializable**
- No user-defined constructor is required
- No implicit zero-initialization is performed

This is a **structural rule**, not user-declared.

### 2.2 Variable Declaration Rules

- `S s;` is allowed **only if `S` has a trivial constructor**
- If `S` is not trivially constructible:
  - `S s;` is **forbidden**
  - Explicit initialization via `init` is mandatory

```citron
S s;              // allowed only if S is trivial
S s = init(...);  // always allowed
```

---

## 3. Uninitialized Variables

- Variables may exist in an **uninitialized state**
- Reading an uninitialized variable is a **compile-time error**
- Assignment via `init` transitions:
  - `uninitialized → initialized`
- `init` is allowed **only once** per variable

### 3.1 `init` Semantics

```citron
S s;           // uninitialized
s = init(...); // now initialized
```

- After `init`, normal assignment rules apply
- `init` cannot be used on an already-initialized variable

---

## 4. Copy and Move Semantics

Each struct may support:

- copy constructor
- move constructor
- copy assignment
- move assignment

Rules:

- Operations are **structurally composed** from fields
- If any field forbids an operation, the struct forbids it
- Triviality is propagated:
  - a struct is trivially movable/copyable only if all fields are

No implicit fallback is allowed.

---

## 5. Pointers and References

- `&` : address-of
- `*` : dereference
- References are implemented internally as pointers
- Reference declarations may exist, but are **not required** for correctness

Pointer misuse (invalid deref, escaping temporary) is rejected at compile time where possible.

---

## 6. Nullable Types

### 6.1 Nullable Semantics

- Nullable types explicitly represent the presence or absence of a value
- No implicit null checks

### 6.2 Control Flow Narrowing

```citron
if x is null { ... }
if x is not_null { ... }
```

- `is not_null` narrows the type inside the branch

### 6.3 Pattern Matching

```citron
match x {
  case null:
    ...
  case not_null(v):
    ...
}
```

- `v` is a non-null binding
- `v` has a deterministic lifetime within the case scope

---

## 7. Enums

- Enums support:
  - `if case`
  - `match case`
- Pattern matching on enums follows the same narrowing rules as nullable
- Payload lifetimes are scoped to the matching branch

---

## 8. Function Parameters

Supported parameter modifiers:

- `[in]`      : read-only input
- `[out]`     : must be uninitialized at call site, initialized by callee
- `[move]`    : ownership transfer
- `[forward]` : move if temporary, copy if named variable

Rules:

- `[out]` parameters are guaranteed initialized after return
- `[forward]` selection is decided at call site
- All parameter passing respects copy/move/trivial rules

---

## 9. Shared Ownership

### 9.1 Shared Creation

- Shared ownership is **explicit**
- Created via `shared` keyword

```citron
shared S s = shared S(...);
```

### 9.2 Semantics

- Reference-counted ownership
- Destruction occurs when refcount reaches zero
- Copy/move of shared values follows explicit rules

### 9.3 Capturing References

- Creating shared values from references (`&`) is restricted
- Escaping local lifetimes via shared requires explicit rules
- Implicit lifetime extension is forbidden

---

## 10. Temporaries and `tal`

- Certain bindings (e.g. in `foreach`, pattern matching) are **temporary auto-lifetime (tal)**
- `tal` values:
  - cannot escape their scope
  - may be referenced only within the owning scope
- Dereferencing tal after scope end is forbidden

---

## 11. foreach

```citron
foreach (i in elements) {
  ...
}
```

- `i` is a `tal` binding
- Lifetime of `i` is exactly one loop iteration
- `break` / `continue` must respect destructor ordering

---

## 12. Design Principles (Non-Negotiable)

- No implicit initialization
- No hidden lifetime extension
- No GC
- No “best effort” fallback (errors are preferred)
- Rules must be explainable structurally, not heuristically

---

## Implementation Note (for Codex)

When in doubt:
- Prefer **compile-time errors** over runtime behavior
- Prefer **explicit rules** over implicit convenience
- Do not emulate C++ or Rust unless explicitly stated
