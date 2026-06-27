# Compiler Wiki

Status: current index
Area: compiler

Citron compiler 내부 모델과 lowering 지식의 wiki 입구다.

## Core Topics
- `compile-pipeline.md` : skeleton/cti/body/lowering phase 개요
- `value-and-trait-witness.md` : value witness와 trait witness
- `declaration-model.md` : RDecl / NDecl / EDecl model
- `syntax-ir0-translator.md` : Syntax to MIR(IR0) translator overview
- `mir-value-model.md` : BC/NBC, MExp/MRead/MCreate/MInitExp
- `mir-observable-behavior.md` : observable event model
- `mcreate-and-translation-axes.md` : MCreate surface and lowering primitives
- `qir-call-abi.md` : QIR slots, return destination, parameter passing
- `qevaluator.md` : QIR semantic reference executor
- `member-translation.md` : member lookup/translation helper direction
- `if-is-lowering.md` : conditional binding lowering and lifetime

## Topics To Add
- unknown-size local storage
- opaque sret call lowering
- incremental build dependency

## Removed Source References
- `git history: ai/implementations/`
- `git history: ai/specs/mir/`

## Recent History Notes
- `../notes/2026-06-27-rdecl-rnode-and-lookup-direction.md` : `RDecl` / `NDecl` / `RNode` split, member lookup, resolver responsibility
