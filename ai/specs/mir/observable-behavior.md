# Observable Behavior 스펙

Updated: 2026-04-26
Status: current

Summary
- Observable behavior는 system call 결과만 뜻하지 않는다.
- 언어 실행이 만들어내는 observable event의 내용과 순서를 기준으로 정의한다.
- Evaluation order는 독립적인 관찰 대상이 아니라 observable event 순서를 결정하는 언어 규칙이다.

Observable Events
- external side effect의 순서와 내용
  - command/directive 출력
  - IO 또는 system call
  - process exit value
- mutable 또는 alias 가능한 program state mutation의 순서와 결과
  - ref parameter를 통한 caller object 변경
  - shared mutable state 변경
  - pointer/reference로 접근 가능한 object 변경
- NBC object lifetime operation의 순서
  - construct
  - copy-construct
  - move-construct
  - assign
  - destroy
- trap, panic, diagnostic, throw 같은 비정상 제어 결과와 그 전까지의 observable event 순서
- 최종 반환값 또는 최종 externally visible state

Non-Observable Implementation Details
- 실제 함수 call instruction이 존재했는지 여부
- call frame이 생성되었는지 여부
- register를 썼는지 stack을 썼는지 여부
- physical stack pointer/base pointer의 값
- BC 값의 불필요한 copy 횟수
- padding byte 값
- optimizer가 순수 BC 계산이나 inline 가능한 call boundary를 제거했는지 여부

Evaluation Order
- 언어가 정한 evaluation order는 observable event의 순서를 보존하기 위한 규칙이다.
- 평가가 external side effect, mutable state mutation, NBC lifetime operation, trap/throw/diagnostic을 만들면 그 순서는 관찰 가능하다.
- 순수 BC 계산처럼 observable event를 만들지 않는 평가의 물리 순서는 backend optimization이 바꿀 수 있다.

BC and NBC Boundary
- BC 값은 bitwise copy가 의미를 바꾸지 않으므로 copy 횟수 자체는 observable behavior가 아니다.
- NBC 값은 lifetime operation이 의미 이벤트이므로 copy/move/destroy 발생 여부와 순서가 observable behavior다.
- 최적화는 call boundary나 temporary storage를 제거할 수 있지만, observable event trace를 바꾸면 안 된다.
