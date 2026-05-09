# 회의 / 설계 노트

Date: 2026-05-08
Title: `cti`, import visibility, static interface, `foreach` 선행순서 정리

Status
- draft

Summary
- `foreach`를 ad-hoc 규칙으로 넣기보다, 이후의 interface/conformance 모델 위에서 동작하게 하는 방향을 유지한다.
- 이를 위해 `foreach`나 static interface consumer보다 먼저 `cti` 기반 declaration/import 모델을 정리하는 편이 낫다.
- 사용자가 작성하는 소스는 `ct` 외에 `cti`를 별도로 가질 수 있다.
- 필요하면 `ctm` implementation unit을 별도로 둘 수 있다.
- 기본적으로 `ct`에서 declaration surface를 추출해 내부적으로 implicit `cti`를 만들고, 필요하면 사용자가 수동 `cti`를 제공한다.
- `import`는 구현 파일을 직접 여는 것이 아니라 foreign module의 `cti` surface를 import하는 의미로 본다.
- conformance는 명시만 허용하고, orphan rule은 두지 않는다.
- conformance visibility는 링크 결과가 아니라 import된 `cti` 집합으로만 결정한다.
- interface는 dynamic/static을 분리된 문법으로 두지 않고, 동일 declaration을 사용 맥락에 따라 dynamic 또는 static으로 소비하는 방향을 선호한다.
- `foreach` iterator protocol은 복사 회피와 NBC 수용을 위해 `Next + YieldValue` 2단계 모델을 우선 선호한다.
- 단위 계층은 `library > module > unit`으로 정리하는 방향을 선호한다.
- 같은 module 내부 상호참조는 unit 수준 forward declaration으로 풀고, module 밖에서는 module 단위로만 참조한다.
- 외부/내부 declaration world를 현재 unit에서 명시적으로 사용한다는 뜻은 `import`보다 `using`으로 통일하는 방향을 현재 더 선호한다.

Context
- `foreach`를 먼저 구현하려고 보면 enumeration 대상 판별, item 획득 방식, conformance visibility, import, 상호참조, incremental build가 모두 함께 걸린다.
- 초기에는 `next + yield_value` 멤버를 직접 찾는 구조적 규칙과, interface를 통한 명시적 conformance 방식이 후보였다.
- 이후 방향은 다음처럼 정리되었다.
  - `foreach`를 special case로 만들지 않는다.
  - conformance는 명시적으로 선언되어야 한다.
  - interface는 나중 generic/static consumer와 공유되는 공통 선언 모델이어야 한다.
  - orphan rule 대신 import visibility를 통해 어떤 conformance가 보이는지 제어한다.
- 이 요구를 만족시키려면 declaration surface를 body보다 먼저 수집하는 단계가 필요하며, `cti`가 그 역할을 맡는 것이 자연스럽다.

Decisions
## 0) 단위 계층은 `library > module > unit`으로 본다
- `library`
  - 배포/패키징/링크의 큰 단위
  - 하나의 library 안에 여러 module이 있을 수 있다
- `module`
  - foreign import의 단위
  - foreign forward declaration의 단위
  - 외부에서 보이는 declaration world의 단위
- `unit`
  - translation unit에 대응하는 소스 입력 단위
  - 같은 module 안 unit끼리는 forward declaration 기반 상호참조가 가능하다
  - module 밖에서는 unit을 직접 참조하지 않고 module 단위로만 참조한다

의도:
- `module`은 파일 태그처럼 작은 단위가 아니라, import/export와 공개 declaration world의 단위로 남긴다.
- `unit`은 translation unit 의미로 두어 구현/번역 단위를 설명한다.

## 1) `cti`를 import surface로 둔다
- 사용자는 `ct`뿐 아니라 `cti`를 별도로 작성할 수 있다.
- 기본 동작은 `ct`에서 import 가능한 declaration surface를 추출해 implicit `cti`를 내부적으로 생성하는 것이다.
- declaration을 제한하거나, body 없이 외부 심볼을 노출하거나, conformance surface를 따로 제공하고 싶을 때는 수동 `cti`를 작성한다.
- `ctm`은 interface/implementation을 분리하고 싶을 때 사용하는 implementation unit이다.
- `import`는 구현 파일을 읽는다는 뜻이 아니라 foreign module의 `cti` surface를 import한다는 뜻으로 본다.

권장 해석:
- `ct`: implementation + source declaration
- `cti`: import 가능한 declaration contract
- `ctm`: `cti`에 선언된 entity의 implementation/definition을 두는 unit

split mode:
1. 단일 `ct` 파일
   - declaration surface를 추출해 implicit `cti` / `cti.o` 생성
2. `cti` / `ctm` 분리 모드
   - `cti`에서 interface surface를 기술
   - `ctm`에서 그 선언을 정의
   - `cti`를 compile하여 `cti.o` 생성

## 2) conformance는 명시만 허용한다
- 어떤 타입이 interface를 만족하는지는 구조적 추론으로 자동 판별하지 않는다.
- `Next`, `YieldValue`, `GetEnumerator` 같은 이름/shape만 우연히 맞는다고 해서 conformance로 간주하지 않는다.
- conformance existence는 반드시 declaration으로 적혀 있어야 한다.

이유:
- 진단과 ambiguity 처리가 단순해진다.
- 우연한 이름 일치에 의한 오탐을 줄일 수 있다.
- dynamic/static 공용 interface 모델과도 잘 맞는다.

## 3) orphan rule은 두지 않되, conformance visibility는 import된 `cti`로 제한한다
- 어떤 라이브러리가 제3의 타입과 interface에 대한 conformance를 제공하는 것도 허용한다.
- 다만 링크만 되었다고 conformance가 자동 활성화되지는 않는다.
- 현재 컴파일에서 import한 `cti` 집합 안에 있는 conformance declaration만 visible candidate가 된다.

예:
- `A`가 타입 `S`를 제공
- `B`가 interface `I`를 제공
- `C`가 `extend S : I`를 제공
- 사용자가 `C`의 `cti`를 import해야 `S : I` conformance가 보인다

## 4) interface declaration은 하나만 두고 dynamic/static 사용은 맥락으로 나눈다
- 별도의 `static interface` 문법을 두는 것보다, 동일한 interface declaration을 dynamic과 static 양쪽에서 쓰는 방향을 선호한다.
- 같은 interface를:
  - interface 타입 값/캐스팅/동적 호출에 쓰면 dynamic 의미
  - `foreach`, generic constraint, compile-time protocol checking에 쓰면 static 의미

즉 분리 대상은 declaration kind가 아니라 consumption mode다.

## 5) `foreach` protocol은 `Next + YieldValue` 2단계 모델을 우선 선호한다
- `foreach`에서 item 복사를 피하고, NBC item도 수용하기 위해 `nullable<T>` 단일 반환보다 `Next + YieldValue` 분리 모델이 더 적합하다고 본다.
- 현재 rough contract는 다음에 가깝다.
  - `TEnumerator GetEnumerator();`
  - `T* Next();` 또는 `bool Next(); T* YieldValue();`
- 표면 타입은 우선 `T*`를 선호한다.
  - unsafe 성격이 강하며, 초기에는 사실상 컴파일러 내부 소비에 가깝다.
- 이 모델의 목적은 현재 item을 value로 복사해 돌려주는 것이 아니라 location-like access를 제공하는 데 있다.

Open point:
- `T* Next()`와 `bool Next(); T* YieldValue();` 중 어느 형태가 더 좋은지 최종 확정 필요

## 6) `foreach`나 static interface consumer보다 먼저 `cti`를 구현하는 편이 낫다
- `foreach`, static interface, import visibility, 상호참조, incremental build는 모두 declaration surface 관리에 의존한다.
- 따라서 지금 단계에서는 `foreach`를 바로 구현하기보다 `cti`와 import/declaration 수집 모델을 먼저 갖추는 쪽이 더 안전하다.
- 이는 `foreach`를 미루는 것이 아니라, `foreach`가 올라갈 기반을 먼저 만드는 일로 본다.

## 7) 상호참조는 declaration 단계와 body 단계 분리로 푼다
- `cti`를 전부 먼저 수집하고, 그 뒤 `ct` body를 컴파일하는 2단계 파이프라인을 가정한다.
- 이 경우 declaration-level mutual reference는 상당 부분 해결된다.
- 핵심은 body를 보기 전에 다음 정보가 확정되어 있어야 한다는 점이다.
  - type/interface/function declaration
  - conformance existence
  - requirement signature

즉:
1. 모든 `ct`/`cti`에서 declaration surface를 먼저 수집
2. import된 `cti`들로 visible declaration world를 구성
3. 그 world 위에서 `ct` body를 타입체크/번역

## 7-1) 같은 module 내부 상호참조는 unit 수준에서 허용한다
- 같은 module 내부 unit끼리는 forward declaration을 통해 상호참조할 수 있다.
- 이 상호참조는 declaration collection 단계에서 같은 module의 여러 unit을 먼저 수집한 뒤 complete declaration과 결합하는 방식으로 푼다.
- 반면 module 밖에서는 unit을 직접 참조하지 않고, module surface만 본다.

예:
- 같은 module 내부:
  - `class B;` : same-unit forward declaration
  - `class B from unit foo.bar;` : same-module, other-unit forward declaration
- foreign module:
  - `class B from module M;`

의도:
- `bare class B;`는 같은 파일(unit) 안의 forward declaration shorthand로 남긴다.
- 같은 module이지만 다른 unit에서 complete declaration이 나오는 경우는 `from unit`으로 출처를 적는다.
- 다른 module 타입은 `from module`로 출처를 적는다.
- `class B from ...;`는 local alias를 새로 도입하는 문법이 아니라, `B` forward declaration에 definition origin을 붙이는 문법으로 본다.
- 따라서 사용 시에는 `B`를 그대로 쓰고, `foo.bar::B` 같은 별도 qualification을 요구하지 않는다.

참고:
- 축약형 후보(`from B`, `from @M`, `from #M`)를 검토했으나, unit/module 이름 충돌과 가독성 문제 때문에 현재 초안은 `from unit` / `from module` 표기를 선호한다.
- 같은 방향으로 `B::B`, `foo.bar::B` 같은 qualification 해석도 검토했으나, forward declaration이 alias나 qualified type use처럼 보이는 문제가 있어 채택하지 않는다.

## 7-1-1) `unit` 출처 표기는 논리 경로를 사용한다
- `from unit` 뒤에는 실제 OS 파일 경로가 아니라 module 내부의 논리적인 unit 경로를 적는 방향을 선호한다.
- 현재 선호 표기:
  - `class B from unit foo.bar;`
- 여기서 `foo.bar`는 문자열 path가 아니라 dotted unit identifier chain이다.
- builder는 이 논리 unit 경로를 실제 source path와 매핑한다.

규칙 초안:
- `from unit` 경로는 항상 module root 기준 absolute logical path다.
- 상대경로 `..`는 허용하지 않는다.
- 파일 이름/단위 이름에는 공백을 허용하지 않는 방향을 선호한다.
- `/` 대신 `.` 표기를 선호한다.

이유:
- path/string quoting 문제를 피한다.
- `from unit`이 실제 파일 경로보다 논리 unit 식별자라는 점을 더 잘 드러낸다.
- 현재 unit 위치에 따라 의미가 바뀌는 상대 참조를 피한다.

## 7-2) foreign module은 unit을 구분하지 않는다
- 외부 library/module를 참조할 때는 그 module이 export하는 declaration world만 본다.
- foreign forward declaration과 foreign import의 단위는 module이다.
- 외부 library가 내부적으로 여러 unit으로 나뉘어 있어도, 그 물리적 unit 구조는 외부 API에 새지 않는 편이 좋다.

즉:
- same-module: unit이 의미 있음
- foreign-module: unit은 implementation detail이고, module만 의미 있음

## 7-3) alias는 `using`으로 푼다
- `from ...` 문법은 alias 생성 책임을 지지 않는다.
- 다른 이름으로 부르고 싶을 때는 별도 `using` 문법을 사용한다.

예:
- `class B from module M;`
- `using MB = class B from module M;`

의도:
- `from`은 definition origin 지정
- `using`은 이름 부여/alias

Open point:
- `using MB = class B from module M;`를 parser/symbol level에서 어떻게 모델링할지
  - `class B from ...`를 type reference specifier처럼 볼지
  - type-introducing declaration form처럼 볼지

## 7-4) `import`보다 `using`으로 통일하는 방향을 선호한다
- 외부 module이나 같은 module 내부 unit을 현재 unit에서 “사용한다”는 의미는 `import`보다 `using`이 더 잘 드러난다고 본다.
- alias 문법도 `using`과 자연스럽게 이어진다.
- 따라서 현재 선호안은 다음과 같다.

예:
- `using module A;`
- `using unit foo.bar;`
- `using X = module A;`
- `using FB = unit foo.bar;`
- `class B from module A;`
- `class B from unit foo.bar;`
- `using AB = class B from module A;`
- `using FBB = class B from unit foo.bar;`

대안으로 검토했던 안:
- `import module A;`
- `import unit foo.bar;`
- `import module A as X;`
- `import unit foo.bar as Y;`

현재 `using` 안을 더 선호하는 이유:
- “현재 컨텍스트에서 이 declaration world를 사용한다”는 의미가 더 직접적이다.
- module/unit 사용과 alias 문법을 하나로 통일할 수 있다.
- `import`와 `using`을 둘 다 두면 역할 경계가 다시 흐려질 수 있다.

## 7-5) `using`의 의미와 extension visibility
- `using module A;` 또는 `using unit foo.bar;`는 단순 이름 import가 아니라 declaration world visibility를 여는 동작으로 본다.
- 따라서 그 world 안의 static interface extension / conformance도 함께 visible해져야 한다.
- alias를 붙여도 이 visibility는 사라지지 않는다.

예:
- `using X = module A;`

이 경우:
- `X`는 module `A`를 지칭하는 별칭이지만
- extension/conformance matching은 alias 이름 자체와 무관하게, module `A`가 현재 visible world에 들어왔는지로 판단한다.

즉:
- 이름 lookup은 alias 영향을 받을 수 있음
- extension/conformance lookup은 visible declaration world 기준으로 동작

## 7-6) `forward` 키워드는 현재는 두지 않는 쪽을 선호한다
- `class B;`
- `class B from module A;`
- `class B from unit foo.bar;`

정도면 의미가 충분하다고 보고, `forward class B ...` 같은 별도 키워드는 현재는 거추장스럽다고 판단한다.

## 8) incremental build는 `ct`가 아니라 `cti` 변화 기준으로 본다
- body만 바뀌고 `cti`가 같으면 해당 `ct`만 다시 컴파일하면 된다.
- declaration/conformance surface가 바뀌어 `cti`가 달라지면 그 `cti`를 import하는 downstream을 재컴파일한다.
- 따라서 rebuild 전파 기준은 source text diff보다 normalized `cti` diff에 가깝다.

Implications
## `cti` / `ctm` 구현 형태
- split mode에서는 `cti`가 canonical declaration source가 된다.
- `ctm`은 implementation-only unit으로 본다.
- 현재 선호 방향:
  - `cti`에 선언한 complete declaration은 `ctm`에서 다시 `struct A { ... }` 형태로 반복하지 않는다
  - 대신 `void A::Func() { ... }` 같은 out-of-line definition 형태를 사용한다
- forward declaration은 정의 의무 대상이 아니다.

예:
- `A.cti`
```citron
struct A
{
    void Func();
}
```
- `A.ctm`
```citron
void A::Func() { ... }
```

이유:
- split mode에서 declaration ownership을 `cti`에 고정한다
- `ctm`이 declaration surface를 몰래 바꾸지 않게 한다
- implicit `cti` 생성 모델과 수동 `cti` 모델을 맞춘다

## `ctm` 내부 helper member
- implementation-only helper member가 필요할 때는 `cti`에 public/import surface로 올리는 대신, `ctm` 쪽 local augmentation 기법을 둘 여지가 있다.
- 예시 후보:
  - `extend A { void G() { ... } }`
- 이 경우 그 `extend`는 export/import 대상이 아닌 implementation-local augmentation으로 보는 방향을 선호한다.
- 반면 declaration 없이 `void A::G() { ... }`를 바로 허용하면 `cti`가 canonical declaration source라는 규칙이 약해질 수 있으므로 주의가 필요하다.

## import의 의미
- `using module` / `using unit`은 구현 파일 탐색이 아니라 declaration contract / declaration world visibility를 여는 동작이다.
- visible symbol set과 visible conformance set은 `using`된 `cti`/declaration world의 합으로 결정된다.
- 링크 결과나 파일 검색 순서가 의미를 바꾸면 안 된다.

## module import graph
- unit 간 상호참조는 허용하지만, module 간에는 강한 순환 import를 피하는 방향을 선호한다.
- same-module cycle은 declaration collection으로 풀고, foreign-module cycle은 import DAG를 유지하는 편이 자연스럽다.

## `Builder`의 역할
- 이 모델은 파일 하나를 곧바로 body translation 하는 단순 흐름보다 상위 orchestration이 필요하다.
- 원래 용도였던 `Builder`가 다음 책임을 다시 맡게 될 가능성이 크다.
  - input `ct` / `cti` 수집
  - implicit `cti` 생성
  - import graph 구성
  - declaration/conformance world 구성
  - 이후 body compile orchestration
- 다만 이것이 곧 사용자 CLI가 복잡해진다는 뜻은 아니다.
  - 예: `citron main.ct -o a.out`
  - 내부적으로는 builder-style multi-phase pipeline을 수행할 수 있다.

## Citron식 ODR/consistency 문제
- orphan rule이 없어도 import world 안에서 declaration/conformance 의미는 유일해야 한다.
- 특히 `extend S : I`는 독립 identity로 보고, 현재 visible `cti` world 안에서 둘 이상 보이면 ambiguity/error로 본다.

권장 규칙 초안:
- 현재 import된 `cti` 집합 안에서 declaration identity는 유일해야 한다.
- 같은 `S : I` conformance identity가 visible set 안에서 둘 이상이면 에러다.
- `cti` declaration과 `ct` definition의 시그니처는 정확히 일치해야 한다.
- import/name lookup/conformance lookup은 body를 보지 않고 `cti`만으로 결정되어야 한다.
- 같은 import set이면 의미도 같아야 한다.

Open Points
- `ct`, `cti`, `ctm` 파일이 home module / unit identity를 어떻게 얻는지
- `cti` 문법이 `ct`의 declaration-only subset인지, 별도 문법 요소를 둘지
- 수동 `cti`가 있을 때 implicit `cti`와 어떤 검증 관계를 둘지
- `extend` declaration과 requirement implementation body를 `cti`/`ct` 사이에 어떻게 분리할지
- split mode에서 `ctm` 내부 helper member를 위한 local augmentation 문법을 둘지
- interface requirement 중 constructor/static member exposure를 현재 [docs/Interface.md] 방향처럼 그대로 유지할지
- `Next + YieldValue` 중 최종 iterator contract shape를 어떻게 고정할지
- incomplete conformance declaration을 허용할 경우, 어느 시점에 completeness를 강제할지
- `from unit` / `from module` 문법을 그대로 둘지, 향후 더 짧은 표기를 추가할지
- module과 library 관계를 source 문법/빌드 메타데이터 중 어디에 둘지

Recommended implementation order
1. `Builder`가 관리할 compile phase를 정리한다.
2. `ct`에서 implicit `cti`를 추출하는 최소 규칙을 만든다.
3. 수동 `cti` 로딩과 import resolution을 붙인다.
4. visible declaration/conformance world를 구성한다.
5. interface/conformance declaration model을 그 world 위에 올린다.
6. 그 다음 static consumer(`foreach` 등)를 구현한다.

Action Items
- [ ] `cti` 최소 surface 항목 목록(type/interface/function/extern/extend/forward decl) 초안 작성
- [ ] implicit `cti` 생성 규칙과 수동 `cti` 우선순위 규칙 정리
- [ ] import resolution이 `cti` world를 어떻게 구성하는지 compile phase 관점에서 정리
- [ ] `extend S : I` declaration/definition 분리 모델 초안 작성
- [ ] `foreach` iterator contract를 `T* Next()` vs `bool Next(); T* YieldValue()` 중 하나로 좁히기
