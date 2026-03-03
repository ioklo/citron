# 회의 / 실험 노트
 
Date: 2026-03-03
Title: (수정) MCreate 유지 + MRead 도입 정리
 
Summary
- 본 노트는 초안에서 “`MCreate` 제거”를 검토했으나, 이후 논의로 **`MCreate`를 ‘생성(계획) 컨테이너’로 유지**하는 방향으로 수정되었습니다.
- 읽기(read) 입력은 `MOperand` 대신 **`MRead`** 로 명확히 분리합니다.
- BC(bitwise-copyable) 값은 `MExp`로, NBC(non-bitwise-copyable) 생성/초기화 의미는 `MInitExp`로 모델링합니다.
- NBC 반환 호출은 호출자 관점에서 항상 sret(dest-passing) 규약을 사용하여 “겉보기에는 항상 RVO”로 취급합니다.
 
Decisions
- `MExp`는 **항상 BC 결과만** 표현합니다.
  - `MExp` 노드 생성 시점 또는 verifier에서 `GetType()->IsBitwiseCopyable()==true` 불변식을 둡니다.
  - BC load/store는 `MExp_Load`, `MExp_Store`로 표현합니다(체인 대입 지원 목적).
- `MInitExp`는 **항상 NBC 결과만** 표현합니다.
  - “생성자/복사/이동/nullable wrapping” 등 의미 이벤트가 필요한 값 생성을 `MInitExp_*`로 모델링합니다.
- `MCreate`는 유지합니다. 역할은 “초기화 문맥에서 사용할 **생성(계획) 컨테이너**”입니다.
  - BC 생성(계획): `MCreate_Bitwise(MExp*)`
  - NBC 생성(계획): `MCreate_Init(MInitExp*)`
- 읽기(read) 입력은 `MRead`로 분리합니다(기존 `MOperand` 대체).
  - 정규화 정책(권장):
    - BC 읽기 입력은 `MRead_Value(MExp*)`만 사용합니다(필요 시 `MExp_Load(MLoc*)`로 승격).
    - NBC 읽기 입력은 `MRead_Location(MLoc*)`만 사용합니다(NBC rvalue는 materialize로 `MLoc`를 만든 뒤 전달).
- NBC를 리턴하는 모든 call은 **항상 sret(dest-passing)** 호출 규약으로 lowering합니다.
  - 외부(MIR/QIR)에서 “RVO 적용 여부”를 별도 노드로 구분하지 않습니다.
  - 함수 내부에서 in-place return이 어려우면 copy/move fallback을 사용합니다.
  - 필요 시 `[non-rvo]`로 강제 제어합니다(정책 기존 합의 유지).
 
Details
### 1) 값 카테고리
- BC 값: `MExp`
  - 리터럴/산술/비교/BC 반환 call 등 “값으로 들고 다녀도 의미 손실이 없는 것”
- NBC 값: `MInitExp`
  - ctor/copy/move/nullable wrapping/string literal 등 “의미 이벤트가 필요한 값”

### 2) 읽기 입력 타입: `MRead`
- 목적: “읽기(read) 문맥에서 입력을 값(value) 또는 location으로 받는다”를 명시합니다.
- 제안 형태:
  - `MRead_Value { MExp* exp; }` (BC 전용)
  - `MRead_Location { MLoc* loc; }` (NBC 전용을 기본으로 권장)
- 정규화(권장):
  - BC를 location으로 읽어야 하는 경우(변수/필드 등)는 `MExp_Load(loc)`로 승격해 `MRead_Value`로 통일합니다.
  - NBC rvalue가 읽기 문맥에 끼어들면 `MLoc_Materialize(MCreate_Init{...})`로 임시 location을 만든 뒤 `MRead_Location`으로 전달합니다.

### 3) 초기화(Init) 문맥
- NBC는 “값”을 리턴/전달하지 않고, 항상 “어떤 place를 채운다”로 완결되어야 합니다.
- 따라서 `MInitExp`를 사용하는 노드/명령어는 **직접 dest(place)를 마련**해야 합니다.
  - 예: `LocalVarDecl` 초기화, `MLoc_Materialize`(임시 place 마련), return/yield 등

### 4) NBC call과 RVO
- NBC 반환 call은 “값을 리턴하는 exp”가 아니라, lowering 관점에서 항상 sret로 동작합니다.
- MIR에서는 다음 중 한 형태로 수렴하는 것이 목표입니다.
  - (A) `MInitExp_Call*` 자체가 “dest를 받는 호출”로 lowering되는 것으로 가정하고, init 문맥에서만 등장
  - (B) 별도의 init 명령어가 `MInitExp_Call*`을 받아 sret 호출을 수행

### 5) 대표 노드 정책(요약)
- `new struct`:
  - BC struct면 `MExp_NewStruct`
  - NBC struct면 `MInitExp_NewStruct`
- `struct ctor`, `string literal`:
  - 생성자 의미가 명확하므로 `MInitExp_*`로 표현합니다.
- `new nullable`:
  - inner가 BC면 `MExp_NewNullable(MExp* inner)`
  - inner가 NBC면 `MInitExp_NewNullable(MInitExp* inner)`(또는 init 문맥에서 inner를 생성하도록 구성)
 
### 6) Intrinsic 인자 모델
- Intrinsic의 인자를 `vector<MArgument>`로 통일하는 것은 가능합니다.
- 대신 intrinsic translation에서 일반 함수처럼 “시그니처 체크”를 수행하여,
  - 읽기 intrinsic: 허용되는 `MArgument` variant를 제한(예: Exp/Loc만)
  - 갱신 intrinsic(`++/--` 등): `Loc`만 허용
  - `Move/Create/Params` 등의 허용 여부를 kind별로 엄격히 검사합니다.
 
Action Items
- [ ] MIR 타입에서 `MCreate`/`MInitExp`/`MRead`의 불변식을 코드로 강제(생성 시점 또는 verifier).
- [ ] `MRead` 도입 시 기존 `MOperand` 사용처를 정리/이관(`is`, 멤버 접근, 비교 등).
- [ ] `MInitExp` 계층/visitor 설계 및 NBC 생성 의미(`StructCtor/CopyCtor/MoveCtor/Call/NewNullable` 등) 이관/정리.
- [ ] NBC 반환 call의 sret 규약을 lowering에서 일관되게 적용(QIR/LLVM 경로 포함).
- [ ] Intrinsic 시그니처 체크 테이블(허용 `MArgument` variant + 타입 제약) 설계.
