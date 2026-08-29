# Associated type 구현 계획

Date: 2026-08-29
Status: active plan

## 목표

`foreach` lowering과 독립적인 작은 예제로 associated type declaration, witness, projection 및 generic function 사용 경로를 검증한다.

```citron
trait MyTrait
{
    type TItem;
    TItem GetItem();
}

struct S : MyTrait
{
    int value;
}

impl S : MyTrait
{
    type TItem = int;

    int GetItem()
    {
        return value;
    }
}

T.TItem Func<T>(T item) where T : MyTrait
{
    return item.GetItem();
}

void Main()
{
    var x = Func<S>(S(3));
    @$x
}
```

초기에는 function type argument inference를 요구하지 않고 `Func<S>`처럼 explicit type argument를 사용한다. Inference가 구현되면 `Func(S(3))`도 같은 결과를 내는 테스트를 추가한다.

기대 결과는 `3`이다.

## 구현 항목과 의존 순서

### 1. Trait associated type requirement

- trait member로 `type TItem;`을 parse한다.
- syntax member와 semantic associated type requirement declaration을 추가한다.
- trait member type namespace에서 `TItem`을 찾을 수 있어야 한다.
- `TItem GetItem()`의 `TItem`이 해당 requirement를 가리켜야 한다.

### 2. Transparent type alias declaration

- `type TItem = int;`를 정식 type declaration으로 parse하고 symbol tree에 등록한다.
- alias target type expression을 해석한다.
- type lookup에서 alias를 사용할 수 있어야 한다.
- exact type/signature 비교 전에 transparent alias를 target type으로 normalize할 수 있어야 한다.
- `using X = T;` unit-local convenience alias와 혼동하지 않는다.

### 3. Impl associated type witness matching

- impl의 `type TItem = int;`를 `MyTrait::TItem` requirement의 witness로 연결한다.
- impl 또는 구현 대상 type에 same-name concrete nested type declaration이 있으면 그것도 type witness 후보로 인정한다.
- missing witness, duplicate/ambiguous witness를 진단한다.
- requirement function의 associated type을 witness type으로 치환한 뒤 impl function signature와 비교한다.

핵심 positive case:

```text
requirement: TItem GetItem()
type witness: TItem = int
impl method:  int GetItem()
```

### 4. Generic associated type projection lookup

- `T : MyTrait` constraint가 있는 문맥에서 `T.TItem`을 resolve한다.
- semantic type은 단순 nested member가 아니라 `<T as MyTrait>::TItem` projection으로 표현한다.
- concrete self type에서는 conformance의 type witness로 projection을 normalize한다.

```text
<S as MyTrait>::TItem -> int
```

- 같은 이름의 associated type을 제공하는 constraint가 여러 개면 ambiguity를 진단할 수 있어야 한다. 정확한 surface disambiguation syntax는 후속 설계로 둘 수 있다.

### 5. Generic function implementation

- generic function body는 abstract type parameter와 constraint를 사용해 typecheck한다.
- 일반 generic은 monomorphize하지 않고 shared code로 낮춘다.
- runtime entry point는 필요한 type metadata와 trait witness를 hidden argument로 받는다.
- unknown-layout value parameter/return은 address/indirect passing과 value witness를 사용한다.
- `item.GetItem()`은 `T : MyTrait` witness의 function entry를 호출한다.
- `Func`의 return destination을 `GetItem` witness call의 return destination으로 전달할 수 있어야 한다.

### 6. Function generic type argument inference (deferred 가능)

- 초기에는 `Func<S>(S(3))`처럼 explicit type arguments를 요구할 수 있다.
- 후속으로 argument type `S`에서 `T = S`를 추론해 `Func(S(3))`를 허용한다.
- inference는 associated type 구현의 선행 조건으로 만들지 않는다.

## 테스트

### Positive

- associated type requirement parsing.
- impl type alias witness parsing.
- trait/impl symbol relation과 type witness 연결.
- `TItem GetItem()` requirement가 `int GetItem()` impl로 만족됨.
- generic constraint를 통한 `T.TItem` projection lookup.
- `<S as MyTrait>::TItem`이 `int`로 normalization됨.
- explicit type argument를 사용한 core example의 기대 출력 `3`.
- inference 구현 후 `Func(S(3))`의 기대 출력 `3`.

### Negative

- impl에 `TItem` witness가 없음.
- `type TItem = int`인데 impl method가 다른 반환 타입을 사용함.
- associated type requirement와 같은 이름의 witness가 중복되거나 모호함.
- `T : MyTrait` constraint 없이 `T.TItem`을 사용함.
- associated type constraint가 추가된 경우 witness type이 constraint를 만족하지 않음.

## 이번 범위에서 제외

- trait requirement의 `some Trait`.
- associated type inference.
- associated type equality constraint.
- generic associated type(GAT).
- 일반 generic의 필수 monomorphization.
- template/macro code instantiation.
