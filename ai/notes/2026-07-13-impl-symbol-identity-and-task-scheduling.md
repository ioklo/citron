# 회의 / 설계 노트

Date: 2026-07-13
Title: impl 내부 정보, declaration identity, SmTranslator task scheduling

## Summary

- `impl`은 이름으로 resolve되는 tree declaration이 아니라, struct 또는 extension의 내부 conformance 구현 정보로 본다.
- 외부 surface를 나타내는 `RSymbol`과 source/internal implementation payload인 `N*Info`를 분리한다.
- tree child의 고유성은 `Identifier`가 결정한다. 함수 overload signature는 identifier에서 제거하지 않되, 가변 `vector` 대신 공유 가능한 불변 parameter-signature handle로 가볍게 만든다.
- SmTranslator의 전역 unit 간 순서는 phase로 유지하고, 한 unit 내부의 세밀한 선행 조건은 task dependency로 표현하는 방향을 검토한다.

## 1. impl의 소유와 공개 surface

`impl`은 이름을 선언하지 않으므로 `RDecl` tree의 child가 아니다. `RDecl`은 이름으로 resolve 가능한 declaration이라는 불변식을 유지한다.

```text
RStructDecl
  └─ NStructInfo
       └─ impl S : Trait의 implementation data

RExtensionDecl
  └─ NExtensionInfo
       └─ impl Bundle for S : Trait의 implementation data
```

- `RStructDecl`, `RExtensionDecl`은 외부에 보이는 named symbol이다.
- `NStructInfo`, `NExtensionInfo`는 witness function body, associated type 구현, requirement 대응 같은 내부 정보를 가진다.
- `(target, trait)` conformance 사실, public extension bundle의 target/trait 목록, 외부에서 필요한 witness identity는 module declaration surface에 남긴다.
- `N*Info`는 `RDecl` base의 공통 variant/tag로 두기보다 해당 concrete `R*Decl`에 typed payload로 붙이는 편이 잘못된 조합을 표현하지 않는다.

## 2. Identifier와 함수 overload

tree의 자식 노드는 유일해야 하므로, function overload를 구분하는 signature 정보는 identifier identity에 필요하다. 따라서 name-only key로 단순화하는 것은 적합하지 않다.

현재 `RIdentifier`의 `std::vector<RType*> paramIds`는 `GetIdentifier()`마다 생성되므로 무겁다. 검토한 방향은 다음과 같다.

```text
RIdentifier
  - name
  - typeParamCount
  - paramIdsHandle  // 불변 parameter signature를 가리키는 pointer 또는 작은 handle
```

- parameter signature는 parameter type, passing kind, variadic 여부처럼 overload/ABI 구분에 필요한 정보를 담는다.
- tree lookup에 mangled string을 직접 쓰지 않는다. mangle/demangle은 external function symbol id/CTI 경계에서만 필요하다.
- `RTypeArguments*`처럼 factory 수명 아래에서 공유하는 의미 객체를 가리키는 형태를 고려할 수 있다.

## 3. Phase와 unit-local task dependency

`impl` binding은 target struct shell과 trait requirement signature가 준비된 뒤에만 가능하다. 이를 전역 phase만으로 계속 표현하면 phase 수가 늘어난다.

검토 방향:

- unit 간 순서와 external module boundary는 global phase/CTI가 담당한다.
- 한 unit 내부에서는 task가 `requires`와 `provides` readiness를 선언한다.
- task 구현은 lambda로 가능하지만, graph/debug/diagnostic을 위한 `TaskId`, 표시명, source location은 필요하다.

```text
ImplBind(S, Trait)
  requires: S.Declared, Trait.SignatureComplete
  provides: Conformance(S, Trait).Bound
```

task graph는 먼저 등록하고 freeze한 뒤, 실행 전에 missing provider와 cycle을 검사할 수 있다. task 실행 중 임의로 dependency를 추가하면 이 사전 검사의 효용이 줄어든다.

## 4. Task order 제약

cycle을 구조적으로 줄이기 위해 task에 작은 의미 단위의 order/rank를 둔다.

```text
SymbolShell < Signature < Conformance < Synthesis < Body
```

task는 자기보다 낮은 order의 task에만 의존하도록 제한한다. 이 방식은 phase와 달리, 높은 order task가 필요한 lower-order dependency만 완료되면 실행될 수 있다. 단, 숫자 order를 임의로 늘리지 않고 의미 있는 소수의 milestone으로 유지해야 한다.

## 5. 병렬화

dependency graph는 phase barrier보다 세밀한 병렬화를 허용하지만, 현재는 unit 단위 병렬화만으로도 충분할 수 있다.

- global phase 안에서 여러 unit을 병렬 처리한다.
- unit 내부 task graph는 우선 단일 thread로 운용해 readiness/diagnostic 모델을 안정화한다.
- graph 도입의 우선 목적은 병렬화보다 local dependency와 phase 난립 방지다.

## Open Points

- canonical conformance header와 `NStructInfo` implementation payload의 정확한 API 경계
- parameter-signature handle의 소유자, equality 및 CTI serialization 방식
- task order를 compile-time task-kind constant로 둘지, 등록 시 metadata로 둘지
- unit-local graph의 freeze 시점과 task failure propagation 규칙
