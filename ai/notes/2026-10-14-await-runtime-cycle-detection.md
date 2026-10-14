# await 의존성의 런타임 순환 검출 검토

Date: 2026-10-14
Status: conditional user preference; 우선 검토 방향, 세부 설계 미확정

## 사용자 의향

- 사용자는 런타임 검출의 이득이 더 크다고 판단되면 그 방향으로 진행해도 괜찮다고 밝혔다.
- 앞서 우려한 정상 입력의 컴파일 실패, 사용자 우회책 부재, 예상 밖 의존성으로 인한 구조 변경 및 대응 지연 위험은 해소된 것으로 취급하지 않는다.

## 에이전트 판단과 검토 방향

- readiness 경계를 탐색하는 현재 단계에서는 전면적인 정적 DAG 강제보다 명시적 의존성 추적과 런타임 cycle 검출로 실제 구조를 확인하는 이득이 더 크다고 판단한다. 측정이나 구현으로 검증된 결론은 아니다.
- 일반적인 async 언어의 안전장치만으로 compiler의 의미적 의존성 순환이 해결되지는 않는다.
- runtime 검출은 무한 대기를 피하고 요청 종류, 대상 선언, 요청 위치와 순환 경로를 진단하는 방향이다.
- source의 잘못된 순환과 compiler가 과도한 준비 조건을 요구해 생긴 내부 순환을 구분해야 한다.
- 정상 재귀와 forward reference를 회귀 테스트로 검증하고 안정된 의존성 경계부터 제한하는 절충안을 검토한다.
- 실제 필수 의존성 cycle은 실행 순서 변경이나 단일 스레드 실행만으로 해결되지 않는다. 로그가 원인을 보여도 수정이 국소적이라는 보장은 없다.

## task 구현 비교 — 미확정

- 대상은 alias만이 아니라 Syntax에서 declaration-space RSymbol을 구성하고 body를 번역하는 SmPhaseManager 전체다.
- GlobalFuncTask는 symbol 생성/body 번역에 같은 객체를 등록하고 rFuncDecl을 보관한다. coroutine은 이런 단계 사이의 실행 위치와 지역 상태를 한 함수에 표현할 수 있다.
- Microsoft cpp-async는 즉시 실행, task 결과의 한 번 소비, 별도 공유 상태 및 atomic 사용, 완료 시 frame 정리라는 계약을 갖는다. task 객체를 버려도 coroutine 실행 자체가 중단되는 것은 아니다.
- 자체 SmTask는 scheduler의 frame 소유권, 실패 시 waiter/queue 정리, 허용 await 대상, 진단 metadata를 compiler에 맞추기 쉽지만 정확한 수명 관리와 유지보수를 직접 책임진다.
- 에이전트는 자체의 작은 task를 우선 추천했으나 사용자 확정은 아니다. 성능 우위는 측정하지 않았다.
- 기존 expected 오류 전파 매크로는 일반 return을 사용하므로 coroutine 본문에 그대로 사용할 수 없다. expected를 task 결과로 감싸는 것만으로 자동 전파되지 않는다.
- ImplTraitTask는 실행 중 ImplTraitFuncTask를 등록한다. 등록 종료/freeze 시점을 설계할 때 현재 동적 등록을 고려해야 한다.

## 호출 API와 Feature 객체 비교 — 후보

- Require(readiness) 이후 일반 context API를 사용하는 방식과, co_await context.ResolveType(syntax, scope)처럼 필요한 결과를 직접 요청하는 방식을 비교했다.
- 결과별 API는 provider가 선행 조건을 책임지지만, 아무 task나 자유롭게 기다리게 하면 compiler 내부 순환을 사전에 제한하기 어렵다.
- 사용자 제안: co_await context.GetTypeResolveFeature()로 객체를 얻고 그 객체에서 ResolveType/GetBase를 동기 호출한다. 이 Feature는 연산을 안전하게 사용할 수 있다는 capability 역할이다.
- Feature 획득이 모든 메서드의 준비를 보장하면 한 연산만 필요한 소비자도 다른 연산의 준비까지 기다려 barrier가 커질 수 있다.
- API를 TypeResolver 객체로 묶되 각 메서드를 await하는 안은 나중에 적용 가능한 코드 조직 선택이다. 당장의 핵심은 결과별 완료 계약과 허용 의존성이다.
- RType identity의 획득은 그 타입의 base/member/conformance/body 전체 완성을 뜻하지 않는다. signature만 필요한 소비자가 body 전체 task를 기다리면 가짜 cycle을 만들 수 있다.

## 정적 예방과 런타임 검출의 구분

- 요청 종류별 허용 간선을 DAG로 제한하거나 strict lower-order를 강제하고 모든 실제 대기에 적용하면 scheduler cycle을 구조적으로 배제할 수 있다. 자체 promise의 await_transform과 typed request는 구현 후보였다.
- 같은 order를 허용하면 그 내부의 cycle 부재는 보장되지 않는다. 여러 결과를 중간에 publish하는 coroutine은 현재 계산 구간의 분류도 필요하다.
- alias/base처럼 의미적으로 재귀적인 해석은 단순한 종류 DAG에 맞지 않으므로 별도 resolver의 규칙이 필요하다.
- 런타임에 완전한 필수 의존성 그래프가 있으면 cycle 탐색은 일반 그래프 문제다. 어려움은 외부 이벤트, OR 조건 등을 포함한 대기 의미를 정확하게 기록하는 데 있다.
- reference count와 비슷하게 국소 정보만으로는 순환을 알 수 없지만, 관계를 추적하면 검출 가능하다. 일반 프로그램의 모든 미래 대기를 정확하게 정적으로 분석하는 것과 제한된 compiler request graph의 검출은 구분한다.

## 다른 언어의 async 조사 요약

- C#: 일반 Task 상호 await는 cycle 예외 없이 미완료로 남을 수 있다. lock 본문의 await 금지와 ConfigureAwait는 일부 위험에 대한 장치이며 일반 wait graph 검사와 다르다.
- Swift: actor reentrancy는 actor 점유로 인한 일부 교착을 피하지만 임의 task 결과 대기의 순환을 막지 않는다. CheckedContinuation의 resume 규약 검사도 일반 cycle 검출은 아니다.
- Rust: Future/Pending/Waker 자체에는 의미적 wait graph가 없다. Send 제약과 await_holding_lock lint는 일부 패턴을 잡는다. async 재귀 E0733은 future 타입의 무한 크기 문제이며 순환 대기 금지가 아니다.
- TypeScript/JavaScript: Promise의 직접 self-resolution은 TypeError로 reject하지만 서로 다른 Promise의 순환을 일반적으로 검출하지 않는다. Node의 미완료 top-level await 종료 동작도 cycle 경로 검출과 다르다.
- 이 조사에서 네 언어의 일반 async 모델만으로 Citron이 요구하는 사전 순환 차단을 얻을 수 없다고 판단했다.

Sources (2026-10-14 확인):
- https://github.com/microsoft/cpp-async/blob/main/include/async/task.h
- https://github.com/microsoft/cpp-async#task
- https://devblogs.microsoft.com/dotnet/are-deadlocks-still-possible-with-await/
- https://devblogs.microsoft.com/dotnet/configureawait-faq/
- https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock
- https://github.com/swiftlang/swift-evolution/blob/main/proposals/0306-actors.md
- https://github.com/swiftlang/swift-evolution/blob/main/proposals/0300-continuation.md
- https://github.com/swiftlang/swift/blob/main/stdlib/public/Concurrency/Task.cpp
- https://doc.rust-lang.org/std/future/trait.Future.html
- https://tokio.rs/tokio/tutorial/shared-state
- https://rust-lang.github.io/rust-clippy/stable/index.html#await_holding_lock
- https://rust-lang.github.io/async-book/07_workarounds/04_recursion.html
- https://tc39.es/ecma262/multipage/control-abstraction-objects.html#sec-promise-resolve-functions
- https://nodejs.org/api/esm.html#top-level-await

## ResolveType 내부 barrier — 최신 사용자 제안

- 사용자 제안: RSymbol tree가 존재해도 구성 중이면 검색 결과를 확정할 수 없다. 모든 Type Tree의 구성 보장을 기다리는 barrier가 필요하며, 호출자보다 ResolveType 등의 내부 구현에서 사용한다.
- 아래는 이에 대한 에이전트의 보완 해석이며 구체 계약의 사용자 확정은 아니다.
- barrier를 모든 타입의 해석 완료가 아니라 검색 대상 타입 declaration shell의 수집/등록 종료로 정의하는 후보를 권한다. 이름, lexical 위치와 lookup에 필요한 binder 정보가 준비되고 해당 수집 범위에 새 타입 선언이 추가되지 않아야 한다.
- alias target과 direct base는 barrier 이후에도 미해석일 수 있으며 실제 필요 시 별도 결과를 await한다.
- 검색 실패뿐 아니라 검색 성공도 등록 종료가 필요할 수 있다. 이후 더 가까운 scope의 선언이 추가되면 먼저 찾은 이름에 잘못 결합할 수 있으므로, 현재 찾았다는 이유만으로 barrier를 건너뛰면 안 된다.
- shell 등록 완료는 선언 처리 coroutine 전체의 종료와 분리해야 한다. shell 등록 후 ResolveType을 호출하는 coroutine 전체를 barrier가 기다리면 자신이 기다리는 결과를 다시 기다리는 cycle이 생긴다.
- barrier 활성화에는 수집 대상/provider의 등록 종료와 필수 작업 성공을 확인해야 한다. barrier 대기도 의존성 추적에 포함한다.
- 현재 SmTranslator.cpp의 TranslateSyntax는 모든 script를 방문한 뒤 phaseManager.Run()을 호출한다. 내부 barrier는 이 실행 순서가 주던 수집 경계를 명시하는 방향이다.
- 후보 구조: 검색 집합의 확정에는 내부 barrier, 확정된 집합에서 필요한 결과의 준비에는 개별 await. barrier의 정확한 module/unit/scope 범위, 동적 선언 추가와 실패 처리는 미확정이다.

## 기존 기록과의 관계 및 미확정 사항

- 2026-10-06의 사전 차단 요구를 조건부로 완화하는 의향이다. 기존 wiki의 unit-local/lower-order-only 규칙을 대체하는 구체 규칙은 아직 확정하지 않는다.
- 결과별 await API, Feature/capability 객체, task 구현 선택, 검출 알고리즘과 provider 등록/종료 계약은 미확정이다.
- 관련 기록: `2026-10-06-feature-based-scheduling-and-cycles.md`, `../wiki/compiler/translation-task-scheduling.md`.

소스 및 wiki는 변경하지 않았다. 구현과 테스트는 수행하지 않았다.
