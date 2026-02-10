# Lambda capture 정책 논의 (2026-02-09)

Updated: 2026-02-10

## 목표
- 람다 작성은 간결하게 유지한다.
- 복사/참조 의미를 예측 가능하게 만든다.
- `struct` 캡쳐에서 비용/aliasing을 명시적으로 드러낸다.

## 합의 초안
1. 암시 캡쳐 기본값은 복사다.
2. `struct`는 암시 캡쳐를 금지한다.
3. `struct`를 캡쳐할 때는 복사/참조를 반드시 명시한다.
4. 참조 캡쳐(`ref`, `&`)를 사용한 람다는 escape 불가로 제한한다.

## 규칙 상세
- non-struct 로컬(`int`, handle class 등):
  - `() => ...`에서 사용 시 암시적으로 복사 캡쳐된다.
  - 참조가 필요하면 `&x`처럼 명시한다.
- struct 로컬:
  - `() => s` 형태의 암시 캡쳐는 에러다.
  - `[s]`(copy) 또는 `[&s]`(ref)처럼 캡쳐 방식을 명시해야 한다.
- `ref` 캡쳐 람다:
  - 반환, 힙 저장, async 태스크 전달 등 escape 경로를 금지한다.
  - 즉, 생성된 현재 스코프 안에서만 즉시 사용 가능해야 한다.

## 예제
```citron
var x = 3;
var l = () => x; // x는 복사 캡쳐
```

```citron
var x = 7;
var l = [&x]() { x++; return x; }; // x는 ref 캡쳐, 매 호출 시 증가, escape 불가
```

```citron
var s = S();
var l = () => s; // 에러: struct 캡쳐 방식(copy/ref)을 명시해야 함
```

```citron
var s = S();
var l = [s]() { return s; }; // s는 명시적 복사 캡쳐
```

```citron
var s = S();
var l = [&s]() { s.Update(); }; // s는 명시적 ref 캡쳐, escape 불가
```

## 진단 방향
- struct 암시 캡쳐 에러 메시지 예:
  - `struct 캡쳐는 명시해야 합니다. [s] 또는 [&s]를 사용하세요.`
- ref 캡쳐 escape 에러 메시지 예:
  - `ref 캡쳐 람다는 현재 스코프를 벗어날 수 없습니다.`

## 오픈 포인트
- 문법 고정: 캡쳐 리스트 문법은 `[s]`(copy) / `[&s]`(ref)로 확정한다.

## Addendum (2026-02-10): `this` 캡쳐 규칙
- `this`는 암시 캡쳐를 금지한다. 캡쳐 리스트에서만 캡쳐할 수 있다.
- `struct` 인스턴스 메서드의 `this`:
  - `&this`만 허용한다 (reference only).
  - `&this`를 캡쳐한 람다는 `ref` 캡쳐와 동일하게 escape 불가다.
- `class`(handle/reference type) 인스턴스 메서드의 `this`:
  - `this`만 허용한다 (copy only; handle 값 복사).
  - handle 복사 캡쳐이므로 일반적인 boxed lambda/escape 규칙과 충돌하지 않는 한 escape 가능하다.

## Addendum (2026-02-10): 캡쳐 구현 (Resolve + NeedCapture)
- 캡쳐는 IdentifierResolve 과정에서 "발견되면 자동 캡쳐 대상으로 등록"한다.
- 자동 캡쳐는 항상 `copy`로 간주한다.
  - 따라서 `struct`/`this`는 자동 캡쳐 대상에서 제외(= 에러)한다.
- Resolve 부수효과를 줄이기 위해 `CapturePlan`을 별도 패스로 만들지 않고, Resolve 결과에 "캡쳐 필요"를 인코딩한다.

### 표현
- `RMember_NeedCapture { name, RMember* member }`
  - `member`는 한 단계 바깥 스코프에서 resolve 된 결과를 나타낸다.
  - 중첩된 람다 캡쳐는 `NeedCapture(NeedCapture(...(member)))` 형태로 표현한다.
  - `NeedCapture` 중첩 깊이는 FuncContext(람다 컨텍스트) 깊이와 일치해야 하며, 불일치 시 `assert`로 감시한다.

### 적용(스테이징/백트래킹)
- Resolve는 `NeedCapture`를 반환하고, 실제 람다 환경(캡쳐 슬롯) 반영은 호출자/컨텍스트가 staging 트랜잭션 안에서 수행한다.
- backtracking으로 후보가 바뀌면 staging 롤백으로 함께 되돌릴 수 있어야 한다.

## Addendum (2026-02-10): `ref`(alias) 자동 캡쳐 의미
- `ref` 변수는 "포인터 값을 가진 값"이 아니라, 기존 location에 대한 alias로 취급한다.
- 따라서 자동 캡쳐(= copy)는 `ref` 자체를 유지하는 것이 아니라, alias가 가리키는 대상의 값을 캡쳐 시점에 복사하는 의미다.

### 예제
```citron
var x = 1;
var& y = x;
var l = () => y; // 자동 copy 캡쳐: x의 값 스냅샷을 캡쳐

x = 2;
l(); // 1
```

- aliasing(레퍼런스 캡쳐)이 필요하면 캡쳐 리스트에서만 명시적으로 허용한다.
```citron
var x = 1;
var& y = x;
var l = [&y]() => y; // 명시 ref 캡쳐, escape 불가

x = 2;
l(); // 2
```

## Addendum (2026-02-10): 람다/inline 본문 문법 및 `return`
람다 표현은 크게 expression-body / stmt-body 두 가지가 있다.

### 1) expression-body
- 형태: `[](<params>) => <expr>`
- 캡쳐가 필요 없으면 `[]`는 생략 가능하다.
  - 예: `x => x + 1`
- 반환 타입 표기(`-> RetType`)는 지원하지 않는다. 결과 타입은 `<expr>`의 타입이다.
- 파라미터:
  - 0개인 경우 `()`는 필수다.
  - 각 파라미터의 타입은 생략 가능하다.
  - 파라미터가 1개이고 타입을 생략할 때는 괄호를 생략할 수 있다.

### 2) stmt-body
- 형태: `[](<params>) (-> RetType)? { <stmt>... }`
- 단, 파라미터가 없고 `-> RetType`도 생략한 경우 `()`를 생략할 수 있다:
  - 예: `[] { <stmt>... }`
- 캡쳐가 필요 없더라도 `[]`는 반드시 써야 한다.
- 반환 타입 표기(`-> RetType`)는 옵션이다.
- 파라미터:
  - 원칙적으로 `()`는 필수다(위 예외 제외).
  - 각 파라미터의 타입은 생략 가능하다.

### 공통: 반환
- stmt-body에서 마지막 expression 자동 반환은 도입하지 않는다.
- 반환값이 필요한 경우 `return <expr>;`을 사용한다.
- `inline { ... }`는 expression block으로서 값이 필요한 경우 `return <expr>;`을 사용한다.
