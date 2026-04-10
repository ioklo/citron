# 회의 / 실험 노트

Date: 2026-04-09
Title: `inline`, `catch_resume`, `exp block` 규칙 정리

Summary
- `inline { ... }`는 expression 위치에서 statement block을 실행해 값을 하나 만드는 구문으로 둔다.
- `inline`과 `catch_resume`는 값-producing block으로 보고, 블록 내부에서 최종 값은 `leave value;`로 지정한다.
- `exp block`은 "마지막에 stmt 대신 exp가 올 수 있는 block"이라는 syntax 범주로 두되, 마지막 exp의 의미는 소비하는 문맥이 정한다.
- `catch_return`과 `catch_error`는 내부 의미를 `leave`로 통일하기보다, surface syntax에서 각각 `return`, `throw`를 그대로 쓰는 쪽을 선호한다.

Context
- `inline`은 expression 자리에서 statement block을 사용하게 해 주는 구문이다.
- 기본 예시:

```citron
var i = inline { F(); G(); leave 3; };
```

- 위 코드는 `F(); G();`를 실행한 뒤 최종 값 `3`을 남기고, 전체 `inline` expression의 결과가 `3`이 된다.
- `inline`은 expression이므로 `void` 결과를 허용하지 않는다. `void` 목적이라면 일반 statement block을 사용한다.

Decision
- `inline`은 값-producing expression으로 둔다.
- `inline`의 본문은 statement block처럼 실행되지만, 정상 경로는 최종 값을 남겨야 한다.
- 최종 값 지정은 명시적으로 `leave value;`를 사용한다.

예:

```citron
var i = inline {
    F();
    G();
    leave 3;
};
```

- `inline` 본문에는 tail expression sugar를 허용한다.

```citron
var i = inline {
    F();
    G();
    3
};
```

- 위 형태는 semantic level에서 아래와 동등하게 취급한다.

```citron
var i = inline {
    F();
    G();
    leave 3;
};
```

- `inline`의 타입은 가능한 한 source에서 보이는 순서대로 정한다.
  - 1) `hintType`이 있으면 그것을 우선 사용
  - 2) 없으면 첫 `leave` 또는 tail expression의 타입으로 결정
  - 3) 이후 다른 `leave`/tail expression은 그 타입과 일치해야 한다

예:

```citron
int i = inline { ... };                // inline의 기대 타입은 int
var i = inline { if (cond) leave 3; leave "hi"; } // 첫 leave가 int이므로 두 번째는 타입 에러
```

- `inline`의 error type은 현재 함수의 declared error type을 따른다.

```citron
void F() throws E
{
    var i = inline {
        throw E();
    };
}
```

## `exp block`의 의미
- `exp block`은 "마지막에 stmt 대신 exp가 올 수 있는 block"이라는 syntax 레벨 개념으로 본다.
- 중요한 점은, `exp block` 자체가 공통 semantic을 갖는 것은 아니라는 점이다.
- 마지막 exp의 의미는 block을 소비하는 문맥이 정한다.

예:
- `inline`에서 tail exp는 `leave exp;`로 해석
- `catch_resume`에서 tail exp sugar를 허용한다면, 역시 `leave exp;`로 해석 가능
- 다른 문맥에서는 tail exp를 허용하지 않거나, 별도 의미를 정의할 수 있다

즉, "tail exp가 자동으로 return/leave/yield가 된다"를 block 공통 규칙으로 두지 않는다.

## `catch_resume`
- `catch_resume`는 실패한 식의 대체값을 만드는 문맥이므로, `inline`과 마찬가지로 값-producing block으로 본다.
- 따라서 내부에서 최종 값 지정은 `leave value;`를 사용하는 쪽으로 정리한다.

예:

```citron
try F() catch_resume(e) {
    Log(e);
    leave 3;
}
```

- 이 규칙을 택하면 `inline`과 `catch_resume`이 같은 mental model을 공유하게 된다.
  - 둘 다 statement block을 실행한다
  - 둘 다 최종 값은 `leave`로 지정한다

## `catch_return`, `catch_error`
- 내부 semantic을 추상화하면 `leave` 같은 공통 escape로 설명할 수도 있다.
- 그러나 surface syntax에서는 각각의 제어 효과가 직접 보이는 쪽을 더 선호한다.

따라서 권장 문법은 다음과 같다.

- `catch_return`: `return value;`
- `catch_error`: `throw value;`

예:

```citron
try F() catch_return(e) {
    return 3;
}

try F() catch_error(e) {
    throw Convert(e);
}
```

- 이유:
  - `catch_return`에서 `return`은 함수에서 빠져나간다는 의도가 즉시 보인다
  - `catch_error`에서 `throw`는 에러로 전환해 던진다는 의도가 즉시 보인다
  - `leave`로 통일하면 내부 모델은 단순해질 수 있어도, surface syntax의 직관성이 떨어질 수 있다

Rationale
- `inline`은 expression인데 statement block 재사용을 원하므로, `leave`는 statement-like body 안에서 expression result를 지정하는 명시적 escape로 적합하다.
- tail expression sugar는 사용성을 높여 주지만, 그 의미를 block 공통 semantic으로 두면 문맥별 해석 충돌이 생길 수 있다.
- 따라서 `exp block`은 syntax 공유에만 사용하고, semantic은 소비하는 construct가 부여하는 편이 더 안전하다.
- `catch_resume`은 값-producing block이라 `leave`가 잘 맞지만, `catch_return`과 `catch_error`는 value production보다 control effect가 핵심이므로 `return`/`throw`가 더 읽기 쉽다.

Open Points
- `inline`과 `catch_resume`에서 tail expression sugar를 둘 다 허용할지, 아니면 core rule을 `leave`만으로 제한할지
- `inline`의 모든 reachable path가 값을 남겨야 한다는 규칙을 어떤 단계에서 진단할지
- `exp block`을 허용하는 다른 construct가 추가될 때 tail exp 해석 규칙을 어디까지 공유할지

Action Items
- [ ] `inline`의 syntax/type/error 규칙을 스펙에 반영
- [ ] `catch_resume`의 block body에서 `leave`를 사용하는 방향을 구현/문서화
- [ ] `exp block`은 syntax 범주이고 semantic은 소비 문맥이 정한다는 원칙을 명문화
