# 회의 / 설계 노트

Date: 2026-05-15
Title: trait / concept / associated type 초기 설계

Status
- draft

Summary
- static contract는 `trait`로 도입한다.
- `concept`는 공통 `where` constraint 묶음으로 도입하되, 장기적으로 C++ concept처럼 더 넓은 predicate 체계로 확장할 여지를 둔다.
- trait의 associated type은 `type` requirement로 선언한다.
- 구현체는 `using` type alias 또는 실제 nested type declaration으로 trait의 `type` requirement를 충족할 수 있다.
- `This`는 associated type이 아니라 trait body에서 사용할 수 있는 special self type으로 둔다.

Context
- 이전에는 static interface / trait / concept 중 어떤 축을 쓸지 논의했다.
- `foreach`, `RefEnumerable`, associated type, 명시 conformance를 고려하면 static contract의 본체는 `trait`가 자연스럽다.
- 다만 반복되는 `where` 절을 묶고, 나중에 SAT/predicate 성격의 constraint를 확장하기 위해 `concept`도 별도 레이어로 둘 수 있다.
- Citron에서 `T&`는 일반 type layer의 first-class type이 아니므로, trait associated type과 pointer/reference 정책은 명확히 분리해야 한다.

Decisions
## 1) `trait`는 static contract다
- `trait`는 compile-time contract를 나타낸다.
- 주 사용처는 generic/static polymorphism이다.
- `trait`는 명시적 conformance 대상으로 본다.
- `interface`와는 분리한다.
  - `trait`: static contract
  - `interface`: dynamic/runtime dispatch contract

## 2) `trait` 이름에는 `I` 또는 `T` prefix를 붙이지 않는다
- `I` prefix는 기존 interface naming 관례로 남긴다.
- `T` prefix는 type variable naming과 충돌하기 쉬우므로 trait 이름에는 쓰지 않는다.
- 예:
  - trait: `RefEnumerable`, `RefEnumerator`
  - interface: 필요 시 `IRefEnumerable`, `IRefEnumerator`

## 3) `This`는 special self type이다
- `This`는 associated type이 아니다.
- trait를 구현하는 concrete self type을 뜻한다.
- 사용자가 `type This;`로 선언하거나 구현하지 않는다.
- trait body와 constraint에서 현재 구현 타입을 가리킬 때 사용한다.

예:
```citron
trait Cloneable
{
    This Clone();
}
```

## 4) trait의 `type`은 associated type requirement다
- trait body의 `type X;`는 "이 trait를 구현하는 타입은 `X`라는 type member를 제공해야 한다"는 requirement다.
- 즉 associated type은 required type member로 해석한다.

예:
```citron
trait RefEnumerator
{
    type Item;
    Item* Next();
}
```

## 5) 구현체는 `using` 또는 nested type으로 `type` requirement를 충족한다
- 구현체/`extend` 안에서는 `type X = ...`를 쓰지 않는다.
- 실제 type member를 만드는 문법은 `using` 또는 nested type declaration이다.
- trait checker는 required type member가 구현 타입의 type namespace에서 resolve되는지 확인한다.

예: `using` type alias로 충족
```citron
extend SEnumerator : RefEnumerator
{
    using Item = int;
    int* Next() { ... }
}
```

예: 실제 nested type으로 충족
```citron
struct SEnumerator
{
    struct Item
    {
        int value;
    }
}

extend SEnumerator : RefEnumerator
{
    Item* Next() { ... }
}
```

예: 본체에 이미 있는 type member로 충족
```citron
struct SEnumerator
{
    using Item = int;
}

extend SEnumerator : RefEnumerator
{
    int* Next() { ... }
}
```

## 6) associated type declaration에는 `where`를 허용한다
- Citron은 trait 자체에 `where`를 두지 않는다.
- associated type 사이 관계를 표현하려면 associated type declaration의 `where`를 사용한다.
- cross-associated-type relation은 모호함을 줄이기 위해 qualified form을 선호한다.

예:
```citron
trait RefEnumerable
{
    type Item;
    type Enumerator : RefEnumerator
        where This.Item == Enumerator.Item;

    Enumerator GetEnumerator();
}
```

## 7) `concept`는 공통 `where` constraint 묶음이다
- 초기 `concept`는 반복되는 type constraint를 이름 붙여 재사용하는 용도다.
- 장기적으로 C++ concept처럼 predicate 체계로 확장할 수 있다.
- 초기 constraint atom 후보:
  - `T : SomeTrait`
  - `T.Item == int`
  - `T.Item : class`
  - `T.Item : struct`
  - `T.Item : Base`

예:
```citron
concept SameItem<X, Y>
{
    X.Item == Y.Item;
}

concept IntRefEnumerable<T>
{
    T : RefEnumerable;
    T.Item == int;
}
```

## 8) trait와 concept는 함께 사용될 수 있다
- associated type declaration의 `where`에서도 concept를 사용할 수 있다.
- 직접 constraint를 적어도 된다.

예:
```citron
concept SameItem<X, Y>
{
    X.Item == Y.Item;
}

trait RefEnumerable
{
    type Item;
    type Enumerator : RefEnumerator where SameItem<This, Enumerator>;

    Enumerator GetEnumerator();
}
```

또는:
```citron
trait RefEnumerable
{
    type Item;
    type Enumerator : RefEnumerator
        where This.Item == Enumerator.Item;

    Enumerator GetEnumerator();
}
```

## 9) associated type inference는 v1에서 제외한다
- Swift처럼 function witness signature에서 associated type을 추론하는 기능은 초기 범위에서 제외한다.
- 구현체는 필요한 type member를 명시적으로 제공해야 한다.
- constraint는 추론보다 검증에 사용한다.
- 나중에 필요하면 명백한 경우만 추론하는 기능을 별도 확장으로 검토한다.

예:
```citron
extend S : RefEnumerable
{
    using Item = int;
    using Enumerator = SEnumerator;

    SEnumerator GetEnumerator() { ... }
}

extend SEnumerator : RefEnumerator
{
    using Item = int;

    int* Next() { ... }
}
```

Rationale
- `trait`는 static contract의 identity와 explicit conformance를 표현하기 좋다.
- `concept`는 반복되는 constraint를 줄이고, 향후 더 풍부한 predicate 체계로 확장할 수 있다.
- associated type을 required type member로 보면 `using` alias와 nested type declaration이 모두 자연스럽게 trait requirement를 만족할 수 있다.
- `type`을 trait requirement 전용으로 두고, 구현체에서는 `using`/nested type을 사용하면 "요구"와 "제공"이 문법상 분리된다.
- associated type inference를 제외하면 cti/import surface가 명시적이고 진단이 단순해진다.

Open Points
- `concept`의 constraint atom을 어디까지 허용할지
- `T.Item : Base`가 class inheritance, interface implementation, trait conformance 중 무엇을 우선 의미하는지
- `using` type alias의 visibility와 `extend`가 추가한 type member의 export 규칙
- 같은 이름의 type member가 본체와 여러 `extend`에서 중복 제공될 때의 충돌 규칙
- `trait` conformance witness body를 `cti` / `ctm` split mode에서 어떻게 나눌지

Action Items
- [ ] trait declaration 문법 초안 작성
- [ ] associated type requirement / type member lookup 규칙 초안 작성
- [ ] concept constraint atom 목록과 resolution rule 초안 작성
- [ ] `RefEnumerable`, `RefEnumerator`를 이 문법으로 표준 예시화
