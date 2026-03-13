# 회의 / 정정 노트

Date: 2026-03-12
Title: `MExp_Store.src`는 `create`가 아니라 `read`

Status
- current
- This note supersedes the `MExp_Store.src` classification in `2026-03-09-sexp-translation-read-create-loc.md`.

Summary
- 기존 노트에서 `MExp_Store.src : MExp*`를 의미상 `create`로 분류한 부분을 정정합니다.
- 현재 합의 기준에서 `store`와 `assign`은 초기화(init)가 아니라 갱신(write/update) 문맥입니다.
- 따라서 source는 `MCreate`가 아니라 `MRead` 축으로 해석하는 것이 맞습니다.

Background
- `MCreate`는 "어떤 place를 생성/초기화할 것인가"를 나타내는 계획 컨테이너입니다.
- 반면 `store`와 `assign`은 이미 존재하는 destination을 갱신하는 연산입니다.
- 이 차이를 유지해야 `Init`과 `Assign/Store`의 의미가 MIR에서 섞이지 않습니다.

Correction
- 잘못 분류된 문장:
  - `MExp_Store.src : MExp*` 는 의미상 `create`
- 정정:
  - `MExp_Store.src`는 의미상 `read`
  - 보다 직접적으로는 BC source이므로 `MRead_BC`로 모델링하는 것이 자연스럽습니다.

Decision
- `Init` 문맥의 입력은 `MCreate`를 사용합니다.
- `Assign` / `Store` 문맥의 입력은 `MRead`를 사용합니다.
- NBC assign은 기존 합의대로 `MStmt_Assign(dest, MRead_NBC src)`를 유지합니다.
- BC store도 같은 축에서 보며, 장기적으로 `MExp_Store(dest, MRead_BC src)`가 더 일관적입니다.

Rationale
- `store`는 새 값을 생성하는 연산이 아니라, source를 읽어서 destination에 써 넣는 연산입니다.
- `a = b;`가 NBC일 때 내부적으로 `a.copy_assign(b)` 또는 `a.move_assign(tmp)` 같은 의미로 내려가더라도,
  `b` 또는 `tmp`는 assign에 전달되는 읽기 입력입니다.
- rvalue RHS가 필요한 경우에도 순서는 다음과 같습니다.
  - 먼저 `MCreate`로 생성 계획을 만든다
  - 필요하면 `MLoc_Materialize`로 임시 place를 만든다
  - assign/store 직전에는 `MRead`로 정규화한다
- 즉 rvalue가 중간에 `MCreate`를 거칠 수는 있지만, assign/store의 직접 입력 의미는 여전히 `read`입니다.

Examples
- `var x = b;`
  - init 문맥이므로 RHS는 `MCreate`
- `a = b;`
  - assign 문맥이므로 RHS는 `MRead`
- `a = F();` where `F()` returns NBC
  - `tmp = MLoc_Materialize(MCreate_NBC(...))`
  - `Assign(dest=a, src=MRead_NBC(tmp))`
- `a = i + 1;` where BC
  - `Store(dest=a, src=MRead_BC(MExp_Add(...)))`

Implications
- `MStmt_Assign.src : MRead_NBC`는 현재 방향과 일치합니다.
- `MExp_Store.src`를 `MCreate_BC`로 두는 것은 축이 섞인 상태이며, `MRead_BC` 쪽이 더 적합합니다.
- `read/create/loc` 분류표를 다시 작성할 때 `Store`는 `write` 계열이지만 source 인자는 `read`로 분류해야 합니다.

Follow-up
- [ ] `2026-03-09-sexp-translation-read-create-loc.md`를 읽을 때 이 정정 노트를 함께 참조하도록 연결할지 검토
- [ ] `MExp_Store.src` 타입을 `MRead_BC`로 바꿀지 구현 측면에서 검토
- [ ] `ai/implementations/` 쪽 현재 유효 규칙 문서에도 반영할지 검토
