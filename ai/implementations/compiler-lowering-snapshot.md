# Compiler Lowering Snapshot

Updated: 2026-03-06

Scope
- `Syntax`에서 MIR(IR0) 및 이후 lowering으로 이어지는 현재 구현 방향을 요약한다.
- 실제 코드 작업 전 변경 영향 범위를 빠르게 잡기 위한 스냅샷이다.

Current Direction
- 선언 모델은 `RDecl` 중심으로 통일한다.
- MIR 값 모델은 BC/NBC 분리, `MRead`, `MCreate`, `MLoc_Materialize` 기준으로 정렬한다.
- NBC 반환 call은 sret(dest-passing) 규약을 전제로 translator와 lowering을 맞춘다.
- `MStmt_If`와 바인딩 수명 관리 노드는 내부적으로 분리할 수 있다.

Hot Areas
- `src/SyntaxIR0Translator/`
- `src/MIR/`
- `src/QIR/`

Implementation Notes
- translator에서 읽기 문맥과 생성 문맥을 먼저 구분한다.
- `MOperand` 기반 코드는 `MRead` 방향으로 점진 이관한다.
- bitwise-copyable 타입과 일반 struct의 경로를 혼합하지 않는다.
- 조건부 바인딩은 surface syntax보다 MIR lifetime 관리 관점에서 검증한다.
