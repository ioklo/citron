# 회의 / 실험 노트

Date: 2026-03-05
Title: Error 채널(throws) + try/do-catch + tail expression + labeled break/continue

Summary
- 함수 시그니처에 에러 타입 1개를 부여하는 `throws` 기반 에러 채널 모델을 정리했다.
- 공통 catch 스코프(`do { } catch_*`)와 호출 지점의 명시 표식(`try expr`)을 분리해, 장황함과 explicitness를 동시에 확보한다.
- 일부 컨텍스트(`inline`, `catch_*`)에서만 tail expression을 허용해 간결한 값 생성 문법을 제공한다.
- 다중 break/continue는 `label: for (...)` 라벨 방식으로 지원하고, `catch_break/catch_continue`로 에러를 제어흐름으로 변환한다.

Decisions
- Error 채널 표기: 함수당 에러 타입 1개
  - 예: `int F() throws Error { ... }`
  - `throws` 생략 문법은 사용하지 않는다(가독성/검색성).

- 전파 규칙
  - 호출한 함수의 `throws` 타입이 현재 함수와 동일하면 `try`를 생략할 수 있고 자동 전파한다.
  - `throws` 타입이 다르면 반드시 `try ... catch_*`로 변환/처리한다.

- `do { } catch_*`와 `try expr`의 역할 분리
  - `do { } catch_*`는 공통 catch “정의 스코프”만 제공한다.
  - 실제로 catch가 적용되는 호출은 블록 내부에서 `try <expr>`로 명시한다.
  - `try <expr> catch_* ...`는 로컬 override로 허용하며, 이 경우 가장 가까운 `do-catch`보다 우선한다.

- catch 종류(의미는 유지, 이름은 길게 둔다)
  - `catch_resume(E e) ...`: 실패한 식의 대체값을 만들어 계속 진행
  - `catch_return(E e) ...`: 현재 함수에서 즉시 return
  - `catch_error(E e) ...`: 현재 함수의 `throws` 타입으로 에러 변환(throw)

- tail expression 허용(세미콜론 없이)
  - tail expression은 `expr;`이 아니라 블록 마지막이 `expr`로 끝나는 형태를 의미한다.
  - 아래 컨텍스트에서만 tail expression을 허용한다.
    - `inline { ... expr }`
    - `catch_resume(...) { ... expr }`
    - `catch_return(...) { ... expr }`
    - `catch_error(...) { ... expr }`
  - 위 컨텍스트에서는 `return`도 허용한다(지원해야 할 stmt 예외가 많음). 단, style/lint로 tail expr 사용을 유도할 수 있다.
  - 일반 `{ }` 블록에서는 마지막 `expr`을 특별 취급하지 않는다.

- `inline`의 목적/제약
  - `inline { }`는 “값을 만드는” 것이 목적이다.
  - 따라서 `inline`은 위 tail expression 규칙을 통해 값 생성이 가능해야 한다.

- `await` 결합 순서
  - prefix 조합은 `try await <expr>`를 표준으로 둔다(의미: `await` 결과를 에러 채널과 함께 받는다).

- labeled break/continue
  - 라벨 문법은 `label: for (...) { }` 형태로 고정한다(가장 익숙한 형태).
  - `break label;`, `continue label;`을 지원한다.

- catch에서 break/continue 변환
  - `catch_break(E e);` / `catch_break(E e) label;`
  - `catch_continue(E e);` / `catch_continue(E e) label;`
  - 라벨 생략 시 가장 가까운 break/continue 대상에 적용한다.
  - `catch_break`는 loop/switch 맥락에서만 허용, `catch_continue`는 loop 맥락에서만 허용한다(그 외는 컴파일 에러).

Notes
- 블록 전체를 `try { ... } catch ...`로 감싸고 내부 호출을 암묵적으로 처리하는 방식은, 어느 호출이 처리되는지 리뷰가 어려워 v1에서는 피한다.
- `do-catch`는 반복되는 catch 로직을 묶되, 적용 지점은 `try`로 남겨 explicitness를 유지한다.

Action Items
- [ ] 문서/스펙에 `do-catch` + `try expr` 우선순위 규칙 명문화
- [ ] catch_*의 타입 규칙(예: `catch_resume`는 원래 식 타입과 일치) 정리
- [ ] tail expression 파싱/AST 규칙과 “허용 컨텍스트 목록” 확정
- [ ] labeled break/continue와 `catch_break/catch_continue` 진단 규칙 정리
