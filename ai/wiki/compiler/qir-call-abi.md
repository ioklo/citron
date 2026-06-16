# QIR Call ABI

Status: draft current
Area: compiler, QIR, ABI
Keywords: QIR, call ABI, slot, Direct, Indirect, Ref, HiddenReturnDestPtr, managed_ptr

## Layering
- MIR expresses ABI-neutral language semantics.
- QIR expresses logical slots and call/return/passing mode.
- QEvaluation executes QIR semantics as a reference executor.
- LLVM/backend materializes registers, stack, hidden sret parameter, stack arguments, and target calling convention.

## Slot Model
- QIR slot is a logical storage/value carrier referenced by function body.
- Slot index does not implicitly encode meaning.
- Slot should carry role metadata.

Recommended roles:
- `HiddenReturnDestPtr`
- `This`
- `Parameter`
- `Local`
- `Temp`

## Direct Return
- Direct return result is not stored in a callee slot.
- Callee slot storage disappears when callee frame is destroyed.
- Direct return writes to hidden frame metadata `returnDest`.
- QEvaluation may make `returnDest` point directly to caller destination slot storage.
- LLVM lowering may materialize this as register return depending on target ABI.

## Indirect Return
- Indirect return uses caller-provided return storage.
- Current preferred shape:
  - no hidden direct `returnDest`;
  - slot 0 role = `HiddenReturnDestPtr`;
  - slot 0 stores pointer value to return object storage, not the return object itself.
- Direct/void return functions do not have `HiddenReturnDestPtr` slot.
- Caller owns indirect return object destruction and storage reclaim.

## This And Parameters
- `this` remains a normal slot.
- Parameters remain normal slots.
- In indirect return functions, `HiddenReturnDestPtr` comes before `this`, explicit arguments, locals, temps.
- In direct/void return functions, `this` or first parameter/local may be the first slot.
- Meaning is determined by slot role metadata, not slot index.

## Parameter Passing Modes
`Direct`:
- Callee sees independent by-value parameter slot.
- QEvaluation may create callee-owned storage and copy the value.
- LLVM lowering may pass register/SSA and materialize only when address is needed.

`Indirect`:
- Callee slot stores pointer value to caller-provided storage/object.
- NBC by-value parameter follows construct/destroy owner convention.

`Ref`:
- Callee slot stores pointer/reference value aliasing caller object.
- Callee mutation is observed by caller.

## Call Arguments
`QArg_CallArg_AddrOfSlot(s)` is an address value for call setup.

It does not mean callee slot storage aliases caller slot storage. Formal parameter passing mode and callee slot role decide interpretation.

Examples:
- Direct formal + slot argument
  - copy value into callee-owned storage
- Indirect/Ref formal + addr-of-slot argument
  - store pointer value in callee slot
- `HiddenReturnDestPtr` slot + addr-of-slot argument
  - store return destination pointer
- Nested indirect return
  - pass pointer value from current function's `HiddenReturnDestPtr` slot to nested call return destination

## Managed Pointer Slot Annotation
Some slots semantically represent an object but physically store pointer value to that object.

Current direction:
- keep the existing slot model;
- show representation type in QIR/debug output;
- add `managed_ptr` annotation where current function owns cleanup of pointed object.

Examples:
```text
slot s1: string
slot s2: string*, managed_ptr
```

Cleanup:
- direct object slot
  - destructor gets object address, e.g. `AddrOfSlot`
- `managed_ptr` slot
  - slot value itself is object address, so destructor uses slot value

BC/NBC and direct/indirect representation remain separate axes:
- BC/NBC is value semantics axis.
- direct/indirect-like slot representation is ABI/representation axis.

## Open Points
- Whether indirect return should use hidden first slot or unify with hidden `returnDest`.
- Whether `QSlotRole` is public QIR model or translator-private metadata.
- Whether `QReturnPassingMode_Indirect { size_t index }` should become role-based.
- NBC indirect parameter destructor owner: callee-destroy vs caller-destroy.
- LLVM materialization rule when Direct parameter address is needed.
- Whether `managed_ptr` means cleanup ownership only or always also indirect representation.
- How to display unmanaged pointer slots such as ref parameters.

## History
- `ai/implementations/qir-call-abi-and-evaluator.md`
- `ai/notes/2026-04-26-qir-call-slots-and-observable-behavior.md`
- `ai/notes/2026-04-30-qir-slot-managed-ptr-direction.md`
