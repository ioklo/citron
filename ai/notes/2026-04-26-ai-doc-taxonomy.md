# 회의 / 실험 노트

Date: 2026-04-26
Title: AI docs taxonomy: language, MIR spec, and implementation snapshots

Summary
- Citron의 실제 semantic 기준은 MIR로 본다.
- 표면 문법/타입 규칙과 MIR semantic spec을 모두 `ai/specs` 아래에 두되, 디렉터리를 분리한다.
- QIR, QEvaluator, LLVM lowering 같은 target-facing 중간 단계는 `ai/implementations`에 둔다.
- 다만 최종 분류 기준은 MIR/QIR 같은 단계 이름이 아니라, 문서 내용이 semantic contract인지 C++ implementation decision인지로 본다.

Decisions
## 1) `ai/specs/language`는 surface language 규칙을 둔다
- 사용자가 직접 쓰는 문법, 타입, 제어흐름, ownership, nullable 규칙을 둔다.
- 디렉터리 이름은 `language`를 유지한다.
- 나중에 표면 문법 문서가 많아지면 `syntax`로 세분화할 수 있다.

Examples:
- 함수/제어흐름 표면 규칙
- `void`, `string`, `shared`, `box`, `&` 규칙
- nullable/iteration surface contract
- `if`/`is` 바인딩 문법

## 2) `ai/specs/mir`는 canonical Citron semantics를 둔다
- MIR는 표면 언어의 모호함을 제거한 Citron semantic form이다.
- 따라서 MIR value model과 observable behavior처럼 backend가 보존해야 하는 의미 규칙은 spec으로 관리한다.
- MIR spec은 backend가 무엇이든 보존해야 하는 의미를 정의한다.

Examples:
- BC/NBC value model
- `MRead`/`MCreate`/`MLoc_Materialize`
- NBC lifetime event
- observable event trace
- evaluation order가 observable event 순서를 정한다는 규칙

## 3) `ai/implementations`는 QIR/lowering/evaluator 규약을 둔다
- QIR는 target language로 가는 중간언어 성격이 강하다.
- QEvaluation은 QIR semantic reference executor다.
- LLVM/backend lowering은 physical ABI, register/stack, call frame을 결정한다.
- 이 영역은 Citron semantics 자체라기보다 semantics를 구현하는 방식이므로 `ai/implementations`에 둔다.
- MIR 관련 내용이라도 C++ class/API/layout, translator algorithm, verifier implementation, storage/cache 구조 같은 구현 결정은 `ai/implementations`에 둔다.

Examples:
- QIR slot role
- Direct/Indirect return lowering shape
- QEvaluator frame metadata
- ABI policy object
- translator/lowering implementation snapshot

## 4) Notes are append-only logs by default
- `ai/notes`는 회의/실험 기록 로그로 취급한다.
- 오래된 note의 내용을 최신 결정에 맞춰 고쳐 쓰는 것은 지양한다.
- 최신 결정과 충돌하는 과거 note가 있으면:
  - 새 note를 추가하거나,
  - 같은 날짜의 진행 중인 note에 update/addendum을 추가하거나,
  - spec/implementation snapshot에서 최신 결론을 반영한다.
- 과거 note 안의 링크가 깨지는 경우도 가능하면 과거 기록을 직접 수정하기보다, 색인이나 최신 snapshot에서 새 위치를 안내한다.

Open Points
- `ai/specs/language`를 장기적으로 `ai/specs/syntax`로 rename할지 여부.
- MIR spec이 커지면 `value-model`, `control-flow`, `call-lifetime`, `observable-behavior` 등으로 더 쪼갤지 여부.
