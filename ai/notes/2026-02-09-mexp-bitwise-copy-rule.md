# MExp bitwise copy 규칙 논의 (2026-02-09)

## 배경
- 기존 `MExp_Load` 계열이 값 materialize 역할을 했지만, 이름만으로는 생성자 호출 필요 여부가 드러나지 않음.
- `struct`는 생성자 호출이 의미적으로 필요하므로, 단순 load/복사로 처리하면 규칙 위반 가능성이 있음.

## 핵심 합의
1. `struct` 타입에는 `MExp_Load` 류를 사용하지 않는다.
2. bitwise로 통 복사가 가능한 타입에서만 bitwise-copy 계열 expression을 사용한다.
3. `Load`보다 의도가 명확한 이름으로 바꾸는 것이 바람직하다.

## 네이밍 결론
- 최종 후보: `MExp_BitwiseCopy`
- 의도: 임의의 `MLoc` 값을 다른 storage로 materialize할 때, 생성자 호출 없이 bitwise copy로 옮긴다.

## 허용/비허용 범위
- 허용: trivial bitwise-copy 가능 타입
  - Primitive(`int` 등)
  - 조건을 만족하는 `class`, `interface`
- 비허용: 생성자/복원 로직이 필요한 `struct`

## Instance `this` 타입/전달 규칙 (추가 합의)
- Citron에서 `class` 타입은 handle(reference type), `struct`는 value type으로 취급한다.
- 인스턴스 함수의 `this` 타입:
  - `struct S` 인스턴스 함수: `this: S&`
  - `class C` 인스턴스 함수: `this: C` (handle 값)
- 호출 시 전달 방식:
  - `struct` 수신자: `S&`를 전달하므로 QIR 레벨에서는 포인터 전달
  - `class` 수신자: `C`를 전달하므로 handle 값 복사 전달
- 의미:
  - `struct` 메서드에서의 변경은 수신자 저장소 원본에 직접 반영된다.
  - `class` 메서드에서는 handle 복사본을 받지만 동일 객체를 가리키므로 객체 멤버 변경은 반영된다.
  - `class`에서 `this` 자체 재바인딩은 호출자 변수 바인딩을 바꾸지 않는다.

## MIR 생성 단계 적용
- 타입체크를 하면서 MIR을 생성하므로, `MExp_BitwiseCopy`의 생성 규칙에서만 강제하면 충분하다.
- 즉, 사용 단계 사후 검사보다 "노드 생성 자체를 불가"로 막는 정책을 사용한다.

## 예시
```cpp
var x = 1;
var y = x;

// MIR
MStmt_LocalVarDecl(
  "y",
  MStmt_LocalVarDeclInit_Exp(
    MExp_BitwiseCopy(MLoc_LocalVar("x"))
  )
)
```

## 실무 가이드
- 기존 `MExp_Load` 계열이 bitwise 의미로 쓰인 지점은 점진적으로 `MExp_BitwiseCopy`로 치환한다.
- `struct` 경로는 별도 생성/복원 경로(ctor/deserialization 의미)를 유지한다.

## Assign / Chain assign 정책 (추가 합의)
- `MExp_BitwiseAssign`를 도입해 bitwise-assignable 타입의 대입을 표현한다.
- chain assign(`a = b = c = ...`)은 **bitwise 타입에서만 허용**한다.
- `struct`처럼 `copy_assign`/`move_assign` 의미가 필요한 타입은 chain assign을 지원하지 않는다.
- `struct` 대입은 statement 단위로 분리해 명시적으로 작성한다.

## Struct assign fallback 철회 (추가 합의)
- 이전 아이디어: `copy_assign`/`move_assign`이 없을 때 `dtor` 호출 후 `copy ctor`/`move ctor` 호출로 fallback.
- 결론: 이 fallback은 철회한다.
- 이유: `s = F(s)` 같은 식에서 `s`를 먼저 파괴한 뒤 우변 평가가 진행되면, 우변이 `s`를 읽는 경우 의미/안전성이 깨질 수 있다.
- 정책: `struct` assign은 자연스럽게 막고(미지원), 필요 시 명시적 API/문법으로만 허용한다.

## `[BitwiseCopy] struct` 어노테이션 (추가 합의)
- 표기 예: `[BitwiseCopy] struct S { ... }`
- 제약: 해당 `struct`의 모든 멤버 변수 타입은 bitwise copy 가능 타입이어야 한다.
- 검증: 컴파일러는 선언 시점에 멤버를 재귀적으로 검사해 조건 위반 시 진단한다.

### 의미
- `[BitwiseCopy]`가 붙은 `struct`는 bitwise-copyable 타입으로 간주한다.
- 해당 타입의 copy/move 관련 경로는 사용자 정의 copy/move special member 호출 대신 bitwise copy로 처리한다.
- 적용 대상 경로:
  - copy ctor 관련 복사
  - move ctor 관련 복사
  - copy assign 관련 대입
  - move assign 관련 대입

### 진단 방향
- 멤버 제약 위반 시 에러 메시지 예:
  - ``[BitwiseCopy] struct의 멤버는 모두 bitwise-copyable 이어야 합니다.``

### 예시
- 허용: `a = b = c = 1;`
- 비허용: `a = b = c = s;` (`S`가 struct)

### 진단 방향
- 에러 메시지 예: `체인 대입은 bitwise-assignable 타입에서만 지원됩니다. struct 대입은 분리해서 작성하세요.`
