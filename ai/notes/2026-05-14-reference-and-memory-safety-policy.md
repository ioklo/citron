# 회의 / 설계 노트

Date: 2026-05-14
Title: reference / memory safety 정책 정리

Status
- draft

Summary
- Citron v1은 Rust 수준의 전역 메모리 안전성을 목표로 하지 않는다.
- plain reference는 표면 문법에 존재하지만, type layer의 일반적인 first-class type constructor로 두지 않는다.
- type layer의 기본 축은 `T`와 `T*`이며, `T&`는 제한된 문맥에서만 나타나는 alias/reference 표기다.
- `T&`의 lifetime safety, dangling-free, iterator/reference invalidation 부재는 언어가 전역 보장하지 않는다.
- 복잡한 자료구조에서 stable reference를 일반적으로 보장하려는 시도는 v1 범위에서 제외한다.

Context
- `T&`를 Rust식 safe reference로 만들려는 시도는 return, temporary, parameter lifetime, aliasing, container invalidation까지 한꺼번에 끌어들여 설계 비용이 매우 커진다.
- 그 복잡도를 감수해도 raw pointer, FFI, 복잡한 runtime object graph 때문에 전체 메모리 안전성을 완전히 보장하기는 어렵다.
- Citron의 주 사용자는 언어의 제약보다 표현력과 단순한 모델을 더 중시하며, `T&`가 나오면 lifetime을 직접 고려하는 작업 방식을 전제로 둔다.
- 따라서 Citron은 "전역 soundness"보다 "제한된 표면 reference 문법 + 단순한 pointer/value 모델" 쪽이 더 적합하다.

Existing direction to preserve
- `struct` 인스턴스 메서드의 `this`는 `S&`로 본다.
- plain ref parameter는 `T&`로 표기하며 alias passing mode를 사용한다.
- parameter kind로 `[in] T&`, `[move] T&`, `[forward] T&` 같은 변형이 이미 논의되어 있다.
- 로컬 alias 선언은 `var& y = x;` 같은 표면 형태를 사용해 왔다.
- 즉 `&`는 "일반 type constructor"라기보다, parameter / return / local alias / implicit `this` 같은 특정 surface slot에 붙는 표기라는 해석이 현재 방향과 더 잘 맞는다.

Decisions
- Citron은 전역 dangling-free / use-after-free 부재를 목표로 하지 않는다.
- plain reference는 허용한다. 다만 `T&`를 type layer의 일반 타입으로 승격하지는 않는다.
- type layer의 기본 축은 다음 둘로 둔다.
  - value / handle 의미의 `T`
  - raw pointer 의미의 `T*`
- `T&`는 제한된 표면 문맥에서만 허용한다. 현재 기준 후보는 다음과 같다.
  - 함수 parameter
  - 함수 return
  - 로컬 alias 선언
  - `struct` 인스턴스 메서드의 implicit `this`
- 위 의미에서 `T&`는 "non-owning alias/reference"다.
  - storage를 소유하지 않는다.
  - lifetime safety는 언어가 정적으로 보장하지 않는다.
  - 일부 obvious misuse 진단은 넣을 수 있지만, soundness claim은 하지 않는다.
- `T&`를 일반 타입처럼 다루는 기능은 v1에서 열지 않는다.
  - field type
  - generic type argument
  - container element type
  - nullable / tuple / 기타 일반 type expression 내부의 자유로운 출현
  는 우선 비대상으로 둔다.
- `T*`는 계속 허용한다.
  - raw pointer 자체를 금지하거나, 포인터를 만들기 위해 별도 `unsafe` 표기를 강제하는 방향은 우선 채택하지 않는다.
  - pointer/reference 사용의 위험성은 타입과 연산 의미로 드러난다고 본다.
- container mutation에 따른 reference / iterator invalidation은 언어 공통 규칙이 아니라 각 타입의 contract로 둔다.
  - 예: `List<T>`는 구조 변경 후 기존 element reference가 invalid 될 수 있다.
  - 어떤 자료구조가 stable reference를 제공할지는 그 자료구조의 별도 설계 문제로 본다.
- 복잡한 자료구조에서 `T*`/`T&`를 장기간 유지하는 안전성까지 언어 차원에서 일반 해법으로 풀려 하지 않는다.
  - 그 요구가 구현을 심하게 제한하면, 그런 사용 패턴을 지원하지 않는 쪽을 우선 고려한다.

Non-Goals
- Rust 수준의 전역 lifetime / borrow safety
- `T&`를 일반적인 first-class type constructor로 만드는 것
- 모든 reference return의 soundness 보장
- 모든 container에 대한 stable reference 제공
- iterator/reference invalidation의 전역 자동 추적

Implications
- `return T&`는 허용 가능하다.
  - 다만 dangling 가능성은 언어 모델의 일부로 받아들인다.
- `T&` 관련 논의는 "이 타입을 일반 type expression 어디에나 둘 수 있는가"보다 "어떤 surface slot에서 reference 표기를 허용할 것인가" 중심으로 정리한다.
- `foreach`, iterator, collection API 설계는 전역 메모리 안전보다 각 API의 contract가 무엇을 보장하는가 중심으로 정리한다.
- 안전성이 필요한 일부 영역은 별도 checked abstraction이나 runtime trap을 사용할 수 있지만, 이를 언어 전체 기본 모델로 일반화하지는 않는다.
- 프로젝트 차원의 규율, 코드 리뷰, 테스트가 여전히 중요하다.

Rationale
- Citron은 표현력과 단순한 정신 모델을 우선한다.
- `T&` 자체를 포기하면 언어 사용성이 크게 떨어진다.
- 반대로 `T&`를 sound하게 만들거나 일반 타입으로 승격하려는 시도는 언어 전체를 과도하게 복잡하게 만들고, 실제 사용성 대비 이득이 작다.
- 현재까지의 노트 흐름도 `T&`를 "parameter/alias/this 문맥의 reference 표기"로 다루는 쪽에 더 가깝다.
- 따라서 Citron은 C++ 쪽에 더 가까운 reference/pointer 모델을 채택하되, `T&`는 제한된 surface form으로 유지하는 방향이 더 적합하다.

Action Items
- [ ] `T&`가 허용되는 surface slot 목록(parameter/return/local alias/implicit this)을 별도 노트로 확정
- [ ] `return T&`의 최소 진단 규칙(local 반환, temporary 반환 등)을 별도 노트로 정리
- [ ] `List<T>`의 reference / iterator invalidation contract 초안 작성
- [ ] `foreach`가 value / reference / pointer 중 어떤 binding mode를 제공할지 별도 노트로 정리
