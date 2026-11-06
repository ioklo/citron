# Coroutine task의 반환형과 오류 전달

Date: 2026-11-02
Status: 철회됨 (2026-11-06). 아래 내용은 당시의 결정 이력이며 현재 적용하지 않는다. `2026-11-06-coroutine-scheduler-withdrawal.md` 참고.

## 확정

- 일반 순회처럼 호출자가 완료 결과를 기다리지 않는 작업은 `SmScheduledTask`로 표현한다. 호출 즉시 실행하고 필요한 Feature를 기다리는 곳에서 중단한다.
- 중단 가능한 결과 요청은 `SmTask<T>`를 사용하고, 실패 가능하면 T 자리에 `std::expected<Value, DiagPtr>`를 둔다. 항상 동기적으로 끝나는 계산은 일반 값 또는 기존 expected 반환을 유지한다.
- `SmScheduledTask` 경계에서 scheduler에 오류를 전달하고 해당 작업을 종료한다. 처음 논의한 `Fail` 명칭과 첫 오류 즉시 중단만 허용하는 정책은 아래의 세 진단 API로 구체화했다.
- 이벤트는 `SmEvent`로 표현하고 작업 내부에서 `co_await scheduler.Wait(event)`로 기다린다.

## 근거

- `expected<SmScheduledTask, DiagPtr>`는 작업 시작 이후 발생하는 오류를 전달할 수 없어 작업 전체의 실패 결과로 부적합하다.
- scheduler의 실패 상태만 설정해도 현재 coroutine의 실행이 자동으로 중단되지는 않는다. 결과 요청의 expected 확인과 실패 경계의 명시적 종료를 유지한다.
- 이 결정은 기존 scheduling wiki의 unit-local/lower-order 제약을 대체하는 결정이 아니다.

## 진단 API와 중단 경계 — 추가 확정

- `Fatal(diag)`: 오류를 기록하고 즉시 전체 실행 중단을 요청한다.
- `FatalDeferred(diag)`: 오류를 기록하되 현재 phase의 독립적으로 검사 가능한 작업을 계속하고, 다음 전역 phase 진입 전에 전체 실행을 중단한다. 철자는 `Deferred`다.
- `Warning(diag)`: 경고를 기록하고 계속한다. 경고만으로 최종 번역 결과를 실패시키지 않는다.
- `Fatal`과 `FatalDeferred`는 모두 최종 번역 결과를 실패시킨다. 두 API의 차이는 실패 여부가 아니라 실행 중단 시점이다.
- 전역 phase 경계는 scheduler의 `WaitBuildAllTypeSymbol` 같은 Wait API로 구현하는 현재 구조를 사용한다. 현재 헤더의 구체 이름은 `WaitBuildAllTypeSymbols()`다. 지연된 오류가 있으면 해당 경계 이후의 작업을 재개하지 않는다.
- `FatalDeferred`의 중단 시점은 실행 큐가 비는 시점이 아니라 다음 전역 phase 진입 경계다. 별도의 전역 phase 체계를 새로 도입하는 결정이 아니다.

## 안전한 구현을 위한 후속 조건

- 추가 검사는 실패한 결과와 독립적인 작업에 한정한다. 실패한 결과를 성공으로 간주해 소비자를 진행시키면 안 된다.
- 작업의 종료와 결과 준비 성공을 구분해야 한다. 실패한 provider 때문에 barrier가 영원히 대기하거나, 실패를 준비 성공으로 처리해서는 안 된다.
- 실패한 결과를 기다리는 작업의 실패 전달/취소, barrier의 종료 집계와 frame 정리의 구체 구현은 후속 설계다.
- 진단 API 호출만으로 현재 C++ 호출 흐름이 자동으로 중단되는 것은 아니다. 실패한 작업의 명시적 종료가 필요하다.

## 미확정 / 후속 구현

- 실패 후 새 작업 시작 및 continuation 재개 차단, 이벤트 handler 해제, 중단된 frame의 안전한 정리.
- 큐가 빈 상태와 전체 작업 완료의 구분 및 미완료 작업 추적.
- Run/RunLoop의 구체 반환형, 예외 처리, 진단 소유권과 각 진단 API의 구체 구현.
- 기존 cpp-async promise와 scheduler 사이의 frame 소유권 조정.

이 기록에서는 소스를 수정하거나 테스트를 실행하지 않았다.
