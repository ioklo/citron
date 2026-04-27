# AI 스펙 색인

현재 유효한 표면 언어 규칙과 MIR semantic contract를 유지합니다. MIR/QIR의 C++ 구현 결정, evaluator, lowering 구현 규약은 `ai/implementations`, 회의/실험 논의는 `ai/notes`를 봅니다.

- `language/` : 사용자가 보는 표면 문법, 타입, 제어흐름, ownership, nullable 규칙
- `mir/` : 모호함이 제거된 canonical Citron semantics와 observable behavior. C++ class/API/layout 결정은 여기 두지 않는다.

추천 시작점
- `language/functions-and-control-flow.md`
- `language/types-and-ownership.md`
- `language/nullable-and-iteration.md`
- `language/if-and-is.md`
- `mir/value-model.md`
- `mir/observable-behavior.md`
