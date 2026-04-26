# 함수/제어흐름 스펙

Updated: 2026-04-26
Status: current

RVO Policy
- 반환은 기본적으로 RVO 경로를 요구한다.
- 예외적으로 `[non-rvo]`로 비-RVO 경로를 허용한다.
- 표면 함수 시그니처는 유지한다.
- 내부 lowering에서는 caller-provided return storage를 사용할 수 있다.
- NBC 반환 call은 기본적으로 dest-passing/RVO 경로로 취급한다.
- `return G();`처럼 반환 결과를 다시 반환하는 형태는 가능한 경우 caller-provided return storage를 전파하는 lowering 대상이다.

Call Result Categories
- 호출은 반환 종류에 따라 의미 범주를 나눈다.
  - BC 반환 call: 값으로 관찰 가능한 결과를 만든다.
  - NBC 반환 call: caller-provided place에 object를 생성한다.
  - `void` 반환 call: 값이나 object result를 만들지 않고 side effect만 가질 수 있다.
- 이 분류는 표면 함수 시그니처를 바꾸지 않는다.

Error Channel
- 함수는 최대 하나의 에러 타입을 `throws`로 선언한다.
- 호출한 함수의 `throws` 타입이 현재 함수와 같으면 `try` 없이 자동 전파한다.
- 에러 타입이 다르면 반드시 `try ... catch_*`로 변환하거나 처리한다.

Try and Catch
- `do { } catch_*`는 공통 catch 정의 스코프다.
- 실제 catch 적용 호출은 `try <expr>`로 명시한다.
- `try <expr> catch_* ...`는 로컬 override로 허용한다.
- catch 종류는 `catch_resume`, `catch_return`, `catch_error`를 사용한다.
- `try await <expr>`를 표준 조합 순서로 사용한다.

Tail Expression
- tail expression은 `inline {}` 및 `catch_*` 블록에서만 허용한다.
- 일반 블록은 마지막 expression 자동 반환을 하지 않는다.

Return Completeness
- non-void 함수는 모든 정상 fallthrough 경로가 반환값을 제공해야 한다.
- `inline`, `leave`, labeled control flow, `try`/`catch`가 섞인 복합 경로의 최종 판정은 compiler lowering/CFG 단계에서 수행할 수 있다.
- `void` 함수는 명시 `return;`이 없어도 함수 끝에서 정상 종료할 수 있다.

Labeled Control Flow
- 다중 break/continue는 `label: for (...)` 문법을 사용한다.
- `break label;`, `continue label;`을 지원한다.
- `catch_break`, `catch_continue`로 에러를 제어흐름으로 변환할 수 있다.

Lambda Capture
- 암시 캡쳐 기본값은 copy다.
- `struct`는 암시 캡쳐를 금지하고 `[s]` 또는 `[&s]`를 요구한다.
- ref 캡쳐 람다는 escape 불가다.
- `this`는 암시 캡쳐를 금지한다.
- `struct` 메서드의 `this`는 `&this`만 허용하고 escape 불가다.
- `class` 메서드의 `this`는 `this` copy 캡쳐만 허용한다.
- stmt-body 람다와 `inline`은 명시 `return`을 사용한다.
