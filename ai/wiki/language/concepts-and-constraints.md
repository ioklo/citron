# Concepts And Constraints

Status: draft current
Area: language, type system
Keywords: concept, constraint, where, associated type

## Current Direction
- `concept`는 반복되는 `where` constraint 묶음으로 도입한다.
- 장기적으로 C++ concept처럼 더 넓은 predicate 체계로 확장할 여지를 둔다.
- 초기에는 type constraint를 이름 붙여 재사용하는 용도가 중심이다.
- Trait 자체에는 `where`를 두지 않고, associated type declaration의 `where`를 사용한다.

## Constraint Atom Candidates
초기 후보:
```citron
T : SomeTrait
T.Item == int
T.Item : class
T.Item : struct
T.Item : Base
```

`T.Item : Base`가 class inheritance, interface implementation, trait conformance 중 무엇을 우선 의미하는지는 open point다.

## Concept Examples
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

Concept는 associated type declaration의 `where`에서도 사용할 수 있다.

```citron
trait RefEnumerable
{
    type Item;
    type Enumerator : RefEnumerator where SameItem<This, Enumerator>;

    Enumerator GetEnumerator();
}
```

## Associated Type Constraint
Associated type 사이 관계는 qualified form을 선호한다.

```citron
trait RefEnumerable
{
    type Item;
    type Enumerator : RefEnumerator
        where This.Item == Enumerator.Item;

    Enumerator GetEnumerator();
}
```

## Open Points
- `concept` constraint atom을 어디까지 허용할지
- `T.Item : Base`의 의미 우선순위
- Concept와 direct constraint의 normalization/equality rule
- Concept가 future predicate system으로 확장될 때의 compatibility

## History
- `ai/notes/2026-05-15-trait-concept-associated-type-design.md`
