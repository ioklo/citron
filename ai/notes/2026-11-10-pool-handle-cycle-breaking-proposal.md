# Pool handle을 통한 순환 참조 해소 제안

Date: 2026-11-10
Status: 제안 기록; pool 의미론과 구현 방식 미확정

## 배경과 사용자 제안

- Class 객체는 `new`로만 생성하고 RC로 수명을 관리한다. Pool을 별도의 객체 생성 경로나 arena allocation 기능으로 만들지 않는다.
- Class/interface handle은 retain/release를 고려하여 NBC로 분류하려는 의도다. 현재 wiki의 BC 분류와 충돌하며, 해당 기준 문서의 정정은 이번 노트 범위에 포함하지 않는다.
- 순환 참조를 해소하기 위해, RC 포인터를 `pool`로 한 번 더 감싸고 `pool_handle`을 통해 일괄 해제하는 방식을 제안했다.
- 앞선 후보였던 RC와 무관한 shared 객체 강제 삭제 대신 검토하는 방향이다. Weak와의 병행 여부는 미정이다.

사용자 예제:

```citron
class A { pool B b; }
class B { pool A a; }

pool_handle h;
pool B b = pool (h, new B());
pool A a = pool (h, new A());

// 상호 참조. pool 값도 .으로 멤버에 접근한다.
b.a = a;
a.b = b;

h.destroy();
```

위 예제는 제안 문법과 의도를 나타낸다. 필드 접근성 및 초기화 규칙까지 검증된 실행 예제는 아니다.

## 논의 중인 의미 모델

다음은 사용자 예제를 실현하기 위해 assistant가 제안한 공유 슬롯 모델이며, 사용자 확정 규칙은 아니다.

- `pool (h, new A())`는 생성된 객체의 strong RC 포인터를 h에 등록된 공유 슬롯에 보관한다.
- `pool A`의 복사는 객체의 독립적인 strong 참조를 각각 만드는 대신 같은 슬롯을 공유한다.
- 객체 내부의 `pool` 필드도 슬롯을 통해 대상을 참조한다. 따라서 슬롯을 무효화하면 외부 변수와 내부 필드 모두 그 무효화를 관찰한다.
- `h.destroy()`는 등록된 슬롯들을 무효화하고, 슬롯이 보관하던 strong 참조들을 해제한다.
- Pool 밖에 일반 strong 참조가 있다면 객체는 계속 살아남을 수 있다. 이 모델의 destroy는 참조 카운트와 무관한 객체 메모리 강제 해제가 아니다.

```text
h가 관리하는 슬롯 A --strong--> A 객체
h가 관리하는 슬롯 B --strong--> B 객체
A.b -------------------------> 슬롯 B
B.a -------------------------> 슬롯 A
```

슬롯과 객체 사이에 순환이 생기더라도 명시적 destroy가 슬롯의 소유 참조를 끊는 모델이다. Pool에 넣었다는 사실만으로 자동 순환 수집을 보장하지 않는다.

## 안전성과 수명에 관한 추가 제안

아래 역시 확정되지 않은 검토 항목이다.

- 남아 있는 `pool` 값이 슬롯 상태를 확인할 수 있도록, 객체 해제 이후에도 필요한 관리 정보의 수명을 보장한다.
- 무효화된 pool 값의 `.` 접근은 검사하여 실패시키는 방안을 검토한다. 실패 방식과 nullable 표현은 미정이다.
- Destroy 시 모든 슬롯을 먼저 무효화한 뒤 strong 참조들을 해제하는 방안을 검토한다. 소멸자가 다른 pool 연결을 따라가거나 destroy에 재진입할 때의 규칙이 필요하다.
- Pool을 통한 메서드 호출은 호출 종료까지 receiver의 strong 참조를 임시 확보하는 방안을 검토한다. 메서드 내부에서 h.destroy()가 호출되어도 실행 중인 receiver가 소멸하지 않게 하려는 목적이다.

## 미해결 쟁점

- `pool_handle`의 복사·이동·소멸 규칙, 명시적 destroy 없이 scope를 벗어날 때의 처리, 중복 destroy와 재사용 허용 여부
- 슬롯 관리 정보의 소유권과 회수 방식. 슬롯을 단순 RC로 관리하면 destroy 전 순환이 남을 수 있으므로 h의 정리 책임을 명확히 해야 한다.
- Pool 값에서 일반 strong class/interface handle을 꺼낼 수 있는지와 그 문법
- 서로 다른 pool 간 연결 및 일반 strong 필드를 함께 사용할 때 순환 해소의 범위
- `.` 접근의 정확한 검사 범위, 반환된 필드 참조·raw pointer의 유효 기간, 호출 인자 평가 중 destroy가 발생하는 경우
- 소멸 순서, 소멸 중 재진입, 새 객체 등록, 동시 접근과 destroy의 동기화 여부
- 빈 pool 필드의 초기 상태와 객체 생성·필드 초기화 규칙
- 개별 슬롯 삭제·재사용 기능의 필요성과, 지원할 경우 stale handle 판정 방식

## 관련 현재 기준

- [Types and references](../wiki/language/types-and-references.md): 현재 class/interface handle의 BC 분류와 ownership 기록
- [MIR value model](../wiki/compiler/mir-value-model.md): BC/NBC 및 lifetime operation 모델

이번 작업은 제안 노트와 current agenda만 갱신한다. 언어 wiki의 확정 규칙이나 소스 구현은 변경하지 않는다.
