# Wiki Migration Plan

Status: active plan

이 문서는 기존 `ai/specs/`와 `ai/implementations/` 내용을 `ai/wiki/`로 옮기기 위한 작업 계획이다. `ai/notes/`는 history로 유지하며, 필요한 결정만 wiki topic에 반영한다.

## Goals
- Agent가 `ai/index.md` -> `ai/wiki/home.md` -> topic page 순서로 현재 지식을 찾을 수 있게 한다.
- 기준 내용은 wiki topic page에 한 번만 둔다.
- `notes`는 시간순 맥락과 과거 결정을 보존하는 history로 유지한다.
- `specs`와 `implementations`는 wiki가 충분히 채워진 뒤 `archive`로 옮긴다.

## Non-Goals
- `docs/` 아래 공식 문서를 수정하지 않는다.
- 모든 note를 wiki로 복사하지 않는다.
- 과거 논의의 모든 대안을 현재 결정처럼 옮기지 않는다.
- 파일 크기를 인위적으로 작게 맞추지 않는다. 주제에 필요한 만큼 쓰되, 찾기 쉬운 제목과 링크를 둔다.

## Migration Rules
- Wiki topic은 현재 유효한 결론을 먼저 쓴다.
- 오래된 결정과 현재 결정이 충돌하면 현재 결정을 명시하고, history 링크로 과거 note를 남긴다.
- `specs` / `implementations`의 본문을 그대로 복사하기보다 현재 언어로 정리한다.
- 한 topic이 language와 compiler를 모두 걸치면 한쪽에 primary page를 두고 다른 쪽 index에서 링크한다.
- Open point는 wiki에 남기되, 결정된 규칙과 섞지 않는다.
- Migration이 끝난 원본 파일은 바로 삭제하지 않고, archive 이동 전까지 deprecated reference로 둔다.

## Current Sources
Deprecated specs:
- `ai/specs/language/functions-and-control-flow.md`
- `ai/specs/language/if-and-is.md`
- `ai/specs/language/nullable-and-iteration.md`
- `ai/specs/language/types-and-ownership.md`
- `ai/specs/mir/value-model.md`
- `ai/specs/mir/observable-behavior.md`

Deprecated implementations:
- `ai/implementations/compiler-lowering-snapshot.md`
- `ai/implementations/decl-model.md`
- `ai/implementations/if-is-binding-snapshot.md`
- `ai/implementations/member-translation-snapshot.md`
- `ai/implementations/qir-call-abi-and-evaluator.md`
- `ai/implementations/syntaxir0translator-implementation.md`

Recent history notes to consult first:
- `ai/notes/2026-06-16-some-opaque-result-and-cti.md`
- `ai/notes/2026-05-22-nested-decl-visibility-and-resolved-cti.md`
- `ai/notes/2026-05-15-trait-concept-associated-type-design.md`
- `ai/notes/2026-05-14-trait-refenumerable-foreach-direction.md`
- `ai/notes/2026-05-14-reference-and-memory-safety-policy.md`
- `ai/notes/2026-05-12-module-visibility-and-internal-fdecl-direction.md`
- `ai/notes/2026-05-08-cti-import-and-static-interface-direction.md`
- `ai/notes/2026-05-05-mcreate-surface-and-translation-split.md`

## Target Wiki Shape
Existing seed pages:
- `ai/wiki/current-decisions.md`
- `ai/wiki/language/some-opaque-result.md`
- `ai/wiki/language/trait-and-interface.md`
- `ai/wiki/language/module-and-cti.md`
- `ai/wiki/language/types-and-references.md`
- `ai/wiki/compiler/compile-pipeline.md`
- `ai/wiki/compiler/value-and-trait-witness.md`

Planned language pages:
- `ai/wiki/language/functions.md`
- `ai/wiki/language/control-flow.md`
- `ai/wiki/language/if-is-binding.md`
- `ai/wiki/language/nullable-type.md`
- `ai/wiki/language/enumerable-and-foreach.md`
- `ai/wiki/language/visibility-and-reachability.md`
- `ai/wiki/language/nested-declarations.md`
- `ai/wiki/language/concepts-and-constraints.md`
- `ai/wiki/language/lambda-and-closures.md`
- `ai/wiki/language/seq-and-generators.md`

Planned compiler pages:
- `ai/wiki/compiler/declaration-model.md`
- `ai/wiki/compiler/syntax-ir0-translator.md`
- `ai/wiki/compiler/mir-value-model.md`
- `ai/wiki/compiler/mir-observable-behavior.md`
- `ai/wiki/compiler/mcreate-and-translation-axes.md`
- `ai/wiki/compiler/qir-call-abi.md`
- `ai/wiki/compiler/qevaluator.md`
- `ai/wiki/compiler/member-translation.md`
- `ai/wiki/compiler/if-is-lowering.md`
- `ai/wiki/compiler/incremental-build.md`

Planned process pages:
- `ai/wiki/process/windows-build-and-test.md`
- `ai/wiki/process/generated-files.md`

## Recommended Order
### Phase 1: Language Surface
- [x] Migrate nullable and iteration surface into `language/nullable-type.md` and `language/enumerable-and-foreach.md`.
- [x] Migrate type/reference/ownership rules into `language/types-and-references.md`.
- [x] Migrate functions/control flow into `language/functions.md` and `language/control-flow.md`.
- [x] Migrate `if` / `is` binding into `language/if-is-binding.md`.
- [x] Fold recent trait/interface/concept notes into `language/trait-and-interface.md` and `language/concepts-and-constraints.md`.
- [x] Add visibility/reachability and nested declaration pages from recent notes.

### Phase 2: Compiler Semantics
- [x] Migrate MIR value model into `compiler/mir-value-model.md`.
- [x] Migrate MIR observable behavior into `compiler/mir-observable-behavior.md`.
- [x] Migrate declaration model into `compiler/declaration-model.md`.
- [x] Migrate SyntaxIR0Translator snapshot into `compiler/syntax-ir0-translator.md`.
- [x] Migrate MCreate/read/init translation axes into `compiler/mcreate-and-translation-axes.md`.
- [x] Migrate member translation snapshot into `compiler/member-translation.md`.
- [x] Connect compiler lowering snapshot to compiler topic pages.
- [x] Migrate if/is lowering snapshot into `compiler/if-is-lowering.md`.

### Phase 3: QIR / ABI / Evaluator
- [x] Migrate QIR call ABI into `compiler/qir-call-abi.md`.
- [x] Migrate QEvaluator behavior into `compiler/qevaluator.md`.
- [x] Connect value witness / opaque sret decisions to QIR ABI page.

### Phase 4: Process
- [x] Split Windows build/test and generated file rules into wiki process pages.
- [x] Keep `ai/process/` as source reference until process wiki pages are checked.

### Phase 5: Cleanup / Archive
- [x] Check that `ai/wiki/current-decisions.md` points to all important topic pages.
- [x] Check that `ai/wiki/language/index.md` and `ai/wiki/compiler/index.md` have no stale "Topics To Add" entries for migrated pages.
- [x] Add archive directory plan.
- [ ] Move `ai/specs/` and `ai/implementations/` to archive only after wiki pages cover their current content.

## Per-Topic Checklist
For each migrated topic:
- [ ] Read the deprecated source file.
- [ ] Read related recent notes.
- [ ] Write/update the wiki page with current rules first.
- [ ] Add examples where they reduce ambiguity.
- [ ] Add compiler notes only if they affect implementation.
- [ ] Link history notes at the bottom.
- [ ] Update relevant index page.
- [ ] Update `current-decisions.md` if the topic has top-level decisions.
- [ ] Leave the original source file in place until archive phase.

## Archive Criteria
`ai/specs/` and `ai/implementations/` can move to archive when:
- every file listed in Current Sources has a corresponding wiki topic;
- index pages point to the wiki topic instead of deprecated files;
- `current-decisions.md` summarizes the major active decisions;
- no active workflow tells agents to add new content to `specs` or `implementations`.
