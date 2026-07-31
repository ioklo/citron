# 회의 / 설계 노트

Date: 2026-07-30
Title: Swift식 opaque result identity와 `RType_Opaque` interning

## Status

- opaque result type identity와 `RType_Opaque`의 semantic shape는 확정했다.
- factory storage container 선택은 구현 시 검증할 방향이다.

## Confirmed Decisions

### 1. `some Trait`는 Swift식 declaration-scoped opaque type이다

surface syntax는 계속 함수 return position 전용 marker로 유지하지만, semantic model은
`RFuncReturn_Normal(RType_Opaque(...))`를 사용한다.

opaque result type identity는 단순 trait constraint가 아니라 다음으로 정한다.

```text
opaque-result owner declaration
+ complete applied generic arguments
```

같은 trait constraint나 같은 backing concrete type을 반환해도 owner declaration이 다르면
static opaque type은 다르다.

```citron
some Tr F() { return X(); }
some Tr G() { return X(); }
```

`F()`와 `G()` 결과는 서로 다른 opaque type이다. 반대로 같은 declaration을 같은 generic
arguments로 호출한 결과는 같은 opaque type이다. 비교 연산은 declared trait constraint가
그 연산을 제공할 때만 가능하다.

`return F()`로 구현한 `some Tr G()`는 허용할 수 있다. `G`의 implementation은 F의 opaque
type을 하나의 fixed underlying type으로 고르지만, caller-visible static identity는 여전히
G 소유 identity이며 F와 합쳐지지 않는다.

### 2. 모든 applied generic argument가 identity에 들어간다

trait type argument만으로는 충분하지 않다. owner의 outer generic arguments와 함수 own
generic arguments를 flattened order로 모두 포함한다.

```citron
struct S<T1>
{
    some Tr<T1> F<T2>();
}

S<T1>.F<short>()
```

의 opaque type identity owner application은 `[T1, short]`다. `T1`은 body compile 중
open type variable일 수 있으며, 이는 normal constructed type처럼 `Apply`로 이후
치환한다.

```text
S<T1>.F<short>()  -> Opaque(F.result, [T1, short])
S<int>.F<short>() -> Opaque(F.result, [int, short])
```

따라서 `F<X>()`와 `F<Y>()`의 result type은 다르다. generic body의
`Opaque(F.result, [U])`와 `Opaque(F.result, [Y])`도 `U == Y`가 증명되지 않는 한 다르다;
특정 `G<Y>()` instantiation은 generic body를 재-type-check하지 않는다.

### 3. `RType_Opaque`는 두 applied declaration relation을 보관한다

현재 함수 return 범위의 intended shape는 다음과 같다.

```cpp
class RType_Opaque : public RType
{
public:
    RAppliedDecl<RTraitDecl> appliedTrait;
    RAppliedDecl<RFuncDecl> appliedOwnerFunc;
};
```

- `appliedTrait`는 source-level trait surface와 witness lookup에 사용한다.
- `appliedOwnerFunc`는 opaque static type identity의 owner이며, `typeArgs`는 outer와
  function generic arguments를 모두 포함해야 한다.

함수 object는 return type을 만들기 전에 skeleton에서 생성할 수 있으므로, function
pointer를 owner로 참조하는 것 자체는 construction cycle을 만들지 않는다. 향후 property나
subscript opaque result까지 허용하면 function-specific owner field를 common opaque-result
owner abstraction으로 일반화한다.

### 4. `RTypeIdentifier` 형식

opaque type identifier는 trait constraint와 applied owner function identity를 함께 쓴다.

```text
$O(TraitRTypeIdentifier,OwnerFuncRIdentifier<FullAppliedArgs...>)
```

예:

```text
$O(MyModule::Tr<$T0>,MyModule::S.F(...)<$T0,$ps>)
```

실제 owner segment spelling(예: generated `.result` segment)은 RIdentifier encoding을
구현할 때 정하되, trait ID만의 `$OMyModule::Tr<...>`로 opaque type을 intern하지 않는다.

### 5. Factory interning storage 후보

identity fields가 immutable인 flyweight `RType`은 type object 자체를 hash/equality source로
쓸 수 있다. `is_transparent` hash와 equality comparator를 사용하면 lookup view로 `find`할
수 있어 별도 stored key를 만들 필요가 없다.

현재 구현 실험 후보는 다음이다.

```cpp
std::deque<RType_Opaque> opaqueTypeStorage;
std::unordered_set<RType_Opaque*, RTypeOpaqueHash, RTypeOpaqueEqual> opaqueTypeIndex;
```

set은 pointer만 저장하되 hash/equality가 pointee의 immutable identity fields를 읽는다.
`deque`는 append-only factory storage와 stable `RType*`를 제공한다. 이 선택은
`unordered_set<T>`가 element를 const로 노출해 current mutable `RType*` API와 맞지 않는
문제를 피한다. 성능/메모리 측정과 exception-safety 처리는 구현 시 확인한다.

## Rationale

Swift의 opaque result는 declaration과 applied generic arguments로 unique하다. 이 identity는
same-owner result 간 `Self`/associated type/generic relation을 보존하면서, 다른 declaration의
같은 trait constraint를 type-erased existential처럼 합치지 않는다.

Citron도 metadata accessor와 opaque sret ABI를 사용하더라도, ABI metadata identity와
static opaque type identity를 분리하지 않는다. metadata accessor는 owner-specific opaque
type의 runtime layout/witness contract를 제공한다.

## Follow-up

- `$O(...)` 내부의 generated opaque-result owner segment를 `RIdentifier` grammar에 고정한다.
- `RAppliedDecl<RFuncDecl>::typeArgs`가 outer + function arguments의 complete application을
  실제로 보장하도록 call-resolution/creation path를 정리한다.
- `RType_Opaque::Apply`와 factory hash/equality를 four-field identity에 맞춰 구현하고 tests를
  추가한다.
- factory storage 후보를 구현·측정한 뒤 표준 interning container pattern으로 채택할지 결정한다.
