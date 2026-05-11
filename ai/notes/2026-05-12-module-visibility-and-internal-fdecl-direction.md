# 회의 / 설계 노트

Date: 2026-05-12
Title: module 내부 상호참조, 접근 제어, `extend`, 내부 `fdecl` 방향 재정리

Status
- draft

Summary
- 사용자 작성 모델의 기본은 `ct` 하나로 두고, 같은 module 안의 unit들은 자동으로 상호참조 가능하게 하는 방향을 선호한다.
- 사용자 지정 forward declaration은 두지 않고, compiler 내부에서 skeleton/fdecl 수집으로 같은 역할을 처리한다.
- `cti/ctm` 분리는 기본 작성 모델로 강제하지 않고, 필요하면 build artifact 또는 외부 선언 전용 형태로 축소하는 방향을 선호한다.
- top-level declaration의 기본 접근성은 `internal`로 두고, 외부 module에 노출할 declaration만 `public`을 붙인다.
- member / nested declaration은 기본 접근성을 type kind에 따라 둔다.
  - `struct`: 기본 `public`
  - `class`: 기본 `private`
- member / nested declaration에서는 기본 접근성을 명시적으로 다시 적는 것을 금지하는 방향을 선호한다.
- `internal`은 top-level declaration과 member function / nested type에만 허용하고, member variable에는 허용하지 않는 방향을 선호한다.
- `private`는 접근 제한을 뜻하고, `internal`은 module 밖으로 export되지 않음을 뜻하는 방향으로 정리한다.
- `extend`는 강한 trusted augmentation으로 보고, private 접근을 허용하는 방향을 선호한다.
- same-module test target이 `extend`를 이용해 private/internal 테스트 seam을 만드는 방향을 선호한다.
- incremental build는 module 전체 변경이 아니라 실제로 사용한 외부 declaration 단위의 dependency/hash 비교로 좁히는 방향을 선호한다.

Context
- 초기에는 `cti/ctm` split과 `using unit` 기반 import surface를 중심으로 설계를 밀어보려 했으나, 같은 module 안 상호참조와 helper/member visibility, 테스트 seam 문제를 같이 풀다 보니 기본 작성 모델은 더 단순해야 한다는 쪽으로 생각이 이동했다.
- 특히 C++의 forward declaration이 하는 역할(상호참조 해결, include dependency 축소)은 사용자 문법이 아니라 compiler 내부 `fdecl` 수집과 build dependency tracking으로 더 잘 대체할 수 있다고 보게 되었다.
- access 정책은 원 저자의 의도를 보호하지만, 실제 패치/적응/테스트 상황에서는 public API만으로는 너무 제약이 많다는 경험이 누적되었고, 이를 위해 `extend`를 더 강한 메커니즘으로 보는 방향이 나왔다.

Decisions
## 1) 같은 module 안 unit들은 자동으로 상호참조 가능하게 한다
- `using unit`은 두지 않는 방향을 선호한다.
- 같은 module 안의 top-level declaration은 compiler가 먼저 모두 수집하고, 이후 각 unit은 그 declaration world를 자동으로 본다고 가정한다.
- 사용자는 forward declaration을 직접 적지 않는다.
- compiler 내부에서는 skeleton/fdecl 수집이 같은 역할을 한다.

예:
```citron
// a.ct
struct A { B* b; }

// b.ct
struct B { A* a; }
```

이 경우 사용자는 별도 forward declaration 없이도 작성할 수 있어야 한다.

## 2) unit 처리 순서 문제는 `using` 그래프가 아니라 complete dependency 그래프로 푼다
- 같은 module 안의 declaration 이름은 먼저 전부 수집한다.
- 이후 각 decl이 complete해지기 위해 어떤 다른 decl의 complete가 필요한지 dependency를 계산한다.
- 이 dependency graph를 topological sort하여 decl completion 순서를 정한다.
- graph에 cycle이 생기면 에러다.

예:
```citron
// a.ct
struct A { B b; }

// b.ct
struct B { int x; }
```
- `A` complete에는 `B` complete가 필요하므로 `B -> A` 순서로 처리 가능하다.

```citron
// a.ct
struct A { B b; }

// b.ct
struct B { A* a; }
```
- `B`는 `A*`만 사용하므로 `A` complete를 요구하지 않는다.
- complete dependency는 `B -> A`이고, 따라서 처리 가능하다.

```citron
// a.ct
struct A { B b; }

// b.ct
struct B { A a; }
```
- `A -> B`, `B -> A`가 동시에 필요하므로 complete dependency cycle로 에러다.

## 3) 내부 `fdecl`은 기존 skeleton phase 결과를 공식적인 declaration 상태로 해석한다
- `fdecl`은 compiler 내부 상태다.
- 의미는 “이 declaration이 존재한다는 사실과 stable identity는 확보되었지만, complete surface는 아직 채워지지 않은 상태”다.
- 기존 skeleton phase가 만드는 선언 골격은 사실상 내부 `fdecl`과 같은 역할을 한다.
- 이후 member/signature/base/interface 정보를 채우면 `decl`, body까지 채우면 `impl`로 본다.

의도:
- 사용자에게 forward declaration 문법을 강요하지 않으면서도, compiler 내부에서는 declaration 수집과 이후 complete 판정을 분리할 수 있게 한다.

## 4) `cti/ctm`은 기본 작성 모델로 강제하지 않는다
- 사용자의 기본 source 작성 형태는 `ct` 하나로 둔다.
- compiler/builder는 `ct`를 읽고 module 내부 declaration world를 수집한 뒤, 필요하면 외부 공개 surface artifact를 자동 생성한다.
- `cti/ctm` split은 기본 언어 작성 모델보다 build artifact 또는 외부 선언 전용 형식으로 약화시키는 방향을 선호한다.
- 특히 `cti`는 module의 외부 공개 declaration surface를 담는 artifact로 보는 것이 자연스럽다.

## 5) top-level declaration 접근성
- top-level declaration 기본값은 `internal`이다.
- 외부 module에 공개할 top-level declaration만 `public`을 붙인다.
- top-level `private`는 두지 않는 방향을 선호한다.
- `public`은 top-level type declaration, global function declaration, top-level `extend` declaration에도 붙을 수 있다.

예:
```citron
struct HiddenHelper { }     // internal
public struct S { }         // external visible

void Helper() { }           // internal
public void API() { }       // external visible
```

## 6) member / nested declaration 접근성
- `struct`의 member / nested declaration 기본값은 `public`이다.
- `class`의 member / nested declaration 기본값은 `private`이다.
- 기본 접근성을 문법에 다시 적는 것은 에러로 두는 방향을 선호한다.
  - `struct` 안에서 `public` 명시는 에러
  - `class` 안에서 `private` 명시는 에러

예:
```citron
struct S
{
    void F();          // OK, public by default
    public void G();   // error
    private void H();  // OK
}

class C
{
    void F();          // OK, private by default
    private void G();  // error
    public void H();   // OK
}
```

## 7) `internal`은 helper / non-export 용도로 둔다
- `internal`은 top-level declaration에 사용할 수 있다.
- member에서는 function과 nested type에만 사용할 수 있다.
- member variable에는 `internal`을 허용하지 않는 방향을 선호한다.
- `internal` member/function/type은 현재 module 안에서는 존재하고 사용할 수 있지만, module 밖 export surface에는 포함되지 않는다.

예:
```citron
public struct S
{
    internal void Normalize() { }
    internal struct Helper { }
    internal int x; // error
}
```

의도:
- helper/implementation-only member는 함수와 nested helper type이면 충분하다고 본다.
- field에 `internal`을 허용하면 layout/ABI/export surface 해석이 너무 복잡해진다.

## 8) `private`와 `internal`의 의미 분리
- `private`는 접근 제한을 뜻한다.
- `internal`은 module 밖으로 export되지 않음을 뜻한다.
- 즉:
  - `private`: symbol은 더 넓은 declaration world에 존재할 수 있지만 접근은 제한된다.
  - `internal`: symbol 자체가 외부 module surface에 포함되지 않는다.

## 9) nested declaration의 외부 공개 여부
- top-level에서는 `public`이 module export를 뜻한다.
- nested declaration에서는 `public`이면 외부에 보일 수 있고, `internal`이면 외부에 보이지 않는다.
- `private` nested declaration은 외부에 존재가 보일 수 있으나 접근은 제한되는 방향을 선호한다.

예:
```citron
public class S
{
    internal struct X { }
    struct Y { }          // class default = private
    public struct Z { }
    protected struct A { }
}
```

## 10) C#식 inconsistent accessibility 규칙을 따른다
- 어떤 declaration의 시그니처에 등장하는 타입은, 그 declaration을 볼 수 있는 범위에서도 볼 수 있어야 한다.
- 더 좁은 접근성의 타입을 더 넓은 접근성의 declaration 시그니처에 쓰면 에러다.

예:
```citron
public class S
{
    internal struct X { }
    struct Y { }          // private
    public struct Z { }
    protected struct A { }

    X F(); // method default = private, 허용
    Y G(); // method default = private, 허용
    Z H(); // method default = private, 허용
    A I(); // method default = private, 허용
}
```

반면:
```citron
public class S
{
    internal struct X { }
    public X F(); // error
}
```

의도:
- “선언이 보이는 곳에서는 그 시그니처 타입도 보여야 한다”는 C#식 규칙으로 가독성과 진단 일관성을 확보한다.

## 11) `extend`는 강한 trusted augmentation으로 본다
- `extend`는 단순한 extension method sugar보다 더 강한 메커니즘으로 본다.
- private 멤버 접근을 허용하는 방향을 선호한다.
- 같은 module 안의 `extend`는 internal에도 접근할 수 있다.
- 외부 module의 `extend`는 internal에는 접근할 수 없지만, private에는 접근 가능한 방향을 선호한다.

의도:
- access 정책 때문에 패치, 적응층, 테스트 seam이 막히는 문제를 줄인다.
- `extend`는 “의도가 있으면 내부를 건드릴 수 있는 escape hatch” 역할을 맡는다.
- 버전 결합 비용은 사용자가 감수하되, 언어가 아예 금지하지는 않는다.

## 12) 테스트는 same-module test target + `extend` seam으로 푼다
- test를 외부 module consumer처럼 돌리면 internal/private 접근 문제가 커진다.
- test용 target을 따로 두고, 현재 module 내부에서 테스트를 수행하는 방향을 선호한다.
- 다만 same-module test target이라도 ordinary code에서는 private 접근이 안 되므로, 필요하면 `extend`를 사용해 seam을 만든다.

예:
```citron
internal extend S
{
    int __test_get_x() { return x; }     // private 접근
    void __test_normalize() { Normalize(); } // private/internal 접근
}
```

## 13) 외부 의존성 증분 빌드는 declaration 단위 추적을 지향한다
- builder는 어떤 module을 `using`했는지만 기록하면 안 된다.
- 실제로 사용한 외부 declaration identity와 그때 본 signature hash를 기록해야 한다.
- module 전체가 다시 빌드되더라도, 현재 unit이 실제로 쓴 declaration의 signature가 바뀌지 않았으면 재컴파일하지 않는 방향을 선호한다.
- 이를 위해 builder service 또는 `.deps` artifact가 declaration-level dependency를 기록하는 방향을 검토한다.

예:
- `Core` module를 `using`했더라도 실제로 `Core.String`, `Core.Logger.Print`만 썼다면, `Core.HashMap` 변경만으로는 재컴파일되지 않아야 한다.

Open Points
- nested `private` declaration이 외부 surface에 “존재 정보”까지 실릴지, 아니면 완전히 감출지
- `protected` nested type / member와 C#식 accessibility 비교 규칙을 symbol model에서 어떻게 구현할지
- external module의 `extend`가 private 접근까지 허용될 때, 진단/문서/버전 정책을 어떻게 설명할지
- `cti` artifact가 export surface만 담을지, private presence 같은 최소 metadata도 담을지
- declaration-level dependency hash와 build service 캐시를 어떤 파일 포맷으로 저장할지
- same-module test target이 ordinary unit과 정확히 같은 visibility world를 공유할지, 별도 test-only metadata를 둘지

Action Items
- [ ] same-module declaration collection + complete dependency graph 구성 절차를 별도 문서로 정리
- [ ] `NGlobalFuncDecl`, `NStructDecl`, `NClassDecl`, `NInterfaceDecl`, `extend` declaration에 대해 `fdecl/decl/impl`별 채워지는 필드 표 작성
- [ ] top-level / member / nested visibility 규칙을 parser / symbol model 관점에서 표로 정리
- [ ] C#식 inconsistent accessibility 검사를 type/member/global function 예제로 정리
- [ ] `extend`의 private/internal 접근 허용 규칙을 별도 메모로 승격할지 검토
- [ ] builder의 declaration-level dependency tracking 산출물(`.deps`, declaration hash) 초안 작성
