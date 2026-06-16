# Functions

Status: draft current
Area: language, functions
Keywords: function, call, RVO, non-rvo, throws, lambda

## RVO Policy
- Return은 기본적으로 RVO path를 요구한다.
- 예외적으로 `[non-rvo]`로 non-RVO path를 허용한다.
- Surface function signature는 유지한다.
- 내부 lowering에서는 caller-provided return storage를 사용할 수 있다.
- NBC return call은 기본적으로 dest-passing / RVO path로 취급한다.
- `return G();`처럼 call result를 다시 return하는 형태는 가능한 경우 caller-provided return storage를 전파하는 lowering 대상이다.

## Call Result Categories
Call은 return kind에 따라 의미 범주를 나눈다.

```text
BC return call
  -> value로 관찰 가능한 result를 만든다.

NBC return call
  -> caller-provided place에 object를 생성한다.

void return call
  -> value/object result 없이 side effect만 가질 수 있다.
```

이 분류는 surface function signature를 바꾸지 않는다.

## Error Channel
- Function은 최대 하나의 error type을 `throws`로 선언한다.
- 호출한 function의 `throws` type이 현재 function과 같으면 `try` 없이 자동 전파한다.
- Error type이 다르면 반드시 `try ... catch_*`로 변환하거나 처리한다.

## Lambda Capture
- 암시 capture 기본값은 copy다.
- `struct`는 암시 capture를 금지하고 `[s]` 또는 `[&s]`를 요구한다.
- Ref capture lambda는 escape 불가다.
- `this`는 암시 capture를 금지한다.
- `struct` method의 `this`는 `&this`만 허용하고 escape 불가다.
- `class` method의 `this`는 `this` copy capture만 허용한다.
- Statement-body lambda와 `inline`은 명시 `return`을 사용한다.

Lambda와 closure capture는 이후 별도 `lambda-and-closures.md`로 더 자세히 나눌 수 있다.

## History
- `git history: ai/specs/language/functions-and-control-flow.md`
- `ai/notes/2026-02-08-rvo-nrvo-design-discussion.md`
- `ai/notes/2026-02-09-lambda-capture-policy.md`
