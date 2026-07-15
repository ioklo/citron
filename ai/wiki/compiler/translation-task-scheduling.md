# Translation Task Scheduling

Status: draft current
Area: compiler, translator
Keywords: SmTranslator, task, phase, dependency, readiness, unit, scheduling

## Scope

SmTranslator는 unit 간의 큰 순서는 global phase로 유지하고, 한 unit 내부에서만 task dependency를 사용한다. 외부 module declaration은 CTI/RSymbol을 통해 이미 완성된 입력으로 취급하며 현재 unit task graph의 node가 아니다.

## Global Phase Boundary

phase는 모든 unit에 적용되는 semantic barrier다. 예를 들어 declaration shell 수집, signature 완성, conformance binding, implicit synthesis, body translation은 phase 후보가 된다.

unit 간에 특정 task를 직접 기다리게 하지 않는다. 다른 unit의 declaration 상태가 필요하면 그 상태를 전 module에 보장하는 earlier global phase를 둔다.

## Unit-Local Task Graph

각 task는 실행 본문과 함께 요구/산출 readiness를 선언한다.

```text
ImplBind(S, Trait)
  requires: S.Declared, Trait.SignatureComplete
  provides: Conformance(S, Trait).Bound
```

- task는 graph/debug/diagnostic을 위한 stable `TaskId`, 표시명, source location을 가진다.
- task 본문은 간단한 경우 lambda로 구현할 수 있다. runtime identity와 C++ class 이름은 별개다.
- task 등록을 완료한 뒤 graph를 freeze하고, provider 누락과 cycle을 실행 전에 진단한다.
- task 실행 중 새 dependency를 추가하는 것은 피한다. 꼭 필요하면 등록 즉시 검증하고 최종 freeze 검사도 수행한다.

## Order / Rank

unit-local task는 작은 의미 단위의 order를 가진다.

```text
SymbolShell < Signature < Conformance < Synthesis < Body
```

task는 자신보다 낮은 order의 task에만 의존한다. 이 제약은 dependency cycle을 구조적으로 막는다. 이는 phase와 달리 같은 order의 독립 task와, 직접 필요한 lower-order task만 완료되면 다음 task를 실행할 수 있게 한다.

order는 임의의 숫자 목록이 아니라 semantic milestone enum으로 유지한다. 전역적으로 모든 task가 어떤 milestone을 기다려야 할 때만 phase barrier로 승격한다.

## Execution And Parallelism

- 우선 unit 내부 graph는 단일 thread deterministic queue로 실행한다.
- unit 단위 병렬화는 global phase 안에서 수행할 수 있다.
- task graph의 일차 목적은 phase 난립을 막고 local readiness를 드러내는 것이다. 내부 task 병렬화는 RFactory, N*Info mutation, diagnostics의 thread safety와 deterministic ordering이 확립된 뒤 검토한다.

## Related

- `compile-pipeline.md`
- `syntax-ir0-translator.md`
- `declaration-model.md`
- `../../notes/2026-07-13-impl-symbol-identity-and-task-scheduling.md`
