# 회의 / 설계 노트

Date: 2026-06-29
Title: accessibility, struct 상속, trait 구현과 외부 extension 방향

## Summary

- namespace 수준의 `public` / `private`는 외부 접근 가능 여부를 결정하며, 접근 가능한 선언은 module surface에 export되는 것으로 본다. 별도의 export modifier는 두지 않는다.
- class member는 `public` / `protected` / `private`, struct member는 `public` / `private`를 사용한다.
- struct는 다른 struct를 상속하지 않는다. 따라서 struct에는 `protected`를 두지 않는다.
- struct 선언의 trait 목록은 conformance 선언이며, 실제 witness 구현은 별도의 `impl` 블록에 둔다.
- 외부 module도 기존 타입에 trait conformance를 추가할 수 있다. 외부 conformance는 이름 있는 `extension` bundle로 선언하고, 소비 파일에서 `extend` 지시문으로 명시적으로 활성화한다.

## 1. Accessibility와 export

처음에는 symbol tree accessor와 class/struct member accessor를 분리할지 검토했다.

- namespace member의 `public` / `private`는 외부 module에서 접근 가능한지를 결정한다.
- 별도의 export 의미를 추가하지 않는다. 접근 규칙상 외부에서 접근 가능한 declaration이 곧 export surface다.
- class/struct member accessibility는 immediate `outer`의 종류에 따라 의미가 달라진다.
- nested type은 타입이면서 enclosing type의 member다. nested type 자체에 accessor를 두 개 저장하지 않고, `outer -> child` 관계의 member accessibility 하나를 적용한다.

현재 방향:

| 위치 | 허용 accessor |
|---|---|
| namespace/module member | `public`, `private` |
| class member | `public`, `protected`, `private` |
| struct member | `public`, `private` |

`protected`는 containment tree만으로 판정할 수 없고 declaring class, derived class, receiver type을 함께 봐야 한다. 따라서 장기적으로 accessibility check는 단일 tree-only `RDecl::CanAccess`보다 별도 policy/checker가 담당하는 편이 자연스럽다.

## 2. Struct 상속 재검토

현재 구현에는 struct가 base struct 하나와 여러 interface를 갖는 모델이 남아 있다. base struct의 field를 derived struct의 memberwise constructor에 펼치는 코드도 있다.

검토 결과 concrete struct 상속은 다음 문제를 추가한다.

- value slicing과 base conversion
- field layout 및 ABI 결합
- base constructor/destructor 규칙
- override와 member lookup
- struct의 `protected` 의미

field 재사용만을 위한 `embed` / `include`도 검토했지만 member promotion, 이름 충돌, ABI 결합 등의 규칙이 추가되므로 보류한다.

현재 결정:

- struct는 concrete struct를 상속하지 않는다.
- struct의 `:` 뒤에는 trait conformance declaration만 올 수 있다.
- struct member에는 `protected`를 허용하지 않는다.
- storage reuse는 당분간 일반 named-field composition으로 해결한다.
- 기존 struct base list에서 interface가 맡던 static contract 역할은 trait conformance로 옮긴다.

## 3. 원본 module의 trait conformance

타입 선언부만 보아도 구현 trait 목록을 알 수 있게 conformance를 타입에 선언한다.

```citron
public struct S : Trait1, Trait2
{
}

impl S : Trait1
{
    // Trait1 witness implementation
}

impl S : Trait2
{
    // Trait2 witness implementation
}
```

규칙:

- `struct S : Trait1, Trait2`는 S의 원본 module이 제공하는 canonical conformance declaration이다.
- 각 `(S, Trait)`에는 같은 module 안에 정확히 하나의 `impl S : Trait { ... }`가 있어야 한다.
- 원본 module이 선언한 canonical conformance와 같은 `(S, Trait)`를 외부 extension이 다시 선언할 수 없다.

## 4. 외부 module의 이름 있는 extension

외부 module도 자신이 소유하지 않은 타입에 trait conformance를 제공할 수 있어야 한다. 발견성과 충돌 관리를 위해 이름 있는 bundle을 사용한다.

```citron
public extension MyBundle for S : Trait3, Trait4;

impl MyBundle for S : Trait3
{
    // Trait3 witness implementation
}

impl MyBundle for S : Trait4
{
    // Trait4 witness implementation
}
```

규칙:

- `extension`은 이름 있는 외부 conformance bundle declaration이다.
- 한 module은 같은 target에 여러 bundle을 선언할 수 있다.
- accessor는 `extension` declaration에만 붙인다. `impl`에는 붙이지 않는다.
- 각 manifest 항목에는 같은 module에 정확히 하나의 matching `impl`이 필요하다.
- `impl Bundle for S : Trait`의 target과 trait는 bundle declaration의 항목과 semantic identity가 일치해야 한다.
- declaration과 implementation은 lexical scope 어디에나 둘 수 있지만, 중복 검사는 module 전체에서 수행한다.

외부 extension은 trusted augmentation으로 본다.

- external `impl Bundle for S : Trait`는 S의 private member에 접근할 수 있다.
- 이를 위해 S의 provider artifact는 일반 name lookup에는 노출하지 않는 private semantic/ABI 정보를 extension compilation에 제공할 수 있어야 한다.
- 이 정책은 캡슐화 및 ABI 결합 비용이 있으므로 artifact shape와 compatibility 진단은 후속 설계가 필요하다.

## 5. 소비 파일의 extension 활성화

외부 extension은 module import만으로 자동 활성화하지 않는다. 소비 source file이 필요한 bundle 항목을 명시한다.

```citron
import ExtModule;

extend MyBundle for S : Trait3;
extend MyBundle for S : Trait3, Trait4;
```

규칙:

- `extend`는 외부 extension bundle을 활성화하는 file-level directive다.
- bundle provider module은 소비 module의 직접 dependency여야 한다.
- v1에서는 `for S`와 trait 목록을 모두 필수로 쓴다.
- target 또는 trait가 bundle declaration과 일치하지 않으면 오류다.
- directive에 나열한 trait만 해당 파일에서 활성화된다.
- 동일한 concrete `(type, trait)`에 둘 이상의 활성 bundle이 매칭되면 실제 conformance가 필요한 사용 지점에서 ambiguity error를 낸다.
- target/trait 생략형과 bundle 전체 wildcard 활성화는 후속 버전으로 미룬다.

## 6. Import, using alias와 type alias

외부 module의 declaration world는 `import`로 현재 unit에 연다.

```citron
import A;
import A as MyA;
```

규칙:

- `import A;`는 A의 declaration world를 현재 unit에 연다.
- `import A as MyA;`는 같은 동작을 수행하면서 unit-local module alias `MyA`를 추가한다.
- `import`와 `using`은 선언 unit 밖으로 export되지 않는다.
- `import unit` / `using unit`은 두지 않는다. 같은 module의 모든 unit declaration을 body보다 먼저 자동 수집해 forward reference를 해결한다.
- module qualification은 `import`를 대체하지 않는다. `A.Name` 또는 `global::A.Name`을 쓰더라도 먼저 A를 import해야 한다.
- `A.Name`은 lexical lookup의 영향을 받을 수 있고, `global::A.Name`은 root module name을 명시한다.

기본적으로 imported declaration은 unqualified lookup한다.

```citron
import ExtModule;
extend MyBundle for S : Trait3;
```

이름 충돌이 있으면 alias 또는 explicit qualification을 사용한다.

```citron
import ExtModule as Ext;
extend Ext.MyBundle for S : Trait3;
```

`using`은 unit-local convenience alias에 사용한다.

```citron
using Items = Dictionary<string, List<int>>;
```

- `using` alias는 symbol tree의 정식 declaration 또는 module export surface가 아니다.
- accessibility modifier를 붙이지 않고 선언 unit 밖에서 접근할 수 없다.

`type`은 type-decl-space에 들어가는 정식 transparent type alias declaration이다.

```citron
public type UserId = int;

class C
{
    public type V = int;
}
```

- `type` declaration은 namespace/class/struct member가 될 수 있다.
- enclosing declaration 및 자신의 accessibility에 따라 다른 unit/module에서 접근할 수 있다.
- trait associated type requirement와 witness도 `type Item;` / `type Item = int;` 형태를 사용한다.

## 7. Keyword 역할

최종적으로 세 keyword의 역할을 분리한다.

| keyword | 역할 |
|---|---|
| `extension` | 이름 있는 외부 conformance bundle 선언 |
| `impl` | canonical 또는 extension conformance의 witness 구현 |
| `extend` | 소비 파일에서 외부 extension 활성화 |

## Open Points

- generic family extension 및 specialization의 정확한 surface syntax와 overlap 판정
- class가 base class와 trait conformance를 함께 선언하는 문법
- private extension surface의 artifact format, ABI compatibility와 versioning
- `extend` directive가 참조한 provider dependency를 incremental build metadata에 기록하는 방법
- target/trait 생략형 또는 wildcard activation을 후속 버전에 추가할지
- generic type alias surface와 alias cycle 진단 규칙
