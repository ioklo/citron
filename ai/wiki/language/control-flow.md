# Control Flow

Status: draft current
Area: language, control flow
Keywords: try, catch, tail expression, return completeness, label, break, continue

## Try And Catch
- `do { } catch_*`는 common catch definition scope다.
- 실제 catch 적용 call은 `try <expr>`로 명시한다.
- `try <expr> catch_* ...`는 local override로 허용한다.
- Catch kind는 `catch_resume`, `catch_return`, `catch_error`를 사용한다.
- `try await <expr>`를 standard composition order로 사용한다.

## Tail Expression
- Tail expression은 `inline {}` 및 `catch_*` block에서만 허용한다.
- 일반 block은 마지막 expression을 자동 반환하지 않는다.

## Return Completeness
- Non-void function은 모든 normal fallthrough path가 return value를 제공해야 한다.
- `inline`, `leave`, labeled control flow, `try`/`catch`가 섞인 복합 path의 최종 판정은 compiler lowering / CFG 단계에서 수행할 수 있다.
- `void` function은 명시 `return;`이 없어도 function 끝에서 정상 종료할 수 있다.

## Labeled Control Flow
- Multi-level `break` / `continue`는 `label: for (...)` 문법을 사용한다.
- `break label;`과 `continue label;`을 지원한다.
- `catch_break`, `catch_continue`로 error를 control flow로 변환할 수 있다.

예:
```citron
outer: for (...)
{
    for (...)
    {
        break outer;
    }
}
```

## History
- `git history: ai/specs/language/functions-and-control-flow.md`
- `ai/notes/2026-04-10-control-flow-and-parameter-cleanup-open-points.md`
