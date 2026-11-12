# Class·struct 생성, init, interface 및 수명 논의

Date: 2026-11-10
Status: 논의 기록; 사용자 결정과 미확정 후보를 구분

## 기록 범위와 상태

이 노트는 class 설명이 기존 문서에 얼마나 있는지 확인한 뒤 이어진 설계 논의를 정리한다. 사용자 결정, 검토 중인 제안, assistant의 추가 제안은 구분한다. 소스 구현을 확인하거나 변경한 기록이 아니며, 아래 모든 항목이 현재 wiki에 반영되었다는 뜻도 아니다.

- Class 생성자의 `init x` 표기와 struct/class 생성 방식 구분은 사용자 결정이다. [Class constructors](../wiki/language/class-constructors.md)에 기준 규칙을 기록했다.
- Pool은 별도 제안이며 의미론을 확정하지 않았다. [Pool 제안 상세](2026-11-10-pool-handle-cycle-breaking-proposal.md)를 함께 본다.

## 1. 기존 문서의 상태

- `docs/`의 class 연산 직접 관련 문서는 9개, 약 336줄로 대부분 문법 및 예제다. 생성, 함수 호출, 필드 접근, 필드 참조, cast/is/as 및 interface 관련 연산을 다룬다.
- 목차나 본문에서 참조하는 `Class.md`, `Class_Constructor.md`, `Class_Member_Function.md`, `Class_Member_Variable.md`는 확인 당시 없었다.
- 현재 wiki에는 handle 의미, BC/NBC 분류, 접근성, this 캡처, nested generic identity 등이 분산되어 있다. Class 전체 명세는 부족하다.
- 기존 예제의 `as`/`is` 혼용 등 오류가 있어 기존 docs 예제를 현재 확정 의미론으로 그대로 취급하지 않는다.

## 2. Class 선언과 멤버 구성

사용자가 제시하고 후속 수정한 방향:

```text
<class-decl> =
    [<outer-accessor>] 'class' <class-name>
    ['<' <generic-parameters> '>']
    [':' <base-name> (',' <base-name>)*]
    '{' <class-member-decl>* '}'

<class-member-decl> =
      <class-decl> | <struct-decl> | <enum-decl>
    | <trait-decl> | <impltrait-decl> | <type-alias>
    | <class-ctor-decl> | <class-dtor-decl>
    | <class-func-decl> | <class-var-decl>
```

- 구성은 기본적으로 struct와 비슷하다. 처음 목록에서 빠졌던 nested class declaration도 포함한다.
- Base 목록에는 class를 최대 하나 두고, 나머지는 interface로 한다. 순서 등 세부 제약은 정하지 않았다.
- 함수에 `virtual`, `override`, `final`을 사용할 수 있다. 조합, signature 및 dispatch의 상세 규칙은 후속 사항이다.
- Class 멤버의 기본 접근성은 private이며 기본 접근성을 명시한 `private int x;`는 오류라는 사용자 설명이다.
- 처음의 “protected는 함수류만 허용”은 사용자가 “변수에 protected 금지”로 수정했다. Nested type이나 alias까지 금지하는 규칙이 아니다.
- 기존 소멸자의 접근 지정자 금지/항상 public 규칙은 별개로 유지한다.

### 기존 기록과의 관계

현재 visibility wiki는 class member에 public/protected/private를 허용한다고 설명하지만 변수에 protected를 금지하는 이번 제한은 아직 해당 문서에 반영하지 않았다. Nested protected type은 이번 수정과 양립한다.

## 3. Class와 struct의 생성 경로

- 사용자 방향: class 객체는 무조건 `new`로 생성한다.
- Heap 생성은 `new`, `box` 같은 명시적 생성 연산으로 나타내고, struct/enum의 일반 값 생성은 생성자 표현으로 나타내려는 의도다. 구체적인 전체 heap 생성 문법을 이번 논의에서 확정한 것은 아니다.
- Pool은 별도 자료구조로 두며 class 객체를 arena에서 직접 만드는 별도 생성 경로를 추가하지 않는다.

## 4. Struct의 자동 memberwise constructor

사용자는 기존의 trivial constructor라는 명칭을 memberwise constructor로 정리하고, 이후 struct/class의 생성 문법을 억지로 통일하지 않기로 했다.

```citron
struct S
{
    int x;
    int y;
}

var s = S(1, 2);
```

- Struct는 전체 필드를 선언 순서대로 초기화하는 자동 memberwise constructor를 기본 제공한다.
- 비활성화하려면 `S(...) = delete;`를 사용한다.
- 사용자는 `memberwise S() = delete;`도 대안으로 언급했다. 대안 표기는 아직 확정하지 않았다.
- 기존 docs에는 일반 생성자를 선언하면 memberwise constructor가 자동으로 없어지는 규칙이 없으며, 두 후보가 모두 적용되면 모호성 오류로 설명한다. 이 부분은 기존 문서의 설명을 확인한 것이며 구현은 검증하지 않았다.
- 무인자 기본 생성자와 memberwise constructor는 구분한다. 과거 요약에 있는 다른 의미의 trivial/default initialization 설명을 이 개념에 섞지 않는다.

### 직접 초기화의 의미

```citron
S s = S(3, move z);
```

호출 인자를 각 최종 필드 저장소의 초기화에 직접 적용한다. 일반 매개변수 객체를 생성하고 이를 다시 필드로 복사·이동하는 단계를 뜻하지 않는다. 다만 기존 객체를 인자로 주는 경우 필요한 복사·이동 생성까지 없어지는 것은 아니다.

## 5. Class 생성자의 init 인자 — 확정

Class에는 자동 memberwise constructor를 제공하지 않는다. 생성자와 인자 목록은 직접 작성하되, 선택한 필드는 `init` 인자로 직접 초기화한다.

```citron
class C
{
    int x;
    int y;
    string s;

    public C(init x, init y)
    {
        s = "hi";
    }
}

var c = new C(1, 2);
```

- `init x`는 같은 이름의 instance field를 지정하고 그 필드의 타입을 사용한다.
- 인자로 해당 필드를 직접 초기화하고, 생성자 본문 진입 전에 그 필드의 초기화를 마친다.
- 나머지 필드는 본문에서 별도로 초기화한다.
- 모든 필드를 인자로 받으려면 `public C(init x, init y, init s) {}`처럼 각각 적는다.
- Init 인자는 reference를 전달하는 인자가 아니다. 사용자는 컴파일러 전용 표현도 `[init] T&` 대신 init 개념으로 나타내는 편이 맞다고 지적했다. 전체 내부 signature 문법까지 확정한 것은 아니다.

### 표기 검토 이력

1. 필드에 `trivial`을 붙이고 생성자에는 `...`을 쓰는 후보를 검토했다. 필드 표시가 여러 생성자에 어떻게 적용되는지와 생성자의 실제 인자 목록이 불분명하다는 비용이 있다.
2. 생성자에 `trivial x`를 쓰는 후보를 검토했다. 사용자는 일부 필드를 직접 초기화하고 나머지를 본문에서 처리할 수 있는 정도면 충분하다고 했다.
3. `forward`, `memberwise`, `field`, `this.x`, `direct`를 검토했다. Forward는 기존 `[forward]` 매개변수와 혼동 가능성이 있다.
4. Struct와 class의 생성 문법을 통일하려 하지 않고, class에서 명시적 direct 인자를 사용하는 방향으로 정리했다.
5. 최종적으로 사용자는 `direct` 대신 `init`을 쓰기로 명시적으로 확정했다. 이전 표기들은 대체된 후보다.

Base 생성자 호출, 초기화 순서, 실패 cleanup, 중복/상속 필드 지정과 이름 충돌은 미해결 세부 사항이다.

## 6. 미초기화 struct의 초기화와 대입

사용자가 과거 의도로 설명하고 확인한 모델은 다음과 같다.

```citron
S s = uninit;
s = S(...); // s가 미초기화이므로 초기화로 확정
s = S(...); // 이미 초기화되었으므로 대입
```

같은 문장이 실행 시점에 따라 초기화와 대입 중 하나가 되는 형태는 허용하려는 방향이 아니다.

```citron
S s = uninit;

while (...)
{
    s = S(...); // 첫 실행은 초기화, 후속 실행은 대입일 수 있어 불명확
}
```

이런 경우 사용자는 nullable을 사용해야 한다고 설명했다.

```citron
S? s = null;

while (...)
{
    s = S(...);
}
```

이 경우 nullable 저장소 자체는 처음부터 초기화되어 있고, 값의 부재/존재를 nullable의 의미로 처리한다. 미초기화 상태를 숨은 runtime flag로 추적해 동일 문장을 초기화/대입으로 자동 분기시키는 모델과 구분한다. 모든 제어 흐름 합류와 반복에 대한 정적 분석의 세부 규칙은 별도 설계가 필요하다.

### 과거 init 기록의 정정 맥락

- 삭제된 `docs/Citron_Core_Semantics_for_Codex.md`에는 `S s;`, `s = init(...);`, 변수당 한 번만 init 허용 등의 설명이 있었다. 현재 wiki는 상충하는 초기화 설명 때문에 이를 확인 필요 항목으로 둔다.
- 기존 `docs/Struct.md`에는 미초기화 필드 대입이 생성으로 처리되며 명시적으로 init을 붙일 수 있다는 설명이 있으나, 완성된 표면 문법은 제시하지 않는다.
- 이번 사용자 설명을 기준으로, 과거 `init(...)` 형태를 현재 확정 사용자 구문으로 제시하지 않는다.
- 기존 `docs/Citron_Concepts.md`의 컴파일러 전용 `S([init] int& i, [init] T& t)`도 참조 전달을 암시하므로 이번 의도에 맞는 설명으로 보지 않는다.
- 생성자 인자 `init x`는 위 과거의 별도 init 표현식과 구분되는 확정 문법이다.

## 7. Interface 구현을 class 본체와 분리

사용자는 interface 구현이 class의 일반 구현과 섞이지 않기를 원했다.

```citron
class C<T> : B, IComparable
{
    // 상태와 class의 일반 멤버
}

impl C<T> : IComparable
{
    bool Compare() { /* ... */ }
}
```

- Class 내부의 `bool IComparable.Compare()`도 후보였으나 struct의 trait impl과의 일관성이 고민이었다.
- Assistant는 별도 impl을 우선 사용하고, interface 구현 함수가 class의 일반 멤버로 자동 추가되지 않는 모델을 제안했다. 두 표기의 병행 여부까지 사용자 확정된 것은 아니다.
- Interface impl 자체와 class의 virtual/override/final을 분리하고 필요하면 impl 본문에서 class의 virtual 함수를 호출하는 방식은 assistant 제안이다.
- 현재 구현 범위는 struct의 trait impl이므로 class/interface impl은 후속 확장이 필요하다.

## 8. Base interface 계약과 구현 상속

사용자는 base class의 interface 구현을 derived가 물려받고, derived에서 interface를 재정의할 수 있는 방향을 제시했다.

```citron
class B : IComparable { }
class D : B { }

B b = new D();
IComparable i = b;
```

- B가 보장하는 interface 계약은 D에서도 유효해야 한다. 사용자는 `class D : B, do-not-inherit IComparable`처럼 계약을 제거하는 방안이 위 변환과 충돌함을 지적했다.
- Base 구현을 사용하지 않고 derived의 재구현을 강제하는 별도 표기는 논리적으로 가능하지만 assistant의 추가 후보이며 채택하지 않았다.
- 실제 D 객체를 B로 보거나 D로 보거나 같은 interface 구현을 선택하도록 하자는 것은 assistant의 권고다. 구체적 witness/dispatch 선택 규칙은 미확정이다.
- Interface 전체를 재구현하고 일부 함수만 base 구현과 자동 혼합하지 않는 방안 역시 assistant 제안이며 확정하지 않았다.

## 9. RC, NBC, weak 및 reset

- 사용자는 class가 RC로 수명을 관리하며, 순환 참조를 다룰 weak도 필요하다고 설명했다. Weak의 구체적인 표면 문법과 승격 연산은 정하지 않았다.
- Class/interface handle은 RC를 고려해 NBC로 분류하려던 의도였으며 기존 BC 분류를 수정해야 한다고 명시했다.
- Handle 복사는 객체 복제와 다르다. Retain/release 및 마지막 strong 참조 해제에 따른 소멸을 보존해야 한다. 이동 시 retain 없이 소유 참조를 이전하는 것은 논의에서 설명한 구현 모델이다.
- 현재 types-and-references 및 MIR value model wiki의 BC 목록에는 아직 class/interface handle이 남아 있다. 이번 노트 작성으로 그 문서들이 수정된 것은 아니다.

### Reset 검토와 pool로의 전환

- 사용자는 RC와 무관하게 객체를 삭제하고 nullptr로 바꾸는 shared reset을 순환 참조 해소 수단으로 제안했다.
- Assistant는 변수 하나만 null로 바꾸며 강제 삭제하면 다른 참조가 dangling이 되고, 전체 참조를 무효화하려면 공통 관리 상태 및 접근 규칙이 필요함을 지적했다.
- 한 연결만 일반 reset으로 해제하거나 객체별 Disconnect 동작을 두는 것은 assistant가 제시한 대안이다. 외부에서 순환 내부에 접근할 수 없으면 충분하지 않다.
- 사용자는 순환 참조 해소를 다른 방식으로 검토하기로 했고, pool/arena wrapping을 제안했다. 강제 삭제 reset을 채택한 것은 아니다.

## 10. Pool 제안 요약

```citron
class A { pool B b; }
class B { pool A a; }

pool_handle h;
pool B b = pool (h, new B());
pool A a = pool (h, new A());

b.a = a;
a.b = b;

h.destroy();
```

- Class 생성은 여전히 new로 수행하고 RC 포인터를 pool로 감싼다. Pool 값도 `.`으로 멤버 접근이 가능해야 한다는 사용자 제안이다.
- 초기에는 arena 소유/비소유 연결 및 ID 기반 pool도 검토했으나 사용자는 위의 RC 포인터 wrapping 형태를 제시했다.
- Assistant는 객체의 strong 참조를 공유 슬롯에 보관하고 pool 값들이 슬롯을 공유하며 destroy가 슬롯 무효화 및 strong 참조 해제를 수행하는 모델을 제안했다. 사용자 확정 의미론은 아니다.
- Pool 밖의 strong 참조, 관리 정보 수명, 무효화된 값의 접근, 소멸 중 재진입, 실행 중 receiver 보호, 서로 다른 pool 연결과 동기화는 열려 있다.
- 예제는 의도 설명용이며 접근성/필드 초기화까지 검증된 실행 예제는 아니다.
- 상세 근거와 후속 쟁점은 [별도 pool 노트](2026-11-10-pool-handle-cycle-breaking-proposal.md)에 유지한다.

## 11. Memberwise update 후속 논의

사용자는 이미 초기화된 목적 저장소에 memberwise 생성 표현을 대입할 때, 임시 S를 생성한 뒤 S의 copy_assign/move_assign을 호출하는 대신 각 필드를 직접 갱신하는 방식을 제안했다.

```citron
S s = S(a, b); // memberwise 초기화
s = S(c, d);   // memberwise update 후보
```

이를 명시적으로 제공하는 선언으로 다음 형태를 제안했다.

```citron
special update(update x, update y);
```

- 사용자 제약: x, y는 각자 해당 인자로 update 가능해야 한다.
- 생성자의 init 항목과 달리 기존 필드의 값을 갱신한다.
- Assistant는 필드에서 타입을 가져오고, 본문 없는 선언으로 지정 필드들의 update를 생성하는 해석을 제안했다. 전체 signature/matching 규칙은 미정이다.
- 일반 생성자 호출의 본문을 생략하는 기능으로 해석하지 않는다. Memberwise 표현과 update 선언의 연결 규칙, 일반 copy/move assignment와의 선택 관계를 정해야 한다.

### 자기 참조와 평가 순서 — 미정

```citron
// 처음 s.x = 1, s.y = 2
s = S(s.y, s.x);
```

- 우변 인자 값을 갱신 전에 보존하면 교환 의미가 가능하다.
- 각 항목의 인자를 평가한 뒤 즉시 필드를 갱신하는 순차 의미라면 결과가 (2, 2)가 될 수 있다.
- 사용자는 이런 사용을 금지하지 않고 주의 사항으로 설명하는 방안도 언급했다. Assistant는 미정의 동작으로 방치하기보다 순서를 정의해야 한다고 제안했다.
- 순차 갱신을 확정한 것은 아니다. 별칭을 통한 간접 참조와 NBC 인자의 보존, 평가 부작용도 후속 쟁점이다.

### 효율 검토와 현재 결론

- 사용자는 memberwise update가 항상 효율적이지 않으며 벌크 memcpy가 더 빠를 수 있다고 지적했다.
- BC 값에서는 컴파일러의 aggregate scalar replacement나 복사 최적화로 임시 구조체가 제거될 수 있다. NBC에서는 생성·대입·소멸의 관찰 가능한 동작이 달라 같은 변환을 보장할 수 없다.
- LLVM SROA 및 GCC SRA 공식 문서를 참고했지만, Citron에서의 IR 생성·최적화 결과나 성능을 실험한 것은 아니다.
  - https://www.llvm.org/docs/Passes.html#sroa-scalar-replacement-of-aggregates
  - https://gcc.gnu.org/onlinedocs/gcc/Optimize-Options.html#index-ftree-sra
- 후속 사용자 결정: 일단 update는 그대로 유지한다. `special update(update x, update y);` 제안을 제거하지 않으며, 세부 의미론과 구현은 추가 설계 대상으로 남긴다.

## 기록 및 구현 상태

- 이 노트는 지금까지의 논의와 결정 이력을 기록한다. 확정된 class init 규칙은 관련 wiki를 함께 참조한다.
- 기존 docs, BC 분류 문서, visibility 문서의 잔여 충돌을 이 기록만으로 해소한 것으로 간주하지 않는다.
- 소스 변경 및 빌드·테스트는 수행하지 않았다.
