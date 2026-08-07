# 회의 / 설계 노트

Date: 2026-08-07
Title: AppliedDecl의 tenv와 nested trait requirement 비교

## 결론

- AppliedDecl의 적용 인자와 적용 위치의 타입 변수 환경(`tenv`)은 구별한다.
  적용 인자는 formal type parameter가 어떤 type으로 치환됐는지를 나타내고,
  tenv는 그 위치에서 보이는 전체 type variable의 lexical 순서다. AppliedDecl의
  type argument나 declaration path에 실제로 나타나지 않는 type variable도 tenv에
  남는다.
- type argument는 concrete type일 필요가 없다. 열린 type variable 자체를
  argument로 적용할 수 있다. 따라서 모든 argument가 전달된 applied
  declaration도 여전히 open일 수 있다.
- nested trait requirement와 impl requirement는 각자의 tenv 위치로 type
  variable을 정규화해 비교한다. 단, 두 generic signature의 binder 위치가
  대응된다는 것이 먼저 확정되어야 한다. source name만 다른 bound type
  parameter를 같은 것으로 보는 이 비교를 alpha-equivalence로 부른다.

## 예시

```citron
struct X<T1>
{
    trait Tr<T2>
    {
        T2 F<T3>(T3 t3);
    }

    struct S<T4> : Tr<list<T4>> { }

    impl S<T5> : Tr<list<T5>>
    {
        list<T5> F<T6>(T6 t6) { ... }
    }
}
```

`Tr<list<T5>>`의 complete application은 outer `X<T1>`까지 포함해 다음과
같다.

```text
tenv [T1, T5] => X<T1>.Tr<list<T5>>
```

여기서 trait declaration의 `F`를 얻고, trait까지 적용된 arguments와 함수의
열린 type parameter를 합친다.

```text
F formal parameters: [T1, T2, T3]
F applied arguments: [T1, list<T5>, T3]

tenv [T1, T5, T3] => X<T1>.Tr<list<T5>>.F<T3>
```

impl 함수도 자신의 열린 type parameter를 적용해 비교용 AppliedDecl로
표현한다.

```text
tenv [T1, T5, T6] => X<T1>.impl_<T5>.F<T6>
```

반환형과 인자형은 type만 떼어 비교하지 않고 tenv와 함께 비교한다.

```text
tenv [T1, T5, T3] => list<T5>  -> list<$1>
tenv [T1, T5, T6] => list<T5>  -> list<$1>

tenv [T1, T5, T3] => T3        -> $2
tenv [T1, T5, T6] => T6        -> $2
```

따라서 반환형과 첫 인자형은 일치한다. 실제 requirement match에서는 이 밖에도
requirement identity, 함수 type parameter 개수/제약, 인자 개수와 전달 방식을
확인한다.

## 자료구조 방향

generic analysis에서 tenv는 항상 이용 가능해야 한다. 단, `RAppliedDecl`이
tenv를 직접 보유할지는 미확정이다. tenv 배열을 값으로 복사해 넣으면
AppliedDecl이 무거워질 수 있으므로, 빈 environment singleton 및 persistent
outer link를 갖는 immutable/shared `RTypeEnv`를 둔다. `RAppliedDecl`에 handle
또는 pointer 하나를 저장하는 방안과 decl-space/body-space가 공유 환경을
제공하는 방안을 비교한다.

```text
env0 = []
env1 = env0 + T1
env2 = env1 + T5
env3 = env2 + T3
env4 = env2 + T6
```

이 경우 requirement 함수는 `env3`, impl 함수는 `env4`를 가리키며, 공통
prefix는 공유한다.

닫힌 application도 tenv를 가진다. 예를 들어 다음의 `List<int>`는 concrete type
arguments만 가지지만 적용 위치가 nested generic scope 안이므로 tenv는 비어 있지
않다.

```citron
struct S<X>
{
    struct T<Y>
    {
        void F() { var x = new List<int>(); }
    }
}
```

```text
tenv [X, Y] => List<int>
```

이 타입은 정규화할 type variable을 포함하지 않으므로 정규화 후에도 `List<int>`다.
tenv는 이 application이 만들어진 generic lexical context를 보존한다.

## 비교 규칙

exact application equality와 alpha-equivalence를 구분한다.

```text
[A.X] => List<A.X>
[F.Y] => List<F.Y>
```

는 applied type argument의 origin이 다르므로 exact equality에서는 다르다.
다만 두 binder 위치를 대응시켜 제네릭 형태를 비교하는 경우에는 각각
`List<$0>`이 되어 alpha-equivalent하다. 반대로 서로 다른 body에서 얻은
`List<int>` 두 개는 tenv가 달라도 exact application equality에서는 같다.

tenv origin까지 같은지를 확인하는 일반 `IsSameContext` API는 현재 필요성이
낮다. 현재 문맥에서 type/AppliedDecl을 사용할 수 있는지는 equality가 아니라
별도 validity 검사로 둔다.

## 미확정 사항

- `RTypeEnv`의 owner와 lifetime
- environment interning 여부
- exact equality/hash는 declaration/type argument origin만 비교하고,
  alpha-equivalence는 binder correspondence를 받는 별도 API로 두는 구체적 shape
- tenv를 AppliedDecl의 저장 필드로 둘지, decl-space/body-space가 제공할지의
  최종 선택
