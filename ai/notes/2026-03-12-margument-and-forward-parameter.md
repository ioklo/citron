# 회의 / 정리 노트

Date: 2026-03-12
Title: `MArgument` 축과 `[forward] T&` 해석 규칙

Status
- current

Summary
- `MArgument`는 caller가 실제로 어떤 형태의 인자를 넘기는지 표현합니다.
- `[forward] T&`는 별도의 `MArgument` variant를 만들지 않고, callee가 parameter kind를 보고 해석하는 규약으로 둡니다.
- 따라서 `forward`는 argument-side category가 아니라 parameter-side calling convention입니다.

Background
- 현재 `MArgument`의 구분 목적은 다음 두 가지입니다.
  - 이 인자가 `Loc`인가 `Create`인가
  - 호출 시 소유권을 넘겼는가
- 이 축은 전달 형태를 나타내는 데에는 충분하지만, `[forward]`는 그 자체로 새로운 전달 형태는 아닙니다.
- `[forward] T&`는 caller가 특수한 payload를 만드는 기능이 아니라, callee가 전달된 인자의 value category를 보존해서 처리하는 규약에 가깝습니다.

Decision
- `MArgument`는 전달 형태만 표현합니다.
  - `MArgument_Loc`
  - `MArgument_Create`
  - `MArgument_Move`
- `MArgument_Forward` 같은 별도 variant는 두지 않습니다.
- `[forward] T&`는 `MFuncParameter`의 kind로 남기고, callee가 이를 보고 argument를 해석합니다.
- 해석 규칙은 다음과 같습니다.
  - argument가 `MArgument_Loc`이면 lvalue forwarding
  - argument가 `MArgument_Move`이면 rvalue forwarding

Rationale
- `forward`는 독립적인 storage/ownership category가 아닙니다.
- 실제 전달 payload는 여전히 다음 셋 중 하나입니다.
  - location alias (`Loc`)
  - 생성값 (`Create`)
  - 이동 가능한 source (`Move`)
- 만약 `MArgument_Forward`를 추가하면, 그 안에서 다시 `Loc`인지 `Move`인지 구분해야 해서 의미 축이 중복됩니다.
- 반대로 `MArgument`는 전달 형태만 유지하고, `[forward]`는 callee가 parameter kind로 해석하게 두면 축이 깔끔하게 분리됩니다.

`MFuncParameter` 별 권장 `MArgument` 생성 규칙

1. `T t` (plain value)
- 전반적으로 `MArgument_Create`를 사용합니다.
- `Loc(l) (BC)`
  - `MArgument_Create(MCreate_BC(MExp_Load(l)))`
- `Loc(l) (NBC)`
  - `MArgument_Create(MCreate_NBC(CopyCtorFromLoc(l)))`
- `move Loc(l) (BC)`
  - `MArgument_Create(MCreate_BC(MExp_Load(l)))`
  - BC에서는 non-move와 동일하게 trivialized move로 처리합니다.
- `move Loc(l) (NBC)`
  - `MArgument_Create(MCreate_NBC(MoveCtorFromLoc(l)))`
- `Exp(e) (BC)`
  - `MArgument_Create(MCreate_BC(e))`
- `InitExp(ie) (NBC)`
  - `MArgument_Create(MCreate_NBC(ie))`

2. `T&` (plain ref)
- alias 전달이므로 `MArgument_Loc`만 허용합니다.
- `Loc(l) (BC/NBC)`
  - `MArgument_Loc(l)`
- `move Loc(l) (BC/NBC)`
  - 금지
- `Exp(e) (BC)`
  - 금지
- `InitExp(ie) (NBC)`
  - 금지

3. `[in] T&`
- 읽기 전용 alias 바인딩입니다.
- lvalue는 그대로 `Loc`로 넘기고, rvalue는 materialize 후 `Loc`로 넘깁니다.
- `Loc(l) (BC/NBC)`
  - `MArgument_Loc(l)`
- `move Loc(l) (BC/NBC)`
  - 금지
- `Exp(e) (BC)`
  - `MArgument_Loc(MLoc_Materialize(MCreate_BC(e)))`
- `InitExp(ie) (NBC)`
  - `MArgument_Loc(MLoc_Materialize(MCreate_NBC(ie)))`
- materialized temporary의 lifetime은 호출 full-expression 끝까지 유지되어야 합니다.

4. `[move] T&`
- 소비 가능한 source만 받습니다.
- `Loc(l) (BC/NBC)`
  - 금지
- `move Loc(l) (BC/NBC)`
  - `MArgument_Move(MMoveSource_Loc(l))`
- `Exp(e) (BC)`
  - `MArgument_Move(MMoveSource_Materialized(MLoc_Materialize(MCreate_BC(e))))`
- `InitExp(ie) (NBC)`
  - `MArgument_Move(MMoveSource_Materialized(MLoc_Materialize(MCreate_NBC(ie))))`
- BC의 `move`는 허용하되, lowering은 trivialized move로 처리할 수 있습니다.

5. `[forward] T&`
- caller는 forwarding 전용 variant를 만들지 않습니다.
- lvalue input이면 `Loc`, rvalue/move input이면 `Move`를 생성합니다.
- `Loc(l) (BC/NBC)`
  - `MArgument_Loc(l)`
- `move Loc(l) (BC/NBC)`
  - `MArgument_Move(MMoveSource_Loc(l))`
- `Exp(e) (BC)`
  - `MArgument_Move(MMoveSource_Materialized(MLoc_Materialize(MCreate_BC(e))))`
- `InitExp(ie) (NBC)`
  - `MArgument_Move(MMoveSource_Materialized(MLoc_Materialize(MCreate_NBC(ie))))`
- callee는 parameter kind가 `[forward] T&`인 것을 보고 다음처럼 해석합니다.
  - `MArgument_Loc`이면 lvalue로 본다
  - `MArgument_Move`이면 rvalue로 본다
- 즉 copy할지 move할지, 혹은 다음 호출에서 어떻게 재전달할지는 callee가 결정합니다.

Implications
- `forward`는 argument-side type이 아니라 parameter-side semantics입니다.
- `MArgument`와 `MFuncParameter`의 역할이 다음처럼 분리됩니다.
  - `MArgument`: caller가 넘긴 실제 전달 형태
  - `MFuncParameter`: callee가 그 인자를 소비하는 규칙
- generic에서도 동일한 규칙을 유지할 수 있습니다.
  - caller는 구체 타입이 BC/NBC인지에 따라 `Create`/`Loc`/`Move`를 만든다
  - callee는 `[forward]` 규칙으로 value category를 보존해서 해석한다

Open points
- BC에서 `move` 사용 시 warning을 줄지 여부
- `[forward] T&`를 받는 callee 내부 lowering에서 value category flag를 별도 보존할지, `MArgument` variant만으로 충분한지 검토
- `CopyCtorFromLoc`, `MoveCtorFromLoc` 같은 이름을 실제 MIR 노드명으로 확정할지 검토
