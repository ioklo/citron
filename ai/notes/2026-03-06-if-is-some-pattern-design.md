# 회의 / 실험 노트

Date: 2026-03-06
Title: if/is nullable 패턴에서 `not_null` 대신 `some` 사용

Summary
- `if` 조건의 `is` 표현식을 패턴 중심으로 읽히게 하기 위해 nullable 바인딩 패턴 이름을 다시 검토했다.
- 기존 `not_null(alias)` 표기는 안전 정책 설명에는 유리했지만, 함수 호출처럼 보여 패턴 문법과 잘 맞지 않는다는 문제가 있었다.
- 최종적으로 nullable 바인딩 패턴은 `some`을 사용하고, unchecked 표기는 기존과 같이 바인딩 변수 뒤의 `!`를 유지하는 방향으로 정리했다.

Context
- 기존 nullable 관련 노트에서는 `if (exp is not_null(alias))`와 `alias!` 기반 checked/unchecked talias 정책을 사용하고 있었다.
- 이후 `if` 조건의 `is`를 null/type/enum 패턴까지 포함하는 단일 문법 계열로 다루려는 방향이 강해졌다.
- 이 과정에서 `not_null(alias)`는 패턴이라기보다 함수 호출처럼 보여, `c is D d`, `e is E.Second(x, _)` 같은 다른 패턴들과 표면 일관성이 떨어진다는 문제가 드러났다.

Alternatives discussed
- `not_null(alias)` 유지
  - 장점: checked/unchecked talias 정책을 이름 자체로 설명하기 쉽다.
  - 단점: 패턴이라기보다 호출처럼 보이고, 괄호형 표기가 `is` 패턴 계열에서 혼자 튄다.
- `not_null alias` 형태로만 변경
  - 장점: 괄호를 없애 읽기감은 조금 나아진다.
  - 단점: 여전히 `null`의 반대 패턴으로는 직관이 약하다.
- `some alias`로 변경
  - 장점: `x is null`의 반대가 `x is some s`로 자연스럽고, `c is D d`와 같은 패턴+바인딩 형태로 정렬된다.
  - 단점: 타입 이름이 `Nullable`인데 `some`을 쓰는 점은 처음에는 약간 이질적으로 느껴질 수 있다.

Decision
- nullable 바인딩 패턴 이름은 `not_null` 대신 `some`을 사용한다.
- 표면 문법은 괄호형이 아니라 공백형을 사용한다.
  - `x is some s`
  - `x.s is some s!`
- `null`과 `some`은 nullable 전용 내장 패턴으로 취급한다.
- `some`은 값 존재를 뜻하는 패턴이며, `Nullable<T>`와 `NullableInplace<T>`를 표면에서 구분하지 않고 공통으로 사용한다.
- unchecked 표기는 기존과 같이 바인딩 변수 뒤의 `!`를 사용한다.
  - `s`  : checked bind
  - `s!` : unchecked bind

Rationale
- `x is null` / `x is some s`는 nullable 패턴 쌍으로 읽히며, `null`의 반대 패턴이 무엇인지 코드만 보고도 이해하기 쉽다.
- `x is some s`는 `x is D d`와 동일한 표면 구조를 가져, `is` 우변이 패턴이라는 감각을 강화한다.
- `some`을 nullable 전용 내장 패턴으로 정의하면 내부 표현이 tagged(`Nullable<T>`)인지 inplace(`NullableInplace<T>`)인지 숨기면서도 일관된 표면 문법을 유지할 수 있다.
- checked/unchecked 안전 정책은 패턴 이름보다 바인딩 변수의 `!`에 실리는 편이 더 직접적이다.

Safety policy mapping
- 기존 정책의 의미는 유지한다.
- `x is some s`
  - checked talias를 요청한다.
  - 로컬 대상은 checked 검사를 수행한다.
  - 비로컬 대상은 checked 보장을 못 하면 경고를 낸다.
- `x is some s!`
  - unchecked talias를 강제한다.
  - 비로컬 대상에서는 경고 없이 허용한다.
  - 로컬 대상에서는 불필요한 unchecked 사용으로 경고를 낼 수 있다.

Examples
- `if (x is null)`
- `if (x is some s)`
- `if (x.s is some s!)`
- `if (c is D d)`
- `if (e is E.Second(x, _))`

Implications
- 기존 `not_null(alias)` / `not_null(alias!)` 예시와 진단 문구는 `some alias` / `some alias!` 기준으로 옮겨야 한다.
- `if/is` 스펙과 nullable 스펙을 함께 갱신해야 한다.
- 구현에서는 nullable 바인딩 패턴의 AST/파서 표기와 진단 메시지를 동시에 변경해야 한다.

Action Items
- [x] `ai/specs/language/if-and-is.md`를 `some` 기준으로 갱신
- [x] `ai/specs/language/nullable-and-iteration.md`의 nullable 패턴 표기를 `some` 기준으로 갱신
- [ ] parser/translator에서 nullable 패턴 키워드 및 진단 문구 변경 범위 확인

