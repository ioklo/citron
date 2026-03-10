# 회의 / 실험 노트

Date: 2026-03-09
Title: SExp translation 기본 엔트리를 Read/Create/Loc 기준으로 정리

Summary
- `SExp`를 항상 `MExp`/`MInitExp`/`MLoc`로 직접 번역하는 방식보다, 먼저 문맥 의미를 `Read` / `Create` / `Loc`로 나누는 쪽이 더 자연스럽다는 점을 확인했다.
- 기본 엔트리는 `TranslateSExpToMRead`, `TranslateSExpToMCreate`, `TranslateSExpToMLoc` 세 축으로 두고, `MExp`와 `MInitExp`는 하위 구체 표현으로 보는 방향으로 정리했다.
- `MExp`/`MLoc`/`MInitExp` 인자를 받는 MIR 노드들은 각 인자가 실제로 `read` / `create` / `loc` 중 무엇을 의미하는지 따로 분류해야 한다는 결론에 도달했다.

Context
- 기존에는 `SExp -> MExp`, `SExp -> MInitExp`, `SExp -> MLoc` 같은 직접 번역을 중심으로 생각하는 흐름이 있었다.
- 그러나 MIR 값 모델을 `BC/NBC`, `MRead`, `MCreate`, `MLoc` 기준으로 재정렬하면서, 호출자 입장에서 중요한 정보는 "결과 타입이 무엇인가"보다 "이 자리가 읽기 자리인가, 생성 자리인가, 위치 자리인가"라는 점이 더 분명해졌다.
- 특히 같은 `MExp*` 인자라도 `if`의 조건처럼 read인 경우와 `MExp_Store.src`처럼 create인 경우가 섞여 있어, `MExp*`만으로는 문맥 의미가 드러나지 않는 문제가 확인되었다.

Decision
- `SExp translation`의 기본 엔트리는 아래 세 가지로 본다.
  - `TranslateSExpToMRead`
  - `TranslateSExpToMCreate`
  - `TranslateSExpToMLoc`
- `MRead`는 읽기 문맥의 표준 입력으로 사용한다.
  - BC read는 `MRead_Value(MExp*)`
  - NBC read는 `MRead_Location(MLoc*)`
- `MCreate`는 생성/초기화 문맥의 표준 입력으로 사용한다.
  - BC create는 `MCreate_Bitwise(MExp*)`
  - NBC create는 `MCreate_Init(MInitExp*)`
- `MExp`, `MInitExp`, `MLoc`는 기본 public entry라기보다, `MRead`/`MCreate`/`MLoc` 체계 아래의 구체 표현으로 본다.
- 특정 MIR 노드가 더 구체적인 형태만 받을 수 있다면, 해당 자리에서 `MRead`/`MCreate`를 잘라내거나 검증하면서 에러를 낸다.

Rationale
- 번역 함수 이름이 호출자가 요구하는 의미와 직접 대응한다.
  - read 자리면 `TranslateSExpToMRead`
  - create 자리면 `TranslateSExpToMCreate`
  - lvalue/place 자리면 `TranslateSExpToMLoc`
- BC/NBC 분기와 materialize 판단을 호출자마다 반복하지 않고 공통화할 수 있다.
- 에러를 "이 자리는 read가 필요하다" 또는 "이 자리는 create가 필요하다" 같은 문맥 기준으로 진단할 수 있다.
- `MExp`/`MInitExp`를 기본 entry로 두지 않으면, 결과 타입 중심 사고보다 문맥 의미 중심 사고로 translator를 재구성할 수 있다.

Detailed model
- Read 자리
  - 생성 없이 값을 읽기만 하는 문맥이다.
  - BC read는 `MExp`로 표현한다. place가 오면 필요 시 `MExp_Load`로 올린다.
  - NBC read는 `MLoc`로 표현한다. 필요하면 `MLoc_Materialize`를 통해 임시 place를 만든다.
- Create 자리
  - 값을 생성해서 어떤 place를 채우거나 반환하는 문맥이다.
  - BC create는 `MExp`로 표현한다.
  - NBC create는 `MInitExp`로 표현한다.
- Loc 자리
  - 저장 위치 자체가 필요한 문맥이다.
  - 결과는 `MLoc`다.
  - 단, 어떤 하위 노드의 입력이 모두 `loc`여야 하는지는 별도 검토가 필요하다.

Implications for MIR arguments
- `MRead`와 `MCreate` 인자는 이미 의미가 명확하므로 추가 분류가 필요 없다.
- 반대로 `MExp*`, `MLoc*`, `MInitExp*` 인자를 받는 MIR 노드들은 그 인자가 실제로 `read` / `create` / `loc` 중 무엇을 의미하는지 따로 분류해야 한다.
- 예:
  - `MStmt_If.cond : MExp*` 는 의미상 `read`
  - `MExp_Store.src : MExp*` 는 의미상 `create`
  - `MStmt_LocalRefDecl.loc : MLoc*` 는 의미상 `loc`
  - `MExp_Load.loc : MLoc*` 는 의미상 `read`
  - `MInitExp_StructCtorKind_Copy.src : MLoc*` 는 의미상 `read`
- 따라서 "필드 타입"과 "문맥 의미"를 구분해서 봐야 한다.

Refinement notes
- `MDirective_*`는 현재 필드 타입은 `MLoc*`지만, 의미상 `loc`보다는 `read`에 가깝다.
- `MExp_Stmt.finalExp`는 고정적으로 `read` 또는 `create`로 못 박기 어렵고 상위 문맥 의존이다.
- `MInitExp_Stmt` / `MExp_Stmt`는 미래 확장을 위해 MIR에는 남겨두되, 기본 translation 핫패스의 중심보다는 특수 block-expression 경로로 격리하는 편이 낫다.
- `MLoc_ListIndexer`의 결과는 `loc`이지만, 입력 `list`와 `index`는 의미상 `read`로 보는 편이 자연스럽다.
  - 특히 `list`가 런타임 `List<T>` 하나만 의미한다면 `NBC read`, 즉 사실상 `MRead_Location`으로 보는 것이 맞다.

Open points
- `MExp*` / `MLoc*` / `MInitExp*` 인자를 받는 MIR 노드 전체에 대해 문맥 의미(`read/create/loc`) 분류표를 별도 문서로 정리할 필요가 있다.
- 문맥 의미가 고정된 자리에는 `MExp*` 대신 `MRead_Value`, `MCreate_Bitwise` 같은 더 직접적인 타입을 쓰는 것이 도움이 될지 추가 검토가 필요하다.
- `MExp_Stmt.finalExp` 같은 문맥 의존 필드를 어디까지 유지할지, 또는 더 강한 타입으로 쪼갤지 결정이 필요하다.

Action Items
- [ ] MIR 노드 인자별 `read/create/loc` 분류표 작성
- [ ] `SyntaxIR0Translator`의 기본 entry를 `MRead` / `MCreate` / `MLoc` 중심으로 재점검
- [ ] 문맥 의미가 고정된 필드에 wrapper 타입(`MRead_Value`, `MCreate_Bitwise` 등)을 직접 쓰는 방안 검토
