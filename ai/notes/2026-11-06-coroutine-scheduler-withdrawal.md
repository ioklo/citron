# Coroutine scheduler 전환 전면 철회

Date: 2026-11-06
Status: 사용자 확정

## 결정

- `SmPhaseManager`를 사용하는 대신 `SmTaskScheduler`를 사용하는 방안을 전면 철회한다.
- 기존 `SmPhaseManager` 기반 phase 방식을 유지한다.
- 프로젝트에서 `cpp-async`를 제거한다.
- 2026-11-02에 확정했던 coroutine task 반환형, scheduler 진단 API 및 관련 후속 설계는 이 전환안과 함께 철회한다.

## 이유

- coroutine을 사용해도 큰 이득이 없다.
- 오히려 문법적으로 제약을 걸기가 더 어려워진다.
- 해결하려던 세밀한 dependency의 파악·추적 문제는 해결되지 않는다.
- 따라서 지금의 phase 방식이 더 낫다고 판단했다.

## 남은 문제와 작업

- 세밀한 dependency를 어떻게 파악하고 추적할지는 여전히 미해결이다. 새 task graph나 대체 설계를 채택한 결정은 아니다.
- `cpp-async` 제거는 확정된 작업 방향이다. 이 기록 작업에서는 소스·빌드 설정·의존성 파일을 수정하지 않았으며 제거 완료를 검증하지 않았다.
- 이전 논의는 `2026-10-06-feature-based-scheduling-and-cycles.md`, `2026-10-14-await-runtime-cycle-detection.md`, `2026-11-02-coroutine-task-error-handling.md`에 이력으로 남긴다.

문서만 갱신했으며 빌드와 테스트는 실행하지 않았다.
