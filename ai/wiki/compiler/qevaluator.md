# QEvaluator

Status: draft current
Area: compiler, QIR, evaluator
Keywords: QEvaluator, QEvaluation, observable event trace, reference executor

## Role
QEvaluator is a semantic reference executor for QIR.

It does not need to reproduce backend performance or exact register/stack behavior. Its job is to preserve QIR meaning and observable event trace.

## What It Must Preserve
- by-value parameter independence
- ref/alias parameter aliasing
- Direct/Indirect return destination meaning
- NBC construct/copy/move/assign/destroy order
- external side effect order
- mutable state mutation order
- trap/diagnostic/throw order

## What It Does Not Need To Match
- physical call frame layout
- physical register/stack choice
- backend BC copy count
- exact stack pointer/base pointer values
- whether a backend would inline or remove a call boundary

## Call Setup Interpretation
- Direct parameter may be copied into callee-owned storage.
- Indirect/Ref parameter stores pointer value in callee slot.
- Direct return writes to hidden `returnDest`.
- Indirect return writes/constructs through `HiddenReturnDestPtr` slot pointer.

Direct parameter copy in QEvaluation does not imply QIR exposes a physical copy requirement.

## Observable Boundary
QEvaluator should validate observable behavior as defined in `mir-observable-behavior.md`.

Observable events include:
- external side effects;
- mutable/aliasable state mutation;
- NBC lifetime operations;
- trap/panic/diagnostic/throw;
- final return value or final externally visible state.

## Testing Direction
QEvaluator tests should focus on:
- parameter independence vs aliasing;
- Direct/Indirect return destination correctness;
- NBC lifetime event ordering;
- side effect and trap ordering;
- managed pointer cleanup behavior.

## History
- `git history: ai/implementations/qir-call-abi-and-evaluator.md`
- `ai/notes/2026-04-26-qir-call-slots-and-observable-behavior.md`
