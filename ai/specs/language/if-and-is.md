# If/Is 바인딩 스펙 초안

Updated: 2026-03-06
Status: draft

Summary
- 표면 문법에서는 `is`를 최대한 유지한다.
- 바인딩을 만드는 `is`는 일반 expression 문맥에서는 금지하고, `if` 조건의 top-level에서만 허용하는 방향을 사용한다.
- 내부 MIR에서는 바인딩 수명 관리가 필요한 `if`를 별도 bind 성격 노드로 유지할 수 있다.

Rules
- 아래 형태는 모두 `is` expression으로 본다.
  - `c is null`
  - `c is D`
  - `c is D d`
  - `o_c is some c`
  - `e is E.Second(x, _)`
- 바인딩 없는 `is`는 일반 expression 문맥에서 허용한다.
- 바인딩 있는 `is`는 일반 expression 문맥에서 금지한다.
- 바인딩 있는 `is`는 `if` 조건의 top-level에서만 허용한다.
- body에서 보이는 바인딩은 조건식이 true가 되는 모든 경로에서 생성되는 바인딩만 허용한다.

Notes
- `if (a is D d && d.x > 0)`는 허용 가능한 방향이다.
- `if (a is D d || cond) { use(d); }`는 body에 `d`가 항상 존재하지 않으므로 허용하지 않는다.
- 내부 MIR 이름은 `IfTest`보다 `IfBind`가 더 자연스럽다.

