# Ref parameter 및 view 사용 규칙

Date: 2026-11-25
Status: 사용자 확정

## 결정

- 함수 인자에서 `const T&` 대신 전용 view와 reference helper를 사용한다.
- `const std::vector<T>&`는 값으로 받는 `std::span<T>`로 대체한다. 실제 const 객체를 사용하지 않는 프로젝트 방침에 따라 `span<const T>`를 기본 대체형으로 삼지 않는다.
- `const std::string&`는 값으로 받는 `std::string_view`로 대체한다.
- 그 외 `const T&`는 `InRef<T>`로 대체한다. `T&`와 `T&&`는 각각 그대로 사용한다.
- `TakeRef<T>`는 별도의 semantic 분류가 아니라 `T&`와 `T&&` 입력을 모두 처리할 때 쓰는 도구다. 저장 시 `Take()`는 lvalue 입력을 복사하고 rvalue 입력을 이동한다.
- 기존 forwarding reference 우선, 복잡해지면 `TakeRef<T>`를 쓰는 선택 기준은 유지한다.
- `std::shared_ptr<T>`의 InRef/TakeRef 형태는 `InRefPtr<T>` / `TakeRefPtr<T>`로 쓴다. `(*t)->` 대신 `t->`로 접근하기 위한 전용 helper다.
- `InRefPtr<T>`를 다른 InRefPtr 인자에 넘길 때는 wrapper 그대로 전달한다. 공유 소유권을 저장하는 지점에서 `Share()`를 호출한다.
- `TakeRefPtr<T>`는 shared_ptr의 lvalue/rvalue 입력에 따라 복사/이동을 선택해야 할 때 사용한다. InRefPtr로 받는 호출에 `move()`를 적어도 shared_ptr 소유권이 이동하지는 않는다.

## 적용 범위와 수명

- 이번 기록은 함수 인자 규칙이다. 반환형 규칙을 일괄 변경하는 결정은 아니다.
- 실제 `const T` 객체를 사용하지 않는 기존 전제는 유지한다. InRef는 C++의 const 제한을 타입으로 강제하는 장치가 아니다.
- span/string_view와 reference helper는 원본을 소유하지 않는다. 호출 이후 보관할 때는 원본 수명을 보장하거나 소유하는 값으로 복사/이동한다.
- vector의 크기 변경 등 컨테이너 자체의 조작이 필요한 `T&` 인자를 span으로 무조건 바꾸는 규칙은 아니다.

## SmTranslator 정리 및 검토 상태

- 사용자가 working copy에서 Ref 사용을 정리했다. 주요 변경은 `InRef<shared_ptr<T>>`에서 `InRefPtr<T>`로의 전환과 저장 지점의 Share 명시다.
- 검토에서 SmPhaseManager 호출의 중복 Share, InRefPtr 전환 후 남은 move, TranslateSyntax의 const vector& 인자를 후속 정리 후보로 안내했다.
- 사용자가 세 항목을 모두 정리했고 마지막 두 vector 인자는 span으로 바꿨다고 보고했다.
- 검토 당시 변경된 SmTranslator cpp 29개는 MSVC `/Zs` 구문 검사를 통과했다. 일반 x64 Debug 빌드는 Infra/Logging의 C1902(PDB 관리자 불일치)로 중단되어 링크와 실행 검증은 완료하지 못했다.
- 위 검증은 후속 3건 정리 이전 상태에 대한 결과다. 이번 기록 작업에서는 후속 수정본 빌드/실행 검증을 하지 않았다.

## 관련 기록

- 현재 코드 스타일: [src/RULES.md](../../src/RULES.md)
- 기존 결정 이력: [2026-11-06 Ref parameter 정책](2026-11-06-ref-parameter-policy.md)
