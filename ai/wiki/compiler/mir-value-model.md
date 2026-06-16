# MIR Value Model

Status: draft current
Area: compiler, MIR
Keywords: MIR, BC, NBC, MExp, MRead, MCreate, MInitExp, MLoc

## Current Rules
- MIR is Citron's canonical semantics after surface syntax ambiguity is removed.
- MIR separates bitwise-copyable (BC) values from non-bitwise-copyable (NBC) value events.
- BC copy count is not observable behavior.
- NBC lifetime operations are observable events and must be preserved.
- Read context and create/init context are represented separately.

## BC / NBC
BC values:
- primitive
- `[BitwiseCopy] struct`
- class/interface handle value
- values whose meaning is preserved by bitwise copy

NBC values:
- ordinary struct
- tuple containing NBC values
- nullable struct or other values requiring construct/destroy/restore semantics

## Core Shape
- `MExp` expresses BC results.
- `MInitExp` expresses NBC initialization computation.
- `MRead` expresses read-context input.
- `MCreate` expresses create-context plan for consumers such as return, expression statement, local init, materialize.
- `MLoc_Materialize` is used when an rvalue needs a place.

Current naming may differ across code and older specs. The important model is:

```text
BC:
  value/read driven
  MExp / MRead_Exp
  MCreate_BC as create-context adapter

NBC:
  destination/place driven
  MInitExp / MRead_Loc
  MCreate_NBC as init-context adapter
```

## Return
- NBC return call lowers through caller-provided storage / dest-passing / RVO.
- MIR does not decide whether QIR represents return destination as hidden frame field or hidden first slot.
- `return G();` can forward caller-provided return storage when possible.

## Bitwise Policy
- Only bitwise-copyable types use bitwise copy/assign paths.
- Ordinary struct does not get implicit bitwise load/copy.
- `[BitwiseCopy] struct` requires all members to be bitwise-copyable.
- Chain assignment is allowed only for bitwise-assignable types.

## Construction And Assignment
- Initialization and assignment are distinct in MIR.
- NBC initialization exposes ctor/copy/move/RVO semantic events.
- Assigning ctor/call RHS into an already initialized destination uses materialize then assign path.
- Construct/copy/move/assign/destroy order for NBC objects is part of observable event trace.

## Follow-Up
- Enforce BC/NBC invariants at verifier or construction time.
- Move remaining `MOperand`-based code toward `MRead`.

## History
- `git history: ai/specs/mir/value-model.md`
- `ai/notes/2026-05-05-mcreate-surface-and-translation-split.md`
