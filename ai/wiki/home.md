# Citron AI Wiki

Status: current entry point
Area: wiki, search
Keywords: wiki, index, search, routing, current-decisions, current-agenda, language, compiler, process

이 wiki는 Citron 설계와 구현 지식을 agent가 빠르게 찾기 위한 현재 기준 문서 공간이다. `notes`는 history이고, 이곳은 현재 유효한 이해를 주제별로 정리한다.

## Search Order
처음 복귀하거나 새 agent가 들어오면 아래 순서로 본다.

1. `ai/current-agenda.md`
2. `ai/wiki/current-decisions.md`
3. `ai/wiki/language/index.md` 또는 `ai/wiki/compiler/index.md`
4. 작업 주제에 해당하는 topic page
5. 세부 히스토리가 더 필요하면 `ai/notes/`

## Areas
- `language/` : 사용자가 보는 언어 규칙, 타입, trait/interface, module/cti
- `compiler/` : compiler phase, symbol/declaration model, ABI/lowering, witness table
- `process/` : 빌드, 테스트, 생성 파일 규칙

## Routing Hints
- 문법, 타입 시스템, trait/interface, visibility, module/import, surface syntax는 `language/`부터 본다.
- declaration model, `RNode`, lowering, MIR/QIR, resolver, witness, translator는 `compiler/`부터 본다.
- 빌드, 테스트, generator, generated file rule은 `process/`부터 본다.
- 현재 주제가 진행 중 설계인지, 아직 미확정 쟁점이 있는지 먼저 알고 싶으면 `ai/current-agenda.md`를 본다.
- 이미 확정된 큰 규칙만 빠르게 보고 싶으면 `ai/wiki/current-decisions.md`를 본다.

## Document Policy
- 새 기준 지식은 `ai/wiki/`에 쓴다.
- 시간순 논의와 실험 기록은 `ai/notes/`에 쓴다.
- 기존 `specs`와 `implementations` 내용은 active tree에 유지하지 않는다. 필요하면 git history에서만 확인한다.
- `docs/`는 AI가 직접 수정하지 않는다.

## Topic Page Shape
각 topic page는 필요에 따라 유연하게 쓰되, agent 검색을 위해 상단에 아래 정보를 되도록 둔다.

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
