# Type alias와 base 해석 순서

Date: 2026-08-31
Status: discussion candidate; 사용자 확정 전

## 문제

- BuildTypeSymbol에서 alias를 포함한 type declaration shell을 모두 등록한다.
- alias RHS는 inherited type member lookup을 요구할 수 있다.
- 반대로 class base expression도 alias expansion을 요구할 수 있다.
- 따라서 alias 전체 해석과 hierarchy 전체 해석을 단순한 전후 phase로 분리하면 일반적인 상호 의존을 처리하지 못한다.

## 제안 후보

- BuildTypeHierarchy 안에서 ResolveAliasTarget(alias)와 ResolveDirectBases(class)를 필요에 따라 호출하고 결과를 cache한다.
- 각 request는 Unresolved / Resolving / Resolved / Failed 상태를 갖는다. 같은 실행 stack의 Resolving request를 다시 요구하면 resolution dependency cycle을 진단하는 안이다.
- nominal type identity를 얻는 데 그 타입의 base나 member 전체 완성을 요구하지 않는다. inherited lookup이 필요한 시점에만 해당 direct base 정보를 요구한다.
- direct base binding과 inheritance graph의 cycle 검증을 구분한다. base binding이 완료되어도 실제 상속 cycle은 별도 검사가 필요하며, 완료 전 inherited chain traversal에도 cycle guard가 필요하다.
- phase가 성공적으로 끝날 때 모든 alias target과 direct base를 완료하고 상속 cycle도 검사한다. 사용하지 않은 alias의 오류도 이때 드러낸다.
- alias RHS는 alias 선언의 lexical/unit context에서 해석한다. generic outer arguments는 사용 시 substitution하며 접근성은 기존 name-use 규칙을 유지한다.

## Lookup 불변식

- 아직 base를 해석하지 않은 상태를 base 없음 또는 inherited member 없음으로 취급하지 않는다.
- inherited lookup이 미완료라면 outer lexical fallback을 보류한다. 그렇지 않으면 나중에 발견되는 inherited member 대신 outer 이름에 잘못 결합할 수 있다.
- worklist를 택하더라도 NotFound / Pending / Error를 구분하고 의존성 완료 시 재개해야 한다. 현재 보이는 이름에 먼저 결합한 뒤 실패 항목만 반복하는 방식은 부적합하다.
- 기존 header 규칙은 자기 type parameter를 확인한 뒤 outer의 normal lookup을 사용한다. 따라서 nested class X의 base expression에서 outer C의 inherited alias를 찾는 것은 이 규칙과 일치한다.

## 기존 scheduling 안과의 충돌 — 미확정

- translation-task-scheduling.md의 unit-local / lower-order-only dependency 제약은 같은 단계의 alias/base 상호 의존을 직접 표현하지 못한다.
- 같은 module의 다른 unit에 선언된 alias/base에도 의존할 수 있다. 제한 없는 forward reference를 지원하려면 module 범위 resolver 또는 이에 준하는 module 전체 dependency 조정이 필요하다.
- global phase barrier를 유지하되 이 phase 내부에 module 범위 resolver를 허용하는 안을 제안한다. 기존 scheduling 규칙은 아직 변경하지 않는다.

## 구현 확인

- 현재 SmPhaseManager::Run은 BuildTypeHierarchy task를 등록 순서로 한 번씩 실행한다.
- SmDeclContext의 type lookup은 binder, direct member, inherited member, outer 순서다.
- SmDeclContext_ClassDecl의 inherited lookup은 준비된 GetUnboundBaseClass 결과를 사용하며 readiness/error를 별도로 표현하지 않는다.
- 소스는 수정하지 않았으며 구현 및 테스트는 수행하지 않았다.
