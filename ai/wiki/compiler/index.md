# Compiler Wiki

Status: current index
Area: compiler
Keywords: compiler, RNode, declaration model, resolver, MIR, QIR, lowering, witness, translator

Citron compiler 내부 모델과 lowering 지식을 agent가 빠르게 찾기 위한 index다.

## Routing Hints
- `RNode`, declaration tree, resolver 책임, `RDecl` 이행은 `declaration-model.md`
- 현재 `NSymbol`에서 `RSymbol`로의 declaration 구현 이행 상태와 잔재는 `nsymbol-rsymbol-migration.md`
- declaration / symbol / resolver 구현 네이밍은 `implementation-naming.md`
- phase 구분, skeleton/cti/body 흐름은 `compile-pipeline.md`
- unit-local task dependency와 phase 경계는 `translation-task-scheduling.md`
- MIR value semantics와 create/read/init 축은 `mir-value-model.md`, `mcreate-and-translation-axes.md`
- witness, ABI, call lowering은 `value-and-trait-witness.md`, `qir-call-abi.md`
- syntax to MIR(IR0) translator는 `syntax-ir0-translator.md`
- member lookup helper와 conditional binding lowering은 `member-translation.md`, `if-is-lowering.md`

## Core Topics
- `compile-pipeline.md` : skeleton/cti/body/lowering phase 개요
- `value-and-trait-witness.md` : value witness와 trait witness
- `declaration-model.md` : `RDecl`에서 `RNode`로 가는 declaration / symbol model
- `nsymbol-rsymbol-migration.md` : `NSymbol` declaration 구현을 `RSymbol`로 옮긴 범위와 후속 정리
- `implementation-naming.md` : declaration / symbol / resolver implementation naming
- `syntax-ir0-translator.md` : Syntax to MIR(IR0) translator overview
- `translation-task-scheduling.md` : unit-local task dependency, readiness, global phase boundary
- `mir-value-model.md` : BC/NBC, MExp/MRead/MCreate/MInitExp
- `mir-observable-behavior.md` : observable event model
- `mcreate-and-translation-axes.md` : MCreate surface and lowering primitives
- `qir-call-abi.md` : QIR slots, return destination, parameter passing
- `qevaluator.md` : QIR semantic reference executor
- `member-translation.md` : member lookup/translation helper direction
- `if-is-lowering.md` : conditional binding lowering and lifetime

## Removed Source References
- `git history: ai/implementations/`
- `git history: ai/specs/mir/`

## Recent History Notes
- `../notes/2026-06-27-rdecl-rnode-and-lookup-direction.md` : `RDecl` / `NDecl` / `RNode` split, member lookup, resolver responsibility
