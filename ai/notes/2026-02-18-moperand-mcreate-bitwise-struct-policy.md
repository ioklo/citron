# MOperand / MCreate 정리 + Bitwise 정책 (2026-02-18)

## 목적
- “읽기” 문맥과 “생성(초기화)” 문맥을 IR에서 분리합니다.
- bitwise-copyable 타입은 bitwise 경로로, 그 외 타입(일반 struct 등)은 ctor/move/copy 이벤트가 IR에 드러나도록 강제합니다.
- 결과적으로 “어디에 값이 저장되는지”, “생성자/대입 연산이 호출되는지”가 IR에서 숨지 않도록 합니다.

## 타입 분류 (정책)
Citron 타입은 크게 두 군으로 나눕니다.

### 1) BitwiseCopyable 타입
bitwise-copy로 값의 의미를 보존할 수 있는 타입군.
- primitive
- `[BitwiseCopy] struct`
- class, interface (handle/reference 의미의 값으로 취급)

### 2) Non-bitwise 타입 (항상 의미 이벤트 필요)
bitwise copy로는 의미를 보존할 수 없고, 생성/소멸/복사/이동 의미 이벤트가 필요하다고 간주하는 타입군.
- 일반 struct
- struct를 포함하는 tuple
- nullable struct 등 (값 표현에 소멸/복원 의미가 개입될 수 있는 케이스)

> 주: “bitwise-copyable struct vs 일반 struct”를 타입 시스템의 kind로 분리하지 않고, trait/어노테이션 기반(`IsBitwiseCopyable(T)`)으로 판정합니다.

## 두 컨텍스트: `MOperand` vs `MCreate`

### A) 읽기 전용 입력: `MOperand`
`if` 조건처럼 “값을 읽기만” 하는 문맥에서 사용합니다. `MOperand` 자체는 “어디에 생성한다”를 표현하지 않습니다.

- `MOperand_Exp(MExp*)`
  - **bitwise-copyable 타입의 `MExp`**만 허용합니다(리터럴/연산/함수 호출 결과 등).
  - 의도: “이미 값으로 존재하는 것”을 읽기 자리에서 직접 사용.
- `MOperand_Loc(MLoc*)`
  - **place/borrow**를 나타냅니다(일반 struct 포함 가능).
  - struct가 읽기 입력으로 필요하면 원칙적으로 `MOperand_Loc`로만 전달되며, 값 materialize/ctor 이벤트를 암시적으로 발생시키지 않습니다.

예:
- `if (f)`에서 `f`가 `bool` 로컬이면 `MOperand_Loc(MLoc_LocalVar(f))`가 가능.
- `if (S().x == 0)`처럼 struct rvalue가 끼어드는 경우, `S()`는 아래 `Materialize` 규칙을 통해 place로 만든 뒤 `MOperand_Loc`로 사용합니다.

### B) 위치에 값을 “생성”: `MCreate`
`var/field` 초기화 RHS처럼 “dest place에 값을 채우는 의미 이벤트”를 표현합니다.

#### `MCreate` 변형(초안)
1) `MCreate_BitwiseLoc(MLoc* srcLoc)`
   - dest를 bitwise-copy로 초기화합니다(소스는 place).
   - 제약: `IsBitwiseCopyable(type_of(srcLoc))` 이어야 합니다.

2) `MCreate_BitwiseExp(MExp* srcExp)`
   - dest를 bitwise-copy로 초기화합니다(소스는 value exp).
   - 제약: `IsBitwiseCopyable(type_of(srcExp))` 이어야 합니다.

3) `MCreate_StructCtorCopy(MLoc* srcLoc)`
   - dest에 copy-ctor 의미로 초기화합니다.
   - 제약: src는 **일반 place**여야 하며(필요 시 별도 invariant), 타입은 non-bitwise struct 계열입니다.

4) `MCreate_StructCtorMove(MMoveSource src)`
   - dest에 move-ctor 의미로 초기화합니다.
   - `MMoveSource`는 “move의 출처”를 표현하며, 최소한 아래 둘을 구분합니다.
     - `FromLoc(MLoc* srcLoc)` : 문법에 `move`가 명시된 경우의 lvalue move
     - `FromMaterialize(MLoc* tmp)` : rvalue를 임시에 materialize한 뒤 move-ctor로 소비

5) `MCreate_StructCtor(CustomCtorDecl, vector<MArgument> args)`
   - dest에 사용자 정의 ctor(또는 direct construct) 의미로 초기화합니다.

6) `MCreate_RVO(Func, MArguments)`
   - dest에 직접 생성 가능한 함수 호출.
   - lowering에서 “dest를 함수에 전달하여 직접 구성” 이벤트가 보이도록 합니다.

## Materialize (rvalue → place)
일반 struct 계열은 rvalue를 “값으로 들고 다니는” 것을 피하고, 필요 시 임시 place로 materialize합니다.

예: `S().x`
- `tmp = MLoc_Materialize( MCreate_StructCtor(...) 또는 MCreate_RVO(...) )`
- `fieldLoc = MLoc_StructVar(tmp, S::x, ...)`
- 이후 읽기 문맥에서는 `MOperand_Loc(fieldLoc)`로 사용

> 주: 이 노트는 “struct rvalue `MExp`를 제거(또는 최소화)”하는 방향을 전제로 합니다.

## Assign (initialized 상태에서의 대입)
dest가 이미 초기화되어 있을 때의 “갱신” 이벤트는 `MCreate`와 분리합니다.

- `MStmt_BitwiseAssign(MLoc* dest, MExp* src)`
  - 제약: dest/src 타입은 bitwise-copyable.

- `MStmt_StructCopyAssign(MLoc* dest, MLoc* srcLoc)`
  - 제약: dest/src는 non-bitwise struct 계열.

- `MStmt_StructMoveAssign(MLoc* dest, MMoveSource src)`
  - `MMoveSource`의 출처 구분은 ctor-move와 동일 원칙을 따릅니다.

## 규칙 요약 (invariants)
- bitwise-copyable이 아닌 타입에는 `MCreate_Bitwise*` / `MStmt_BitwiseAssign`를 만들지 않습니다.
- non-bitwise struct는 “암시적 bitwise load/copy”를 허용하지 않습니다.
- 읽기 문맥(`MOperand`)은 “생성 이벤트”를 암시적으로 발생시키지 않습니다.
  - struct가 값이 필요해지는 지점이 있다면 반드시 `Materialize + (Copy/Move/Ctor)`처럼 의미 이벤트를 노드로 드러냅니다.

## 열린 항목
- `MLoc_Materialize`의 수명 경계(full-expression)와 dtor 처리 방식.
- `MMoveSource`에서 “명시 move(from loc)”와 “materialize-from-rvalue”를 언제/어디서 강제할지.
- `MOperand_Exp` 허용 범위(“bitwise 타입의 모든 MExp” vs “리터럴/일부 연산만”)를 어느 단계에서 체크할지.

