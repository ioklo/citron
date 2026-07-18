# Module And CTI

Status: draft current
Area: language, build, import
Keywords: module, unit, cti, rcti, using, skeleton, fdecl

## Current Direction
- `cti`는 declaration/import boundary다.
- 사용자 기본 작성 모델은 `ct` 하나다.
- `cti/ctm` split은 기본 작성 모델로 강제하지 않고, build artifact 또는 외부 선언 전용 형태로 축소하는 방향이다.
- `some` opaque result metadata 모델을 택하면 consumer-facing `rcti`는 없어질 수 있다.
- 같은 module의 unit declaration은 body보다 먼저 자동 수집하며 `using unit`은 두지 않는다.

## Unit And Module
- `module`은 외부 import와 declaration world의 단위다. import consumer compiler는 module의 모든 declaration name/semantic metadata를 lookup candidate로 가질 수 있고, actual use는 accessibility checker가 판정한다.
- `unit`은 translation unit에 가까운 source 입력 단위다.
- 외부 module consumer는 provider module의 unit 구조를 직접 보지 않는 방향을 선호한다.

## Import Module

외부 module의 declaration world는 source unit에서 명시적으로 연다.

```citron
import A;
import A as MyA;
```

- `import A;`는 A의 declaration을 unqualified lookup 후보로 만든다. private declaration을 포함한 candidate의 실제 사용 가능 여부는 accessibility checker가 결정한다.
- `import A as MyA;`는 동일하게 declaration world를 열고 unit-local qualification alias를 추가한다.
- `import`와 `using` alias는 선언 unit 밖으로 export되지 않는다.
- `import unit` / `using unit`은 두지 않는다. 같은 module의 unit declaration은 body compilation 전에 자동으로 함께 수집한다.
- `A.Name`, `global::A.Name` 같은 qualification은 `import A;`를 대체하지 않는다.
- 이름 충돌 시 module alias qualification을 우선 사용하고, `global::A.Name`은 root module name을 직접 지정해야 할 때 사용한다.

## CTI
`cti`는 body보다 먼저 사용할 수 있는 declaration surface다.

`some` opaque result를 위해 `cti`는 아래 정보를 담을 수 있어야 한다.
- opaque result identity
- metadata accessor symbol
- opaque sret ABI marker
- declared trait constraint

Concrete backing type은 `cti`에 싣지 않는다.

## External Extension Surface

외부 module은 이름 있는 `extension` bundle로 foreign type의 trait conformance를 제공할 수 있다.

```citron
public extension MyBundle for S : Trait1, Trait2;
```

- extension bundle의 이름, target과 trait 목록은 provider module의 declaration metadata surface에 포함한다. bundle accessibility는 ordinary consumer가 이를 `extend`할 수 있는지 판정한다.
- module world를 여는 것만으로 bundle을 활성화하지 않는다. 소비 file이 `import Provider;` 뒤 `extend Bundle for S : Trait;`로 필요한 bundle 항목을 선택한다.
- 선택된 bundle/witness identity는 code generation과 dependency metadata에 명시적으로 남아야 한다.
- external extension implementation은 target private member에 접근할 수 있으므로, provider artifact는 private declaration을 포함한 semantic/ABI metadata를 제공할 수 있어야 한다. ordinary consumer는 이를 lookup candidate로 얻을 수 있지만 access policy 때문에 사용할 수 없다.

## RCTI Reconsidered
이전 모델에서는 `some` backing type/layout을 consumer가 알아야 해서 `rcti`를 검토했다.

현재 `some` 모델에서는 consumer가 metadata accessor/value witness를 통해 값을 다루므로, 외부 consumer가 읽는 `rcti`는 없어질 수 있다.

남을 수 있는 것은:
- provider 내부 body compile cache
- incremental build cache
- implementation-only resolved metadata

## Open Points
- `cti` format에 opaque result ABI contract를 어떻게 표현할지
- declaration-level dependency hash와 `.deps` artifact shape
- manual `cti`와 implicit `cti`의 검증 관계
- private extension surface의 artifact shape와 ABI compatibility policy
- generic extension specialization 및 overlap을 artifact에서 표현하는 방법

## History
- `ai/notes/2026-05-08-cti-import-and-static-interface-direction.md`
- `ai/notes/2026-05-12-module-visibility-and-internal-fdecl-direction.md`
- `ai/notes/2026-06-16-some-opaque-result-and-cti.md`
- `ai/notes/2026-06-29-accessibility-struct-trait-extension-direction.md`
