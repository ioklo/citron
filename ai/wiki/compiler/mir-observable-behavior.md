# MIR Observable Behavior

Status: draft current
Area: compiler, MIR, semantics
Keywords: observable behavior, evaluation order, BC, NBC, lifetime, side effect

## Current Rules
- Observable behavior is defined by the content and order of observable events produced by language execution.
- It is not limited to system calls.
- Evaluation order is a language rule that determines observable event order; it is not an independent observable object.

## Observable Events
Observable events include:
- external side effects
  - command/directive output
  - IO or system call
  - process exit value
- mutable or aliasable program state mutation
  - caller object mutation through ref parameter
  - shared mutable state mutation
  - pointer/reference reachable object mutation
- NBC object lifetime operations
  - construct
  - copy-construct
  - move-construct
  - assign
  - destroy
- abnormal control results and their preceding event order
  - trap
  - panic
  - diagnostic
  - throw
- final return value or final externally visible state

## Non-Observable Implementation Details
These are not observable by themselves:
- whether a physical function call instruction exists
- whether a call frame is created
- register vs stack use
- physical stack pointer/base pointer value
- unnecessary BC copy count
- padding byte values
- optimizer removing pure BC computation or inlineable call boundaries

## Evaluation Order
- If evaluation creates external side effects, mutable state mutation, NBC lifetime operation, trap/throw/diagnostic, its order is observable.
- If evaluation creates no observable event, such as pure BC calculation, backend optimization may reorder it as long as event trace is preserved.

## BC / NBC Boundary
- BC values preserve meaning under bitwise copy, so copy count itself is not observable.
- NBC values have lifetime operations as semantic events.
- Optimizations may remove call boundaries or temporary storage, but must not change observable event trace.

## History
- `ai/specs/mir/observable-behavior.md`
