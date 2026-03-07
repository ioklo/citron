# Nullable/Iteration 스펙

Updated: 2026-03-06
Status: current

Nullable Representation
- 중첩 nullable은 의미적으로 병합하지 않는다.
- 타입 계층에서 `nullable<nullable T>`를 허용한다.
- `C?`는 `NullableInplace<C>` 계열의 전용 압축 표현으로 취급한다.
- 일반 `T?`는 `Nullable<T>` tagged 표현으로 유지한다.
- 제네릭 `T?`는 `T`가 ref로 확정되더라도 압축형으로 자동 전환하지 않는다.

Naming
- 압축형 nullable 이름은 `NullableInplace<T>`를 사용한다.
- 일반 tagged nullable 이름은 `Nullable<T>`를 사용한다.

Nullable Pattern
- `if (exp is some alias)` 패턴을 지원한다.
- `alias!`로 unchecked talias를 명시할 수 있다.
- `some alias`는 checked bind를 뜻하며, checked talias는 로컬 대상에서만 보장한다.
- `some alias!`는 unchecked bind를 뜻한다.
- 비로컬 대상에서는 `alias!` 없는 checked 요청에 경고를 낸다.

Foreach
- `foreach`는 `for`로 완전 desugaring하지 않는다.
- MIR에서 `MStmt_Foreach` 의미 보존 노드로 유지한다.
- next/cast 의미는 `MStmt_Foreach` 내부 경로에서 표현한다.

Iterator Contract
- 기본 next 시그니처는 `nullable<TItem> GetNext();` 또는 `nullable<TItem> GetNext() TError;`다.
- `GetNext`의 null은 정상 종료를 의미한다.
- `TError`는 종료와 별개의 실패 채널이다.


