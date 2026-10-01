# Alias 해석과 병렬화 가능성

Date: 2026-09-28
Status: discussion candidates; 멀티스레드 구현 결정은 사용자 요청으로 보류

## 단일 스레드 해석 후보

- 사용자 제안: alias를 Unresolved로 수집한 뒤 순회하고, Resolve 중 필요한 다른 alias를 먼저 재귀적으로 해석한다. 재귀 호출에서 Resolving을 만나면 cycle 오류로 처리한다.
- 보완 논의: 사용된 alias뿐 아니라 선언된 모든 alias를 검사하고, 같은 module의 모든 unit에서 선언 shell을 먼저 등록한다.
- 바깥 순회는 Resolved/Failed를 건너뛴다. 각 Resolve가 완료된 뒤 다음 항목을 처리하는 단일 스레드 모델에서 바깥 순회가 Resolving을 만나면 내부 상태 관리 오류다.
- 성공 시 Resolved, 실패 시 Failed로 전이하고 의존성 실패를 전파한다. phase 종료 전에 모두 처리하므로 외부 사용 시점까지 해석을 미루지는 않지만, phase 내부 순서는 의존성을 따라 결정한다.
- alias/base 상호 의존성, 미완료 inherited lookup의 outer fallback 금지, 별도 inheritance cycle 검사는 기존 논의대로 고려해야 한다.
- 구체적인 구현 방식은 아직 확정하지 않았다.

## 멀티스레드 가능성 검토

- 다른 worker의 Resolving은 정상적인 실행 중 상태일 수 있으므로 그 상태만으로 cycle을 판단할 수 없다.
- task가 미완료 의존성을 만나면 Waiting으로 보류하고 worker는 다른 작업을 실행하는 방식이 후보다. 의존성 완료 후 task를 재개하며 실패도 전파한다.
- A → B는 A가 B의 완료를 필요로 한다는 관계다. 간선 추가 전에 B에서 A로 도달 가능한지 검사하면 cycle을 검출할 수 있다. 검사와 추가는 함께 동기화해야 한다.
- 대안은 Ready/Running 작업이 없고 Waiting 작업이 남은 안정된 상태에서 의존성 그래프를 일괄 검사하는 것이다. 순환 자체와 그 순환 때문에 대기하는 작업은 구분한다.
- task 상태 변경, 완료 통지, RFactory/symbol 변경, 결과 공개와 진단 순서의 동기화가 추가로 필요하다.
- 사용자는 이번에는 멀티스레드 가능성을 알아보는 정도로 마무리하기로 했다. 병렬 scheduler나 cycle 알고리즘을 채택한 것으로 취급하지 않는다. 기존 scheduling 규칙도 변경하지 않는다.

## 관련 기록

- `2026-08-31-type-alias-and-base-resolution.md`
- `../wiki/compiler/translation-task-scheduling.md`

소스 변경, 구현 및 테스트는 수행하지 않았다.
