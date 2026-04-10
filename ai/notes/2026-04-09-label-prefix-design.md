# 회의 / 실험 노트

Date: 2026-04-09
Title: label declaration/reference 문법 재검토

Summary
- `inline`에 label을 도입하면서, `leave`의 labeled target 표기를 함께 재검토했다.
- 기존 전통적 문법인 `label: stmt`는 익숙하지만, `leave`가 target과 value를 함께 받는 문장이라 labeled form 설계가 애매해진다.
- 여러 후보를 비교한 결과, declaration/reference 모두 `:label` prefix를 쓰는 방향이 가장 일관되고 간결하다는 결론에 기울었다.

Context
- `for`, `while`, `switch`처럼 `inline`도 label target이 될 수 있게 하려는 요구가 생겼다.
- 목표 예시는 다음과 같다.

```citron
var i = :label inline {
    var j = :label2 inline {
        leave 3;          // 가장 가까운 :label2 대상
        leave :label "hi"; // 바깥 :label 대상
    };
};
```

- 문제는 `leave`가 단순 jump가 아니라 value를 함께 받는다는 점이다.
- 기존 문법을 유지하면 선언은 `label:`인데, `leave` 쪽 target 표기를 어떻게 할지가 애매해진다.

검토한 후보
## 1) 기존 postfix declaration 유지

```citron
label: for (...)
label: while (...)
label: inline { ... }
```

- 장점:
  - 전통적인 label 문법과 가장 가깝다.
  - 기존 사용자에게 익숙하다.
- 단점:
  - `leave`는 target과 value를 함께 받아야 하므로 문법이 길어지거나 모호해진다.
  - 예: `leave label outer "hi";`는 너무 장황하다.
  - 예: `leave outer "hi";`는 짧지만 expression과 충돌 가능성이 있다.

## 2) prefix label marker 도입
후보:
- `:label`
- `@label`
- `#label`

검토 결과:
- `@label`
  - 일반적으로 annotation 연상이 강하다.
  - Citron에서는 이미 `@`가 command stmt에 쓰이므로 제외한다.
- `#label`
  - comment 연상이 있다.
  - 아주 치명적이지는 않지만 언어 표면에서 다소 튈 수 있다.
- `:label`
  - 기존 `label:`과 시각적으로 연관성이 있다.
  - declaration/reference를 한 문법으로 통일하기 쉽다.

## 3) label 위치
### 앞(prefix)

```citron
:label for (...)
:label while (...)
:label switch (...)
:label inline { ... }
```

### 중간

```citron
for :label (...)
while :label (...)
switch :label (...)
inline :label { ... }
```

- 중간 방식은 각 statement parser에 특수 처리를 추가할 가능성이 높다.
- 앞(prefix) 방식은 statement 진입 전에 label을 먼저 읽으면 되므로 더 균일하다.

Decision
- declaration/reference 문법은 `:label` prefix 방식으로 통일하는 쪽을 우선 선호한다.
- 권장 형태:

```citron
:loop while (cond) {
    if (x) break :loop;
    continue :loop;
}

:outer inline {
    :inner inline {
        leave 3;
        leave :outer "hi";
    }
}
```

- 즉 label declaration과 label reference가 모두 같은 표면 형태를 가진다.

```citron
:label <stmt>
break :label;
continue :label;
leave :label <expr>;
```

Rationale
- `leave`는 target과 value를 동시에 받기 때문에, 기존 `label:` declaration + `leave ??? expr` 조합은 문법적으로 깔끔하게 정리하기 어렵다.
- `leave label outer expr` 같은 명시적 문법은 가능하지만 장황하다.
- `leave outer expr`는 짧지만 expression과의 파싱 충돌 우려가 있다.
- 반면 `leave :label expr`는 파싱이 단순하고, `leave expr`와도 깔끔하게 구분된다.
- declaration까지 `:label`로 통일하면 다음 장점이 있다.
  - label 문법이 한 가지 모양으로 수렴한다.
  - `for`, `while`, `switch`, `inline` 모두 동일 규칙을 공유한다.
  - `break`, `continue`, `leave`의 labeled reference도 동일 규칙을 공유한다.

Tradeoff
- 가장 큰 단점은 전통적인 `label:` 문법과 방향이 반대라는 점이다.
- 즉 `:label stmt`는 처음 읽을 때 심리적으로 어색할 수 있다.
- 그러나 다른 후보들은 이미 언어 내 기존 의미와 충돌하거나, `leave`와 결합할 때 지나치게 장황해진다.
- 전체 문법의 통일성과 `leave` 파싱 안정성을 고려하면, prefix `:label`의 장점이 더 크다고 판단한다.

Open Points
- 정말 모든 label target(`for`, `while`, `switch`, `inline`)에 동일하게 `:label` prefix를 적용할지
- 기존 note의 label shadowing 금지 정책을 이 문법에도 그대로 적용할지
- parser에서 `:label stmt`를 statement 앞 공통 prefix로 처리할지, 개별 statement parser에서 처리할지

Action Items
- [ ] `inline` label과 `leave :label expr` 문법을 syntax 차원에서 구체화
- [ ] `for`/`while`/`switch`/`inline`의 label declaration을 `:label` prefix로 통일할지 최종 결정
- [ ] label declaration/reference 문법 변경이 기존 diagnostic 및 MIR `labelId` 정책과 어떻게 연결되는지 정리
