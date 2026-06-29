# Language Wiki

Status: current index
Area: language
Keywords: language, syntax, type system, trait, interface, module, import, visibility, alias, nested declaration

사용자가 보는 Citron 표면 언어 규칙과 타입 시스템을 agent가 빠르게 찾기 위한 index다.

## Routing Hints
- 언어 철학, explicit 지향, resolution 성향은 `design-principles.md`
- module, import, using, type alias, declaration/export boundary는 `module-and-cti.md`, `type-aliases.md`
- visibility, accessibility, private/public, not-visible type flow는 `visibility-and-reachability.md`
- trait, interface, extension, impl, associated type은 `trait-and-interface.md`
- nested type/trait identity와 conformance는 `nested-declarations.md`
- 함수, 제어 흐름, nullable, foreach는 각 surface topic page를 직접 본다.

## Core Topics
- `design-principles.md` : explicit 지향, 의미 중복 최소화, explicit resolution escape hatch
- `some-opaque-result.md` : `some Trait` opaque result
- `trait-and-interface.md` : static trait, dynamic interface, callable `func<>`
- `module-and-cti.md` : module/unit, `cti`, import surface, `rcti` 재검토
- `type-aliases.md` : module import, unit-local `using` alias와 declaration-level `type` alias
- `types-and-references.md` : value/pointer/reference surface, `T&` 제한
- `nullable-type.md` : nullable representation and nullable pattern
- `enumerable-and-foreach.md` : RefEnumerable / ValueEnumerable / foreach direction
- `functions.md` : function calls, RVO, error channel, lambda capture summary
- `control-flow.md` : try/catch, tail expression, return completeness, labels
- `if-is-binding.md` : `is` expression and conditional binding rules
- `concepts-and-constraints.md` : concept and associated type constraints
- `visibility-and-reachability.md` : name visibility vs compiler reachability
- `nested-declarations.md` : nested type/trait identity and conformance

## Removed Source References
- `git history: ai/specs/language/`
- `ai/notes/`
