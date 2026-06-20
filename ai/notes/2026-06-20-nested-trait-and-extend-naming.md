# 회의 / 설계 노트

Date: 2026-06-20
Title: nested trait 시나리오와 `extend` 키워드 임시 선택

Summary
- nested `trait`는 단순 문법 장식이 아니라, class/struct 내부 전용 contract를 감추는 시나리오가 있다.
- 이 경우 nested `extend`도 함께 성립 가능하다고 보는 편이 자연스럽다.
- conformance block 키워드는 `impl`, `extension`, `extend`를 비교했지만, 현재 구현 단계에서는 짧고 다루기 쉬운 `extend`를 우선 사용한다.

Context
- trait와 `some T`를 구현하는 과정에서, nested declaration syntax에 `trait`를 넣을지 결정할 필요가 생겼다.
- 처음에는 nested `trait`가 꼭 필요하지 않을 수 있다고 보았지만, 내부 전용 opaque contract를 감추는 용도는 실제 시나리오가 있다고 판단했다.

Scenario
```citron
class C<T>
{
    trait Trait
    {
    }

    struct S
    {
    }

    extend S : Trait
    {
    }

    some Trait F()
    {
        return S();
    }
}
```

이 경우 기대하는 점:
- `Trait`는 `C` 내부에서만 보이는 contract가 된다.
- `S`도 내부 helper 타입으로 둘 수 있다.
- `some Trait`는 외부에 concrete type을 숨기면서, 내부에서는 명확한 contract를 가질 수 있다.

Direction
- nested `class`, `struct`, `trait`는 논리적으로 가능하다고 본다.
- nested `extend`도 함께 성립 가능하다고 본다.
- outer generic parameter capture를 허용한다면 nested declaration identity는 outer type arguments를 포함한다.
- 따라서 `C<int>.Trait`와 `C<string>.Trait`는 다른 declaration identity로 본다.

Naming
- `impl`
  - requirement body를 채운다는 느낌이 강하다.
  - conformance declaration 이름으로는 약간 좁게 느껴질 수 있다.
- `extension`
  - 선언 키워드로는 안정적이지만 길다.
  - Swift 선례와 비슷한 범용 확장 블록 의미로는 잘 맞는다.
- `extend`
  - 가장 짧고 실험 단계에서 다루기 쉽다.
  - 지금은 trait 구현 블록 표면 키워드로 우선 사용한다.
  - 나중에 `impl` 또는 `extension`으로 바꿀 가능성은 열어 둔다.

Parser Note
- `some` 뒤 타입이 실제로 trait인지 여부는 parser가 아니라 semantic 단계에서 확인하는 편이 맞다.
- parser는 `some`을 함수 return position marker로만 읽고, 그 뒤에 오는 `TypeExp`의 kind 확인은 이후 단계로 넘긴다.

Implementation Note
- declaration parser 이름은 `ParseNestedDecl`보다 `ParseMemberDecl` 쪽이 현재 범위에 더 잘 맞는다.
- 이유는 같은 parser가 class/struct 내부뿐 아니라 namespace, script root에서도 재사용되기 때문이다.
