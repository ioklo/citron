# 회의 / 설계 노트

Date: 2026-06-21
Title: `some Trait`의 내부 type 모델과 `RFuncReturn` 정리 방향

Summary
- `RFuncReturn_NotSet`은 주로 람다 리턴 추론 때문에 들어간 상태로 보이며, `R*` 외부 인터페이스 계층에는 어울리지 않는다.
- `some Trait`는 `RFuncReturn`의 별도 variant로 두기보다, 내부적으로 `RType_Opaque`로 인코딩하는 방향을 검토했다.
- 이 경우 syntax에서 `some`을 일반 type expression으로 승격할 필요는 없고, 계속 함수 declaration의 return 자리에서만 읽는 쪽을 유지한다.

Context
- trait / extend / some return 구현을 진행하면서 `BuildTypeDependentSymbolContext::MakeFuncReturn`에서 `SFuncReturn_Opaque`를 어떤 `R*` 모델로 바꿀지 결정할 필요가 생겼다.
- 동시에 기존 `RFuncReturn_Set / NotSet / ForCtor` 구성은 의미 축이 섞여 있다는 점이 드러났다.

## 1) `RFuncReturn_NotSet`는 translator 쪽 open state로 내리는 것이 자연스럽다

현재 `RFuncReturn_NotSet`의 실제 용도는 mostly 람다 리턴 추론이다.

즉:
- body 분석 중에는 아직 리턴 타입이 확정되지 않았고
- `return expr`를 만나면 타입을 굳히고
- 끝까지 `return`이 없으면 `void`로 본다

이 상태는 declaration/import boundary보다는 body translation 중간 상태에 가깝다.

따라서 장기 방향은:
- `RFuncReturn_NotSet`를 `RFuncReturn`에서 제거
- open return 추론 상태는 `FuncContext_Lambda` 또는 translator 전용 상태로 이동

## 2) `some Trait`를 `RFuncReturn_Opaque`로 둘지, `RType_Opaque`로 둘지 비교

처음에는 아래 후보를 생각했다.

```text
RFuncReturn =
  Normal(type)
  Opaque(trait)
  CtorNone
```

하지만 이후 논의에서, `some Trait`는 함수 return category 자체라기보다
"opaque value type that is only allowed in return position at surface syntax"로 보는 편이 더 일관적일 수 있다고 보았다.

따라서 현재 더 선호하는 방향은:

```text
RFuncReturn =
  Normal(type)
  CtorNone

RType_Opaque =
  traitDecl
  traitTypeArgs
```

즉 `some MyTrait<T>` 함수는 내부적으로:

```text
RFuncReturn_Normal { type = RType_Opaque(MyTrait, [T]) }
```

처럼 표현할 수 있다.

## 3) `RType_Trait`를 따로 두지 않는 안

이번 논의에서는 `RType_Trait`와 `RType_Opaque`를 둘 다 두는 안보다,
`trait`는 일반 값 타입으로 `RType` 계층에 올리지 않고,
`some Trait`일 때만 `RType_Opaque`를 만드는 쪽이 더 깔끔하다는 쪽으로 기울었다.

즉:
- `trait` 자체는 constraint / conformance 대상
- 값처럼 다뤄지는 내부 type 계층에는 직접 올리지 않음
- `some Trait`만 `RType_Opaque`를 통해 값 타입처럼 표현

이렇게 하면:
- `trait`가 variable type, parameter type처럼 보이는 문제를 피할 수 있다
- `some Trait`의 특수성이 더 분명해진다

후보 shape:

```cpp
class RType_Opaque : public RType
{
    RTraitDecl* traitDecl;
    RTypeArguments* traitTypeArgs;
};
```

## 4) `MakeFuncReturn`에서는 trait 여부를 확인한다

syntax의 `SFuncReturn_Opaque`는 parser 단계에서는 모양만 읽고,
semantic 단계에서 실제로 trait를 가리키는지 확인하는 편이 맞다.

즉 `BuildTypeDependentSymbolContext::MakeFuncReturn`에서는:
- syntax의 trait name/type args를 해석하고
- 실제로 trait declaration인지 확인한 뒤
- `RType_Opaque` 또는 최종 `RFuncReturn`을 만든다

중요한 점:
- parser가 trait 여부를 확인하는 것은 아님
- parser는 `some`을 return position marker로만 읽는다
- trait 여부 확인은 body/symbol building 단계 책임이다

## 5) surface syntax는 그대로 둔다

내부에서 `RType_Opaque`로 인코딩하더라도, surface syntax에서 `some`을 일반 type expression으로 열 필요는 없다.

즉 계속:
- `some T`는 함수 declaration의 return 자리에서만 허용
- 변수 / 인자 / field / generic argument 위치에서는 금지

로 유지한다.

이 경우 대응은 대략 이렇게 된다.

```text
surface syntax: some MyTrait F()
parser: SFuncReturn_Opaque(MyTrait)
semantic: RFuncReturn_Normal(RType_Opaque(MyTrait))
```

이 구조는 surface rule과 internal representation을 분리하면서도, 사용자 모델은 좁게 유지할 수 있다.
