# Feature 기반 scheduling과 순환 검사

Date: 2026-10-06
Status: discussion candidate; 사용자 확정 전

## 사용자 제안

- 현재 SmPhaseManager의 phase 중심 구조가 복잡하므로 필요한 Feature를 클래스로 표현한다.
- Feature 활성화 또는 여러 Feature의 동시 충족을 조건으로 실행할 task를 등록한다.
- task 안에서 아직 활성화되지 않은 Feature를 활성화할 수 있다.
- 이 모델에서 circular dependency를 어떻게 검출할지 검토한다. 소스 수정 요청은 아니다.

## 검토 방향

- Feature를 특정 semantic 정보가 준비되었다는 단조로운 사실로 정의하는 후보다. 예: AliasTargetReady(A), DirectBasesReady(C), AllTypeShellsRegistered(Module).
- 실제 결과를 저장한 뒤 Feature를 활성화한다. 중복 활성화는 최초 활성화 이후 callback을 다시 실행하지 않도록 한다.
- 결과를 완성해 활성화하는 Publish/Activate와, 결과가 필요해 provider 실행을 요청하거나 기다리는 Require는 의미가 다르다. API 명칭은 미확정이다.
- 활성화 후 실행할 callback 등록은 이벤트 관계다. 이 관계의 고리는 이미 활성화된 Feature를 재활성화하는 것으로 끝날 수 있으므로 그 자체를 circular dependency 오류로 취급하지 않는다.
- cycle 검사는 아직 충족되지 않은 필수 의존성에 대해 수행한다. 단일 provider를 전제로 Feature -> provider task -> 필요한 미활성 Feature 관계를 추적하면 DFS로 순환 경로를 진단할 수 있다.
- task가 어떤 Feature를 제공할지는 등록 시 scheduler에 알려야 한다. 임의 lambda 안에 숨은 Activate 호출만으로 provider를 알 수는 없다. 동적으로 발견된 대기는 scheduler의 명시적 API로 등록해야 한다.
- 간선 추가 시 되돌아오는 경로를 찾거나, Ready/Running 작업이 없고 미완료 요구가 남은 안정된 상태에서 그래프를 검사한다. provider 누락, provider 실패와 cycle은 구분한다.
- 여러 Feature를 모두 요구하는 AND 조건은 각 미활성 Feature를 필수 의존성으로 기록한다. 선택 가능한 여러 provider/OR 조건은 단순한 고리 검출만으로 오류를 확정할 수 없어 초기 모델과 분리해서 검토한다.
- global barrier가 필요한 조건은 AllTypeShellsRegistered(Module) 같은 Feature로 표현할 수 있으나, 등록 완료를 어떻게 보장할지는 별도 설계가 필요하다.

## 기존 기록과의 관계

- 기존 translation-task-scheduling.md는 unit-local / lower-order-only 의존성을 선호하고 task 동적 의존성 추가를 제한한다. 새 제안은 이 제약을 재검토하는 후보이며 기존 wiki 규칙을 변경하지 않았다.
- 멀티스레드 구현은 2026-09-28의 보류를 유지한다. 이번 논의는 단일 스레드 scheduling 모델부터 대상으로 한다.

## 관련 기록

- `2026-08-31-type-alias-and-base-resolution.md`
- `2026-09-28-alias-resolution-and-parallelism.md`
- `../wiki/compiler/translation-task-scheduling.md`

소스 변경, 구현 및 테스트는 수행하지 않았다.

## 범위 재확인과 await 비교

- 사용자는 alias 해석만이 아니라 Skeleton/BuildTypeHierarchy/BuildNonTypeSymbol/implicit symbol/body 번역 등 SmPhaseManager 전체를 대상으로 한다고 명확히 했다.
- alias에 한정해 결과 task + await를 우선 권한 이전 답변은 전체 scheduler의 선택을 확정한 것으로 취급하지 않는다.
- 전체 모델에서는 module의 선언 수집 완료, 선언별 signature 준비, type의 member 집합 완성 등 필요한 semantic readiness의 경계를 먼저 정의해야 한다. task 실행과 이 조건을 만족하는 시점은 일대일이 아닐 수 있다.
- await는 의존성을 사용하는 문법이고 Feature/readiness는 기다리는 대상의 의미이므로 함께 사용할 수 있다. named result task의 완료가 정확히 같은 readiness를 뜻하면 별도 Feature 없이도 같은 모델을 표현할 수 있다.
- 후보 방향: semantic readiness handle을 명시하고 task 안에서 이를 await하여 동적 의존성을 등록한다. 별도 callback 기반 등록 또는 coroutine 채택 여부는 미확정이다.
- 타입 이름이나 overload 후보가 앞으로 추가될 수 있는 상태를 lookup 완료로 취급하지 않는다. member/signature 수집 완료와 등록 종료 조건을 표현해야 한다.
- 상호 재귀 함수의 body 번역은 상대 함수의 signature를 요구할 수 있지만 상대 body 번역 완료까지 기다릴 필요는 없다. 불필요하게 큰 task 간 의존성이 만드는 가짜 cycle을 피한다.

## 비교 대상 재확인: fence의 크기

- 사용자 질문의 핵심은 Feature와 await의 문법 차이가 아니라 큰 capability fence(CanSearchTypeName, CanSearchTypeNameThroughInheritance)와 선언별 completion(SignatureReady(func)) 중 어느 것이 전체 구조를 이해하기 쉬운가이다.
- 검토 의견: 전체 compiler 진행 상태를 파악하는 데는 큰 semantic fence가 단순하다. 선언별 readiness는 국소적인 의존성을 정확히 표현하지만 전체 상태와 cycle 진단의 경우의 수를 늘린다.
- 후보 방향은 큰 fence를 기본 경계로 두고, 같은 경계 내부의 상호 의존성에만 선언별 resolver/readiness를 사용하는 것이다. alias/base의 같은 단계 의존성은 큰 fence만으로 분리할 수 없다는 기존 문제를 유지한다.
- CanSearchTypeName 등의 이름은 직접 이름 후보 수집 완료인지 alias expansion까지 완료인지 정확한 보장 범위를 정해야 한다. fence 활성화에는 해당 작업 집합의 등록 완료와 모든 필수 provider의 성공이 필요하다.
- 이 비교와 추천은 아직 사용자 확정 전이다. coroutine이나 선언별 Feature의 전면 도입을 결정하지 않는다.

## 배포 전 의존성 설계 검증 요구

- 사용자는 배포 후 미처 발견하지 못한 compiler 내부 circular dependency 때문에 정상 입력의 컴파일이 실패하는 상황을 미리 막고 싶다고 명확히 했다. 실행 중 cycle 진단만으로 이 요구를 충족하지 않는다.
- 따라서 제한 없는 동적 await를 기본으로 권한 의견은 재검토한다. 선언 인스턴스는 동적으로 결정해도 기다릴 수 있는 작업/Feature 종류를 사전에 제한하고 검증하는 방향을 후보로 검토한다.
- task kind별 가능한 필수 의존성을 선언하고 종류 그래프가 DAG인지 검증하면, 모든 실제 대기가 이 관계를 준수하는 범위에서 scheduler wait cycle을 구조적으로 배제할 수 있다. 선언만 하고 임의 await를 허용하면 보장이 성립하지 않는다.
- 이는 모든 instance를 phase별로 함께 실행하는 global fence와 다른 제약이며, 독립 instance의 진행은 허용할 수 있다.
- alias/base 등 의도적으로 재귀 의존성이 필요한 부분은 단순한 종류 DAG에 들어가지 않으므로 별도 resolver의 규칙과 검증이 필요하다. 정상 source의 재귀를 잘못된 compiler wait cycle로 만드는 과도한 readiness 조건도 검토해야 한다.
- 이 방안은 사용자 확정 전이며 기존 wiki 규칙을 변경하지 않는다.

## Rust query 시스템 조사

- 공식 rustc 개발 가이드에 따르면 query는 이름/종류, key(예: DefId), 결과 타입, provider로 정의한다. 실행 시 다른 query 호출을 통해 의존성을 발견하고 결과를 캐시한다.
- query 호출은 tcx.type_of(def_id) 같은 일반 함수 호출 형태다. 이 모델을 도입하는 데 coroutine await가 필수인 것은 아니다.
- 일반 query engine은 같은 query와 같은 key의 순환 호출을 실행 중 검출한다. query 종류 자체의 의존성을 사전 DAG로 강제해 모든 instance cycle을 배제하는 모델로 설명되어 있지는 않다.
- 병렬 compiler 문서는 worker가 대기 관계로 진행하지 못할 때 deadlock handler가 cycle을 검출/보고한다고 설명한다.
- 배포 전 검증에는 compiletest 및 실제 ecosystem code를 컴파일하는 Crater 등의 테스트를 사용하지만, 모든 예상 밖 순환을 구조적으로 배제하는 보장은 아니다.
- 실제 Rust 1.90에서 기존 코드가 type-checking query cycle로 실패한 regression #146813이 보고됐고 I-cycle 및 stable-to-stable regression으로 분류됐다. 사용자가 우려한 배포 후 내부 순환 문제가 Rust에도 발생한 구체적 사례다.
- Citron에 대한 해석: query 모델은 공통 순환 진단과 테스트를 제공하나, 그 채택 자체로 정상 입력의 내부 dependency cycle이 없다는 사전 보장을 얻지는 못한다. 앞선 종류 DAG 제안은 Rust 구현의 설명이 아니라 Citron의 후보 설계다.

Sources (2026-10-06 확인):
- https://rustc-dev-guide.rust-lang.org/query.html
- https://rustc-dev-guide.rust-lang.org/queries/query-evaluation-model-in-detail.html
- https://rustc-dev-guide.rust-lang.org/parallel-rustc.html
- https://rustc-dev-guide.rust-lang.org/tests/intro.html
- https://github.com/rust-lang/rust/issues/146813

## 같은 order를 허용하는 order 제약 후보

- 사용자 제안: Feature에 order를 두고 의존 방향을 제한하되 같은 order의 의존은 허용하여 circular dependency 후보를 같은 order 내부로 좁힌다.
- 검토: A -> B가 A 완성에 B가 필요함을 뜻할 때 order(B) <= order(A)를 강제하면, 실제 cycle의 모든 node는 같은 order여야 한다. 서로 다른 order를 포함한 cycle은 구조적으로 배제된다.
- order 위반은 그 자체로 서로 다른 계층 사이의 설계 오류다. 같은 order로 한정되는 것은 order 검사에 통과한 뒤 남을 수 있는 cycle이다.
- 같은 order 간선만으로 cycle을 검사할 수 있지만, 같은 order의 순환 부재는 여전히 별도 검증이 필요하다. 아직 모든 입력의 정상 컴파일을 보장하는 설계는 아니다.
- task가 여러 Feature를 제공하거나 중간에 Feature를 활성화한다면 실제로 어느 Feature를 준비하는 계산 구간인지 분리해야 한다. generic task 완료 대기 등 scheduler를 우회하는 관계가 있으면 Feature order만으로 보장이 성립하지 않는다.
- 캐시 hit 여부와 무관하게 요구 관계의 order를 검증하면 입력/실행 순서에 따라 구조적 오류가 숨는 것을 피할 수 있다.
- order를 올리거나 모두 같은 order로 만들어 검사를 통과시키는 것은 제약을 약화한다. semantic milestone을 기준으로 order를 정하고 같은 order 허용 관계를 명시적으로 제한하는 방안을 후보로 검토한다.
- 기존 wiki의 lower-order-only(<) 규칙을 same-order 허용(<=)으로 완화하는 제안이므로 아직 확정하지 않고 기존 결정은 변경하지 않는다.

## 작은 준비 상태에서 fence를 도출하는 방향

- 사용자 관찰: 작은 Feature를 먼저 정의하고 큰 fence를 정했어야 하는데, 큰 fence 중심으로 개발하여 작은 준비 상태로 나누기 어려워진 것으로 보인다.
- 코드 근거: StructTask::SynthesizeMemberwiseCtor는 struct의 필드 목록과 각 필드의 선언 타입으로 생성자 signature를 만든다. 지금의 phase 순서는 모든 non-type symbol 및 post 작업 완료 이후에 이를 실행하므로 실제 요구보다 큰 준비 조건을 외부 실행 순서로 부여한다.
- 후보 절차: 소비하는 코드가 요구하는 정보/불변식을 찾고, 이를 만드는 작업과 준비 상태를 정의한 뒤 직접 의존성을 작성한다. 그 그래프에서 order 및 집합 완료 fence를 도출한다.
- Feature의 크기는 함수/메서드 개수보다 독립적으로 사용할 수 있는 semantic 결과나 완성된 집합을 기준으로 한다. 필드 목록 완성과 각 필드 타입 준비는 별개의 조건이다.
- task마다 클래스를 만드는 것, coroutine 채택, 모든 query의 동적 실행은 이 방향으로부터 자동 확정되는 사항이 아니다.
- 작은 Feature로 먼저 나누는 것 자체가 순환 부재를 보장하지는 않으나, 실제 필요한 의존성과 phase가 추가한 의존성을 구분해 검토하기 쉽게 한다.
- 아직 구체적 구조는 사용자 확정 전이며 소스 및 기존 wiki 규칙은 변경하지 않았다.
