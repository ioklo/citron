# MIR Spec Index

MIR는 표면 문법의 모호함을 제거한 Citron의 canonical semantics로 취급합니다.

- `value-model.md` : BC/NBC, `MRead`, `MCreate`, materialize, lifetime event 규칙
- `observable-behavior.md` : observable event, evaluation order, BC/NBC observation boundary

Boundary
- `ai/specs/language/`는 사용자가 보는 표면 문법과 타입 규칙을 둔다.
- `ai/specs/mir/`는 desugaring 이후에도 보존되어야 하는 Citron semantic rule을 둔다.
- MIR 관련 내용이라도 C++ class/API/layout, translator algorithm, verifier implementation 결정은 `ai/implementations/`에 둔다.
- `ai/implementations/`는 MIR/QIR 구현 결정, evaluator, target lowering 같은 구현 규약을 둔다.
