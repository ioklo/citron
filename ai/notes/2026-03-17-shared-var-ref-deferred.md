# 회의 / 잠정 결정 노트

Date: 2026-03-17
Title: `shared var&`는 당분간 도입하지 않고 `var&`는 단일 축으로 유지

Status
- tentative
- current working decision

Summary
- `shared var x = shared 3;` 같은 형태는 1단계 ownership 표기로 계속 사용/권장할 수 있다.
- 반면 `shared var& y = x;` 형태는 당분간 도입하지 않는다.
- 현재는 `var&`를 단일 ref 축으로 유지하고, `shared`는 별도의 ownership/alias 타입 constructor로 유지한다.
- `shared var&`의 필요성은 실제 사용 사례가 쌓이면 다시 검토한다.

Background
- 기존 합의에서 `shared<T>`는 shared ownership 타입이다.
- 또한 `&`는 polymorphic address-of로 정리되었고, `shared <- &shared`는 금지되어 있다.
  - 참고: `2026-02-23-box-shared-polymorphic-address-design.md`
  - 참고: `ai/specs/language/types-and-ownership.md`
- 최근 작업 중 `shared var&` 문법/타입을 다시 추가할지 고민이 있었지만, Syntax에서 한 차례 제거된 흔적도 확인되었다.
- 이로 인해 과거 방향이 `var&`를 단일 축으로 단순화하는 쪽이었을 가능성을 검토했다.

Discussion
- `shared var x`는 ownership/alias 의미가 명확하다.
  - shared handle을 값처럼 들고 다니는 모델이다.
- 하지만 `shared var& y = x`는 의미가 애매해진다.
  - `y`가 shared handle 자체의 alias인지
  - 아니면 shared가 가리키는 대상의 alias인지
  - assign/rebind 시 무엇이 바뀌는지
  - plain `var&`와 어떤 역할 차이가 있는지
- 즉 `shared`가 이미 alias/ownership 성격을 갖고 있는데, 그 위에 다시 `&`를 얹으면 의미 축이 중복될 수 있다.
- 현재 시점에서는 이 중복이 실제로 필요한지 확신할 수 없다.

Current Decision
- 당분간 `shared var&`는 지원하지 않는다.
- `var&`는 plain ref 축으로만 유지한다.
- shared alias/공유 참조는 계속 `shared<T>` 자체로 표현한다.
- 필요할 경우 shared handle은 값 복사/대입으로 전달한다.

Rationale
- 의미 축을 단순하게 유지하는 편이 현재 언어/IR 정리 방향과 맞다.
- `shared<T>`는 ownership/alias 의미를 이미 담고 있으므로, `shared var&`는 중복 가능성이 높다.
- 실제 사용 사례 없이 문법/타입 축을 다시 늘리면 resolver, assign semantics, diagnostics가 모두 복잡해질 수 있다.
- 따라서 지금은 보수적으로 빼 두고, 사용하면서 필요성이 확인되면 다시 여는 편이 낫다.

Open Questions For Future Revisit
- `shared var&`가 필요한 실제 코드 패턴이 있는가?
- 필요하다면 그 의미는 다음 중 어느 쪽인가?
  - shared handle 자체의 alias
  - shared가 가리키는 대상의 alias
- `shared var&`가 들어오면 assign/rebind semantics는 어떻게 정의할 것인가?
- plain `var&` 및 `shared<T>`만으로 충분히 표현 가능한가?

Follow-up
- [ ] 실제 사용 사례에서 `shared var&` 필요 여부 관찰
- [ ] 필요 사례가 생기면 assign/rebind/type-check 규칙을 별도 문서로 재정리
