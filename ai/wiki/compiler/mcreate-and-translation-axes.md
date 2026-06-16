# MCreate And Translation Axes

Status: draft current
Area: compiler, MIR, QIR lowering
Keywords: MCreate, MRead, MExp, MInitExp, MLoc, MqTranslator, Direct, Indirect

## Current Direction
- Keep `MCreate` as a MIR surface type for now.
- Do not use `TranslateMCreate-*` as the central lowering primitive.
- Use `TranslateMExp`, `TranslateMLoc`, and `TranslateMInitExp` as the central lowering axes.
- Treat `MCreate_BC` as create-context payload that lowers through read/value path.
- Treat `MCreate_NBC` as destination-aware init path.
- Decide `Direct` / `Indirect` passing in MIR -> QIR lowering using ABI knowledge, not in MIR.

## Why Keep MCreate
The following consumers still benefit from a single create-context surface type:
- `MStmt_Return`
- `MStmt_Exp`
- local var init (`MStmt_LocalVarDeclInit_Create`)
- `MLoc_Materialize`

`MCreate` keeps "this position is create-context" explicit without changing all consumer fields at once.

## Lowering Primitives
Central primitives:
```text
TranslateMExp(...)
TranslateMLoc(...)
TranslateMInitExp(initExp, dest, ...)
```

`TranslateMRead(...)` may remain as an adapter:
```text
MRead_Exp -> TranslateMExp
MRead_Loc -> TranslateMLoc and adapt result
```

`TranslateMCreate(...)`, if it remains, should be a thin dispatch wrapper.

## BC Create
`MCreate_BC` remains a MIR variant but is not a destination-aware BC primitive.

Recommended interpretation:
```text
MCreate_BC { MExp* exp }
lowering -> TranslateMExp(exp, ...)
```

BC lvalue in create-context remains normalized through load:
```text
Loc(l) (BC) -> MCreate_BC(MExp_Load(l))
```

## NBC Create
`MCreate_NBC` remains an `MInitExp` wrapper.

```text
MCreate_NBC(initExp)
lowering -> TranslateMInitExp(initExp, dest, ...)
```

NBC is place/destination driven, not value-result driven.

## Call Argument Lowering
`MArgument` surface shape remains:
- `MArgument_Create`
- `MArgument_Loc`
- `MArgument_Move`

Rules:
- `MArgument_Create(MCreate_BC)` + ABI `Direct`
  - read `MExp` as value;
  - produce `QArg_CallArg` directly when possible.
- `MArgument_Create(MCreate_BC)` + ABI `Indirect`
  - prepare slot/storage;
  - fill it with `MCreate_BC`;
  - pass address-like `QArg_CallArg`.
- `MArgument_Create(MCreate_NBC)`
  - continue indirect/create path;
  - NBC does not directly produce value read call arg.

## MRead vs Direct / Indirect
`MRead_Exp` / `MRead_Loc` are MIR source shape axes.

```text
MRead_Exp
  BC expression read

MRead_Loc
  location read, BC or NBC
```

`Direct` / `Indirect` are ABI passing mode axes at call boundary.

They are not equivalent:
- `MRead_Exp` can lower to indirect passing for large BC values.
- `MRead_Loc` can lower to direct passing if a BC location is loaded into direct value.

Decision order:
```text
1. inspect MIR source shape
2. ABI decides Direct / Indirect at call edge
3. translator combines both into QIR argument/result shape
```

## Open Points
- Exact return shape of `TranslateMExp`: slot-only or const results too.
- BC discard path for `MStmt_Exp`.
- Common helper for call argument slot materialization.
- Whether `TranslateMCreate` remains public helper or private dispatch.
- ABI direct passing fast path range.

## History
- `ai/notes/2026-05-05-mcreate-surface-and-translation-split.md`
- `ai/specs/mir/value-model.md`
