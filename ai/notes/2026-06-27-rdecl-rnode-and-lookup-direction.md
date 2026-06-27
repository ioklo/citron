# 회의 / 설계 노트

Date: 2026-06-27
Title: `RDecl`/`NDecl` 재정리와 `RNode` tree 방향

Summary
- `RDecl`는 semantic tree node 역할과 declaration category 역할이 섞여 있었고, 특히 `GetTypeMember` / `GetMember` / `ResolveIdentifier` 책임이 `N*Decl` 구현체 쪽에 실려 있었다.
- 이번 논의에서는 `RDecl`를 직접 확장하기보다 별도 `RNode`를 도입해 semantic tree를 재구성하고, 이후 완전히 옮겨지면 `RDecl`를 제거하는 방향이 더 낫다고 보았다.
- `RTypeDecl`, `RFuncDecl`, `RTypeDeclOuter`, `RFuncDeclOuter`는 tree node가 아니라 category view로 보고, 구현의 개방성(open implementation)과 분류 축의 폐쇄성(closed category)을 분리해서 생각한다.
- `ResolveIdentifier`는 declaration node 메서드라기보다 lexical scope를 포함한 resolver 책임에 가깝기 때문에, 장기적으로 `RDecl`/`RNode` 밖으로 빼는 방향을 선호한다.
- 용어 정리: symbol tree에서 상위 요소는 `outer`, 상속한 클래스는 `base`로 부르고, `parent`는 모호하므로 쓰지 않는 쪽을 선호한다.
- type declaration은 C++처럼 같은 scope에서 같은 이름의 generic type family를 arity만 다르게 overload하지 않는 쪽을 선호한다.
- `RNode`는 우선 tree 모델로 먼저 구성하고, 충분히 안정화된 뒤 path map 중심 모델로 갈지 재검토한다.
- 접근성은 단일 tree-only `CanAccess`로 처리하기보다, module/namespace member와 type member의 정책을 분리한 별도 checker 책임으로 보는 쪽이 더 자연스럽다는 결론에 가까워졌다.

Context
- `variant` wrapper 도입을 검토하면서 `RFuncDecl`/`NFuncDecl`와 유사한 기준을 `RTypeDecl`/`NTypeDecl` 쪽에도 적용할지 논의가 시작되었다.
- 그 과정에서 `RDecl`가 tree node인지, `NDecl`가 별도 tree node인지, `GetMember` / `GetTypeMember` / `ResolveIdentifier`가 누구 책임인지가 동시에 문제로 드러났다.

## 1) 분리하고 싶은 축

현재 선언 모델에는 최소한 아래 세 축이 섞여 있다고 보았다.

1. tree / name lookup 축
   - outer chain
   - identifier
   - member search
   - access / generic scope

2. semantic category 축
   - function decl인지
   - type decl인지
   - type decl outer인지

3. provenance / concrete payload 축
   - source-origin (`N*`)
   - external-origin (`RE*`)
   - future implementation-specific payload

이상적인 방향은 이 세 축을 한 클래스에 몰아넣지 않는 것이다.

## 2) `RDecl`는 tree node, `RTypeDecl` / `RFuncDecl`는 category view

논의 중 핵심 정리는 다음과 같다.

- `RDecl`는 semantic tree node 역할을 맡는다.
- `RTypeDecl`, `RFuncDecl`, `RTypeDeclOuter`, `RFuncDeclOuter`는 tree identity가 아니라 category view다.

즉:

```text
RDecl (or later RNode) = tree node
RTypeDecl / RFuncDecl = category projection
```

여기서 중요한 점은:

- `R*Decl` concrete hierarchy는 implementation에 열려 있다.
  - 예: `NClassDecl`, `REClassDecl`, future class decl implementation
- 반면 `RTypeDecl` / `RFuncDecl` 같은 category axis는 semantic kind 관점에서는 닫혀 있다.

따라서 interface와 variant를 대립적으로 보지 않고:

- concrete semantic kind (`RClassDecl`, `RStructDecl`, ...)는 구현 확장 지점으로 둔다
- category view (`RTypeDecl`, `RFuncDecl`)는 closed sum으로 본다

는 쪽이 더 자연스럽다고 정리했다.

## 3) `NDecl`는 독립 tree node라기보다 source-origin contract에 가깝다

사용처를 보면 `NDecl`는 종종 곧바로 `GetRDecl()`로 넘어간다.

대표적으로:
- type param 생성 base index 계산
- outer `RDecl` 접근
- identifier / open type args 위임

반면 `NDeclVisitor`는 실사용 흔적이 거의 없었다.

따라서 현재 해석은:

- `RDecl`는 실제 semantic tree 축
- `NDecl`는 source-origin decl이라는 phase-local contract

에 가깝다.

즉 장기적으로 `NDecl`를 유지하더라도 "N tree의 공통 베이스"라기보다 "source payload 또는 source owner contract" 쪽으로 축소하는 방향을 선호한다.

## 4) `GetMember` / `GetTypeMember` / `ResolveIdentifier` 검토 결과

### `GetMember`

현재 `N*Decl` 구현을 보면 대부분 "자기 자식 선언들 중 검색 가능한 후보를 이름으로 모아 결정"하는 구조다.

예:
- namespace: namespace / type / global func
- class/struct: nested type / member func / var
- enum elem: enum elem var
- lambda: lambda var

따라서 `RDecl` tree가 실제로 child `RDecl`를 가지게 만들면 `GetMember`는 비교적 공통 로직으로 올릴 수 있다고 보았다.

### `GetTypeMember`

nested type lookup은 child search로 설명 가능하다. 다만 generic type param은 lexical scope로 볼지 child node로 볼지 결정이 필요하다.

즉:
- nested type는 tree child 기반
- type param은 lexical prelude or child node

중 하나로 정리할 수 있다.

### `ResolveIdentifier`

이 메서드는 단순 member search가 아니었다.

현재 구현들은 대체로:
- generic type param
- func param / lambda-local
- member lookup
- outer fallback

을 합친다.

즉 `ResolveIdentifier`는 declaration node의 intrinsic API라기보다 lexical scope를 포함한 resolver algorithm에 가깝다.

따라서 장기 방향은:
- `GetMember`, `GetTypeMember`는 tree node API
- `ResolveIdentifier`는 외부 resolver / scope context 책임

으로 두는 쪽을 선호한다.

## 5) `RNode`를 새로 만들고 semantic tree를 옮긴다

`RDecl`를 바로 뜯기보다 별도 `RNode`를 만들고 그쪽에 tree 책임을 쌓는 이행 경로가 더 안전하다고 보았다.

의도:
- 새 semantic tree 모델을 `RNode`에서 실험
- child attach, member lookup, outer chain을 `RNode`에 축적
- 이후 기존 `RDecl`가 forwarding/bridge 역할만 하도록 축소
- 충분히 이전되면 `RDecl` 제거

이름은 `RSymbolNode`보다 `RNode`가 더 낫다고 보았다.
- `R` 접두사에 이미 `RSymbol` 계층 의미가 들어가 있고
- `RNode`가 짧고 역할이 분명하다

## 6) `GetMember`를 child `RDecl` 기반 공통 로직으로 옮기는 구상

현재 논의에서 가장 실현 가능성이 높아 보인 아이디어는:

- `N*Decl` 생성 시 대응 `RDecl`/`RNode`도 함께 생성
- parent `RDecl`/`RNode`에 child로 자동 attach
- `GetMember`는 자기 child들 중 검색 가능한 category를 이름으로 찾는 공통 로직으로 구현

이 방향의 장점:
- semantic tree와 member lookup이 같은 축에 놓인다
- `N*Decl`는 lookup 주체가 아니라 source payload로 내려갈 수 있다

남은 걱정은 "생성 시 attach를 안 까먹는 것"인데, 이는 수동 규약이 아니라 factory/constructor 단계의 자동 등록으로 막는 것이 좋다고 보았다.

## 7) 아직 열린 점

- generic type param을 `RNode` child로 볼지 lexical scope 데이터로 볼지
- func param / lambda var / local var를 tree child로 볼지 resolver-only lexical entry로 둘지
- `RClassDecl`, `RStructDecl` 같은 concrete semantic node와 `RNode`의 관계를 상속으로 둘지, 별도 payload/view로 둘지
- `N*Decl`를 장기적으로 유지할지, `N*Info` / source facet 형태로 바꿀지

## 8) 2026-06-28 추가 정리

### 8-1) type family는 same-name generic arity overloading을 허용하지 않는다

타입 선언 쪽은 C++ 쪽에 가깝게 가져가기로 기울었다.

- 같은 scope에서 같은 이름의 type declaration은 하나의 family만 둔다
- `MyType<T>` 와 `MyType<T, U>`를 source language 차원에서 같은 이름의 별도 type family로 overload하지 않는다
- C# interop이 필요하면 import / alias layer에서 metadata identity와 arity를 해소한다

반면 함수는 같은 이름의 member를 허용할 수 있고, 실제 매칭은 name-first 이후 signature matching으로 해결하는 방향을 선호한다.

### 8-2) `RNode`는 `RIdentifier`보다 `RName` 중심이 낫다

논의 결과 `RNode`는 tree node로서 최소한의 naming 정보만 들고 있는 쪽이 더 자연스럽다고 보았다.

- `RNode`는 우선 `RName`만 갖는다
- `typeParamCount`와 parameter identity는 node identity가 아니라 declaration-specific metadata 또는 callable/type signature 쪽으로 내린다
- 함수 매칭은 `RIdentifier` key로 끝내지 않고, name으로 후보를 모은 뒤 별도 signature/parameter 정보를 사용해 정밀 매칭한다

즉 `RNode`는 "이름을 가진 scope/member tree node"이고, richer declaration identity는 category/payload 계층이 담당한다.

### 8-3) member 개념을 넓게 본다

이번 논의에서는 member를 "그 scope에서 이름으로 접근 가능한 모든 선언"으로 보자는 쪽으로 기울었다.

이 경우 예:

```text
F
  T
  U
```

처럼 함수의 type parameter도 scope member로 본다.

따라서:
- `GetMember("T")`는 `RTypeParamDecl`을 반환할 수 있다
- type-only lookup은 별도 `ResolveType`류에서 category filter를 적용한다

### 8-4) `ResolveType`와 `ResolveMember`는 shadowing 규칙이 다르다

resolver는 최소 두 종류가 필요하다는 점을 분리해서 인식했다.

1. type-only resolve
   - 현재 scope에서 같은 이름의 non-type member를 찾더라도 miss처럼 취급하고 outer로 갈 수 있다

2. general member resolve
   - 현재 scope에서 어떤 category든 hit가 있으면 즉시 종료한다

즉 `GetMember`는 넓은 scope-local lookup API로 두고, resolver가 문맥에 따라 category projection과 shadowing 정책을 달리 적용하는 쪽을 선호한다.

### 8-5) tree-first, map-later

`RNamePath -> decl` map만으로도 설명 가능한 부분이 있지만, 현재 semantics는 여전히 containment tree, outer 상승, generic scope 누적에 많이 기대고 있다.

따라서:
- 우선 `RNode` tree를 먼저 만든다
- semantic meaning이 충분히 안정화되면
- 그 뒤 path map 중심 저장/lookup 모델로 갈지 검토한다

는 순서를 선호한다.

### 8-6) accessibility는 `RNode` 바깥 정책 계층으로 빼는 쪽이 더 자연스럽다

초기에는 `RNode`/`RDecl`가 accessor와 `CanAccess`를 직접 가지는 안도 검토했지만, protected 규칙을 생각하면 단일 tree-only access check는 충분하지 않다고 보았다.

특히:
- module / namespace member의 accessibility
- class / struct / enum member의 accessibility

는 규칙 축이 다르다.

따라서 현재 leaning은:
- `RNode`는 tree / name / member relation에 집중
- accessibility metadata와 access check는 declaration payload 또는 별도 access checker/policy layer에서 담당

하는 쪽에 더 가깝다.

## Current Leaning

- semantic tree는 `RNode` 중심으로 재구성한다
- `RDecl`는 과도기 bridge로 축소한 뒤 제거를 목표로 한다
- `GetMember`는 tree child 기반 공통화가 가능하다
- `GetTypeMember`는 child + generic scope 정리가 필요하다
- `ResolveIdentifier`는 resolver 바깥으로 뺀다
- `NDecl`는 tree base라기보다 source-origin contract 쪽으로 축소하는 방향을 선호한다
- 용어는 `outer` / `base`를 구분하고 `parent`는 사용하지 않는다
- type declaration은 same-name generic arity overloading을 허용하지 않는 쪽을 선호한다
- `RNode`는 우선 `RName` 중심 node로 두고 richer signature identity는 별도 declaration metadata에 둔다
- resolver는 type-only resolve와 general member resolve를 구분한다
- accessibility는 `RNode` 메서드보다 별도 checker / policy layer로 두는 쪽에 기운다
