# Compiler Lowering Snapshot

Updated: 2026-04-26

Scope
- `Syntax`에서 MIR(IR0) 및 이후 lowering으로 이어지는 현재 구현 방향을 요약한다.
- 실제 코드 작업 전 변경 영향 범위를 빠르게 잡기 위한 스냅샷이다.

Current Direction
- 선언 모델은 `RDecl` 중심으로 통일한다.
- MIR 값 모델은 BC/NBC 분리, `MRead`, `MCreate`, `MLoc_Materialize` 기준으로 정렬한다.
- NBC 반환 call은 caller-provided storage를 사용하는 dest-passing/RVO 규약을 전제로 translator와 lowering을 맞춘다.
- QIR call lowering은 `Direct`/`Indirect`/`Ref` passing mode를 사용하되, physical register/stack layout은 고정하지 않는다.
- Direct return은 callee slot으로 표현하지 않고 hidden return destination으로 다룬다.
- Indirect return은 hidden first slot(`HiddenReturnDestPtr`)을 사용할 수 있으며, 이 slot은 return object가 아니라 return storage pointer value를 담는다.
- `MStmt_If`와 바인딩 수명 관리 노드는 내부적으로 분리할 수 있다.

Hot Areas
- `src/SyntaxIR0Translator/`
- `src/MIR/`
- `src/QIR/`
- `src/IR0IR1Translator/`
- `src/QEvaluator/`

Implementation Notes
- translator에서 읽기 문맥과 생성 문맥을 먼저 구분한다.
- `MOperand` 기반 코드는 `MRead` 방향으로 점진 이관한다.
- bitwise-copyable 타입과 일반 struct의 경로를 혼합하지 않는다.
- 조건부 바인딩은 surface syntax보다 MIR lifetime 관리 관점에서 검증한다.
- QIR slot index만으로 `this`, parameter, return destination 의미를 추론하지 않고 role metadata를 둔다.
- QEvaluator는 backend 성능/메모리 특성이 아니라 observable event trace 보존을 기준으로 구현한다.

Related Snapshots
- `ai/specs/mir/value-model.md`
- `ai/implementations/qir-call-abi-and-evaluator.md`
