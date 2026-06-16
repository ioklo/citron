# 회의 / 설계 노트

Date: 2026-06-16
Title: `some` opaque result와 cti 경계 정리

Status
- draft

Summary
- Citron의 `some Trait`는 Swift식 opaque result에 가깝게 본다.
- `some Trait`는 일반 type expression이 아니라 함수 return position 전용 표기다.
- `some Trait` 결과는 호출자가 concrete backing type을 알지 못하며, 항상 `var`로만 받는다.
- 호출자는 함수별 opaque result metadata accessor를 통해 크기/정렬/값 연산 정보를 얻고, 그 크기만큼 local storage를 할당한 뒤 sret 방식으로 호출한다.
- `some Trait` 값은 source-level에서 concrete type 기능을 사용할 수 없고, 선언된 `Trait` surface만 사용할 수 있다.
- 이 모델을 따르면 cross-unit consumer가 concrete backing type/layout을 알 필요가 없으므로, consumer-facing `rcti`는 없어도 될 가능성이 크다.

Context
- 이전 논의에서는 `some` return의 concrete backing type을 body compile 이후 `rcti`에 기록하고, consumer가 그 resolved artifact를 읽는 모델을 검토했다.
- 이 경우 unit 간 dependency ordering, same-module cycle, public raw `some` export, incremental rebuild 전파가 복잡해졌다.
- Swift의 `some P`는 opaque result type이며, caller가 backing concrete type을 직접 알기보다 opaque result metadata/value witness를 통해 값을 다룰 수 있다.
- Citron도 이 방향을 따르면 `some`을 public API로 노출할 수 있으면서, source-level로 concrete type을 숨기는 의미를 더 강하게 유지할 수 있다.

Decisions / Current Preferences
## 1) `some Trait`는 return position 전용 opaque result marker다
- `some Trait`는 일반 type expression이 아니다.
- 함수 return type을 표현할 때만 쓴다.

허용:
```citron
some MyTrait F()
{
    return S();
}
```

금지:
```citron
some MyTrait x;             // error
void G(some MyTrait x);     // error
List<some MyTrait> values;  // error
```

## 2) 호출자는 `some` 결과를 항상 `var`로 받는다
- source code에서 opaque result type 이름을 직접 쓸 수 없다.
- 호출자는 결과를 `var`로 받으며, 이 값의 source-level 사용 가능 surface는 declared trait로 제한된다.

예:
```citron
var x = F();
x.TraitMethod();      // ok
x.ConcreteMethod();   // error
```

## 3) `some Trait` 값은 concrete backing type을 source-level로 노출하지 않는다
- `some MyTrait F()`의 backing type이 `S`여도 consumer는 `S`를 알 수 없다.
- compiler 내부에서 backing type을 알게 되더라도 source-level lookup은 `MyTrait` surface로 제한한다.
- 서로 다른 `some MyTrait` function은 같은 backing type을 쓰더라도 서로 다른 opaque result identity를 가진다.

## 4) `some` result call은 opaque metadata + sret로 낮춘다
`some MyTrait F()` 호출은 개념적으로 다음 순서로 처리한다.

```text
meta = F.$opaqueResultMetadata()
vw = meta.valueWitness

storage = stack_alloc(vw.size, vw.align)
F(returnDest: storage)

... use storage through MyTrait witness ...

vw.destroy(storage)
```

- `F.$opaqueResultMetadata()`는 함수별 opaque result metadata accessor다.
- metadata는 value witness table을 통해 size, alignment, copy/move/destroy 등 기본 value operation을 제공한다.
- 함수 호출은 caller-provided storage에 결과를 construct하는 sret 방식으로 본다.
- local scope를 벗어나면 value witness의 destroy를 호출하고, local storage lifetime은 끝난다.

## 5) value witness와 trait witness를 구분한다
- value witness는 타입 값을 저장/복사/이동/파괴하기 위한 기본 연산 테이블이다.
- trait witness는 `Trait` requirement를 실제 backing type 구현으로 연결하는 테이블이다.

개념:
```text
ValueWitness
  size
  align
  copy(dest, src)
  move(dest, src)
  destroy(value)

TraitWitness<MyTrait>
  TraitMethod(self, ...)
```

`some MyTrait` 값을 사용할 때 compiler는:
- local storage와 lifetime 관리를 위해 value witness를 사용한다.
- `MyTrait` method 호출을 위해 trait witness를 사용한다.

## 6) `cti`는 opaque result ABI contract를 담는다
`cti`에는 concrete backing type을 직접 기록하지 않는다.

`cti`에 필요한 정보 후보:
- function surface signature
- return이 `some Trait`라는 사실
- opaque result identity
- opaque result metadata accessor symbol
- sret calling convention / opaque return ABI marker
- declared trait constraint와 trait witness를 얻는 규칙

예시 개념:
```text
F : () -> opaque F.result : MyTrait
F.result.metadata_accessor = F.$opaqueResultMetadata
F.calling_convention = opaque_sret
```

## 7) consumer-facing `rcti`는 없앨 수 있다
- 이전 `rcti`는 consumer가 `some` backing concrete type/layout을 알아야 한다는 전제에서 필요했다.
- opaque metadata 방식에서는 consumer가 backing type을 알 필요가 없다.
- 따라서 외부 consumer가 읽어야 하는 별도 `rcti` artifact는 없어도 될 가능성이 크다.
- backing type과 metadata accessor 구현은 provider의 body/object artifact 내부 구현 정보로 남긴다.

가능한 산출물 역할:
```text
cti
  declaration/import/typecheck/codegen boundary
  opaque result identity와 metadata accessor contract 포함

ct.o
  function body
  opaque result metadata accessor implementation
  value witness / trait witness implementation 또는 reference
```

단, compiler 내부 incremental cache로 resolved body info를 저장할 수는 있다. 이 경우 공식 import artifact인 `rcti`라기보다 body cache / impl cache에 가깝다.

## 8) public raw `some` return은 허용 가능하다
- consumer가 concrete backing type을 요구하지 않으므로, public function이 `some Trait`를 직접 return하는 것을 금지할 필요가 줄어든다.
- public API surface는 concrete type이 아니라 opaque result identity와 trait constraint다.
- backing type이 바뀌어도 source-level API는 `F.result : Trait` 형태로 유지될 수 있다.

Rationale
- `some`의 목적은 concrete type을 숨기되, 구현이 고른 하나의 concrete result type으로 고정하는 것이다.
- consumer에게 backing type을 알려주는 방식은 구현은 단순하지만, hidden type이 사실상 downstream compiler artifact에 새어 나간다.
- opaque metadata 방식은 구현 부담이 있지만, `some`의 source-level 의미와 module boundary abstraction을 더 잘 보존한다.
- Citron의 NBC/destination-passing 모델과 sret 기반 opaque result 호출은 방향이 잘 맞는다.
- `some Trait`를 일반 type expression으로 열지 않으면 사용자 모델과 parser/type system 범위를 좁게 유지할 수 있다.

Open Points
- dynamic stack allocation을 MIR/QIR에서 어떤 node로 표현할지
- value witness table의 최소 필드와 calling convention
- trait witness table lookup을 metadata에 붙일지, opaque result identity에서 별도 accessor로 둘지
- `some` result 값의 copy 가능 여부를 trait/ownership 규칙과 어떻게 연결할지
- `some func<R, Params...>`에서 callable trait witness shape를 어떻게 표현할지
- public `some` backing type 변경 시 incremental rebuild와 ABI compatibility를 어떻게 다룰지
- `some Trait`가 여러 trait bound를 받을 수 있을지
  - 예: `some P + Q` 같은 surface를 둘지
- `seq` / generator result를 `some ValueEnumerable<T>` 계열로 볼지, 별도 `seq T` surface로 둘지

Action Items
- [ ] opaque result identity를 symbol model에 어떻게 둘지 초안 작성
- [ ] `cti`에 기록할 opaque result ABI contract 필드 목록 작성
- [ ] value witness table 최소 설계 작성
- [ ] MIR/QIR에서 unknown-size local storage와 opaque sret call 표현 후보 작성
- [ ] `some func<>` 호출/Invoke witness shape 검토
