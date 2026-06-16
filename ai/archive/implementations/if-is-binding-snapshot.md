# If/Is Binding Snapshot

Updated: 2026-03-06

Summary
- `is` expression은 가능한 한 하나의 형태로 유지한다.
- 바인딩 있는 `is`는 `if` 조건의 top-level로 제한하는 방향을 검토 중이다.
- body에서 보이는 바인딩은 조건식 true 경로의 공통 보장 집합으로 계산한다.

Implementation Direction
- parser는 `is` expression을 통합 형태로 유지한다.
- semantic analysis에서 바인딩 포함 여부와 허용 문맥을 검사한다.
- MIR lowering에서는 조건부 바인딩 수명이 필요한 경우 bind 성격 노드로 내린다.
- 내부 노드 이름은 `IfBind` 방향을 우선 검토한다.

Hot Areas
- `src/Syntax/`
- `src/SyntaxIR0Translator/`
- `src/MIR/`
