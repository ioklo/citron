# AI 문서 색인

이 저장소의 AI 지식 문서 진입점입니다. 새 기준 문서는 `ai/wiki/`에 둡니다. `ai/notes/`는 시간순 history로 유지하고, 기존 `specs`와 `implementations` 내용은 wiki로 옮긴 뒤 active tree에는 두지 않습니다. 필요하면 git history에서 확인합니다.

처음 읽을 것:
- `ai/wiki/home.md`
- `ai/wiki/current-decisions.md`
- `ai/wiki/migration-plan.md`
- `ai/wiki/language/index.md`
- `ai/wiki/compiler/index.md`

현재 기준 지식:
- `ai/wiki/` : agent와 사람이 먼저 읽는 현재 설계/구현 지식
- `ai/wiki/language/` : 표면 언어, 타입 시스템, trait/interface, module/cti
- `ai/wiki/compiler/` : 컴파일 파이프라인, MIR/QIR, ABI, witness, lowering
- `ai/wiki/process/` : 빌드/테스트/생성 절차의 wiki 입구

History:
- `ai/notes/` : 회의록, 실험 노트, 결정이 바뀐 과정

Removed source snapshots:
- `git history: ai/specs/` : 기존 표면 언어/MIR 스펙 문서. 새 기준 내용은 wiki에 추가한다.
- `git history: ai/implementations/` : 기존 구현 스냅샷 문서. 새 기준 내용은 wiki에 추가한다.

기타:
- `ai/process/` : 기존 빌드/테스트 절차 문서
- `ai/ai-guidelines/` : AI 에이전트용 작업 규칙과 체크리스트
- `ai/templates/` : 문서 템플릿
