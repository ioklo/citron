# Citron AI Wiki

Status: current entry point

이 wiki는 Citron 설계와 구현 지식을 agent가 빠르게 찾기 위한 현재 기준 문서 공간이다. `notes`는 history이고, 이곳은 현재 유효한 이해를 주제별로 정리한다.

## Reading Order
처음 복귀하거나 새 agent가 들어오면 아래 순서로 읽는다.

1. `ai/wiki/current-decisions.md`
2. `ai/wiki/migration-plan.md`
3. `ai/wiki/archive-plan.md`
4. `ai/wiki/language/index.md`
5. `ai/wiki/compiler/index.md`
6. 작업 주제에 해당하는 topic page

## Areas
- `language/` : 사용자가 보는 언어 규칙, 타입, trait/interface, module/cti
- `compiler/` : compiler phase, symbol/declaration model, ABI/lowering, witness table
- `process/` : 빌드, 테스트, 생성 파일 규칙

## Document Policy
- 새 기준 지식은 `ai/wiki/`에 쓴다.
- 시간순 논의와 실험 기록은 `ai/notes/`에 쓴다.
- `ai/specs/`와 `ai/implementations/`는 deprecated reference다. 새 기준 내용을 추가하지 않는다.
- `docs/`는 AI가 직접 수정하지 않는다.

## Topic Page Shape
각 topic page는 필요에 따라 유연하게 쓴다. 다만 agent 검색을 위해 상단에 아래 정보를 되도록 둔다.

```text
Status:
Area:
Keywords:
```

권장 섹션:
- Current Rules
- Compiler Model
- Examples
- Open Points
- History
