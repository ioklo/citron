# 회의 / 잠정 결정 노트

Date: 2026-03-17
Title: `shared var&` 등 `var` 기반 중첩 sugar는 당분간 도입하지 않음

Status
- tentative
- current working decision

Summary
- `shared var x = shared 3;` 같은 형태는 계속 사용/권장할 수 있다.
- 반면 `shared var&`, `var*&`, `var?&`, `ptr<var>&` 같은 `var` 기반 중첩 sugar는 당분간 도입하지 않는다.
- 현재는 `var`, `shared var`, `var&`, `var*`, `var?`처럼 바깥 껍질 하나만 고정하는 declaration sugar만 유지한다.
- `shared var&`를 포함한 중첩 sugar의 필요성은 실제 사용 사례가 쌓이면 다시 검토한다.

Background
- 기존 합의에서 `shared<T>`는 shared ownership 타입이다.
- 또한 `&`는 polymorphic address-of로 정리되었고, `shared <- &shared`는 금지되어 있다.
  - 참고: `2026-02-23-box-shared-polymorphic-address-design.md`
  - 참고: `ai/specs/language/types-and-ownership.md`
- 한편 `var`는 타입 표현이 아니라, declaration에서 타입을 initializer/문맥으로부터 유도하라는 marker다.
- 따라서 `shared var`는 `shared<var>` 같은 일반 타입 표현이 아니라, decltype induction 결과의 가장 바깥 껍질 하나를 `shared`로 고정하는 declaration sugar로 봐야 한다.
- 최근 작업 중 `shared var&` 문법/타입을 다시 추가할지 고민이 있었지만, Syntax에서 한 차례 제거된 흔적도 확인되었다.

Discussion
- `shared var x`는 ownership/alias 의미가 명확하다.
  - shared handle을 값처럼 들고 다니는 모델이다.
  - 이는 유도된 타입 바깥에 `shared` 껍질 하나를 씌우는 sugar로 볼 수 있다.
- `var&`, `var*`, `var?`도 같은 계열의 declaration sugar다.
  - 유도된 타입 바깥에 각각 ref, ptr, nullable 껍질 하나를 씌운다.
- 하지만 `shared var&`, `var*&`, `var?&`, `ptr<var>&` 같은 형태는 다음 문제가 있다.
  - `var`를 일반 타입 인자처럼 취급하게 보일 수 있다.
  - 바깥 껍질 sugar를 다시 중첩하는 문법이 된다.
  - 의미와 파싱 규칙이 빠르게 복잡해진다.
  - canonical type 표현과 decl sugar 표현의 경계가 흐려진다.
- 특히 `shared var&`는 다음처럼 읽히기 쉬워 의미가 애매해진다.
  - shared handle 자체의 alias인지
  - 유도된 대상 타입의 shared alias인지
  - 단순 ref sugar와 어떤 차이가 있는지
- 현재 시점에서는 이런 중첩 sugar가 실제로 필요한지 확신할 수 없다.

Current Decision
- 당분간 `var` 기반 sugar는 바깥 껍질 하나까지만 허용한다.
- 유지하는 형태:
  - `var`
  - `shared var`
  - `var&`
  - `var*`
  - `var?`
- 당분간 지원하지 않는 형태:
  - `shared var&`
  - `var*&`
  - `var?&`
  - `ptr<var>&`
  - 그 외 `var`를 타입 인자처럼 사용하는 형태
- shared alias/공유 참조는 계속 `shared<T>` 또는 `shared var` 자체로 표현한다.
- 필요할 경우 shared handle은 값 복사/대입으로 전달한다.

Rationale
- `var`는 타입이 아니라 decl-side induction marker이므로, 일반 타입 constructor 조합처럼 자유롭게 중첩시키는 것은 부자연스럽다.
- `shared var`는 특별 허용된 declaration sugar이지만, 이것이 `shared<var>` 같은 타입식을 뜻하는 것은 아니다.
- sugar 범위를 바깥 껍질 하나로 제한하면 문법/파싱/type-check가 단순해진다.
- 실제 사용 사례 없이 sugar 조합을 늘리면 resolver, assign semantics, diagnostics가 모두 복잡해질 수 있다.
- 따라서 지금은 보수적으로 빼 두고, 사용하면서 필요성이 확인되면 다시 여는 편이 낫다.

Open Questions For Future Revisit
- `shared var&` 또는 `var*&` 같은 중첩 sugar가 필요한 실제 코드 패턴이 있는가?
- 그런 조합이 필요하다면, decl sugar로 둘 것인가 아니면 canonical type form으로만 허용할 것인가?
- `var` 기반 중첩 sugar가 들어오면 assign/rebind/type-check semantics를 어떻게 정의할 것인가?
- 현재의 단일 껍질 sugar(`shared var`, `var&`, `var*`, `var?`)만으로 충분히 표현 가능한가?

Follow-up
- [ ] 실제 사용 사례에서 `var` 기반 중첩 sugar 필요 여부 관찰
- [ ] 필요 사례가 생기면 canonical form과 declaration sugar의 경계를 별도 문서로 재정리
