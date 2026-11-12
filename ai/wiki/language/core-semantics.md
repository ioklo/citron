# Core Semantics Guide

Status: current routing guide; legacy claims require confirmation
Area: language, semantics, migration
Keywords: core semantics, lifetime, initialization, init, trivial constructor, copy, move, parameter, tal, TypeParam, TypeVar, TypeArg

이전 `docs/Citron_Core_Semantics_for_Codex.md`의 주제를 현재 wiki에 연결하는 입구다. 별도의 authoritative 의미론 사본을 유지하지 않는다. 현재 규칙과 미확정 항목은 각 주제 문서의 상태를 함께 확인한다. 이 통합 자체가 새로운 언어 규칙의 확정을 뜻하지 않는다.

## Topic Map

| 이전 요약의 주제 | 현재 기준과 검토 위치 |
|---|---|
| Lifetime, scope exit, destructor | [MIR observable behavior](../compiler/mir-observable-behavior.md), [MIR value model](../compiler/mir-value-model.md), 아래 확인 필요 항목 |
| Struct/trivial constructor, uninitialized variable, `init` | [MIR value model](../compiler/mir-value-model.md), 아래 확인 필요 항목 |
| Copy/move construction and assignment | [Types and references](types-and-references.md), [MIR value model](../compiler/mir-value-model.md) |
| Pointer/reference, address-of, shared ownership | [Types and references](types-and-references.md) |
| Nullable and conditional binding | [Nullable type](nullable-type.md), [If/is binding](if-is-binding.md) |
| Enum matching and payload lifetime | [If/is binding](if-is-binding.md), 아래 확인 필요 항목 |
| Function parameters | [Functions](functions.md), [Types and references](types-and-references.md), 아래 확인 필요 항목 |
| Temporaries, `tal`, foreach | [Enumerable and foreach](enumerable-and-foreach.md), 아래 확인 필요 항목 |
| Explicit rules and resolution | [Design principles](design-principles.md) |
| TypeParam / TypeVar / TypeArg | [Declaration model](../compiler/declaration-model.md#generic-terminology) |

## Current Interpretation

- BC/NBC의 구분과 NBC lifetime 연산의 observable event 보존을 따른다. 모든 타입의 copy/move를 하나의 triviality 규칙으로 단정하지 않는다.
- `T&`는 제한된 surface slot의 non-owning alias 표기다. 언어 전체의 dangling-free 또는 reference lifetime safety 보장으로 해석하지 않는다.
- Nullable binding과 foreach는 해당 wiki의 현재 방향 및 열린 쟁점을 따른다. 이전 요약의 예제 문법을 구현 완료 문법으로 간주하지 않는다.

## Legacy Claims Requiring Confirmation

이전 요약에만 있거나 세부 조건이 불충분한 아래 내용은 현재 확정 규칙으로 승격하지 않는다. 해당 기능을 논의할 때 원래 결정과 현재 구현을 대조해 확인한다.

- `S s;`의 허용 조건과 미초기화 상태, default initialization과 trivial constructor의 정의. 이전 요약은 trivial constructor가 있어야 선언 가능하다고 하면서 별도 예제에서는 같은 선언을 미초기화 상태로 설명했다.
- `init`의 정확한 문법·적용 대상·한 번만 허용되는 범위, implicit zero-initialization 금지의 적용 범위.
- 모든 객체가 scope exit에서 파괴된다는 표현과 shared ownership의 마지막 owner 해제 시점 사이의 구분. no-GC/no-deferred-destruction의 정확한 적용 범위와 early exit cleanup 규칙.
- 필드별 copy/move 지원 여부가 enclosing struct에 전파되는 조건과 implicit fallback 금지의 범위.
- `[in]`, `[out]`, `[move]`, `[forward]` 각각의 인자·초기화·선택 규칙. 특히 `[out]`의 호출 전 미초기화 요구와 반환 후 초기화 보장.
- `shared S s = shared S(...)`의 표면 문법과 참조에서 shared ownership/reference를 만드는 조건.
- enum의 `if case`/`match case` 문법과 payload lifetime, `tal` 용어와 escape 제한. 현재 trait 기반 foreach 방향과의 관계.
- “항상 compile-time error를 선호”, “implicit initialization 금지”, “hidden lifetime extension 금지” 같은 포괄적 문구의 구체적인 적용 범위.

## History

- `git history: docs/Citron_Core_Semantics_for_Codex.md` : 삭제 전 요약 원문. 현재 규칙의 우선 출처로 사용하지 않는다.
- [통합 기록](../../notes/2026-11-10-agent-guidelines-simplification.md)
