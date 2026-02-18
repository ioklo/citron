# MIR value category / Init-Assign / Materialize 정리 (2026-02-15)

## 목적
- MIR의 목적: Syntax에 드러나지 않는 의미(값/저장공간 구분, copy/move, RVO, 임시 수명 등)를 누락 없이 명시적으로 표현한다.
- 단, “항상 call 형태로”가 아니라, “항상 의미 이벤트가 보이게” 하는 것을 일관성 기준으로 삼는다.

관련 노트:
- `ai/notes/2026-02-09-mexp-bitwise-copy-rule.md`
- `ai/notes/2026-02-08-rvo-nrvo-design-discussion.md`
- `ai/notes/2026-02-05-struct-ctor-lowering.md`

## 용어
- `place`: 저장공간(lvalue). MIR에서는 `MLoc*`로 표현한다.
- `rvalue`: 계산 결과 값. MIR에서는 `MExp*`로 표현한다.
- `xvalue`: move로 소비되는 값(소유권 이전 의미). place를 가리키지만 소비 의미가 붙는다.
- `full-expression`: 조건식/대입 RHS/리턴값 등 한 덩어리 평가가 끝나는 경계. 임시 객체의 소멸 시점 기준.

## 값 입력 타입(권장)
### 1) 읽기 전용: `MOperand` (유지)
읽기/판정/캐스트/산술-비교 등 “값을 읽기만 하는 문맥”에서 사용한다. 이 문맥 자체는 copy/move/ctor 같은 초기화 이벤트를 암시적으로 발동시키지 않는다.
다만 `MOperand_Exp`의 내부 `MExp`는 함수 호출/생성 등 다양한 계산을 포함할 수 있다(= ctor/call이 “등장”하는 것은 가능).

- `MOperand_Exp{ MExp* }`
- `MOperand_Ref{ MLoc* }`  (place에서 load해서 읽기)

정책:
- `move`는 읽기 문맥에서는 금지(즉 `MOperand`에는 move variant가 없다).
- `if`, `is/as`, 산술/비교 연산 노드는 가능하면 `MOperand`를 입력으로 받는다.

### 2) 생성/초기화 계획: `MCreate` (통합)
기존에 분리되어 있던 `MInit`(초기화 인자)과 “보관/이동을 위한 transfer 입력”은
MIR에서 ctor/copy/move/RVO/bitwise-init을 명시적으로 보이게 해야 한다는 목적에 비추어 `MCreate`로 통합한다.

`MCreate`는 “특정 place를 어떤 의미 이벤트로 채울 것인가”를 표현하며, 다음 5종을 가진다.
- `MCreate_Bitwise(MExp* valueExp)` : bitwise-copyable 타입 전용 초기화(primitive, `[BitwiseCopy]` 등)
- `MCreate_CopyCtor(MLoc* srcPlace)` : copy-ctor initialize (non-bitwise struct 등)
- `MCreate_MoveCtorFromLoc(MLoc* srcPlace)` : move-ctor initialize (명시적 `move`만, 단 src는 materialize가 아님)
- `MCreate_MoveCtorFromMaterialize(MLoc_Materialize* src)` : materialize된 임시에서 move-ctor initialize
- `MCreate_Ctor(ctorDecl, args)` : direct-construct (custom ctor 포함)
- `MCreate_RVO(callInfo)` : 함수 반환 결과를 dest에 직접 바인딩(RVO/NRVO 적용됨을 MIR에서 명시)

정책:
- `MCreate_RVO`는 “dest에 직접 생성”을 나타내는 생성 계획으로, 별도 policy 분리 없이 create variant로 둔다(A안).
- `move`는 move-ctor 의미를 갖는 `MCreate_MoveCtorFromLoc` 또는 `MCreate_MoveCtorFromMaterialize`로만 표현한다(읽기 전용 입력인 `MOperand`에는 move 없음).
- 분석/일관성을 위해 rvalue 경로는 반드시 materialize를 거치게 한다:
  - rvalue(ctor/call 등)에서 move-ctor init이 필요하면 `MLoc_Materialize(MCreate_...)`를 만든 뒤 `MCreate_MoveCtorFromMaterialize`를 사용한다.
- lvalue에서의 move-ctor init은 문법에 `move`가 명시된 경우에만 `MCreate_MoveCtorFromLoc`를 사용한다.
  - 즉, “암시적 move”는 `FromLoc`로 표현하지 않는다.

## rvalue/create -> place: `MLoc_Materialize`
기존 `MLoc_Temp`는 의미를 명확히 하기 위해 `MLoc_Materialize`로 개명한다.

### 시맨틱
- `MLoc_Materialize(MCreate create)`는 “create로 생성되는 값을 임시 place에 materialize”한다.
- 이 임시 객체의 수명은 해당 `MLoc_Materialize`가 속한 `full-expression` 끝까지다.
- 임시의 소멸(dtor)은 scope/lifetime 관리(블록 경계/표현식 경계)로 처리한다. MIR에 항상 dtor call을 노드로 박을 필요는 없다.

### 사용 예시
- `F().x`:
  - `instance = MLoc_Materialize(MCreate_RVO(Call(F)) 또는 MCreate_...(정책에 따라))`
  - `MLoc_StructVar(instance, decl=S::x, ...)`
- `var s = F();` (RVO 미적용):
  - `Init( MCreate_Move( MLoc_Materialize(MCreate_Call?) ) )` 대신,
  - 본 노트의 결론 모델에서는 `Init(MCreate_*)`를 직접 사용하므로 아래 Init 섹션 참조.
- `s = F();` (Assign 문맥):
  - `Assign_Move( MLoc_Materialize(MCreate_RVO/Call/Ctor/...) )` (ctor/call RHS는 materialize 후 move-assign)

## Init / Assign (상태 기반)
### 목적
- 생성(초기화)과 대입(갱신)은 관찰 가능한 의미가 다르므로 MIR에서 구분한다.
- dest의 초기화 상태를 기준으로 규칙을 고정한다.

### dest 상태
- `Uninit`: 저장공간이 초기화되지 않음(사용 불가).
- `Init`: 유효한 값이 존재.

## bitwise-copyable 타입(Primitive 등) 처리
배경:
- primitive 및 bitwise-copyable 타입은 copy/move/ctor/assign 같은 “사용자 정의 의미”가 없거나(primitive),
  있더라도 의미적으로 “bitwise 복사로 충분”한 타입군으로 취급한다(`[BitwiseCopy] struct` 등).
- 이 타입군에 대해 `Init_Copy/Init_Move`를 그대로 사용하면 “move가 소모 의미를 갖는가?” 같은 불필요한 논의가 생긴다.

정책(업데이트):
- bitwise-copyable 타입 초기화/대입은 `MCreate_Bitwise(MExp*)`로 표현한다.
  - 예: `var i = 1;` => `Init( MCreate_Bitwise(MExp_IntLiteral(1)) )`
- 대입 쪽도 동일하게 `Assign( MCreate_Bitwise(...) )` 형태를 둘지,
  기존 `MExp_BitwiseAssign` 등과 어떻게 정합시킬지는 별도 결정 포인트로 둔다.
- non-bitwise(struct 등) 타입은 아래 `MCreate_Copy/Move/Ctor/RVO`, `Assign_Copy/Move` 규칙을 따른다.

네이밍:
- `Raw`는 의미가 너무 넓어(unsafe/raw-bytes, 미검증 값 등) 오해 소지가 크다.
- 의도는 “생성자/대입 없이 bitwise 복사로 처리”이므로 `Bitwise`(또는 `Trivial`)가 더 정확하다.
  - 결론(선호): `Init_Bitwise` (`Raw` 비추천)

### Init (Uninit -> Init)
초기화 문맥에서만 사용한다.
- `Init(MCreate create)`

규칙:
- `Init`는 dest가 `Uninit`일 때만 허용한다.
- create는 타입에 따라 제한한다.
  - bitwise-copyable: `MCreate_Bitwise`
  - non-bitwise struct: `MCreate_CopyCtor/MoveCtorFromLoc/MoveCtorFromMaterialize/Ctor/RVO`
- `MCreate_RVO`는 call/return lowering 규약 및 `[non-rvo]` 정책(별도 노트)과 연동한다.

### Assign (Init -> Init)
대입 문맥에서만 사용한다.
- `Assign_Copy(srcPlace)` : `copy_assign` 호출
- `Assign_Move(srcPlace)` : `move_assign` 호출

중요 규칙:
- Assign에는 `Ctor`/`RVO`에 해당하는 직접 in-place 재구성을 두지 않는다.
  - 이유: `s = F(s)`처럼 dest가 RHS 평가에 관여할 수 있는 alias 케이스에서 “drop 후 재구성”은 안전/의미가 깨질 수 있다.
  - 따라서 ctor/call RHS는 반드시 `MLoc_Materialize + Assign_Move`로 표현한다.

예시:
- `S s = S(...); s = S(...);`
  - `Assign_Move( MLoc_Materialize(MCreate_Ctor(...)) )`
- `S s = S(...); s = F();`
  - `Assign_Move( MLoc_Materialize(MCreate_RVO(Call(F)) 또는 정책상 비-RVO create) )`

## Move-ctor 입력 분리(FromLoc vs FromMaterialize)
`MCreate_MoveCtorFromLoc`과 `MCreate_MoveCtorFromMaterialize`는 “진짜 lvalue에서의 move”와 “rvalue를 임시에 materialize한 뒤 move”를 MIR에서 구조적으로 구분하기 위한 분리다.

불변식(invariant):
- `MCreate_MoveCtorFromLoc(MLoc* srcPlace)`에서 `srcPlace`는 `MLoc_Materialize`가 아니어야 한다.
- `MCreate_MoveCtorFromLoc`는 문법에 `move`가 명시된 경우에만 생성한다(명시 move 전용).
- rvalue를 move-ctor init으로 소비해야 하는 경우는 항상 다음 형태를 사용한다.
  - `tmp = MLoc_Materialize(MCreate_...)`
  - `Init(MCreate_MoveCtorFromMaterialize(tmp))`

분리의 의도:
- 분석에서 “진짜 lvalue move”와 “materialized move”를 명확히 구분할 수 있게 한다.
- `MCreate_*`가 ctor/copy/move/RVO를 명시적으로 표현한다는 MIR 목표와 일치한다.

## `uninit` (로컬 전용)
아이디어: 명시적으로 파괴하고, 동일 로컬을 다시 초기화 경로로 되돌리고 싶을 때 사용한다.

정책:
- `x = uninit;`는 로컬 변수에 대해서만 허용한다(escape/alias 분석 회피).
- 의미: `Drop(x)` 후 `x`를 `Uninit` 상태로 전환한다.
- 이후 `x = ...;`는 Assign이 아니라 Init 규칙을 적용한다.

예시:
```cpp
S s = S(...); // Init
s = uninit;   // Drop + Uninit
s = F();      // Init_RVO 또는 Init_Move(Materialize(Call))
```

## “ctor call을 MIR에서 보이게” vs 일관성
- custom ctor는 인자 해석 결과를 포함해 `MCreate_Ctor`로 명시한다.
- copy/move/bitwise/RVO도 `MCreate_*`로 명시한다(“call 형태”가 아니라 “의미 이벤트”가 보이게).
- lowering 단계에서 non-trivial이면 실제 ctor/assign call로, trivial이면 bitwise/memcpy로 내릴 수 있다.

`MExp_BitwiseCopy`와의 관계(요약):
- bitwise-copyable 타입에 대해서는 `MExp_BitwiseCopy`/`MExp_BitwiseAssign` 같은 경로를 유지할 수 있다.
- struct처럼 copy/move 의미가 필요한 타입은 `Init_*`/`Assign_*`로만 표현한다.

## 시그니처 변경 후보(요약)
아래는 “값/저장공간이 둘 다 들어올 수 있어야 하는 문맥”을 `MOperand` 또는 `MTransfer`로 통일하기 위한 후보들이다.

### `MOperand`(읽기)
- `MStmt_If(cond)`
- `MStmt_For(condExp)`
- `MExp_*Is* / MExp_*As*` (type test/cast 계열의 피연산자)
- `MExp_CallInternal*Operator`의 피연산자들
- `MExp_ListIterator(list)`
- `MLoc_ListIndexer(index)`
- `MExp_StringElem_Exp`의 요소

### `MCreate`(생성/초기화)
- `MExp_Stmt(final)`
- `MExp_Box(inner)`
- `MExp_List(elems)`
- `MExp_NewNullable(inner)`
- `MStmt_Return(value)`
- `MStmt_Yield(value)`
- 로컬 var 초기화 init 데이터(`LocalVarDeclInit`)는 `MTransfer`를 받아야 한다.

## 예시: `if (F().x == 2) {}`
개념적으로:
- `F()` 결과를 `MLoc_Materialize(Call(F))`로 place화
- `.x`는 그 place를 instance로 한 `MLoc_StructVar`로 place
- `==`는 `MOperand`로 읽어 비교
- materialize된 임시의 소멸은 `if` 조건식의 full-expression 끝에서 발생

## 열린 항목
- `MLoc_Materialize`의 정확한 수명 경계 정의(특히 중첩 표현식/단락 평가 등).
- `Init_RVO`가 표현하는 범위(필수 RVO vs 가능한 RVO, `[non-rvo]` 정책과의 결합).
- bitwise-copyable struct 어노테이션(`[BitwiseCopy]`)과 `Init/Assign` 이벤트 간의 우선순위/적용 규칙.
