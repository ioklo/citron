# Some Opaque Result

Status: draft current
Area: language, abi, cti
Keywords: some, opaque result, value witness, trait witness, sret, cti

## Current Rules
- `some Trait`는 함수 return position 전용 opaque result marker다.
- `some Trait`는 일반 type expression이 아니다.
- 표면 표기는 `some T`를 사용하고, `some<T>`는 허용하지 않는다.
- parser에서도 `some`은 일반 type parser가 아니라 함수 declaration의 return 자리에서만 특별히 읽는 쪽을 선호한다.
- 변수, 인자, field, generic argument 위치에 `some Trait`를 쓰지 않는다.
- 호출자는 `some` 결과를 항상 `var`로 받는다.
- source-level에서는 declared `Trait` surface만 사용할 수 있고, backing concrete type의 member는 사용할 수 없다.
- public function이 raw `some Trait`를 return하는 것은 허용 가능하다.

허용:
```citron
some MyTrait F()
{
    return S();
}

var x = F();
x.TraitMethod();
```

금지:
```citron
some MyTrait x;
void G(some MyTrait x);
List<some MyTrait> values;
some<MyTrait> H();
x.ConcreteOnlyMethod();
```

## Meaning
`some Trait`는 "callee가 고른 하나의 concrete result type을 숨긴다"는 뜻이다.

```text
generic return:
  caller chooses T

some return:
  callee chooses one hidden T per declaration

interface/any:
  runtime value may contain different concrete types
```

서로 다른 `some MyTrait` 함수는 backing type이 같더라도 서로 다른 opaque result identity를 가진다.

`some`은 `nullable<T>` / `ptr<T>` 같은 일반 prefix type constructor가 아니라, return position에서만 쓰는 특수 marker다.

## Compiler Model
`some MyTrait F()` 호출은 개념적으로 아래처럼 낮춘다.

```text
meta = F.$opaqueResultMetadata()
vw = meta.valueWitness

storage = stack_alloc(vw.size, vw.align)
F(returnDest: storage)

traitWitness = meta.traitWitness(MyTrait)
traitWitness.TraitMethod(storage, ...)

vw.destroy(storage)
```

- metadata accessor는 함수별 opaque result metadata를 돌려준다.
- value witness는 size/align/copy/move/destroy를 제공한다.
- trait witness는 `Trait` requirement 호출을 backing type 구현으로 연결한다.
- return은 caller-provided storage에 construct하는 opaque sret 방식으로 본다.

## CTI Boundary
`cti`에는 concrete backing type을 직접 기록하지 않는다.

`cti`에 필요한 정보 후보:
- function surface signature
- return이 `some Trait`라는 사실
- opaque result identity
- opaque result metadata accessor symbol
- opaque sret calling convention marker
- declared trait constraint와 trait witness lookup 규칙

개념:
```text
F : () -> opaque F.result : MyTrait
F.result.metadata_accessor = F.$opaqueResultMetadata
F.calling_convention = opaque_sret
```

이 모델에서는 consumer가 backing type/layout을 알 필요가 없으므로 consumer-facing `rcti`는 없어질 수 있다. body compile 결과 캐시는 둘 수 있지만 공식 import artifact라기보다 implementation cache다.

## Open Points
- unknown-size local storage를 MIR/QIR에서 어떻게 표현할지
- value witness table 최소 필드와 ABI
- trait witness lookup을 metadata에 붙일지 별도 accessor로 둘지
- `some` result의 copy 가능 여부와 ownership 규칙
- `some func<R, Params...>`의 callable witness shape
- backing type 변경 시 incremental rebuild와 ABI compatibility
- multiple trait bound syntax를 둘지

## History
- `ai/notes/2026-06-16-some-opaque-result-and-cti.md`
