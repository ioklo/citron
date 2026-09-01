# Visibility And Reachability

Status: current rules; implementation pending
Area: language, name lookup, module
Keywords: visibility, reachability, private, protected, public, alias, contract, cti

## Current Direction
- module import는 provider의 모든 declaration name과 typecheck에 필요한 semantic metadata를 lookup surface에 올린다.
- accessibility는 name lookup filter가 아니라 resolution 뒤의 use-permission check다. inaccessible declaration도 lexical shadowing에 참여하며, access failure 때문에 outer candidate로 fallback하지 않는다.
- `public`/`private`/`protected`는 source context의 accessibility를 결정한다. declaration metadata reachability는 public API/ABI contract 또는 linker export와 별개다.
- 선언 D의 외부 계약에 노출되는 타입·symbol S는 `Access(D) ⊆ Access(S)`를 만족해야 한다. D를 사용할 수 있는 모든 문맥에서 S도 접근 가능해야 한다.
- 이름을 쓰는 선언 문맥에서 접근 가능한지와, 그 이름을 선언의 외부 계약에 노출할 수 있는지는 별도로 검사한다.
- public signature에 private 타입 또는 private type alias를 쓰는 것과 public alias로 private target을 노출하는 것은 금지한다. `var`로 받는다고 잘못된 signature가 허용되지 않는다.
- 이 규칙은 2026-08-31의 앞선 C++식 name-use/public 노출 허용 결정을 대체한다. declaration metadata의 reachability와 native export 정책을 변경하는 결정은 아니다.

## External Contract

- 함수의 반환·매개변수 타입, field/property 타입, base type, 타입 header의 trait, generic type arguments·constraints, alias target은 외부 계약에 포함한다.
- 타입 표현식의 구성 요소도 검사한다. `List<Hidden>`에서 `List`가 public이어도 `Hidden`이 private이면 public signature로 노출할 수 없다. pointer/nullable 등의 wrapper로 감싸도 target의 노출이 없어지지 않는다.
- 공개 conformance를 통해 알 수 있는 associated type witness도 검사한다. 자체 `public` 표기의 유무만으로 판정하지 않는다.
- generic type parameter는 선언의 binder다. 각 binder에 별도의 public modifier를 요구하지 않으며, 외부 계약에 등장하는 constraint와 실제 type arguments의 접근성을 검사한다.
- 함수 body에서 쓰는 타입과 public 타입의 private 구현 멤버는 외부 계약이 아니다. 반환 타입의 모든 field/body를 따라가며 public을 요구하지 않는다.
- `some Trait`에서는 공개된 trait 계약과 숨겨진 backing type을 구분한다. backing type은 private일 수 있다.

## Accessibility By Outer Kind

Accessibility는 declaration의 immediate outer 종류에 따라 해석한다.

| outer | 허용 accessibility |
|---|---|
| namespace/module | `public`, `private` |
| class | `public`, `protected`, `private` |
| struct | `public`, `private` |

- Struct는 concrete struct를 상속하지 않으므로 struct member에 `protected`를 허용하지 않는다.
- Nested type은 자신의 종류와 무관하게 containing outer의 member accessibility 규칙을 따른다.
- `protected`는 symbol containment뿐 아니라 class inheritance와 receiver type을 함께 검사해야 한다.
- `Access(D)`에는 enclosing declaration의 접근 제한도 반영한다. private outer 안의 public member를 무조건 프로그램 전체에 공개된 선언으로 취급하지 않는다.
- 서로 다른 owner에 속한 `protected`는 같은 modifier라는 이유만으로 호환되지 않는다. 선언 위치에서 target에 접근할 수 있는 것만으로는 외부 계약 검사가 끝나지 않는다.
- qualified name의 enclosing declaration과 member 이름, 값에서 지칭하는 member의 접근성은 계속 검사한다. alias normalization으로 사용한 private alias 이름의 외부 계약 검사를 생략하지 않는다.

외부 `extension` implementation은 target의 private member에 접근할 수 있는 trusted augmentation이다. ordinary consumer와 extension compiler 모두 private declaration을 lookup candidate로 얻을 수 있지만, extension context의 access policy만 target private member 사용을 허용한다.

## Protected Owner Example

아래는 nested class가 outer의 protected 이름에 접근할 수 있는 경우의 설계 예다.

```citron
public class Outer
{
    protected class Token {}

    public class Inner
    {
        protected Token Make(); // error: 접근 범위 포함 관계 위반
    }
}

public class Client : Outer.Inner
{
    // Inner의 파생 클래스이므로 Make에는 접근할 수 있지만,
    // Outer의 파생 클래스가 아니므로 Outer.Token에는 접근할 수 없다.
}
```

`Access(Make)`가 `Access(Token)`에 포함되지 않으므로 Make 선언을 거부한다. `Token`과 `Make`가 모두 protected이고 Make 선언 위치에서 Token을 볼 수 있다는 사실만으로 허용하지 않는다.

## Type Aliases And Public Signatures

```citron
public class Api
{
    private class Hidden {}
    private type Count = int;

    public Hidden Make();        // error: private 타입을 반환 계약으로 노출
    public type Result = Hidden; // error: public alias의 private target
    public Count GetCount();     // error: target이 int여도 private alias 사용 금지
    public int GetCountDirect(); // ok
}
```

signature와 alias target의 의미 타입뿐 아니라, 외부 계약에 사용한 정식 type alias 자체의 접근성도 검사한다. unit-local `using`은 정식 type declaration이 아니므로 구분한다. 자세한 alias 규칙은 `type-aliases.md`를 본다.

## Conformance And Extensions

- public struct의 header에 private trait를 직접 넣지 않는다. 내부용 conformance는 해당 trait의 접근 범위를 넘지 않는 extension bundle로 분리한다.
- bundle의 target/trait와 type arguments에도 외부 계약 검사를 적용한다. activation은 접근성을 높여 주지 않는다.
- 공개 conformance의 associated type을 private 타입으로 연결하는 것은 거부한다. `impl`이나 witness에 public modifier가 없어도 conformance를 통한 노출을 검사한다.
- 현재 extension 함수는 trait requirement를 구현하며 임의의 public signature를 추가하는 모델이 아니다. trait/header 접근성, conformance의 trait 인자·associated type 노출, impl signature와 requirement의 일치 검사로 계약을 보장한다.
- trusted extension의 private 접근 권한은 구현 내부 사용 권한이다. signature 일치와 conformance 검사를 건너뛰고 private 타입을 공개할 수 있는 권한이 아니다. future helper/member 확장은 현재 검사의 전제로 삼지 않는다.

## Opaque And Inferred Return

`public some PublicTrait Make()`는 private backing type을 선택할 수 있다. 소비자는 declared trait surface만 사용하며, private concrete type이 반환 signature에 직접 나타나는 경우와 다르다.

일반 return type inference는 계속 별도 open point다. 도입하더라도 `var`나 inferred return을 private concrete type의 public 계약 노출을 허용하는 예외로 취급하지 않는다. 향후 opaque 계약에 concrete associated type equality를 공개하는 문법을 추가한다면, 공개되는 타입에 같은 접근성 원칙을 적용해야 한다.

## Destructor Accessibility
- 소멸자에는 접근 지정자를 두지 않으며 항상 public으로 취급한다. 암시적 cleanup과 value witness destroy에도 소멸자 접근성 검사를 하지 않는다. ownership/lifetime 검사는 이 결정과 별개다.

## Native Export Boundary
- CTI에 private declaration name과 semantic metadata를 제공하는 것은 각 function을 PE export table에 등록한다는 뜻이 아니다.
- 외부 trusted extension의 private member 호출과 opaque result의 metadata/witness 경로 등에는 여전히 유효한 ABI 연결이 필요하다. public signature 접근성 제약만으로 native export 대상을 결정할 수 없다.
- DLL export 한도 처리와 native binding 방식은 아직 미확정이다.

## Open Points
- enclosing scope와 protected owner를 포함하는 접근 범위 비교 API 및 진단 경로
- alias normalization 전 검사 또는 사용한 alias provenance 유지 방식
- 선언 접근성 검사를 수행할 정확한 phase와 semantic readiness
- private declaration metadata의 ABI compatibility/versioning policy
- Inferred public signature를 `cti`에 어떻게 표현할지

## History
- `ai/notes/2026-08-31-declaration-contract-accessibility.md` (현재 규칙; 같은 날 앞선 name-use 결정 대체)
- `ai/notes/2026-05-22-nested-decl-visibility-and-resolved-cti.md`
- `ai/notes/2026-05-12-module-visibility-and-internal-fdecl-direction.md`
- `ai/notes/2026-06-29-accessibility-struct-trait-extension-direction.md`
