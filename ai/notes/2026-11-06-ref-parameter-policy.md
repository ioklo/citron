# Infra/Ref.h 함수 인자 사용 규칙

Date: 2026-11-06
Status: 사용자 확정

## 결정

- `const T&` 함수 인자는 `InRef<T>`를 사용한다.
- `T&`, `T&&` 함수 인자는 각각 그대로 사용한다.
- `const T&`와 `T&&`를 모두 받아야 하면 universal reference(forwarding reference)를 우선 사용한다. 이 방식이 복잡해지면 `TakeRef<T>`를 사용한다.
- 기존 `src/RULES.md`의 두 형태를 모두 받을 때 바로 `TakeRef<T>`를 쓰는 규칙을 위 우선순위로 갱신한다.

## 적용 범위와 전제

- 이번 결정은 함수 인자 사용 규칙이다. 반환형 규칙은 변경하지 않는다.
- 추가 확정: 실제 `const T` 객체는 사용하지 않는다. 위의 `const T&`는 기존 함수 인자 형태를 설명하는 기준이며, 실제 const 객체를 helper로 받아야 한다는 요구가 아니다.
- 따라서 실제 `const T` lvalue를 받기 위한 helper 변경은 필요하지 않다. C++ 소스와 기존 함수 signature는 수정하지 않았다.

`src/RULES.md`, current agenda와 현재 결정 기록을 갱신했다. 문서 변경만 수행했으며 빌드와 테스트는 실행하지 않았다.
