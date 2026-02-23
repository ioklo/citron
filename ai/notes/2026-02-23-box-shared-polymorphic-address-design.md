# Box/Shared + Polymorphic `&` 설계 논의 (2026-02-23)

## 목적
- `shared`와 `box`의 역할을 분리하고, `&` 연산자의 polymorphic 동작 규칙을 정리합니다.
- 구현 전에 문법/타입/진단 정책을 먼저 확정합니다.

## 배경
- 기존 box ptr(fat pointer)가 shared_ptr 역할(수명 유지 객체 + address)을 담당해 왔습니다.
- 새 방향:
  - `shared<T>`: 공유 소유 + fat pointer 참조 모델
  - `box<T>`: unique_ptr 의미, move-only 소유 객체
- 기존 box-ref(pointer) 계열 표현은 제거 대상입니다.

## 논의된 예시
```citron
struct S { int x; }

box<S> b = box S(3);
shared<S> s = shared S(3);
int i = 3;

shared int p1 = &s.x; // fat pointer
int* p2 = &s.x;       // raw pointer
int* p3 = &i;         // raw pointer
int* p4 = &b.x;       // raw pointer

S* p5 = &s;           // owner에서 raw pointer 획득
S* p6 = &b;
shared S p = &s;      // 금지 대상 케이스
```

## 현재 합의

### 1) 소유 모델
- `shared<T>`는 shared ownership.
- `box<T>`는 unique ownership(move-only).

### 2) static shared 참조
- `shared<int> x2 = &C.x` 허용.
- static shared 참조(`&C.x` 등)는 `owner = null` 대신 전역 singleton immortal owner를 사용합니다.
- static 변수마다 control block을 만들지 않고, 프로그램 전체에서 owner 1개를 공유합니다.

### 3) `&` polymorphic 최종 규칙
- 기대 타입이 `T*`이면 raw pointer를 생성합니다.
- 기대 타입이 `shared T`이면 shared(fat pointer) 참조를 생성합니다.
- 기대 타입이 없거나 둘 이상으로 해석 가능하면 컴파일 에러입니다(예: `var x = &...`).

### 4) owner 값 자체에 대한 `&`
- `&s`, `&b`는 허용합니다.
- 단, `T*` 문맥에서만 허용합니다.
- `shared T` 문맥에서는 금지합니다.

예:
- `S* p5 = &s;` 허용
- `S* p6 = &b;` 허용
- `shared S p = &s;` 금지(컴파일 에러)

## 최종 결정
- `&`는 polymorphic 연산자로 유지합니다.
- 기대 타입 `T*`에서는 raw pointer, 기대 타입 `shared T`에서는 shared 참조를 생성합니다.
- `shared <- &shared`는 금지하며 **즉시 컴파일 에러**로 처리합니다.
- shared alias는 `p = s`로만 표현합니다.
- owner 값(`shared<T>`, `box<T>`)에 대한 `&`는 `T*` 문맥에서만 허용합니다.
- static shared 참조는 전역 singleton immortal owner 1개를 공유합니다.
- 따라서 control block 메모리는 O(1)이며, `inc/dec` 경로에서 null 체크 분기 없이 공통 처리합니다.

예시 진단 문구:
- "`&` on owner value can only produce `S*`; use `p = s` for shared alias."

## Action Items
- [x] `shared <- &shared`는 즉시 에러로 확정
- [x] `&`를 polymorphic 연산자로 유지하기로 확정
- [x] `&s`, `&b`는 `T*` 문맥에서만 허용하기로 확정
- [x] static shared 참조는 전역 singleton immortal owner(1개) 사용으로 확정
- [ ] 모호한 `&` 케이스 진단 문구 표준화
- [ ] box pointer 제거 범위(`box T*`, `box var*`) 확정 및 테스트 케이스 업데이트 계획 수립

## 추가 논의 (2026-02-23)

### MSharedExp와 rvalue 허용 범위
- `MSharedExp_ClassVar(MLoc, ClassVar)`의 첫 인자에 `MLoc_Materialize`가 들어갈 수 있습니다.
- 따라서 다음 케이스는 허용 대상으로 봅니다.

```citron
shared<int> i = &(new C()).x;
shared<int> i = &F().x;
```

- 허용 기준은 "rvalue인지 여부"가 아니라 "owner를 안정적으로 추출할 수 있는지"입니다.
- owner 추출 가능 rvalue는 허용합니다.
- owner 추출 불가 rvalue는 금지합니다.
- 이 제약은 `TranslateSExpToMSharedExp` 단계의 타입체킹에서 강제합니다.
- `MSharedExp_StructMember(parent, RStructVar)`는 `parent`의 root가 `class | shared<T> | static`일 때만 생성 가능합니다.
- root가 plain struct rvalue(또는 owner를 만들 수 없는 값)인 경우 `MSharedExp` 생성은 타입 에러입니다.

### 테스트 보강
- `shared <- &shared`는 에러로 확정되었으므로 별도 실패 테스트를 추가합니다.
- 최소 케이스:
  - `shared<S> p = &s; // s: shared<S>` -> 컴파일 에러
  - 대입/인자 문맥에서도 동일 규칙 확인

### 표기 관련 확인
- `c.s.d.t` 예시에서 `c`, `d`는 class, `s`, `t`는 struct라는 전제 유지.
- 해당 전제 기준으로 `D::t`는 class `D`의 field(`t`)로 해석합니다.

